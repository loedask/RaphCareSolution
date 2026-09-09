<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.36+48)</h1>
</div>

Android patient app build **1.8.36** (install code **48**). File name: `RaphCare-v1.8.36+48.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** Opening Devices asked for Nearby devices permission and then immediately tried to connect the watch. On some phones that closed the app as soon as you tapped Allow. This build only asks for permission when you tap Scan or Connect, then waits briefly before connecting.

## Fixed

- Accepting Nearby devices permission no longer force-closes the app on Devices open.
- Permission is requested only from Scan or Connect, not from page open.
- After you allow permission, the app waits a short moment before Bluetooth connect work.

## Updated

- Version label **1.8.36** and install code **48**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.36+48.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Install this build.
2. Open Devices. The app should stay open (no permission dialog yet, or only a hint to tap Scan).
3. Tap Scan. Allow Nearby devices when asked. The app must stay open.
4. Wait for Connecting / Connected, then try Measure.
