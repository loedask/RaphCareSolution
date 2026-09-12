# RaphCare mobile updates

Partner PDFs for each patient Android APK. Match the APK stem: `mobile-update-v1.9.8+74.pdf` goes with `RaphCare-v1.9.8+74.apk`.

## APK roles (keep both)

| Role | APK | Use for |
|------|-----|---------|
| Stable Connect | `RaphCare-v1.8.42+54.apk` | Daily patient Connect (phone Bluetooth / Plugin.BLE). |
| Measure-ready lock | `RaphCare-v1.9.8+74.apk` | Vendor Connect and Measure. Normal Scan → Connect → Measure handoff works on Huawei. Keep this APK. |

Do not treat older **1.9.0** notes as a “preview” checkpoint. That label is retired. Archived PDFs under `archives/` are history only.

| Path | What is here |
|------|----------------|
| `mobile-update-v*.pdf` | Current build PDF (send this with the APK) |
| [`archives/`](archives/) | Older PDFs |
| [`sources/`](sources/) | Markdown, PDF config, CSS (edit here; do not send to partners) |
| [`brand/`](brand/) | Logo used in the PDF header |

Every time `RaphCare.Mobile` gets a new `ApplicationDisplayVersion` or Android `ApplicationVersion` (versionCode), or you publish a sideload APK for the partner.

## Naming

| File | Purpose |
|------|---------|
| `mobile-update-v1.9.8+74.md` | Plain-language changelog for that APK |
| `mobile-update-v1.9.8+74.config.json` | PDF export config |
| `mobile-update-v1.9.8+74.pdf` | PDF to send with the APK |
| `mobile-update.pdf.css` | Shared styles (under `sources/`) |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.9.8+74
```

Archived note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.9.7+73 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
