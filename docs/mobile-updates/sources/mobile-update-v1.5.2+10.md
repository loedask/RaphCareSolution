<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.5.2+10)</h1>
</div>

Android patient app build **1.5.2** (install code **10**). File name: `RaphCare-v1.5.2+10.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** watches. This Android package includes the vendor Bluetooth libraries used by E580 and E585 style bands, so Connect can run the real handshake instead of generic Bluetooth only.

## Added

- Vendor watch libraries inside the Android app (needed for live heart rate and blood oxygen on those bands).
- A desk test list for pairing a real band (builders keep that next to the smoke plan).

## Updated

- The download of those libraries follows the current vendor filenames, so a missing old file no longer blocks a rebuild.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.5.2+10.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient. Open **Devices & wearables** from Home.
2. Turn phone Bluetooth on. Allow Nearby devices if Android asks.
3. Charge and wake the E580 or E585 band. Keep it close. Tap **Scan**.
4. If the list is empty, switch to show all Bluetooth devices and look for the band by name.
5. Tap **Connect**. Confirm the app stays open and shows connected.
6. Wait for a heart rate (and blood oxygen if the band supports the live reading). Wear the band on a wrist.
7. Tap **Disconnect**, then connect again.
8. Optional: enter the serial from the box, pick E580 or E585, tap Register, then Sync readings.

The Y6 Pro 4G emergency watch does not pair on this screen. It uses the cellular emergency path, not phone Bluetooth.

## Related

Wider product status (web admin, Hospital plan, checklists) stays in the dated partner updates under the partner-updates folder. This note is only what changed in the phone APK.
