<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.10+22)</h1>
</div>

Android patient app build **1.8.10** (install code **22**). File name: `RaphCare-v1.8.10+22.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Stop scan on Devices actually ends the Bluetooth search instead of leaving the spinner running.

## Fixed

- Stop scan works while devices are still appearing. Watch status moves to Scan stopped, and the spinner clears.
- Leaving Devices also cancels an in-progress scan (the watch link still stays up when you were already connected).

## Updated

- Version label **1.8.10** and install code **22**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.10+22.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Open Devices and wearables. Tap Scan and wait until nearby bands show.
2. Tap Stop scan. Status should leave "Scanning for wearables..." and the spinner should go away. Nearby rows can stay on screen.
3. Tap Scan again, then leave the page. Coming back should not stay stuck on Scanning.
