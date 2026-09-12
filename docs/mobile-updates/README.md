# RaphCare mobile updates

Partner PDFs for each patient Android APK. Match the APK stem: `mobile-update-v1.9.3+69.pdf` goes with `RaphCare-v1.9.3+69.apk`.

## APK roles (keep both)

| Role | APK | Use for |
|------|-----|---------|
| Stable Connect | `RaphCare-v1.8.42+54.apk` | Daily patient Connect (phone Bluetooth / Plugin.BLE). Keep this as the safe Connect build. |
| Vendor-scan breakthrough | `RaphCare-v1.9.3+69.apk` | Engineer/partner test of Veepoo scan then Connect. Includes Nordic MCU Manager and SLF4J so Connect no longer force-closes at WAIT-CONNECT. |

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
| `mobile-update-v1.9.3+69.md` | Plain-language changelog for that APK |
| `mobile-update-v1.9.3+69.config.json` | PDF export config |
| `mobile-update-v1.9.3+69.pdf` | PDF to send with the APK |
| `mobile-update.pdf.css` | Shared styles (under `sources/`) |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.9.3+69
```

Archived note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.9.2+68 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
