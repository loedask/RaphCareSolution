# RaphCare mobile updates

Partner-facing notes for each patient Android APK (and later iOS builds). Separate from the dated [`../partner-updates/`](../partner-updates/) notes, which cover the wider product.

## When to add a note

Every time `RaphCare.Mobile` gets a new `ApplicationDisplayVersion` and/or Android `ApplicationVersion` (versionCode), or you publish a sideload APK for the partner.

## Naming

| File | Purpose |
|------|---------|
| `mobile-update-v1.5.1.md` | Plain-language changelog for that APK |
| `mobile-update-v1.5.1.config.json` | PDF export config |
| `mobile-update-v1.5.1.pdf` | PDF to send with the APK |
| `mobile-update.pdf.css` | Shared styles (or reuse partner-update CSS) |

## Export PDF

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareMobileUpdatePdf.ps1 -Version 1.5.1
```

## Sideload APK

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Publish-RaphCareAndroidSideload.ps1
```

Output lands under `artifacts/android/` as `RaphCare-v{version}+{build}.apk`.
