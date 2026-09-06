<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (4 Sep 2026, evening)</h1>
</div>

Short follow-up to the earlier 4 Sep note. This drop is the **1.7.0** release cut for the patient Android app. Ops, Portal, cloud AI, and photo storage notes from 3 Sep and earlier today still apply. You do not need to re-read those for this APK.

## 1. Patient Android app (1.7.0)

Sideload file name: `RaphCare-v1.7.0+11.apk`. It talks to the staging API, not localhost. Copy it to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

The package still includes the watch libraries used for E580 and E585 style bands. Chat assistant and the recent crash fixes stay in this build.

One-page phone note: [`mobile-update-v1.7.0+11.pdf`](../mobile-updates/mobile-update-v1.7.0+11.pdf).

## 2. Already covered this week (no change in this note)

- **3 Sep:** Ops site live, Portal hospital-first shell, demo hospital list fix.
- **4 Sep (earlier):** cloud AI on staging, profile photo storage, feature catalog.

## 3. Links

- Portal: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- Ops: [https://raphcare-ops.azurewebsites.net](https://raphcare-ops.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)
- Public site: [raphcare.com](https://raphcare.com)

## Try this

1. Install `RaphCare-v1.7.0+11.apk` and sign in as `demo.patient@raphcare.com`.
2. Open Book appointment and the chat assistant. Both should work on staging.
3. If you have a test band, open Devices and try Scan / Connect.
