<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<div>
<h1>Demo accounts for testing</h1>
<p class="subtitle">RaphCare Demo Clinic · staging · 29 August 2026</p>
</div>
</div>

These logins are for the staging site. They are fake people, not real patients. Use them to try the clinic screens and the patient app. Other hospitals on the same site are left as they are.

<div class="note">
<p><strong>Keep this sheet to yourselves.</strong> It has the shared password. Do not post it on a public page.</p>
</div>

## Where to sign in

**Staff** (hospital admin, doctor, pharmacy, lab): open the admin site, then Professional sign-in.

[https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin)

**Patient:** use the Android app with the patient account below. You can also try [patient sign-in on the same website](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/patient/signin).

You will not get a second email code. After the password, you go straight in.

<div class="password-box">
<p class="label">Password for every demo account</p>
<p class="value">RaphCareDemo!2026</p>
</div>

## Staff accounts

Open **RaphCare Demo Clinic** after you sign in.

| Role | Email | What to try |
|------|--------|-------------|
| Hospital admin | `demo.admin@raphcare.com` | Open the clinic. Check inpatient, collection, appointments, and the waiting screen. |
| Doctor | `demo.doctor@raphcare.com` | Patient chart, visits, notes, prescriptions, lab orders. |
| Pharmacist | `demo.pharmacy@raphcare.com` | Collection: find a pickup, Call, mark collected. |
| Lab | `demo.lab@raphcare.com` | Collection: find the lab order, enter a result. |

## Patient account

| Role | Email | What to try |
|------|--------|-------------|
| Patient | `demo.patient@raphcare.com` | Home, health records, pickup code and QR, notices when staff tap Call. |

There is also a chart named **Demo Inpatient** (no login). That person is already in a bed, so you can see inpatient occupancy without admitting anyone first.

## Pickup codes already in the clinic

Use these on Collection, or type them in search.

| Code | What it is | State |
|------|----------|--------|
| `2DEM2A` | Prescription (Amoxicillin) | Waiting |
| `2DEM2B` | Lab (full blood count) | Waiting |
| `2DEM2C` | Prescription (Paracetamol) | Already called to the counter |

The waiting screen shows pickup codes only. It does not show names or medicines.

## A path that covers most of it

1. Sign in as **hospital admin**. Confirm you land in **RaphCare Demo Clinic**. The left menu should hide. Use **All hospitals** in the top bar to go back to the list.
2. Open **Collection**. Search `2DEM2A`. Tap **Call**. Open the waiting screen in another tab. You should see the code on the TV view.
3. Sign in as **pharmacist** (or stay as admin). Mark that prescription collected. Print a slip if you want. Try the wall poster for the counter.
4. Sign in as **lab**. Find `2DEM2B` and enter a result.
5. Open **Inpatient**. You should see **Demo Inpatient** in a bed. You can admit someone else if you add a free bed, then discharge them.
6. On the **Android app**, sign in as the patient. Health records should show a pickup code and a QR code. When staff tap Call, the app should say come to the counter.

## If something does not load

The staging sites can take a minute to wake up. Refresh once. If sign-in still fails, tell me which email you used and what the screen said.

Please note, these accounts live on staging only. They are not for a live clinic. The public site is [raphcare.com](https://raphcare.com).

<p class="signoff">Thanks,<br />Daskana</p>
