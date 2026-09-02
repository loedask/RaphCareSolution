<div class="doc-header">
<img src="../partner-updates/brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.5.1)</h1>
</div>

Android patient app build **1.5.1** (install code **9**). File name: `RaphCare-v1.5.1+9.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** stability. Opening Book appointment (and several other screens) could close the app on Android. That is fixed, and the same hardening was applied across the patient app.

## Fixed

- **Book appointment** no longer closes the app when you open it from Home or Appointments.
- Safer loading on many screens (appointments, records, insurance, billing, devices, profile, telehealth, and related pages). If something fails to load, the app should stay open and show an error where it can, instead of exiting.
- Clinic, insurance plan, and billing plan pickers are more reliable when the list refreshes.
- Profile and medical save screens navigate back more safely after a successful save.

## Updated

- Internal guards so list refreshes and button enable/disable run on the UI thread (Android was sensitive to this).
- Picker labels use clearer item binding so provider and clinic names show correctly.

## Added

- Nothing new for patients in this build. No new menus or flows. This is a fix release on top of **1.5.0**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.5.1+9.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient.
2. From Home, tap **Book appointment**. The form should open and stay open.
3. From Appointments, tap Book again. Pick a provider if your clinic has one, or choose clinic under Profile if prompted.
4. Open Health records, Devices, and Profile. Confirm those screens still open without the app closing.

## Related

Wider product status (web admin, Hospital plan, checklists) stays in the dated partner updates under the partner-updates folder. This note is only what changed in the phone APK.
