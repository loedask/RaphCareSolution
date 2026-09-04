window.raphCareOpsFleet = {
  _barcodeScan: null,
  _ocrBusy: false,

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

  startBarcodeScan: async function (dotNetHelper) {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
      return false;
    if (typeof BarcodeDetector === "undefined")
      return false;

    var video = document.getElementById("fleet-scan-video");
    if (!video)
      return false;

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
        window.raphCareOpsFleet.stopBarcodeScan();
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
          window.raphCareOpsFleet.stopBarcodeScan();
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

  stopBarcodeScan: function () {
    var scan = window.raphCareOpsFleet._barcodeScan;
    window.raphCareOpsFleet._barcodeScan = null;
    if (scan) {
      if (scan.stop) scan.stop();
      if (scan.frame) cancelAnimationFrame(scan.frame);
    }
    var video = document.getElementById("fleet-scan-video");
    if (video && video.srcObject) {
      video.srcObject.getTracks().forEach(function (track) { track.stop(); });
      video.srcObject = null;
    }
  },

  /**
   * Reads a local image from a file input. Tries barcodes first, then OCR.
   * Does not upload the image; clears the input when finished.
   */
  extractFromFileInput: async function (inputId) {
    var input = document.getElementById(inputId);
    if (!input || !input.files || !input.files[0])
      return { ok: false, error: "nofile" };

    if (window.raphCareOpsFleet._ocrBusy)
      return { ok: false, error: "busy" };

    var file = input.files[0];
    window.raphCareOpsFleet._ocrBusy = true;

    try {
      var barcodes = [];
      var bitmap = null;
      try {
        bitmap = await createImageBitmap(file);
        if (typeof BarcodeDetector !== "undefined") {
          try {
            var detector = new BarcodeDetector({
              formats: window.raphCareOpsFleet._barcodeFormats
            });
            var codes = await detector.detect(bitmap);
            if (codes && codes.length) {
              barcodes = codes
                .map(function (c) { return c.rawValue; })
                .filter(function (v) { return !!v; });
            }
          } catch {
            // Fall through to OCR.
          }
        }
      } catch {
        // createImageBitmap failed; try OCR on the File directly.
      }

      var text = "";
      if (barcodes.length === 0) {
        text = await window.raphCareOpsFleet._recognizeText(bitmap || file);
      }

      if (bitmap && typeof bitmap.close === "function")
        bitmap.close();

      input.value = "";
      return { ok: true, barcodes: barcodes, text: text || "" };
    } catch (err) {
      try { input.value = ""; } catch { /* ignore */ }
      return { ok: false, error: (err && err.message) ? err.message : "failed" };
    } finally {
      window.raphCareOpsFleet._ocrBusy = false;
    }
  },

  _loadTesseract: function () {
    if (window.Tesseract)
      return Promise.resolve();
    return new Promise(function (resolve, reject) {
      var existing = document.querySelector("script[data-raphcare-tesseract]");
      if (existing) {
        existing.addEventListener("load", function () { resolve(); });
        existing.addEventListener("error", function () { reject(new Error("ocrload")); });
        return;
      }
      var script = document.createElement("script");
      script.src = "https://cdn.jsdelivr.net/npm/tesseract.js@5.1.1/dist/tesseract.min.js";
      script.async = true;
      script.setAttribute("data-raphcare-tesseract", "1");
      script.onload = function () { resolve(); };
      script.onerror = function () { reject(new Error("ocrload")); };
      document.head.appendChild(script);
    });
  },

  _recognizeText: async function (source) {
    await window.raphCareOpsFleet._loadTesseract();
    var result = await window.Tesseract.recognize(source, "eng", {
      logger: function () { /* keep quiet */ }
    });
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
