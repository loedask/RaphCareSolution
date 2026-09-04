# RaphCare mobile updates

Partner PDFs for each patient Android APK. Match the APK stem: `mobile-update-v1.6.0+10.pdf` goes with `RaphCare-v1.6.0+10.apk`.

| Path | What is here |
|------|----------------|
| `mobile-update-v*.pdf` | Current build PDF (send this with the APK) |
| [`archives/`](archives/) | Older PDFs |
| [`sources/`](sources/) | Markdown, PDF config, CSS (edit here; do not send to partners) |
| [`brand/`](brand/) | Logo used in the PDF header |

Every time `RaphCare.Mobile` gets a new `ApplicationDisplayVersion` and/or Android `ApplicationVersion` (versionCode), or you publish a sideload APK for the partner.

## Naming

| File | Purpose |
|------|---------|
| `mobile-update-v1.6.0.md` (or `v1.6.0+10` when both parts change) | Plain-language changelog for that APK |
| `mobile-update-v1.6.0.config.json` | PDF export config |
| `mobile-update-v1.6.0.pdf` | PDF to send with the APK |
| `mobile-update.pdf.css` | Shared styles (under `sources/`) |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.6.0
```

Archived note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.1+9 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
