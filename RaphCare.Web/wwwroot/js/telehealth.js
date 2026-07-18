window.raphCareTelehealth = (function () {
  let client = null;
  let localTrack = null;
  let remoteUsers = {};

  async function ensureClient() {
    if (client) return client;
    if (typeof AgoraRTC === "undefined") {
      throw new Error("Agora Web SDK failed to load.");
    }
    client = AgoraRTC.createClient({ mode: "rtc", codec: "vp8" });
    client.on("user-published", async (user, mediaType) => {
      await client.subscribe(user, mediaType);
      if (mediaType === "video") {
        remoteUsers[user.uid] = user;
        const player = document.getElementById("tele-remote-player");
        if (player) {
          player.innerHTML = "";
          user.videoTrack.play(player);
        }
      }
      if (mediaType === "audio") {
        user.audioTrack.play();
      }
    });
    client.on("user-unpublished", (user) => {
      delete remoteUsers[user.uid];
      const player = document.getElementById("tele-remote-player");
      if (player) player.innerHTML = "";
    });
    return client;
  }

  return {
    join: async function (appId, channel, token, uid) {
      const c = await ensureClient();
      await c.join(appId, channel, token || null, uid);
      localTrack = await AgoraRTC.createMicrophoneAndCameraTracks();
      const localPlayer = document.getElementById("tele-local-player");
      if (localPlayer) {
        localPlayer.innerHTML = "";
        localTrack[1].play(localPlayer);
      }
      await c.publish(localTrack);
    },
    leave: async function () {
      if (localTrack) {
        localTrack.forEach((t) => {
          t.stop();
          t.close();
        });
        localTrack = null;
      }
      remoteUsers = {};
      const localPlayer = document.getElementById("tele-local-player");
      const remotePlayer = document.getElementById("tele-remote-player");
      if (localPlayer) localPlayer.innerHTML = "";
      if (remotePlayer) remotePlayer.innerHTML = "";
      if (client) {
        await client.leave();
        client = null;
      }
    }
  };
})();
