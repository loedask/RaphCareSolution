<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.11+23)</h1>
</div>

Android patient app build **1.8.11** (install code **23**). File name: `RaphCare-v1.8.11+23.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** clearer Connected state on Devices, quieter Bluetooth after Connect, and no more odd raw codes in Last reading.

## Fixed

- After Connect, the linked nearby row shows Connected instead of Connect.
- Android no longer pops “This feature is not supported” from unused Bluetooth notify channels on these bands.
- Last reading no longer shows raw codes like `01130000`. When connected without heart rate or oxygen yet, you see a short waiting message instead.

## Updated

- Version label **1.8.11** and install code **23**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.11+23.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Claim and Connect your assigned band.
2. Confirm Watch status says Connected, the matching nearby row says Connected, and no system toast about an unsupported feature.
3. Check Last reading. You should either see heart rate or oxygen, or the waiting line, not a raw hex code.
