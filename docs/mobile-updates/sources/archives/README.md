# Archived mobile update sources

Markdown and PDF config for older APKs. Matching PDFs live in `docs/mobile-updates/archives/`.

## Naming

Same as the APK stem: `mobile-update-v{DisplayVersion}+{versionCode}` (example: `mobile-update-v1.5.1+9.md`).

| File | Purpose |
|------|---------|
| `mobile-update-v1.5.1+9.md` | Note for `RaphCare-v1.5.1+9.apk` |
| `mobile-update-v1.5.1+9.config.json` | PDF export config (stylesheet: `../mobile-update.pdf.css`) |

Stylesheet path: `../mobile-update.pdf.css`. Logo: `../../brand/raphcare-logo.png`.

PDF output: `../../archives/mobile-update-v1.5.1+9.pdf`

## When to archive

When you cut a new sideload (or Play) build and publish its note under `sources/`, move the previous `.md` and `.config.json` here, and move the previous PDF to `docs/mobile-updates/archives/`.

## Re-export from archive

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.1+9 -Archive
```
