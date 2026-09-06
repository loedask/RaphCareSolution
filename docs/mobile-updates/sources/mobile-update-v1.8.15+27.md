<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.15+27)</h1>
</div>

Android patient app build **1.8.15** (install code **27**). File name: `RaphCare-v1.8.15+27.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Measure was failing with a busy Bluetooth error (code -2), and Home sometimes went blank until you restarted the app.

## Fixed

- Watch readings Measure waits longer after Connect, then retries a few times when the watch radio is still busy, instead of stopping on the first code -2.
- Clearer message when Measure fails for that busy-radio case (what to do next, not only a raw code).
- After Measure, the phone reconnects the normal Bluetooth link so Devices can still show Connected.
- Leaving Watch readings no longer tears down the page in a way that left Home empty (white content with the tab bar still there).

## Updated

- Version label **1.8.15** and install code **27**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.15+27.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Devices: Connect your claimed band.
2. Open Watch readings. On the watch open Heart Rate and tap to test so it is actively measuring.
3. Tap Measure now. If you see a busy-radio message, wait a few seconds and Measure again. You should get a heart rate, or a clear stop message (not an endless spinner).
4. Go to Home (and other tabs). Content should stay visible. You should not need to force-close the app to bring Home back.
