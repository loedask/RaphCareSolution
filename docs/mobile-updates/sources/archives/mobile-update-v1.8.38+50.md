<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.38+50)</h1>
</div>

Android patient app build **1.8.38** (install code **50**). File name: `RaphCare-v1.8.38+50.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** After Scan found your watch, tapping Connect could close the app. That path used the watch vendor SDK. This diagnostic build connects with phone Bluetooth instead, so Connect can stay open while we confirm the vendor call.

## Fixed

- Connect no longer goes through the watch vendor SDK connect call that was closing the app.
- Connect uses the phone Bluetooth path after Scan lists your watch.

## Updated

- Version label **1.8.38** and install code **50**.
- Live Measure through the watch SDK is off in this build (expected for this diagnostic).

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.38+50.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Watch nearby, unlocked, on your wrist.
2. Devices → Scan. Confirm your ET585 (or claimed watch) appears.
3. Tap Connect. The app must stay open and reach Connected.
4. Do not expect Measure to work through the watch SDK in this build.
