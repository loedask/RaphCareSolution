<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.4+70)</h1>
</div>

Android patient app build **1.9.4** (install code **70**). File name: `RaphCare-v1.9.4+70.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** After the 1.9.3 breakthrough (Connect past WAIT-CONNECT), Huawei still closed when the app stopped the watch SDK scan during password confirm. This build leaves that scan running so Connect can finish.

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk` (Plugin.BLE Connect). Use 1.9.4 when testing vendor scan and live Measure through the watch SDK.

## Fixed

- Vendor-scan Connect no longer calls stop-scan right after password start (that step was force-closing on Huawei after PWD-1).

## Updated

- Version label **1.9.4** and install code **70**.
- Share probe log and Test breadcrumb tools still available on Devices.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Install `RaphCare-v1.9.4+70.apk`.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.
5. Prefer forgetting the ET585 in phone Bluetooth settings before the first vendor scan (do not OS-pair it first).

## What to try

1. Devices → **Try vendor scan (diagnostic)** with the watch nearby and unlocked.
2. Ideal: app stays open past password; trail reaches HANDSHAKE-OK. Then Watch readings → Measure.
3. Measure needs a linked watch session. If Home still says no watch linked, finish vendor Connect first.
4. If anything fails, tap **Share probe log** and send `raphcare-vendor-probe-log.txt`.
