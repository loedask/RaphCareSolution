<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.37+49)</h1>
</div>

Android patient app build **1.8.37** (install code **49**). File name: `RaphCare-v1.8.37+49.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Manual Scan was tearing down the watch radio and then connecting again on its own. On some phones that disconnect-then-connect sequence closed the app. This build keeps the watch session up during Scan, fills Nearby, and waits for you to tap Connect when needed.

## Fixed

- Scan no longer drops the watch radio and reconnects by itself.
- Scan keeps an existing watch session alive and only frees a plain Bluetooth link when that is what was holding the radio.
- After Scan finishes, Nearby shows your saved claimed watch when it is known. Tap Connect only if you need to pair again.

## Updated

- Version label **1.8.37** and install code **49**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.37+49.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Watch nearby, unlocked, on your wrist.
2. Open Devices. If you were already Connected, stay Connected after Scan.
3. Tap Scan. Allow Nearby devices if asked. The app must stay open.
4. When Scan ends, check Nearby for your claimed watch row. Tap Connect only if you are not already Connected.
5. Watch readings → Measure.
