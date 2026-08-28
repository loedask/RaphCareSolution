window.raphCareLocale = {
  get: function () {
    return localStorage.getItem("web_language") || "en";
  },
  set: function (code) {
    localStorage.setItem("web_language", code || "en");
  },
  setDocumentLang: function (code) {
    document.documentElement.lang = code || "en";
  },
  guardSelectKeys: function (el) {
    if (!el || el.dataset.rcSelectGuarded) return;
    el.dataset.rcSelectGuarded = "1";
    el.addEventListener("keydown", function (e) {
      if (e.key === "Enter" || e.key === "ArrowDown" || e.key === "ArrowUp" || e.key === "Escape") {
        e.preventDefault();
      }
    });
  }
};
