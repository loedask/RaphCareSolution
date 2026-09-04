window.raphCareAuth = {
  getToken: function () {
    return localStorage.getItem("rc_token");
  },
  setToken: function (token, accountKind) {
    localStorage.setItem("rc_token", token);
    localStorage.setItem("rc_account", accountKind);
  },
  clear: function () {
    localStorage.removeItem("rc_token");
    localStorage.removeItem("rc_account");
  },
  getAccountKind: function () {
    return localStorage.getItem("rc_account");
  },
  isSessionValid: function () {
    const token = localStorage.getItem("rc_token");
    const kind = localStorage.getItem("rc_account");
    if (!token || !kind) return false;

    try {
      const segment = token.split(".")[1];
      if (!segment) return false;
      const base64 = segment.replace(/-/g, "+").replace(/_/g, "/");
      const payload = JSON.parse(atob(base64));
      if (!payload.exp) return false;
      const skewMs = 2 * 60 * 1000;
      return payload.exp * 1000 > Date.now() - skewMs;
    } catch {
      return false;
    }
  }
};
