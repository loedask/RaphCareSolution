# Incubators and accelerators

Founder-facing application drafts and the shared apply plan. Not partner product PDFs.

## Share with your co-founder

| File | Use |
|------|-----|
| [`plan.html`](plan.html) | Open in a browser. Priority list, why each program was selected, apply links. |
| [`raphcare-incubator-plan.pdf`](raphcare-incubator-plan.pdf) | Same plan as a PDF to send. |
| [`sources/raphcare-incubator-plan.md`](sources/raphcare-incubator-plan.md) | Source for the PDF. Edit here, then re-export. |

Regenerate the PDF after editing the markdown:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Export-RaphCareIncubatorPlanPdf.ps1
```

Also update `plan.html` in the same change set so HTML and PDF stay aligned.

## Application drafts

Each program gets its own folder when you start an application. Share the form questions (and any old answers) and we fill that folder the same way as YC.

| Folder | Program | Status |
|--------|---------|--------|
| [`yc/`](yc/) | Y Combinator | Draft ready (`application.html`) |

Last reviewed: 5 September 2026

## Folder layout

```text
docs/incubator/
  README.md                      ← this index
  plan.html                      ← shareable plan (HTML)
  raphcare-incubator-plan.pdf    ← shareable plan (PDF)
  brand/                         ← logo for HTML and PDF
  sources/                       ← markdown + PDF config/CSS
  yc/
    README.md
    application.html
  founders-factory-africa/       ← add when that form starts
```

Keep product facts honest. Prefer checklists, feature catalog, demo-accounts sheet, price list, and `docs/Mobile_Android_Test_Hosting.md`.

## How to reuse answers across programs

Most forms ask the same core story. Start from [`yc/application.html`](yc/application.html), then trim:

| Theme | Source | Adjust |
|-------|--------|--------|
| What you build | Company one-liner, product description | Stress hospital Portal, patient app, and Ops wearables, not “telemedicine only” |
| Why now, why you | Why this idea, founders met, who codes | Keep founder-led build; name .NET and Azure staging if they ask for tech |
| Progress | How far along, users, revenue | Honest: staging live, demos ready, no paying clinics yet (until that changes) |
| Market | Competitors, location, make money | SA first, Congo next; Clinic or Hospital monthly pricing from the price list |
| Traction plan | Next stages, Demo Day | Swap “Demo Day” for their demo day or investor day name |

Do **not** paste demo passwords into public program directories or social posts. Share credentials only inside a private application portal or a direct email they request.

## Priority and why selected

Full write-up (why selected, why this rank, apply URLs, windows): [`plan.html`](plan.html) or the PDF.

| Priority | Program | Why selected (short) | Apply |
|---------:|---------|----------------------|-------|
| 1 | Y Combinator | Strongest investor signal; reapply with a real product | [ycombinator.com/apply](https://www.ycombinator.com/apply) |
| 2 | Founders Factory Africa (Scale) | Johannesburg, African startups, product-stage Scale track | [foundersfactory.africa/apply](https://www.foundersfactory.africa/apply) |
| 3 | Techstars (healthcare) | Mentorship and capital; healthcare cohorts fit clinic OS | [techstars.com/accelerators](https://www.techstars.com/accelerators) |
| 4 | Africa Health-Tech Accelerator | Closest digital-health sector match in Africa | [menterprise opportunity](https://menterprise.africa/opportunities/africa-health-tech-accelerator-2026/) (watch next year) |
| 5 | 500 Global Flagship | Global emerging-markets accelerator brand | [500.co/apply-for-an-accelerator](https://500.co/apply-for-an-accelerator) |
| 6 | Google for Startups SA | Equity-free, SA-led; better after traction | [startup.google.com/.../south-africa](https://startup.google.com/programs/accelerator/south-africa/) |
| 7 | AUDA-NEPAD HGS | Pan-African health-systems mandate | [VC4A HGS](https://vc4a.com/auda-nepad/hgs-2026/) (next call) |
| 8 | Antler Nairobi | Africa capital; only if we can relocate | [antler.co/apply](https://www.antler.co/apply) |
| 9 | a16z Speedrun | Large cheque and brand; after more traction | [speedrun.a16z.com](https://speedrun.a16z.com/) |

## What to do this month

1. Submit **YC** using [`yc/application.html`](yc/application.html).
2. Start **Founders Factory Africa Scale**. Share that form here and we add `docs/incubator/founders-factory-africa/`.
3. On Techstars, pick one **open healthcare** cohort if dates work.
4. Bookmark HGS, Africa Health-Tech, and Google SA for the **next** open call.
5. Before each submit, ask for a final pass so answers match HEAD.

## What not to chase first

- Pure idea-stage “find a cofounder” programs (you already have founders and a product).
- US FDA device accelerators (RaphCare is healthcare IT first; do not pitch as a diagnostic device).
- Programs that demand relocation you cannot do unless the deal clearly pays for that move.
- Closed 2026 cohort pages that only say “expired” with no next-year form yet.
