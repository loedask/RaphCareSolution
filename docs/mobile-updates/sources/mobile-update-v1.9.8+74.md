<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.8+74)</h1>
</div>

Android patient app build **1.9.8** (install code **74**). File name: `RaphCare-v1.9.8+74.apk`. Staging API.

**Why this build:** Normal Scan → Connect left the phone on Bluetooth only, so Measure said to disconnect and connect again. This build lets Measure drop that Bluetooth link and finish the watch SDK handshake, then read heart rate.

**Keep for daily Connect:** `RaphCare-v1.8.42+54.apk`.

## Fixed

- After normal Scan and Connect, Measure can hand off to the watch SDK instead of stopping with the Bluetooth-only error.

## Updated

- Version label **1.9.8** and install code **74**.

## Install

1. Install `RaphCare-v1.9.8+74.apk`.
2. Force-stop the separate **H Band** app if it is installed.

## What to try

1. Devices → **Scan** → **Connect** on your claimed watch (normal path).
2. Watch readings → wear snug → **Measure now**. Expect a short wait while the watch SDK links, then a heart rate.
3. Or keep using **Try vendor scan (diagnostic)** if you prefer that path.
4. Blood oxygen may still show a dash.
