window.raphCareUi = window.raphCareUi || {
  guardSelectKeys: function (el) {
    if (!el || el.dataset.rcSelectGuarded) return;
    el.dataset.rcSelectGuarded = "1";
    el.addEventListener("keydown", function (e) {
      if (e.key === "Enter" || e.key === "ArrowDown" || e.key === "ArrowUp" || e.key === "Escape") {
        e.preventDefault();
      }
    });
  },
  bindSelectDismissOutside: function (rootEl, dotNetRef) {
    if (!rootEl || !dotNetRef) return;
    this.unbindSelectDismissOutside(rootEl);
    const handler = function (e) {
      if (rootEl.contains(e.target)) return;
      dotNetRef.invokeMethodAsync("CloseFromOutside");
    };
    rootEl._rcSelectDismiss = handler;
    setTimeout(function () {
      if (rootEl._rcSelectDismiss === handler)
        document.addEventListener("pointerdown", handler, true);
    }, 0);
  },
  unbindSelectDismissOutside: function (rootEl) {
    if (!rootEl || !rootEl._rcSelectDismiss) return;
    document.removeEventListener("pointerdown", rootEl._rcSelectDismiss, true);
    rootEl._rcSelectDismiss = null;
  }
};
