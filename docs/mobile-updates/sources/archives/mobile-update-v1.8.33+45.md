<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.33+45)</h1>
</div>

Android patient app build **1.8.33** (install code **45**). File name: `RaphCare-v1.8.33+45.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Two phone issues. Claimed wearable Connect could show “Watch SDK is unavailable” even when the SDK was in the APK. AI assistant Send could close the app. This build also reads heart-rate status from the watch (loose fit, busy, low battery) so Measure can show a clear message instead of hanging.

## Fixed

- Claimed wearable Connect loads the watch SDK through the phone classloader (stops a false “SDK unavailable” on some installs).
- AI assistant Send no longer force-closes the app when the chat list scrolls after a message.
- Measure shows a clear message when the watch reports wear error, busy, or low battery.

## Updated

- Version label **1.8.33** and install code **45**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.33+45.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Install this build (replace 1.8.32).
2. Devices: Connect the claimed watch. You should get Connected, not “Watch SDK is unavailable”.
3. Watch readings → Measure now. Keep the watch on your wrist. You should get a heart rate, or a short message (loose, busy, charge).
4. Home → AI assistant → type a short question → Send. The app must stay open.
