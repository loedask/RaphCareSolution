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
  }
};
