# Wearable hardware prove-out (E580 and E585)

Desk test for pairing an **E580** or **E585** class band to the Android patient app. Use this after installing a sideload build that includes the vendor Bluetooth libraries (see `docs/12_HBand_SDK_Integration.md`).

**Not this test:** the **Y6 Pro** 4G emergency watch. That SKU does not pair in Devices and wearables.

Last updated: 2026-09-03

---

## What you need

- Android phone with Bluetooth
- Charged E580 or E585 (or ET580 / ET585) band, awake, within about 2 meters
- Sideload APK **1.5.2+10** (`RaphCare-v1.5.2+10.apk`) or later, pointed at Staging
- Matching partner note PDF: `docs/mobile-updates/mobile-update-v1.5.2+10.pdf` (edit under `sources/`)
- Demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet)

Optional: serial number from the box, if you will register the device and sync readings.

---

## Pass or fail

Record each step as pass, fail, or skip. Failures should include the on-screen error and whether the band still appears in the phone's system Bluetooth list.

| # | Step | Pass |
|---|------|------|
| 1 | Uninstall any older RaphCare, then install this APK | |
| 2 | Sign in as the demo patient. Home loads | |
| 3 | Open **Devices & wearables** | |
| 4 | Phone Bluetooth is on. Grant Nearby devices or Bluetooth when asked | |
| 5 | Tap **Scan**. Wait up to 30 seconds | |
| 6 | The band appears in the filtered list (or after switching to show all BLE) | |
| 7 | Tap **Connect**. Status shows connected (no crash) | |
| 8 | A heart rate value appears (wait up to a minute, keep the band on a wrist or finger) | |
| 9 | A blood oxygen value appears, or a clear SpOâ‚‚ start message if the band skips it | |
| 10 | Tap **Disconnect**. The session ends | |
| 11 | Connect again. Readings still update | |
| 12 | (Optional) Enter serial, pick E580 or E585, tap **Register**, then **Sync readings** | |

**Build is ready for live vitals** only when steps 1 through 11 pass on at least one real ET580 or ET585.

---

## If scan finds nothing

1. Wake the band (tap the screen or raise to wake). Charge it if it was dead.
2. Toggle phone Bluetooth off and on.
3. On the Devices screen, switch to show all BLE devices and look by name or signal strength.
4. Confirm Nearby devices permission is Allowed (Android 12 and newer).
5. If the vendor companion app already owns the band, complete any first-time bind there, then try RaphCare again. Forget the device in system Bluetooth settings if Connect keeps failing.

---

## If Connect works but there is no heart rate

1. Keep the band on skin. Many units need a pulse to start sending.
2. Wait a full minute. Vendor live detect is not instant.
3. Note any message that starts with "HBand connect failed" (app then tries generic Bluetooth). Write that down.
4. If you only see a raw hex line, the vendor handshake did not run or the band is not in the HBand family. Rebuild after `tools/download-hband-android-libs.ps1` so the vendor libraries are inside the APK.

---

## After a successful run

1. Mark the engineering checklist row **Live HR + SpOâ‚‚ in patient app via vendor protocol verified on hardware**.
2. Mirror the partner checklist row **Live heart rate and blood oxygen in the app** to Done.
3. Recalculate % and regenerate the two feature checklist PDFs.

Until that run exists, those rows stay Partial.

---

## Related

| Doc | Use for |
|-----|---------|
| [`Web_And_Mobile_Smoke_Plan.md`](Web_And_Mobile_Smoke_Plan.md) | Default smoke (Devices screen open only). This file is the hardware gate. |
| [`../../11_Devices_BLE_E580_E585.md`](../../11_Devices_BLE_E580_E585.md) | BLE behavior and permissions |
| [`../../12_HBand_SDK_Integration.md`](../../12_HBand_SDK_Integration.md) | Vendor libraries and Android rebuild |
| [`../../mobile-updates/`](../../mobile-updates/) | Sideload APK notes (PDF at folder root; edit under `sources/`) |
