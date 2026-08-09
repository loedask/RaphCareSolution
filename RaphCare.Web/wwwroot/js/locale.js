window.raphCareLocale = {
  get: function () {
    return localStorage.getItem("web_language") || "en";
  },
  set: function (code) {
    localStorage.setItem("web_language", code || "en");
  },
  setDocumentLang: function (code) {
    document.documentElement.lang = code || "en";
  }
};
