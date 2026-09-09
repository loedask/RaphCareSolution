<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.32+44)</h1>
</div>

Android patient app build **1.8.32** (install code **44**). File name: `RaphCare-v1.8.32+44.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Disconnect then Connect could still close the app on some phones even after a wait. This build treats Disconnect as a soft disconnect for the watch SDK session: the app shows disconnected, but the watch link stays until you leave the app, so Connect again is safe.

## Fixed

- Disconnect no longer tears down the watch SDK session in a way that crashes on the next Connect.
- Connect on Claimed wearable reuses the kept session instead of opening a second native connection.

## Updated

- Version label **1.8.32** and install code **44**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.32+44.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Install this build.
2. Devices: Connect the claimed watch.
3. Tap Disconnect. Connected clears. The Claimed wearable row can stay in Nearby.
4. Tap Connect again. The app should stay open and show Connected (no crash).
5. Open Watch readings and try Measure.
