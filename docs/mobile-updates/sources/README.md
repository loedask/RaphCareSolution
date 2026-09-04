# Mobile update sources

Edit notes here. Partner-facing PDFs are written one level up in `docs/mobile-updates/` (current) or `docs/mobile-updates/archives/` (old builds).

## Naming

Same label as the APK: `mobile-update-v{DisplayVersion}+{versionCode}`

Example: `RaphCare-v1.7.0+11.apk` uses `mobile-update-v1.7.0+11.md` and `.config.json` here, and outputs `../mobile-update-v1.7.0+11.pdf`.

| File | Purpose |
|------|---------|
| `mobile-update-v1.7.0+11.md` | Plain-language changelog (current) |
| `mobile-update-v1.7.0+11.config.json` | PDF export config |
| `mobile-update.pdf.css` | Shared styles |
| [`archives/`](archives/) | Markdown and config for older builds |

Logo path from these files is `../brand/raphcare-logo.png` (from archives: `../../brand/raphcare-logo.png`). The export script copies the PNG into the export folder as `brand/raphcare-logo.png` so Chromium can embed it.

Do not use display-only names like `mobile-update-v1.5.2.md`.

## When to add a note

Every time `RaphCare.Mobile` gets a new `ApplicationDisplayVersion` or Android `ApplicationVersion` (versionCode), or you publish a sideload APK for the partner.

## Archive

When a newer build becomes current:

1. Move the previous `.md` and `.config.json` into `sources/archives/`.
2. Move the previous `.pdf` into `docs/mobile-updates/archives/`.
3. Keep the `+{build}` in every filename.

See [`archives/README.md`](archives/README.md).

## Export PDF

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.7.0+11
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.1+9 -Archive
```

## Sideload APK

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidSideload.ps1
```

Output lands under `artifacts/android/` as `RaphCare-v{version}+{build}.apk`.
