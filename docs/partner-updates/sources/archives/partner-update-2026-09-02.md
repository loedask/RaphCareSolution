<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (2 Sep 2026)</h1>
</div>

Here is what landed today. The short version: the **Hospital** plan stay-and-counter extras are complete on the partner board. Ward notes, cash bill at discharge, occupancy, nurse invite, casualty waiting screen, theatre list, outbound referrals, SafeCare on the hospital home, return visit at discharge, who is on today, AI draft of the discharge summary, and a separate lab "result ready" notice. Overall partner checklist completion is about **81%**.

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
- **Return visit at discharge:** on discharge, optionally pick a clinician and time for the next outpatient visit before the person leaves.
- **Who is on today:** pick a day and see who is on Morning, Afternoon, or Night. Add or remove staff for a shift.
- **AI discharge draft:** on discharge, tap Draft with AI when the hospital allows it (Organization details has the toggle). Staff edit the text, then save. The draft uses stay reason and ward vitals only, not free-text ward notes. Needs cloud AI keys on the server; otherwise drafting is unavailable. Clinical text goes only to Azure OpenAI you configure, not to public ChatGPT.
- **Lab result ready:** when staff enter a lab result on Collection, the patient gets an in-app notice (and push when configured) that the result is ready. The notice does not include the values. Values stay on the visit health record.

The commercial one-pager matches this. Hospital is billed per site each month and lists these extras: [`raphcare-price-list.pdf`](../raphcare-price-list.pdf).

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

A one-page sheet with the same logins is here: [`raphcare-demo-accounts-v2.pdf`](../raphcare-demo-accounts-v2.pdf). Keep that sheet private. It has the password.

## 3. Patient Android APK (staging)

I built a new Release APK for release **1.5.0** (file name like `RaphCare-v1.5.0+8.apk`). It talks to the staging API, not localhost. I have sent you the install file. Copy it to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

## 4. What was already ready (quick reminder)

Outpatient hospital admin and inpatient beds were already at **100%** on the partner board. Collection counter, waiting screen, visits, clinical team, and the patient chart for staff are unchanged from earlier updates.

## 5. Links

- Admin site: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)
- Public site: [raphcare.com](https://raphcare.com)

## 6. Partner checklist

The partner status board is updated for this work. Rough rollup:

| Area | % |
|------|--:|
| Hospital admin (outpatient) | 100% |
| Inpatient | 100% |
| Patient phone app | 59% |
| Other staff and system | 83% |
| Hospital plan extras (stay and counter) | 100% |
| **Overall** | **81%** |

## Try this week

1. Sign in as hospital admin. Open **RaphCare Demo Clinic**.
2. Open **Inpatient**, find **Demo Inpatient**, add a ward note (try a heart rate or temperature).
3. On discharge, try **Draft with AI**, edit the text, enter a nightly bed rate, and optionally book a return visit. Print the invoice if you want.
4. On the hospital overview, check occupancy numbers and any recent SafeCare alerts.
5. Open **Casualty**, add a walk-in with a triage colour, tap **Call**, open the waiting screen in another tab (codes and colour only).
6. Open **Theatre**, add today's case, then start or complete it.
7. Open **Referrals**, log an outbound referral, then mark it accepted or completed.
8. Open **Roster**, add someone to Morning or Afternoon for today.
9. On Collection, enter a lab result for the demo patient. On the phone app, confirm the "result ready" notice, then open Health records for the values.
10. Install the new Android APK. Sign in as the demo patient and smoke Home, Health records, and Devices.

Thanks,  
Daskana
