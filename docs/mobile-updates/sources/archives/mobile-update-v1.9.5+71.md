<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.5+71)</h1>
</div>

Android patient app build **1.9.5** (install code **71**). File name: `RaphCare-v1.9.5+71.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** On Huawei, vendor scan Connect stayed open through password start (1.9.4), then failed with “confirmDevicePwd overload not found.” The watch SDK method uses a primitive boolean; our call used a boxed Boolean and reflection never matched. This build fixes that match.

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk`.

## Fixed

- Password confirm after vendor Connect now finds the watch SDK method (boolean argument match).

## Updated

- Version label **1.9.5** and install code **71**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Install `RaphCare-v1.9.5+71.apk`.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.
5. Prefer forgetting the ET585 in phone Bluetooth settings before the first vendor scan.

## What to try

1. Devices → **Try vendor scan (diagnostic)** with the watch nearby and unlocked.
2. Ideal: trail reaches PERSON-1 / HANDSHAKE-OK without “confirmDevicePwd overload not found.”
3. Then Watch readings → Measure.
4. If anything fails, tap **Share probe log** and send `raphcare-vendor-probe-log.txt`.
