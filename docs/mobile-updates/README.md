# RaphCare mobile updates

Partner PDFs for each patient Android APK. Match the APK stem: `mobile-update-v1.5.2+10.pdf` goes with `RaphCare-v1.5.2+10.apk`.

| Path | What is here |
|------|----------------|
| `mobile-update-v*.pdf` | Current build PDF (send this with the APK) |
| [`archives/`](archives/) | Older PDFs |
| [`sources/`](sources/) | Markdown, PDF config, CSS (edit here; do not send to partners) |
| [`brand/`](brand/) | Logo used in the PDF header |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.2+10
```

Archived note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.1+9 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
