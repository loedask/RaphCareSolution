# RaphCare partner updates

Partner PDFs for product and web updates (separate from phone APK notes under [`../mobile-updates/`](../mobile-updates/)).

| Path | What is here |
|------|----------------|
| `*.pdf` | Current PDFs to send (dated partner update, price list, demo accounts) |
| [`archives/`](archives/) | Older dated partner-update PDFs |
| [`sources/`](sources/) | Markdown, PDF config, CSS (edit here) |
| [`brand/`](brand/) | Logo used in the PDFs |

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-09-02
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-price-list
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem raphcare-demo-accounts
```

Archived dated note:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCarePartnerUpdatePdf.ps1 -Stem partner-update-2026-08-29 -Archive
```

Details for builders: [`sources/README.md`](sources/README.md).
