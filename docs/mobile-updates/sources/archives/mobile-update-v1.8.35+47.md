<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.35+47)</h1>
</div>

Android patient app build **1.8.35** (install code **47**). File name: `RaphCare-v1.8.35+47.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** After Scan, the page could sit on “Scan finished” with Claimed wearable and nothing else. Manual Scan drops the Bluetooth link first, so Connect is required next. This build connects automatically after Scan, shows Connecting, and uses a busy overlay so the wait is visible.

## Fixed

- After Scan finishes with your claimed watch listed, the app starts Connect for you.
- Connecting shows a clear “Connecting to your watch…” status and busy overlay.

## Updated

- Version label **1.8.35** and install code **47**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.35+47.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Watch nearby, unlocked, on your wrist.
2. Devices → Scan.
3. When Scan ends you should see Connecting (not a dead “Scan finished” screen).
4. Wait for Connected, then Watch readings → Measure.
