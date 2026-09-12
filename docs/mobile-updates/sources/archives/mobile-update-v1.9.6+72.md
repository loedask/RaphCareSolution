<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.6+72)</h1>
</div>

Android patient app build **1.9.6** (install code **72**). File name: `RaphCare-v1.9.6+72.apk`. Staging API.

**Why this build:** Vendor Connect handshake now works (HANDSHAKE-OK on Huawei). Measure was taking an early “still detecting” heart sample, so the phone could show a lower number than the watch face. Watch readings also showed “Heart rate” twice.

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk`.

## Fixed

- Measure waits for a settled heart reading (not the interim detecting sample).
- Watch readings shows the bpm number once under the Heart rate label.

## Updated

- Version label **1.9.6** and install code **72**.

## Install

1. Install `RaphCare-v1.9.6+72.apk` (uninstall older RaphCare only if Android blocks the update).
2. Force-stop the separate **H Band** app if it is installed.
3. Sign in as the demo patient.

## What to try

1. Devices → **Try vendor scan (diagnostic)** until Connect succeeds.
2. Watch readings → wear the band snug → **Measure now** (do not rely on the watch’s own Heart Rate screen alone).
3. Expect a bpm close to what the watch shows after it finishes measuring.
4. If wrong, **Share probe log** from Devices.
