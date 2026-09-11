# RaphCare mobile updates

Partner PDFs for each patient Android APK. Match the APK stem: `mobile-update-v1.8.48+60.pdf` goes with `RaphCare-v1.8.48+60.apk`.

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
| `mobile-update-v1.8.48+60.md` | Plain-language changelog for that APK |
| `mobile-update-v1.8.48+60.config.json` | PDF export config |
| `mobile-update-v1.8.48+60.pdf` | PDF to send with the APK |
| `mobile-update.pdf.css` | Shared styles (under `sources/`) |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.8.48+60
```

Archived note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.8.47+59 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
