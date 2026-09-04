<div class="doc-header">
<img src="../../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.6.0)</h1>
</div>

Android patient app build **1.6.0** (install code **10**). File name: `RaphCare-v1.6.0+10.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** the chat assistant can now give a real reply from the hospital's cloud AI, and the Android crash fixes from 1.5.1 are included.

## Added

- **Chat assistant:** send a short wellness question and you should get a real reply, plus the medical disclaimer. This is not a doctor. For urgent symptoms, contact a clinician or emergency services.

## Fixed

- **Book appointment** no longer closes the app when you open it from Home or Appointments (from 1.5.1).
- Safer loading on many screens. If something fails to load, the app should stay open and show an error where it can, instead of exiting.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.6.0+10.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient.
2. Open the chat assistant and send a short wellness question. You should get a real reply, not the old placeholder.
3. From Home, tap **Book appointment**. The form should open and stay open.
4. Open Health records, Devices, and Profile. Confirm those screens still open without the app closing.

## Related

Wider product status (web admin, Hospital plan, checklists) stays in the dated partner updates under the partner-updates folder. This note is only what changed in the phone APK.
