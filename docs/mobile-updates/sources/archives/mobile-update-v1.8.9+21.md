<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare mobile update (v1.8.9+21)</h1>
</div>

Android patient app build **1.8.9** (install code **21**). File name: `RaphCare-v1.8.9+21.apk`. This build talks to the staging API, not a phone on localhost.

**Why this build:** keep your watch connection when you leave Devices, clearer wrong-watch messages, and a few layout fixes on that screen.

## Fixed

- Leaving Devices and coming back no longer drops a good Bluetooth link. Scan stops; the watch stays connected when it is still in range.
- Connecting a different nearby band shows the Bluetooth address for your claimed watch, so the reject is easier to understand.
- Disconnect only shows when a watch is actually connected.
- Photo of barcode and Claim watch stack full width so the labels are not cut off.
- Nearby watch rows scroll with the page instead of sitting under the bottom tabs.
- After Connect, you should not see a red vendor Bluetooth error while status still says Connected.

## Updated

- Version label **1.8.9** and install code **21**.

## Install

1. Uninstall any older RaphCare build if Android blocks the update.
2. Copy `RaphCare-v1.8.9+21.apk` to the phone (USB, Drive, or email).
3. Allow install from that source if Android asks.
4. Sign in as the demo patient: `demo.patient@raphcare.com` (password on the demo accounts sheet).

This is for sideload testing, not the Play Store.

## What to try

1. Sign in as the demo patient. Claim your assigned watch if you have not already.
2. Scan, then Connect only to the band that matches the Bluetooth address on Watch status.
3. Leave Devices (back to Home), then open Devices again. Status should still say Connected while the band is nearby.
4. If another ET580 or ET585 shows up, try Connect. You should see a clear reject naming your claimed Bluetooth address.
