<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.7+73)</h1>
</div>

Android patient app build **1.9.7** (install code **73**). File name: `RaphCare-v1.9.7+73.apk`. Staging API.

**Why this build:** Connect handshake is working. Measure now listens a few more seconds after the first good heart sample and keeps the latest reading, so the phone is less likely to lock an early number. Blood oxygen is still off on purpose (turning it on while heart measure runs used to crash the app).

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk`.

## Fixed

- Measure holds briefly and uses the latest settled heart rate, not only the first accepted sample.

## Updated

- Version label **1.9.7** and install code **73**.

## Install

1. Install `RaphCare-v1.9.7+73.apk`.
2. Force-stop the separate **H Band** app if it is installed.

## What to try

1. Vendor scan Connect until it succeeds.
2. Watch readings → wear snug → **Measure now** in the app. Do not run the watch’s own Heart Rate “Tap to Test” at the same time.
3. Expect Measure to take about 8 seconds after the first beep/sample before the number settles.
4. Blood oxygen may still show a dash.
