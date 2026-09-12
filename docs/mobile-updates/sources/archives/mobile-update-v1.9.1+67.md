<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.1+67)</h1>
</div>

Android patient app build **1.9.1** (install code **67**). File name: `RaphCare-v1.9.1+67.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** On a Galaxy A32 we finally captured the crash after Connect starts. The watch SDK needs a Nordic update library that was missing from the app. Without it, the app closed at WAIT-CONNECT on both Huawei and Samsung.

## Fixed

- Bundled the required Nordic MCU Manager libraries so Connect can finish after the watch links.

## Updated

- Version label **1.9.1** and install code **67**.
- Same Share probe log tools as before.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.9.1+67.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Devices → **Try vendor scan (diagnostic)** with the watch nearby.
2. Ideal: Connect completes (no red WAIT-CONNECT). Then Watch readings → Measure.
3. If it still closes, Share the probe log again.
