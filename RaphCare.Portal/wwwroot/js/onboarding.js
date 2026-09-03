window.raphCareOnboarding = {
  storageKey: "rc_hospital_onboarding",

  save: function (json) {
    if (!json) return;
    localStorage.setItem(this.storageKey, json);
  },

  load: function () {
    return localStorage.getItem(this.storageKey);
  },

  clear: function () {
    localStorage.removeItem(this.storageKey);
  }
};
