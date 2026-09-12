# RaphCare partner updates

Partner PDFs for product and web updates (separate from phone APK notes under [`../mobile-updates/`](../mobile-updates/)).

| Path | What is here |
|------|----------------|
| `*.pdf` | Current PDFs to send (dated partner update, feature catalog v1, price list v2, demo accounts v3) |
| [`archives/`](archives/) | Older dated partner-update PDFs and prior catalog, price list, and demo-accounts versions |
| [`sources/`](sources/) | Markdown, PDF config, CSS (edit here) |
| [`brand/`](brand/) | Logo used in the PDFs |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-09-06
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-feature-catalog-v1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-price-list-v2
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-demo-accounts-v3
```

Archived dated note or older versioned sheet:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-08-29 -Archive
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-price-list-v1 -Archive
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-demo-accounts-v2 -Archive
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-demo-accounts-v1 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
