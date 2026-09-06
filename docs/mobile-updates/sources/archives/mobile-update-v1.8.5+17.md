<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.5+17)</h1>
</div>

Android patient app build **1.8.5** (install code **17**). File name: `RaphCare-v1.8.5+17.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** safer watch pairing after claim, and Profile rows that match what the app can actually do.

## Fixed

- Profile no longer shows a fake insurance plan name. It uses your plan when one is on file, or says none is on file.
- Payment methods and billing history open different screens (add a card vs plans and invoices).
- Edit Profile can change your photo the same way the Profile camera button does.
- Privacy delete and download no longer pretend the app finished those jobs. They point you to support or your clinic.

## Added

- After you claim an assigned watch, Connect locks to that unit's Bluetooth identity on first successful pair. A different watch is rejected once locked.

## Updated

- Claim the watch before Scan or Connect on Devices and wearables.
- Version label **1.8.5** and install code **17**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.5+17.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient.
2. Open Profile: check the insurance line, tap Payment methods, then Billing history, then Edit Profile and change the photo.
3. Open Privacy: confirm delete offers Help and support instead of a fake sign-out deletion.
4. Open Devices and wearables: claim the assigned serial if needed, then Scan and Connect. Confirm a wrong second watch is rejected after the first pair locks.
