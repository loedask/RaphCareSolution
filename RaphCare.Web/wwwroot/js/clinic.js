window.raphCareClinic = {
  getClinicId: function () {
    return localStorage.getItem("rc_clinic_id");
  },
  setClinicId: function (clinicId) {
    if (clinicId) localStorage.setItem("rc_clinic_id", clinicId);
    else localStorage.removeItem("rc_clinic_id");
  },
  getPendingClinicId: function () {
    return localStorage.getItem("rc_pending_clinic_id");
  },
  setPendingClinicId: function (clinicId) {
    if (clinicId) localStorage.setItem("rc_pending_clinic_id", clinicId);
    else localStorage.removeItem("rc_pending_clinic_id");
  },
  clearPendingClinicId: function () {
    localStorage.removeItem("rc_pending_clinic_id");
  },
  printPage: function () {
    window.print();
  },
  _qrScan: null,
  startQrScan: async function (dotNetHelper) {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia)
      return false;
    if (typeof BarcodeDetector === "undefined")
      return false;

    var video = document.getElementById("qr-scan-video");
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

    var detector = new BarcodeDetector({ formats: ["qr_code"] });
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
          window.raphCareClinic.stopQrScan();
          await dotNetHelper.invokeMethodAsync("OnQrDetected", codes[0].rawValue);
          return;
        }
      } catch {
        // keep scanning
      }
      scan.frame = requestAnimationFrame(tick);
    };
    window.raphCareClinic._qrScan = scan;
    scan.frame = requestAnimationFrame(tick);
    return true;
  },
  stopQrScan: function () {
    var scan = window.raphCareClinic._qrScan;
    window.raphCareClinic._qrScan = null;
    if (scan) {
      if (scan.stop) scan.stop();
      if (scan.frame) cancelAnimationFrame(scan.frame);
    }
    var video = document.getElementById("qr-scan-video");
    if (video && video.srcObject) {
      video.srcObject.getTracks().forEach(function (track) { track.stop(); });
      video.srcObject = null;
    }
  }
};
