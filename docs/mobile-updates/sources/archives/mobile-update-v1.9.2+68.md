<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.9.2+68)</h1>
</div>

Android patient app build **1.9.2** (install code **68**). File name: `RaphCare-v1.9.2+68.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** 1.9.1 still closed at WAIT-CONNECT. The Nordic library was listed in the project but its Java classes never made it into the install file. This build packs those library files the same way as the watch SDK itself.

## Fixed

- Nordic MCU Manager classes are now included in the APK so Connect can finish after the watch links.

## Updated

- Version label **1.9.2** and install code **68**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.9.2+68.apk` to the phone (or use the USB install from your engineer).
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Devices → **Try vendor scan (diagnostic)** with the watch nearby.
2. Ideal: Connect completes without a red WAIT-CONNECT, then Watch readings → Measure.
3. If it still closes, Share the probe log again.
