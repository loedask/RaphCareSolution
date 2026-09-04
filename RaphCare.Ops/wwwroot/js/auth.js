window.raphCareOpsAuth = {
  getToken: function () {
    return localStorage.getItem("rc_ops_token");
  },
  setToken: function (token) {
    localStorage.setItem("rc_ops_token", token);
    localStorage.setItem("rc_ops_account", "professional");
  },
  clear: function () {
    localStorage.removeItem("rc_ops_token");
    localStorage.removeItem("rc_ops_account");
  },
  getAccountKind: function () {
    return localStorage.getItem("rc_ops_account");
  }
};
