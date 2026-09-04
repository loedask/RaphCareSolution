<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.7.0+11)</h1>
</div>

Android patient app build **1.7.0** (install code **11**). File name: `RaphCare-v1.7.0+11.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** release cut for **1.7.0**. It ships with the current Ops and Portal staging stack. Watch (HBand) libraries stay in the package so Connect can still use the real band handshake.

## Updated

- Version label **1.7.0** and install code **11** so this APK matches the release cut.
- Still includes the vendor watch libraries used by E580 and E585 style bands (same as the 1.5.2 line).

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.7.0+11.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient.
2. From Home, open **Book appointment**. The form should open and stay open.
3. Open the chat assistant and send a short wellness question. You should still get a real reply when staging AI is on.
4. Open **Devices & wearables**. Bluetooth scan and Connect should still be available for supported bands.
5. Open Health records, Profile, and related screens. Confirm they open without the app closing.

## Related

Wider product status (Ops, Portal, cloud AI, checklists) stays in the dated partner updates under the partner-updates folder. This note is only what changed in the phone APK.
