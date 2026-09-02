<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (29 Aug 2026)</h1>
</div>

Here is what landed today. The short version: hospital admin and inpatient on the web are ready to demo, staging has a ready-made clinic with logins, and there is a patient Android APK that talks to that same staging site. Overall partner checklist completion is about **77%**.

## 1. Demo clinic and accounts (staging)

Staging seeds **RaphCare Demo Clinic** when the API starts. It does not wipe other hospitals (for example your own clinic stays).

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

Pickup codes already in that clinic:

| Code | What | State |
|------|------|--------|
| `2DEM2A` | Prescription | Waiting |
| `2DEM2B` | Lab order | Waiting |
| `2DEM2C` | Prescription | Already called to the counter |

There is also a chart named **Demo Inpatient** already in a bed, so inpatient occupancy shows without admitting someone first.

A one-page sheet with the same logins is here: [`raphcare-demo-accounts.pdf`](raphcare-demo-accounts.pdf). Keep that sheet private. It has the password.

## 2. Hospital admin on the web

Outpatient hospital admin and inpatient are both at **100%** on the partner board. What you can walk through now:

- **Opening a hospital** hides the left platform menu so the hospital tools have more room. Use **All hospitals** in the top bar to go back. Language and sign out stay in the top bar.
- Hospital pages use a **large header** and a **section menu** on the side (overview, facilities, patients, clinical team, appointments, collection, inpatient, and the rest).
- **Clinical team** is the staff label for clinicians (it used to say Providers).
- **Patient chart** for staff: overview, clinical, coverage, devices, and care. Staff read it here; they do not edit the chart on that page.
- **Visits:** while a visit is open, a hospital admin or doctor can add notes, prescriptions, and lab orders.
- **Collection counter:** search by pickup code, name, or health ID; scan a QR; call a code onto the waiting screen; mark a prescription collected; enter a lab result; cancel or undo; print a slip or a wall poster. The waiting screen shows codes only, not names.
- **Inpatient:** wards, rooms, beds, admit, discharge, transfer, maintenance, and history. Beds on the ward map look like simple top-down beds.
- **Emergency events** show on the patient chart and on the hospital Devices board when the clinic context is set.

Staff roles for invites still include staff, doctor, pharmacist, and lab technician.

## 3. Patient phone app

Screens are largely in place. Progress on the partner board for this area is about **59%**. Catch-up is mostly iPhone video, real push alerts, and deeper wearables.

What changed or is easy to try now:

- **Choose my clinic** by clinic name or a short **RC-** code (patients do not need Guids).
- **Forgot password** by email code, then a new password.
- **Health records** show pickup codes and a QR for items waiting at the hospital. Scan a wall poster at the counter to show only that hospital. When staff tap Call, the app should say come to the counter and show a notice.
- **Home and Profile** icons are monochrome (same teal tint style as the web). Failed taps should not close the app on Android.

Video visit works on Android wiring; iPhone video still needs more setup. Live band vitals and background sync are still partial.

## 4. Android app for the patient side

I built a Release APK that talks to the staging API (not localhost). I have sent you the install file. Copy it to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

On the app, sign in as `demo.patient@raphcare.com` with the password above.

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
| **Overall** | **77%** |

## Still to confirm

1. Sign in as hospital admin. Open **RaphCare Demo Clinic**. Confirm the left menu hides. Use **All hospitals** to return.
2. Open a patient chart and walk the section menu (overview, clinical, coverage, devices, care).
3. On Collection, search `2DEM2A`, tap **Call**, open the waiting screen in another tab.
4. As pharmacist or admin, mark a prescription collected. Try print slip and wall poster if you want.
5. As lab, find `2DEM2B` and enter a result.
6. Open Inpatient and confirm **Demo Inpatient** is in a bed. Check that beds look like top-down tiles on the ward map.
7. Install the Android APK. Sign in as the demo patient. Check health records for a pickup code and QR. When staff tap Call, the app should say come to the counter.

Thanks,  
Daskana
