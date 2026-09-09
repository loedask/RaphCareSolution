<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.34+46)</h1>
</div>

Android patient app build **1.8.34** (install code **46**). File name: `RaphCare-v1.8.34+46.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** On 1.8.33, Devices could sit on “Reconnecting…” then show “Watch SDK connect failed” / connect timed out for the claimed watch (MAC known). Auto-reconnect was fighting Connect and leaving the watch radio hung. This build clears a stuck link, retries once, cools down auto-reconnect after a failure, and tells you to force-stop the separate H Band app when needed.

## Fixed

- Connect adopts an already-open watch SDK link instead of calling connect again (that path often never finishes).
- After a connect timeout, the radio is cleared and Connect retries once.
- Auto-reconnect stops looping for two minutes after a failure so you can tap Connect yourself.
- Error text no longer says “try Measure” when Connect timed out.

## Updated

- Version label **1.8.34** and install code **46**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.34+46.apk` to the phone.
3. On the phone: Settings → Apps → **H Band** (if present) → Force stop.
4. Sign in as the demo patient.

## What to try

1. Put the watch near the phone, unlocked, on your wrist.
2. Force-stop **H Band** if that app is installed.
3. Open Devices. If reconnect fails, wait for the failed hint, then tap **Connect** on Claimed wearable.
4. You should see Connected, not a Measure timeout message.
5. Then Watch readings → Measure.
