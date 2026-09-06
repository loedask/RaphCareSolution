# Archived partner update sources

Markdown and PDF config for older dated partner notes and prior catalog / price list / demo-accounts versions. Matching PDFs live in `docs/partner-updates/archives/`.

Stylesheet path: `../partner-update.pdf.css` for dated notes (or the archived stem’s own `.pdf.css`). Logo in markdown: `../../brand/raphcare-logo.png`. Export copies it into this folder as `brand/` for Chromium.

## Re-export

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-08-29 -Archive
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-demo-accounts-v1 -Archive
```
