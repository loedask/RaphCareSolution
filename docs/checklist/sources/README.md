# Checklist sources

Edit checklists and related plans here. Feature checklist PDFs are written one level up in `docs/checklist/`.

## Documents

| File | Purpose |
|------|---------|
| `raphcare-feature-checklist.md` | Engineering feature board |
| `raphcare-feature-checklist-partner.md` | Partner plain-language twin |
| `raphcare-feature-checklist.pdf.css` | Shared print styles |
| `*.pdf.json` | md-to-pdf configs |
| `Mobile_Release_Ready_Checklist.md` | Mobile release gates |
| `Web_And_Mobile_Smoke_Plan.md` | API / Web / Mobile smoke plan |
| `Wearable_Hardware_Proveout.md` | E580 / E585 desk test |

## Export PDFs

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareFeatureChecklistPdf.ps1
```

Links from these files to other docs under `docs/` use `../../...`. Links to the generated PDFs use `../raphcare-feature-checklist.pdf` (and the partner twin).
