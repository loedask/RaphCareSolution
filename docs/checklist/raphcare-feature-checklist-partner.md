# RaphCare progress (partner view)

This is the **simple** status board. Same topics as the engineering checklist, without code names.

PDF: [`raphcare-feature-checklist-partner.pdf`](raphcare-feature-checklist-partner.pdf)

Engineering detail (APIs, files, checkboxes for builders): [`raphcare-feature-checklist.md`](raphcare-feature-checklist.md)

**Overall completion:** **74%**

**How to read this**

- **Done** = 100% of that row
- **Partial** = 50%
- **Not started** = 0%
- **Not for mobile** / **Not for clinical v1** / **Not for BLE** = left out of the % on purpose
- Section % = average of that section's scored rows
- Overall % = average of all scored rows in sections 1-4

Each section shows its % in the heading. Numbers stay in sync with the engineering checklist when status changes.

Last reviewed: 2026-08-09

---

## 1. Hospital admin (outpatient) - 100%

What staff do in the **web admin panel** for day-to-day clinic work (not overnight beds).

| Area | Status | What you can do |
|------|--------|-----------------|
| Hospital list, register, claim | Done | Find or register a hospital |
| Hospital profile and facilities | Done | Edit details; add physical or virtual locations |
| Staff invites and roles | Done | Invite people; set admin vs staff |
| Patients at a hospital | Done | Grant or remove access; open patient detail |
| Providers and schedules | Done | Add providers; manage schedules |
| Appointments | Done | Book, cancel, reschedule |
| Visits and vitals | Done | Start a visit; record vitals; complete |
| Video join for staff | Done | Start a tele session from admin |
| Dashboard numbers | Done | See clinic metrics |
| Web languages (English, French, Lingala, Swahili) | Done | Change language on sign-in and in the admin portal |
| Same tools inside the **patient phone app** | Not for mobile | Patients use the app; staff use the web admin |

**Bottom line (100%):** outpatient hospital admin is ready on the web.

---

## 2. Inpatient (wards, beds, admit / discharge) - 100%

Overnight stays: set up rooms and beds, put a patient in a bed, discharge them.

| Area | Status | What you can do |
|------|--------|-----------------|
| See bed board (how many free / full) | Done | Open Inpatient on a hospital in admin |
| Add ward, room, bed | Done | Admins can create capacity |
| Admit a patient to a bed | Done | Search patients and pick an available bed |
| Discharge (free the bed) | Done | End an active stay with optional notes |
| Edit or remove wards / rooms / beds later | Done | Edit, deactivate, or delete; archives if history exists |
| Mark a bed "maintenance" | Done | Toggle Available ↔ Maintenance on free beds |
| Move a patient to another bed | Done | Transfer on the active admissions board |
| Past admission history | Done | Filterable history list with load more |
| Same board in the **phone app** | Not for mobile | Unless we later build a clinician app |

**Bottom line (100%):** inpatient lifecycle is complete in admin web (capacity, maintenance, transfer, history).

---

## 3. Patient phone app - 58%

What patients see on Android / iPhone.

### Screens and features

| Area | Status | Notes |
|------|--------|-------|
| Sign up / sign in (email, phone code, voice) | Done | |
| Home and navigation | Done | Some areas can be turned off with feature flags |
| Appointments (list, book, detail) | Done | |
| Care / request a call / join video | Partial | Video call works on **Android**; **iPhone video** still needs more setup |
| Health records | Done | |
| Insurance | Done | |
| Billing / payment methods | Done | |
| Family members | Done | |
| Mental health (mood check-in) | Done | |
| Chat assistant | Partial | Works; real smart replies need cloud AI keys in production |
| Notifications list | Done | Real push alerts need Firebase / store setup |
| Settings and profile | Done | |

### Phones and devices (broader coverage)

