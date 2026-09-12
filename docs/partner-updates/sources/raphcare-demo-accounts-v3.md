<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<div>
<h1>Demo accounts for testing</h1>
<p class="subtitle">RaphCare staging · v3 · 12 September 2026</p>
</div>
</div>

These logins are for the staging site. They are fake people, not real patients. Use them to try the hospital Portal, Practice plan gating, Ops prices, and the patient app. Other hospitals on the same site are left as they are.

<div class="note">
<p><strong>Keep this sheet to yourselves.</strong> It has the shared password. Do not post it on a public page. This replaces the September 2026 v2 sheet.</p>
</div>

## Where to sign in

**Staff** (hospital admin, doctor, pharmacy, lab): open the hospital Portal, then Professional sign-in.

[https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin)

**Platform Ops** (fleet and prices): open Ops, then Professional sign-in.

[https://raphcare-ops.azurewebsites.net/signin](https://raphcare-ops.azurewebsites.net/signin)

**Patient:** use the Android app with the patient account below. You can also try [patient sign-in on the Portal](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/patient/signin).

You will not get a second email code. After the password, you go straight in.

<div class="password-box">
<p class="label">Password for every demo account</p>
<p class="value">RaphCareDemo!2026</p>
</div>

## Plans on staging

| Site | Plan | What you should see | What you should not see |
|------|------|---------------------|-------------------------|
| RaphCare Demo Clinic | Hospital | Beds, collection, pickup waiting screen, casualty, consult waiting | (full Hospital tools) |
| RaphCare Demo Practice | Practice | Appointments, visits, consult waiting display | Inpatient, collection counter, pickup TV, casualty |

After sign-in, use **All hospitals** in the top bar to switch sites. Admin and doctor can open both. Pharmacy and lab are for Demo Clinic (Hospital) only.

## Two waiting displays

| Display | What it is for | Where |
|---------|----------------|-------|
| Consult waiting | Next person into the doctor | Practice and Clinic (and Hospital outpatient). Open from Consult. |
| Pickup waiting | Pharmacy or lab ready codes | Hospital collection only. Open from Collection. |

## Staff accounts (hospital Portal)

| Role | Email | What to try |
|------|--------|-------------|
| Hospital admin | `demo.admin@raphcare.com` | Switch between Demo Clinic and Demo Practice. On Hospital: inpatient, collection, pickup screen. On Practice: Consult, Call next. |
| Doctor | `demo.doctor@raphcare.com` | Same two sites. Patient chart, visits, notes. On Practice: Call next on Consult. |
| Pharmacist | `demo.pharmacy@raphcare.com` | Demo Clinic Collection: find a pickup, Call, mark collected. |
| Lab | `demo.lab@raphcare.com` | Demo Clinic Collection: find the lab order, enter a result. |

## Platform Ops account

Use this on the **Ops** site only (fleet and prices). It is not a hospital staff login for day-to-day clinic work.

| Role | Email | What to try |
|------|--------|-------------|
| Platform Ops | `demo.ops@raphcare.com` | Sign in to Ops. Open **Prices** to see catalog amounts (ZAR and USD). Open Wearable fleet for Demo Clinic, then assign a watch to the demo patient. |

`demo.admin@raphcare.com` can also open Ops if you need a second check. Prefer `demo.ops` for fleet and prices walkthroughs.

## Patient account

| Role | Email | What to try |
|------|--------|-------------|
| Patient | `demo.patient@raphcare.com` | Home, health records, pickup code and QR on Demo Clinic, notices when staff tap Call on Collection. Claim a watch that Ops assigned. |

There is also a chart named **Demo Inpatient** (no login). That person is already in a bed on Demo Clinic.

## Pickup codes already in Demo Clinic

Use these on Collection, or type them in search.

| Code | What it is | State |
|------|----------|--------|
| `2DEM2A` | Prescription (Amoxicillin) | Waiting |
| `2DEM2B` | Lab (full blood count) | Waiting |
| `2DEM2C` | Prescription (Paracetamol) | Already called to the counter |

The pickup waiting screen shows codes only. It does not show names or medicines.

Demo Practice seeds a consult ticket code **PRAC01** so Call next works on a fresh seed.

## Hospital path (Demo Clinic)

1. Sign in as **hospital admin**. Open **RaphCare Demo Clinic**.
2. Confirm you see inpatient and collection in the left sections.
3. Open **Collection**. Search `2DEM2A`. Tap **Call**. Open the pickup waiting screen in another tab.
4. Sign in as **pharmacist** (or stay as admin). Mark that prescription collected.
5. Open **Inpatient**. You should see **Demo Inpatient** in a bed.

## Practice path (Demo Practice)

1. Stay signed in as **admin** or **doctor**. Open **All hospitals**, then **RaphCare Demo Practice**.
2. Confirm inpatient and collection are hidden or blocked.
3. Open **Consult**. You should see a waiting ticket (for example PRAC01).
4. Tap **Call**, then open the consult waiting display in another tab. You should see the code only, not the name.
5. Complete the ticket when done.

## Ops prices path

1. Sign in to Ops as `demo.ops@raphcare.com`.
2. Open **Prices**. Confirm Practice, Clinic, Hospital, and care plan rows show rand and dollar amounts.
3. Optionally pick a hospital below the catalog and change its commercial plan (Practice, Clinic, Hospital, or Network), then save.
4. Change a test amount if you want, save, then put it back.

## If something does not load

The staging sites can take a minute to wake up. Refresh once. If sign-in still fails, tell me which email you used and what the screen said.

Please note, these accounts live on staging only. They are not for a live clinic. The public site is [raphcare.com](https://raphcare.com).

<p class="signoff">Thanks,<br />Daskana</p>
