<div class="doc-header">
<img src="../../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.53+65)</h1>
</div>

Android patient app build **1.8.53** (install code **65**). File name: `RaphCare-v1.8.53+65.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** 1.8.52 kept the scan running during Connect and still stopped at WAIT-CONNECT. This build tries the other watch-SDK Connect overload (mac only) for one more test.

## Updated

- Connect uses the mac-only overload (probe log shows **CONNECT-MACONLY**).
- Still keeps scan warm through Connect (**SCAN-KEEP**).
- Version label **1.8.53** and install code **65**.

## Before you try

1. Forget the ET585 in Android Bluetooth if listed, then toggle Bluetooth off and on.
2. Huawei App launch: RaphCare unrestricted / not auto-manage.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.53+65.apk` to the phone.
3. Force-stop the separate **H Band** app if it is installed.
4. Sign in as the demo patient.

## What to try

1. Devices â†’ **Try vendor scan (diagnostic)**.
2. Share the probe log. Look for **CONNECT-MACONLY** then either past WAIT-CONNECT or still WAIT-CONNECT.
