<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (5 Sep 2026)</h1>
</div>

Here is what landed today. The short version: the patient Home screen shows **real** care data, Active clinic lists **hospitals linked to you**, and staff can add Portal or Ops to the phone home screen. Current Android build is **1.8.4**. Overall partner checklist completion is about **83%**.

## 1. Patient Android app (1.8.4)

Sideload file name: `RaphCare-v1.8.4+16.apk`. It talks to the staging API, not localhost. Copy it to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

What changed in this 1.8 line (since the 1.7.0 note):

- **Honest Home.** Upcoming visits, claimed watches, and synced heart rate or oxygen come from the patient's record. Empty states when there is nothing yet. No demo doctor or fake blood pressure.
- **Wellness tip from mood.** The Home tip follows recent Mental Health check-ins. If you have not checked in, it asks you to log a mood instead of claiming a positive week.
- **Hospitals linked to me.** On Active clinic, hospitals from care links (registration, staff grant, visits) show in their own list. Tap one to make it active. Name search still finds the directory; it is not a membership list.
- **Mental health questionnaires.** PHQ-9 and GAD-7 self-checks on the phone (needs an active clinic). Answers can show on the hospital chart for staff.

One-page phone note: [`mobile-update-v1.8.4+16.pdf`](../mobile-updates/mobile-update-v1.8.4+16.pdf).

## 2. Staff web on a phone (Portal and Ops)

Portal and Ops already tighten on a small screen (top bar, forms, tables). Staff can also add a **home-screen shortcut** from the browser (Add to Home Screen, or Install app, depending on the phone). This is the same website, not a separate staff app, and not an offline app.

Sign-in today is email and password. After you add the shortcut, open it once and confirm you can still sign in.

## 3. Feature catalog

The shareable list of what RaphCare does is updated for phone Home honesty, linked hospitals, and the home-screen shortcut: [`raphcare-feature-catalog-v1.pdf`](raphcare-feature-catalog-v1.pdf). It is not a progress board. Completion % stays on the partner status sheet.

## 4. Demo clinic and accounts (unchanged)

Staging still seeds **RaphCare Demo Clinic** when the API starts. It does not wipe other hospitals.

Shared password for every demo account: **`RaphCareDemo!2026`**

| Who | Email |
|-----|--------|
| Hospital admin | `demo.admin@raphcare.com` |
| Doctor | `demo.doctor@raphcare.com` |
| Pharmacist | `demo.pharmacy@raphcare.com` |
| Lab | `demo.lab@raphcare.com` |
| Platform Ops | `demo.ops@raphcare.com` |
| Patient (phone app) | `demo.patient@raphcare.com` |

Staff sign-in: [Professional sign-in](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin)

Ops sign-in: [Ops sign-in](https://raphcare-ops.azurewebsites.net/signin)

A one-page sheet with the same logins is here: [`raphcare-demo-accounts-v2.pdf`](raphcare-demo-accounts-v2.pdf). Keep that sheet private. It has the password.

## 5. Links

- Portal: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- Ops: [https://raphcare-ops.azurewebsites.net](https://raphcare-ops.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)
- Public site: [raphcare.com](https://raphcare.com)

## 6. Partner checklist

The partner status board is updated for this work. Rough rollup:

| Area | % |
|------|--:|
| Hospital admin (outpatient) | 100% |
| Inpatient | 100% |
| Patient phone app | 61% |
| Other staff and system | 93% |
| Hospital plan extras (stay and counter) | 100% |
| **Overall** | **83%** |

## Try this week

1. Install `RaphCare-v1.8.4+16.apk` and sign in as `demo.patient@raphcare.com`.
2. Open Home. Confirm Upcoming is a real booking or a clear empty state (not a sample doctor). Confirm the wellness tip matches your mood check-ins, or asks you to log one.
3. Open Profile, Active clinic. Confirm **Hospitals linked to me** shows the demo clinic when linked. Tap it to set Active.
4. On a phone browser, open Portal or Ops, sign in with email and password, then Add to Home Screen. Open the shortcut and confirm sign-in still works.
