<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.13+25)</h1>
</div>

Android patient app build **1.8.13** (install code **25**). File name: `RaphCare-v1.8.13+25.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** a Watch readings screen with Measure now, so heart rate and oxygen can reach the phone from E580/E585 bands.

## Added

- **Watch readings** page: heart rate and oxygen cards, Measure now, Sync to clinic, and a Coming later note for blood pressure and other monitors.

## Fixed

- Turning Health Monitor on on the watch alone does not send numbers to the phone. After Connect on Devices, open Watch readings and tap Measure now (open Heart Rate on the watch and tap to test if the phone waits).

## Updated

- Devices points to Watch readings instead of showing a thin Last reading strip.
- Version label **1.8.13** and install code **25**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.13+25.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Devices: Scan and Connect your claimed band until the row says Connected.
2. Tap Open watch readings.
3. On the watch, open Heart Rate (tap to test if needed).
4. Tap Measure now. Heart rate (and oxygen when available) should fill in.
5. Tap Sync to clinic when you have a reading.
