window.raphCareOpsFleet = {
  _barcodeScan: null,
  _liveCamera: null,
  _ocrBusy: false,
  _ocrWorker: null,
  _ocrWorkerPromise: null,

  _barcodeFormats: [
    "qr_code",
    "code_128",
    "code_39",
    "code_93",
    "ean_13",
    "ean_8",
    "upc_a",
    "upc_e",
    "itf",
    "codabar",
    "data_matrix"
  ],

  _cdn: {
    script: "https://cdn.jsdelivr.net/npm/tesseract.js@5.1.1/dist/tesseract.min.js",
    worker: "https://cdn.jsdelivr.net/npm/tesseract.js@5.1.1/dist/worker.min.js",
    core: "https://cdn.jsdelivr.net/npm/tesseract.js-core@5.1.1",
    lang: "https://tessdata.projectnaptha.com/4.0.0"
  },

  startBarcodeScan: async function (dotNetHelper) {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
      return false;
    if (typeof BarcodeDetector === "undefined")
      return false;

    var video = document.getElementById("fleet-scan-video");
    if (!video)
      return false;

    window.raphCareOpsFleet.stopAllCameras();

    var stream;
    try {
      stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "environment" }
      });
    } catch {
      return false;
    }

    video.srcObject = stream;
    await video.play();

    var formats = window.raphCareOpsFleet._barcodeFormats;
    var detector;
    try {
      detector = new BarcodeDetector({ formats: formats });
    } catch {
      try {
        detector = new BarcodeDetector({ formats: ["qr_code", "code_128"] });
      } catch {
        window.raphCareOpsFleet.stopAllCameras();
        return false;
      }
    }

    var stopped = false;
    var scan = {
      stream: stream,
      stop: function () { stopped = true; },
      frame: 0
    };
    var tick = async function () {
      if (stopped) return;
      try {
        var codes = await detector.detect(video);
        if (codes && codes.length > 0 && codes[0].rawValue) {
          stopped = true;
          window.raphCareOpsFleet.stopAllCameras();
          await dotNetHelper.invokeMethodAsync("OnFleetBarcodeDetected", codes[0].rawValue);
          return;
        }
      } catch {
        // keep scanning
      }
      scan.frame = requestAnimationFrame(tick);
    };
    window.raphCareOpsFleet._barcodeScan = scan;
    scan.frame = requestAnimationFrame(tick);
    return true;
  },

  startLiveCamera: async function () {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
      return false;

    var video = document.getElementById("fleet-scan-video");
    if (!video)
      return false;

    window.raphCareOpsFleet.stopAllCameras();

    var stream;
    try {
      stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "environment" }
      });
    } catch {
      try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true });
      } catch {
        return false;
      }
    }

    video.srcObject = stream;
    await video.play();
    window.raphCareOpsFleet._liveCamera = { stream: stream };
    return true;
  },

  stopBarcodeScan: function () {
    window.raphCareOpsFleet.stopAllCameras();
  },

  stopLiveCamera: function () {
    window.raphCareOpsFleet.stopAllCameras();
  },

  stopAllCameras: function () {
    var scan = window.raphCareOpsFleet._barcodeScan;
    window.raphCareOpsFleet._barcodeScan = null;
    if (scan) {
      if (scan.stop) scan.stop();
      if (scan.frame) cancelAnimationFrame(scan.frame);
    }
    window.raphCareOpsFleet._liveCamera = null;

    var video = document.getElementById("fleet-scan-video");
    if (video && video.srcObject) {
      video.srcObject.getTracks().forEach(function (track) { track.stop(); });
      video.srcObject = null;
    }
  },

  captureAndExtract: async function () {
    if (window.raphCareOpsFleet._ocrBusy)
      return JSON.stringify({ ok: false, error: "busy" });

    var video = document.getElementById("fleet-scan-video");
    if (!video || !video.srcObject || video.videoWidth < 2)
      return JSON.stringify({ ok: false, error: "nocamera" });

    window.raphCareOpsFleet._ocrBusy = true;
    try {
      var canvas = document.createElement("canvas");
      canvas.width = video.videoWidth;
      canvas.height = video.videoHeight;
      var ctx = canvas.getContext("2d", { willReadFrequently: true });
      if (!ctx)
        return JSON.stringify({ ok: false, error: "failed" });
      ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

      var result = await window.raphCareOpsFleet._extractFromCanvas(canvas);
      window.raphCareOpsFleet.stopAllCameras();
      return JSON.stringify(result);
    } catch (err) {
      window.raphCareOpsFleet.stopAllCameras();
      return JSON.stringify({
        ok: false,
        error: window.raphCareOpsFleet._errorCode(err)
      });
    } finally {
      window.raphCareOpsFleet._ocrBusy = false;
    }
  },

  /**
   * Snapshot the File synchronously before any await so Blazor re-renders cannot clear it.
   */
  extractFromFileInput: async function (inputId) {
    var input = document.getElementById(inputId);
    if (!input || !input.files || !input.files[0])
      return JSON.stringify({ ok: false, error: "nofile" });

    // Capture before any await / Blazor re-render.
    var file = input.files[0];
    try { input.value = ""; } catch { /* ignore */ }

    if (window.raphCareOpsFleet._ocrBusy)
      return JSON.stringify({ ok: false, error: "busy" });

    window.raphCareOpsFleet._ocrBusy = true;
    try {
      var canvas = await window.raphCareOpsFleet._fileToCanvas(file);
      var result = await window.raphCareOpsFleet._extractFromCanvas(canvas);
      return JSON.stringify(result);
    } catch (err) {
      return JSON.stringify({
        ok: false,
        error: window.raphCareOpsFleet._errorCode(err)
      });
    } finally {
      window.raphCareOpsFleet._ocrBusy = false;
    }
  },

  _errorCode: function (err) {
    var message = (err && err.message) ? String(err.message) : "";
    if (/ocrload|Failed to fetch|Loading CSS chunk|Script error|network/i.test(message))
      return "ocrload";
    if (/ocrfail|recognize|worker|wasm/i.test(message))
      return "ocrfail";
    return message || "failed";
  },

  _fileToCanvas: function (file) {
    return new Promise(function (resolve, reject) {
      var url = URL.createObjectURL(file);
      var img = new Image();
      img.onload = function () {
        try {
          var canvas = window.raphCareOpsFleet._imageToCanvas(img);
          URL.revokeObjectURL(url);
          resolve(canvas);
        } catch (err) {
          URL.revokeObjectURL(url);
          reject(err);
        }
      };
      img.onerror = function () {
        URL.revokeObjectURL(url);
        reject(new Error("imgload"));
      };
      img.src = url;
    });
  },

  _imageToCanvas: function (img) {
    var width = img.naturalWidth || img.width;
    var height = img.naturalHeight || img.height;
    if (!width || !height)
      throw new Error("imgload");

    // Upscale small / distant watch shots so OCR has more pixels.
    var scale = 1;
    var minSide = Math.min(width, height);
    if (minSide < 900)
      scale = Math.min(2.5, 900 / minSide);

    var canvas = document.createElement("canvas");
    canvas.width = Math.round(width * scale);
    canvas.height = Math.round(height * scale);
    var ctx = canvas.getContext("2d", { willReadFrequently: true });
    if (!ctx)
      throw new Error("failed");
    ctx.imageSmoothingEnabled = true;
    ctx.imageSmoothingQuality = "high";
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
    return canvas;
  },

  _invertCanvas: function (source) {
    var canvas = document.createElement("canvas");
    canvas.width = source.width;
    canvas.height = source.height;
    var ctx = canvas.getContext("2d", { willReadFrequently: true });
    if (!ctx)
      throw new Error("failed");
    ctx.drawImage(source, 0, 0);
    var image = ctx.getImageData(0, 0, canvas.width, canvas.height);
    var data = image.data;
    for (var i = 0; i < data.length; i += 4) {
      data[i] = 255 - data[i];
      data[i + 1] = 255 - data[i + 1];
      data[i + 2] = 255 - data[i + 2];
    }
    ctx.putImageData(image, 0, 0);
    return canvas;
  },

  /** Grayscale + contrast. Helps white-on-black Device Info and screen glare. */
  _contrastCanvas: function (source) {
    var canvas = document.createElement("canvas");
    canvas.width = source.width;
    canvas.height = source.height;
    var ctx = canvas.getContext("2d", { willReadFrequently: true });
    if (!ctx)
      throw new Error("failed");
    ctx.drawImage(source, 0, 0);
    var image = ctx.getImageData(0, 0, canvas.width, canvas.height);
    var data = image.data;
    var contrast = 1.45;
    var intercept = 128 * (1 - contrast);
    for (var i = 0; i < data.length; i += 4) {
      var gray = 0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2];
      var v = Math.max(0, Math.min(255, gray * contrast + intercept));
      data[i] = v;
      data[i + 1] = v;
      data[i + 2] = v;
    }
    ctx.putImageData(image, 0, 0);
    return canvas;
  },

  _scoreOcrText: function (text) {
    if (!text)
      return -1;
    var t = String(text);
    var score = Math.min(25, t.trim().length / 8);
    if (/\bMAC\b/i.test(t))
      score += 4;
    if (/\bDevice\s*Info\b/i.test(t))
      score += 3;
    if (/(?:[0-9A-Fa-f]{2}[:\-]){5}[0-9A-Fa-f]{2}/.test(t))
      score += 8;
    else if (/\b(?=[0-9A-Fa-f]*[A-Fa-f])[0-9A-Fa-f]{12}\b/.test(t))
      score += 5;
    if (/\bET?585\b/i.test(t))
      score += 6;
    else if (/\bET?580\b/i.test(t))
      score += 6;
    else if (/\bET?59\d\b/i.test(t))
      score += 1;
    if (/\bVersion\b/i.test(t))
      score += 1;
    return score;
  },

  _textLooksUseful: function (text) {
    return window.raphCareOpsFleet._scoreOcrText(text) >= 6;
  },

  _extractFromCanvas: async function (canvas) {
    var barcodes = [];
    if (typeof BarcodeDetector !== "undefined") {
      try {
        var detector = new BarcodeDetector({
          formats: window.raphCareOpsFleet._barcodeFormats
        });
        var codes = await detector.detect(canvas);
        if (codes && codes.length) {
          barcodes = codes
            .map(function (c) { return c.rawValue; })
            .filter(function (v) { return !!v; });
        }
      } catch {
        // Fall through to OCR.
      }
    }

    if (barcodes.length > 0)
      return { ok: true, barcodes: barcodes, text: "" };

    var contrast = null;
    var inverted = null;
    var invertedContrast = null;
    try { contrast = window.raphCareOpsFleet._contrastCanvas(canvas); } catch { /* keep */ }
    try { inverted = window.raphCareOpsFleet._invertCanvas(canvas); } catch { /* keep */ }
    try {
      if (contrast)
        invertedContrast = window.raphCareOpsFleet._invertCanvas(contrast);
    } catch { /* keep */ }

    var variants = [
      { canvas: canvas, whitelist: null },
      { canvas: contrast, whitelist: null },
      { canvas: inverted, whitelist: null },
      { canvas: invertedContrast, whitelist: null },
      // MAC-only alphabet on the best Device Info prep (white-on-black → inverted contrast).
      {
        canvas: invertedContrast || inverted || contrast || canvas,
        whitelist: "0123456789ABCDEFabcdef:"
      }
    ];

    var bestText = "";
    var bestScore = -1;
    var merged = [];

    for (var i = 0; i < variants.length; i++) {
      var variant = variants[i];
      if (!variant.canvas)
        continue;
      try {
        var text = await window.raphCareOpsFleet._recognizeText(variant.canvas, variant.whitelist);
        if (!text || !text.trim())
          continue;
        merged.push(text.trim());
        var score = window.raphCareOpsFleet._scoreOcrText(text);
        // Prefer MAC-restricted passes slightly when they actually found a MAC.
        if (variant.whitelist && /(?:[0-9A-Fa-f]{2}[:\-]){5}[0-9A-Fa-f]{2}|\b[0-9A-Fa-f]{12}\b/.test(text))
          score += 2;
        if (score > bestScore) {
          bestScore = score;
          bestText = text;
        }
      } catch {
        // Try the next prep.
      }
    }

    // Give the parser every distinct pass so a correct MAC in a weaker overall pass is not lost.
    // Put the highest-scoring pass first so PreferFill / first MAC match stay stable.
    var combined = bestText || "";
    if (merged.length > 1) {
      var unique = [];
      var seen = {};
      var pushUnique = function (chunk) {
        var key = chunk.replace(/\s+/g, " ").toLowerCase();
        if (seen[key])
          return;
        seen[key] = true;
        unique.push(chunk);
      };
      if (bestText)
        pushUnique(bestText.trim());
      for (var m = 0; m < merged.length; m++)
        pushUnique(merged[m]);
      combined = unique.join("\n");
    }

    return { ok: true, barcodes: [], text: combined || bestText || "" };
  },

  _loadTesseract: function () {
    if (window.Tesseract)
      return Promise.resolve();
    return new Promise(function (resolve, reject) {
      var existing = document.querySelector("script[data-raphcare-tesseract]");
      if (existing) {
        if (window.Tesseract) {
          resolve();
          return;
        }
        existing.addEventListener("load", function () {
          if (window.Tesseract) resolve();
          else reject(new Error("ocrload"));
        });
        existing.addEventListener("error", function () { reject(new Error("ocrload")); });
        return;
      }
      var script = document.createElement("script");
      script.src = window.raphCareOpsFleet._cdn.script;
      script.async = true;
      script.setAttribute("data-raphcare-tesseract", "1");
      script.onload = function () {
        if (window.Tesseract) resolve();
        else reject(new Error("ocrload"));
      };
      script.onerror = function () { reject(new Error("ocrload")); };
      document.head.appendChild(script);
    });
  },

  _ensureOcrWorker: async function () {
    if (window.raphCareOpsFleet._ocrWorker)
      return window.raphCareOpsFleet._ocrWorker;
    if (window.raphCareOpsFleet._ocrWorkerPromise)
      return window.raphCareOpsFleet._ocrWorkerPromise;

    window.raphCareOpsFleet._ocrWorkerPromise = (async function () {
      await window.raphCareOpsFleet._loadTesseract();
      var cdn = window.raphCareOpsFleet._cdn;
      var worker = await window.Tesseract.createWorker("eng", 1, {
        workerPath: cdn.worker,
        corePath: cdn.core,
        langPath: cdn.lang,
        logger: function () { /* quiet */ }
      });
      window.raphCareOpsFleet._ocrWorker = worker;
      return worker;
    })();

    try {
      return await window.raphCareOpsFleet._ocrWorkerPromise;
    } catch (err) {
      window.raphCareOpsFleet._ocrWorkerPromise = null;
      throw err;
    }
  },

  _recognizeText: async function (canvas, whitelist) {
    var worker = await window.raphCareOpsFleet._ensureOcrWorker();
    var chars = whitelist
      || "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:-./_ #";
    try {
      await worker.setParameters({
        tessedit_char_whitelist: chars,
        preserve_interword_spaces: "1"
      });
    } catch {
      // Parameters are best-effort.
    }

    var result = await worker.recognize(canvas);
    return (result && result.data && result.data.text) ? result.data.text : "";
  },

  clickFileInput: function (inputId) {
    var input = document.getElementById(inputId);
    if (!input)
      return false;
    input.click();
    return true;
  }
};
