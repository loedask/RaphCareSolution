<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (3 Sep 2026)</h1>
</div>

Here is what landed today on staging. Short version: **Ops** (watch fleet) is live on its own site, the **Portal** admin shell is hospital-first (no platform left menu), and a hospital-list bug for the demo admin account is fixed. You do **not** need a new phone APK for this drop.

## 1. What changed

### Ops (wearable fleet)

- Staging Ops site: [https://raphcare-ops.azurewebsites.net](https://raphcare-ops.azurewebsites.net)
- Sign in with `demo.ops@raphcare.com` (same demo password as the other staging accounts).
- **Add to stock** now uses clear model choices (E585, E580, Y6 Pro) instead of a dropdown.
- Fleet work stays on Ops. It is not on the hospital Portal.

### Portal (hospital admin)

- No platform left menu. The top bar always has **All hospitals**, **Register hospital**, search, language, and sign out.
- If you only have one hospital (like the demo admin path), you land in that hospital’s dashboard. If you have more than one, you start from the hospital list.
- Opening a patient chart from Inpatient (or Collection, Casualty, and similar) and using **Back** returns you to that section with the right place highlighted.

### Fixes

- **Demo hospital list:** `demo.admin@raphcare.com` should only see **RaphCare Demo Clinic**. An unrelated hospital (for example a private test site) must not appear on that account. Ops (`demo.ops`) can still see hospitals when assigning watches.
- Searchable dropdowns close when you click outside, and they are less likely to get clipped by card edges.

## 2. Demo accounts (staging)

Shared password: **`RaphCareDemo!2026`**

| Who | Email | Where |
|-----|--------|--------|
| Hospital admin | `demo.admin@raphcare.com` | Portal |
| Doctor | `demo.doctor@raphcare.com` | Portal |
| Pharmacist | `demo.pharmacy@raphcare.com` | Portal |
| Lab | `demo.lab@raphcare.com` | Portal |
| Patient (phone) | `demo.patient@raphcare.com` | Android APK |
| Ops (fleet) | `demo.ops@raphcare.com` | Ops site |

One-page private sheet: [`raphcare-demo-accounts-v2.pdf`](../raphcare-demo-accounts-v2.pdf).

Portal staff sign-in: [Professional sign-in](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net/professional/signin)

Ops sign-in: [https://raphcare-ops.azurewebsites.net/signin](https://raphcare-ops.azurewebsites.net/signin)

## 3. Patient Android APK

No new APK with this update. Keep using the current staging build **1.5.2** (file like `RaphCare-v1.5.2+10.apk`) and the matching mobile note. Today’s work is web, Ops, and API only.

## 4. Links

- Portal: [https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net](https://raphcare-hqf6gsa3acanargz.southafricanorth-01.azurewebsites.net)
- Ops: [https://raphcare-ops.azurewebsites.net](https://raphcare-ops.azurewebsites.net)
- API: [https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net](https://raphcare-api-eydjcnefhae2dpa2.southafricanorth-01.azurewebsites.net)
- Public site: [raphcare.com](https://raphcare.com)

## 5. Partner checklist

Overall partner board remains about **81%**. Hospital admin and inpatient stay at **100%**. Phone app is unchanged by this drop.

## Try this week

1. Sign in on the Portal as hospital admin. Confirm there is no left platform menu. Confirm you only see **RaphCare Demo Clinic** on All hospitals.
2. Open Inpatient, open **Demo Inpatient**, use **Back**, and confirm you return to Active admissions with that section highlighted.
3. Sign in on Ops as `demo.ops@raphcare.com`. Open Wearable fleet. Add a serial and pick a model (E585, E580, or Y6 Pro), then assign it to the demo patient if you want.
4. On the phone, keep the existing **1.5.2** APK. No reinstall required for this web and Ops drop.

Thanks,  
Daskana
