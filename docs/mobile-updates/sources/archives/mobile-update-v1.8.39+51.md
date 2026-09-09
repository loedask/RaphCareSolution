<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.39+51)</h1>
</div>

Android patient app build **1.8.39** (install code **51**). File name: `RaphCare-v1.8.39+51.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Connect through the watch SDK is back on, using the documented Bluetooth-address connect call from the bundled libraries (not the name-based overload that closed the app on some phones).

## Fixed

- Watch SDK Connect uses the official mac-only connect method.
- Exclusive Connect and live Measure through the watch SDK are enabled again for prove-out.

## Updated

- Version label **1.8.39** and install code **51**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.39+51.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Watch nearby, unlocked, on your wrist.
2. Devices → Scan until your ET585 appears.
3. Tap Connect. The app must stay open and reach Connected.
4. Watch readings → Measure. Confirm a heart rate appears.
5. If Connect still closes the app, capture logcat for lines `CONNECT-1`, `CONNECT-2`, `CONNECT-INVOKE`, and `CONNECT-3`.
