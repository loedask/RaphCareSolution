<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.40+52)</h1>
</div>

Android patient app build **1.8.40** (install code **52**). File name: `RaphCare-v1.8.40+52.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Turning watch-SDK Connect back on (even with the documented address-only call) still closed the app after Scan. This build returns to the proven phone Bluetooth Connect path from 1.8.38.

## Fixed

- Connect uses phone Bluetooth again so the app stays open after Scan.
- Watch-SDK Connect and live Measure stay off until a safe native path exists.

## Updated

- Version label **1.8.40** and install code **52**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.40+52.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Watch nearby, unlocked, on your wrist.
2. Devices → Scan until your ET585 appears.
3. Tap Connect. The app must stay open and reach Connected.
4. Do not expect Measure through the watch SDK in this build.
