<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.12+24)</h1>
</div>

Android patient app build **1.8.12** (install code **24**). File name: `RaphCare-v1.8.12+24.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** stop the brief “This feature is not supported” toast on Connect.

## Fixed

- Connect uses standard Bluetooth only for now. The vendor watch SDK was probing a phone feature many devices do not support, which flashed that Android message even when Connect still succeeded.

## Updated

- Version label **1.8.12** and install code **24**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.12+24.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Scan and Connect your claimed band.
2. Confirm Connected shows on Watch status and on the nearby row.
3. Confirm the short “This feature is not supported” toast no longer appears.
4. Last reading may still wait for heart rate or oxygen until the band sends those readings over standard Bluetooth.
