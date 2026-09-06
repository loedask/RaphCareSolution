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
  },
  // Closes searchable selects when the user clicks/taps outside. A fixed CSS backdrop
  // often misses those clicks when ancestors create stacking contexts (sticky top bar, cards).
  bindSelectDismissOutside: function (rootEl, dotNetRef) {
    if (!rootEl || !dotNetRef) return;
    this.unbindSelectDismissOutside(rootEl);
    const dismiss = function (e) {
      if (rootEl.contains(e.target)) return;
      dotNetRef.invokeMethodAsync("CloseFromOutside");
    };
    rootEl._rcSelectDismiss = dismiss;
    // Defer so the same gesture that opened the select does not immediately close it.
    setTimeout(function () {
      if (rootEl._rcSelectDismiss !== dismiss) return;
      document.addEventListener("pointerdown", dismiss, true);
      document.addEventListener("focusin", dismiss, true);
    }, 0);
  },
  unbindSelectDismissOutside: function (rootEl) {
    if (!rootEl || !rootEl._rcSelectDismiss) return;
    document.removeEventListener("pointerdown", rootEl._rcSelectDismiss, true);
    document.removeEventListener("focusin", rootEl._rcSelectDismiss, true);
    rootEl._rcSelectDismiss = null;
  }
};
