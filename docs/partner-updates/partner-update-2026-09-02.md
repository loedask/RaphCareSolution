<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (2 Sep 2026)</h1>
</div>

Here is what landed this week. The short version: the **Hospital** plan now has real stay and counter extras beyond beds, and you can demo them on staging. Ward notes, a cash bill at discharge, occupancy numbers, a nurse invite, a casualty waiting screen, today's theatre list, outbound referral follow-through, and SafeCare alerts on the hospital home. Overall partner checklist completion is about **75%**. That number dipped a little from last time because we added this Hospital block to the board; most of that block is already Done.

## 1. Hospital plan extras (what changed)

Clinic stays the lighter outpatient product. Network stays multi-site. **Hospital** is where overnight stay and the busy front of house live. What you can walk through now on a hospital site:

- **Ward notes:** while someone is in a bed, a nurse, doctor, or hospital admin can add a note, and optional heart rate, temperature, or oxygen.
- **Bill at discharge:** enter a nightly bed rate and any extra charge. Print that as an invoice. Mark paid in cash. The discharge summary also shows on the patient's phone health records as a stay.
- **Occupancy:** on the hospital overview, see occupancy %, how many people came in or left today, and average length of stay.
- **Nurse job:** invite someone as a nurse. They can write ward notes. They cannot document an outpatient visit.
- **Casualty queue:** add a walk-in with a triage colour. Call a code onto a second waiting screen. The TV shows codes and colour only, not names.
- **Theatre list:** put today's operations on one board. Start, complete, or cancel a case.
- **SafeCare on the hospital home:** recent SOS and fall alerts (last 72 hours) show on the hospital overview and the admin home. Full history stays on Devices.
- **Outbound referrals:** log a referral out of the hospital, then mark it accepted, completed, or cancelled.

Still open on this block: book a return visit at discharge, a simple "who is on today" roster, and an AI draft for the discharge summary. Lab results already show on the visit health record; a separate "your result is ready" notice is still Partial.

The commercial one-pager matches this. Hospital is billed per site each month and lists these extras: [`raphcare-price-list.pdf`](raphcare-price-list.pdf).

## 2. Demo clinic and accounts (staging)

Staging still seeds **RaphCare Demo Clinic** when the API starts. It does not wipe other hospitals.

Shared password for every demo account: **`RaphCareDemo!2026`**

| Who | Email |
|-----|--------|
| Hospital admin | `demo.admin@raphcare.com` |
| Doctor | `demo.doctor@raphcare.com` |
| Pharmacist | `demo.pharmacy@raphcare.com` |
| Lab | `demo.lab@raphcare.com` |
| Patient (phone app) | `demo.patient@raphcare.com` |

Staff sign-in: [Professional sign-in](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin)

Patient on the website (optional): [Patient sign-in](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/patient/signin)

For these demo accounts there is no Microsoft sign-in step, and no second email code after the password.

There is still a chart named **Demo Inpatient** already in a bed, so inpatient and ward notes are easy to try without admitting someone first.

A one-page sheet with the same logins is here: [`raphcare-demo-accounts.pdf`](raphcare-demo-accounts.pdf). Keep that sheet private. It has the password.

## 3. What was already ready (quick reminder)

Outpatient hospital admin and inpatient beds were already at **100%** on the partner board. Collection counter, waiting screen, visits, clinical team, and the patient chart for staff are unchanged from the last update. The patient Android APK for staging is the same path as before if you still have that install file.

## 4. Links

- Admin site: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)
- Public site: [raphcare.com](https://raphcare.com)

## 5. Partner checklist

The partner status board is updated for this work. Rough rollup:

| Area | % |
|------|--:|
| Hospital admin (outpatient) | 100% |
| Inpatient | 100% |
| Patient phone app | 59% |
| Other staff and system | 83% |
| Hospital plan extras (stay and counter) | 73% |
| **Overall** | **75%** |

## Try this week

1. Sign in as hospital admin. Open **RaphCare Demo Clinic**.
2. Open **Inpatient**, find **Demo Inpatient**, add a ward note (try a heart rate or temperature).
3. Discharge that stay (or a test admission). Enter a nightly bed rate, print the invoice, mark paid in cash if you want to see the money step.
4. On the hospital overview, check occupancy numbers and any recent SafeCare alerts.
5. Open **Casualty**, add a walk-in with a triage colour, tap **Call**, open the waiting screen in another tab (codes and colour only).
6. Open **Theatre**, add today's case, then start or complete it.
7. Open **Referrals**, log an outbound referral, then mark it accepted or completed.
8. On Clinical team (or invites), confirm you can invite someone as a **nurse**.

Thanks,  
Daskana
