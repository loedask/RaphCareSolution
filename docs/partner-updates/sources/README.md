# Partner update sources

Edit notes here. Partner-facing PDFs are written one level up in `docs/partner-updates/` (current) or `docs/partner-updates/archives/` (old dated updates and prior versions).

## Current documents

| Stem | Purpose |
|------|---------|
| `partner-update-YYYY-MM-DD` | Dated product / web / API partner note |
| `raphcare-feature-catalog-v1` | Shareable list of what the product does (Portal for hospitals, Ops for platform owners, Mobile for patients) |
| `raphcare-price-list-v1` | Price list PDF |
| `raphcare-demo-accounts-v2` | Staging demo logins sheet (current; keep private) |
| `archives/raphcare-demo-accounts-v1` | Prior demo accounts sheet (Aug 2026) |

Each stem has `.md`, `.config.json`, and often its own `.pdf.css` (dated notes share `partner-update.pdf.css`).

## Versioning (catalog, price list, demo accounts)

When a new version should replace the current sheet:

1. Move the previous stem’s `.md`, `.config.json`, and `.pdf.css` into `sources/archives/`.
2. Move the previous `.pdf` into `docs/partner-updates/archives/`.
3. Create the next stem in `sources/` (for example `raphcare-feature-catalog-v2`) with updated subtitle / `document_title`.
4. Export the new stem to the folder root. Export archived stems with `-Archive` if you need to rebuild an old PDF.

Dated partner updates keep the date in the stem instead of a `vN` suffix.

## Archive

When a newer dated partner update becomes current:

1. Move the previous `.md` and `.config.json` into `sources/archives/`.
2. Move the previous `.pdf` into `docs/partner-updates/archives/`.

See [`archives/README.md`](archives/README.md).

## Export PDF

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-09-02
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-08-29 -Archive
```

Logo for editors: `../brand/raphcare-logo.png` (archives: `../../brand/raphcare-logo.png`). The export script copies that PNG into `sources/brand/` (or `sources/archives/brand/`) for PDF generation, because Chromium will not load parent-folder images.
