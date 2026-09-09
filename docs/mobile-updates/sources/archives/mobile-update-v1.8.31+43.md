<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.31+43)</h1>
</div>

Android patient app build **1.8.31** (install code **43**). File name: `RaphCare-v1.8.31+43.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** On 1.8.30, Disconnect left the Claimed wearable row (expected). Tapping Connect right away could close the app. This build waits for the watch radio to finish disconnecting before it reconnects.

## Fixed

- Disconnect then Connect on Claimed wearable no longer force-closes the app.
- Reconnect uses a real watch name instead of an empty one after Disconnect.

## Updated

- Version label **1.8.31** and install code **43**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.31+43.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Install this build.
2. Devices: Connect the claimed watch (or wait for auto-connect).
3. Tap Disconnect. The Claimed wearable row can stay in Nearby. That is normal.
4. Tap Connect again. Wait a few seconds if the button stays busy. The app should stay open and show Connected.
5. Open Watch readings and try Measure.
