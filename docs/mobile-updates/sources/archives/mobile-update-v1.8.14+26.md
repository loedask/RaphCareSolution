<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.14+26)</h1>
</div>

Android patient app build **1.8.14** (install code **26**). File name: `RaphCare-v1.8.14+26.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Measure now no longer spins forever when the watch handshake stalls.

## Fixed

- Watch readings Measure stops within about a minute if no heart rate arrives, and shows a clear retry message.
- Measure waits briefly after Connect before talking to the watch SDK, and reads heart rate before oxygen (the vendor stack does not like both at once).

## Updated

- Version label **1.8.14** and install code **26**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.14+26.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Devices: Connect your claimed band.
2. Open Watch readings. On the watch open Heart Rate and tap to test so it is actively measuring.
3. Tap Measure now. Within about a minute you should either see a heart rate, or a stop message (not an endless spinner).
4. If it stops with no number, keep the watch on your wrist, tap to test again, then Measure once more.
