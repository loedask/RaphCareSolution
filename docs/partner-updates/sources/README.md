# Partner update sources

Edit notes here. Partner-facing PDFs are written one level up in `docs/partner-updates/` (current) or `docs/partner-updates/archives/` (old dated updates).

## Current documents

| Stem | Purpose |
|------|---------|
| `partner-update-YYYY-MM-DD` | Dated product / web / API partner note |
| `raphcare-price-list` | Price list PDF |
| `raphcare-demo-accounts` | Staging demo logins sheet (keep private) |

Each stem has `.md`, `.config.json`, and often its own `.pdf.css` (dated notes share `partner-update.pdf.css`).

## Archive

When a newer dated partner update becomes current:

1. Move the previous `.md` and `.config.json` into `sources/archives/`.
2. Move the previous `.pdf` into `docs/partner-updates/archives/`.

Price list and demo accounts stay current until you replace them in place (or version the stem if you need history).

See [`archives/README.md`](archives/README.md).

## Export PDF

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-09-02
```

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-08-29 -Archive
```

Logo path from these files is `../brand/raphcare-logo.png` (from archives: `../../brand/raphcare-logo.png`).
