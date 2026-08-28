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
  }
};
