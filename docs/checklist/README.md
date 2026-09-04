# RaphCare checklists

Partner-facing PDFs for the feature checklists. Builder markdown and PDF configs live under [`sources/`](sources/).

| Path | What is here |
|------|----------------|
| `raphcare-feature-checklist.pdf` | Engineering checklist PDF |
| `raphcare-feature-checklist-partner.pdf` | Partner status PDF |
| [`sources/`](sources/) | Markdown, CSS, PDF configs (edit here) |

Other builder plans (smoke, mobile release, wearable prove-out) are markdown-only under `sources/`. They do not have PDFs.

## Export after editing sources

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareFeatureChecklistPdf.ps1
```

Optional: `-Which engineering` or `-Which partner`.

Details: [`sources/README.md`](sources/README.md).