| Area | Status | Notes |
|------|--------|-------|
| Run on **Android** phones | Done | Main day-to-day test target |
| Host API + admin web for remote Android testers | Partial | API and admin web App Services exist; Linux web publish uses a small host project; finish deploy smoke and Play invite |
| Play Store internal test package id | Partial | Android id set to a stable Yindula id; upload via Play Console still needed |
| Run on **iPhone** | Partial | App can target iOS; full release checks still open |
| Same feature checked on **both** Android and iPhone before "ready" | Not started | Use the release checklist when locking a release |
| Video visit on a real Android phone | Partial | Wiring is in; confirm on hardware |
| Video visit on a real iPhone | Not started | Needs iOS video kit wiring |
| Push alerts on Android | Partial | Needs Firebase configured on the server |
| Push alerts on iPhone | Partial | Needs store / push setup + a real device test |
| Connect **E580 / E585** style Bluetooth bands | Partial | Scan includes ET580 / ET585 labels; heart rate / oxygen only when the band speaks standard Bluetooth health profiles |
| Live heart rate and blood oxygen in the app | Partial | Android vendor path wired; confirm on your ET580 / ET585 samples |
| Auto sync and background monitoring | Not started | Manual sync exists; background sync not finished |
| Full band health history (like the vendor companion app) | Partial | Connect and live HR/SpO₂ started; activity/sleep/history still open |
| Activity (steps, calories, distance), sleep, stress | Not started | On the watches; not in RaphCare yet |
| Body temp, ECG, glucose-style screens from the band | Not started | Documented; clinical use of optical glucose stays gated |
| Weather and lifestyle pushes to the watch | Not for clinical v1 | Watch needs an app for weather; not a patient vital |
| Full vendor band SDK (HBand) on Android | Partial | Download libs + app bridge in place; hardware prove-out next |
| Full vendor band SDK on iPhone | Not started | Android first |
| **Y6 Pro** emergency 4G watch flow | Partial | Webhook + SMS + **clinic patient chart** + hospital **Devices** emergency board; OEM mapping / push alerts still future |
| Windows PC build of the app | Not for BLE | Bluetooth patient devices are for phones, not the Windows target |

**Wearable feature list (survives changing watch models):** `docs/14_Wearable_Capability_Catalog.md`

**Bottom line (58%):** the patient app screens are largely there. Catch-up work is mostly **iPhone video**, **real push**, **wearables depth** (live vitals, full sync, background), and **testing the same flows on both phone types**.

---

## 4. Other staff / system pieces - 80%

| Area | Status | Notes |
|------|--------|-------|
| Core clinical and patient APIs | Done | Power the app and admin |
| FHIR export | Done | For interoperability |
| Emergency webhook | Done | Integration hook |
| Staff mental-health assessment store | Partial | List exists; deeper storage later |
| Reporting / big dashboards | Partial | Grows with product needs |

**Bottom line (80%):** core APIs and hooks are in; deeper mental-health storage and reporting grow with product needs.

---

## Rollup

| Area | % | Notes |
|------|--:|-------|
| 1. Hospital admin (outpatient) | 100% | Ready on web |
| 2. Inpatient | 100% | Ready on web |
| 3. Patient phone app | 58% | iPhone video, push, wearables |
| 4. Other staff / system | 80% | Storage and reporting polish |
| **Overall** | **74%** | Excludes "not for mobile / clinical / BLE" rows |

---

## What to try this week (partner)

1. **Admin web:** open a hospital -> Inpatient. Add a ward/room/bed if needed, admit someone, discharge them.
2. **Admin web:** book an appointment and start a visit (outpatient path).
3. **Patient app on Android:** sign in, open Home, try Appointments and Devices (Bluetooth band if you have one).
4. **Patient app on iPhone (if available):** same smoke test; note anything that fails (especially video).

---

## Related reading

| Doc | Who it is for |
|-----|----------------|
| This file | Partners, product, demos |
| `raphcare-feature-checklist.md` | Engineers |
| [`../14_Wearable_Capability_Catalog.md`](../14_Wearable_Capability_Catalog.md) | Watch/band features RaphCare should support (even if the model changes) |
| `Mobile_Release_Ready_Checklist.md` | Release / pilot go-live |
| [`../13_Patient_Device_Packages_and_Fleet.md`](../13_Patient_Device_Packages_and_Fleet.md) | Watch / band product packages |
