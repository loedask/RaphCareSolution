<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.3+69)</h1>
</div>

Android patient app build **1.9.3** (install code **69**). File name: `RaphCare-v1.9.3+69.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Vendor-scan Connect breakthrough. Earlier builds closed right after the watch started linking because Nordic libraries the watch SDK needs were missing from the app. This build includes those libraries. On a Galaxy A32 the app stayed open through Connect notify and password start.

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk` (Plugin.BLE Connect). Use 1.9.3 when testing vendor scan and live Measure through the watch SDK.

## Fixed

- Bundled Nordic MCU Manager libraries and the SLF4J logging jars they need, so Connect no longer force-closes at WAIT-CONNECT.

## Updated

- Version label **1.9.3** and install code **69**.
- Share probe log and Test breadcrumb tools still available on Devices.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Install `RaphCare-v1.9.3+69.apk`.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.
5. Prefer forgetting the ET585 in phone Bluetooth settings before the first vendor scan (do not OS-pair it first).

## What to try

1. Devices → **Try vendor scan (diagnostic)** with the watch nearby and unlocked.
2. Ideal: app stays open; Connect moves past WAIT-CONNECT. Then Watch readings → Measure.
3. If anything fails, tap **Share probe log** and send `raphcare-vendor-probe-log.txt`.
