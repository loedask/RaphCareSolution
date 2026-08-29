<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (29 Aug 2026)</h1>
</div>

Here is what changed today. The main thing is a ready-made demo clinic with logins you can use for testing. Staff use email and password. There is no Microsoft sign-in step for these demo accounts, and no second email code after the password.

## 1. Demo clinic and accounts (staging)

Staging now seeds **RaphCare Demo Clinic** on API startup. It does not wipe other hospitals (for example your own clinic stays).

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

Pickup codes already in that clinic:

| Code | What | State |
|------|------|--------|
| `2DEM2A` | Prescription | Waiting |
| `2DEM2B` | Lab order | Waiting |
| `2DEM2C` | Prescription | Already called to the counter |

There is also a chart named **Demo Inpatient** already in a bed, so inpatient occupancy shows without admitting someone first.

A one-page sheet with the same logins is here: [`raphcare-demo-accounts.pdf`](raphcare-demo-accounts.pdf). Keep that sheet private. It has the password.

## 2. Android app for the patient side

I built a Release APK that talks to the staging API (not localhost). Install file on this machine:

`C:\Users\Sanel\source\repos\RaphCareSolution\artifacts\android\RaphCare.apk`

Copy that file to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

On the app, sign in as `demo.patient@raphcare.com` with the password above.

## 3. Links

- Admin site: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)

## 4. Partner checklist

The partner status board now includes the staging demo sign-in row and a short test path for collection, Call, the waiting screen, and the patient app. Overall completion is about **76%**.

## Still to confirm

1. Sign in as hospital admin. Open **RaphCare Demo Clinic**. Confirm the left menu hides. Use **All hospitals** to return.
2. On Collection, search `2DEM2A`, tap **Call**, open the waiting screen in another tab.
3. As pharmacist or admin, mark a prescription collected. Try print slip and wall poster if you want.
4. As lab, find `2DEM2B` and enter a result.
5. Open Inpatient and confirm **Demo Inpatient** is in a bed.
6. Install the Android APK. Sign in as the demo patient. Check health records for a pickup code and QR. When staff tap Call, the app should say come to the counter.

The full product status board is still the partner checklist.

Thanks,  
Daskana
