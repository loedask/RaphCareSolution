# RaphCare progress (partner view)

This is the **simple** status board. Same topics as the builder checklist, without code names.

**Overall completion:** **76%**

**How to read this**

- **Done** = 100% of that row
- **Partial** = 50%
- **Not started** = 0%
- **Not for mobile**, **Not for clinical v1**, and **Not for BLE** = left out of the % on purpose
- **Later** notes (phone use of the web admin, and similar) are left out of the % until we schedule the work
- Section % = average of that section's scored rows
- Overall % = average of all scored rows in sections 1-5

Each section shows its % in the heading. Numbers stay in sync when status changes.

Last reviewed: 2026-09-02

---

## 1. Hospital admin (outpatient) - 100%

What staff do in the **web admin panel** for day-to-day clinic work (not overnight beds).

| Area | Status | What you can do |
|------|--------|-----------------|
| Hospital list, register, claim | Done | Find or register a hospital. Each hospital gets a short RaphCare reference staff can share. |
| Opening a hospital | Done | The left menu hides so the hospital tools have more room. Use All hospitals in the top bar to go back. Language and sign out are there too. |
| Hospital profile and facilities | Done | Edit details; add physical or virtual locations. The hospital page opens with a large header and a section menu on the side. |
| Staff invites and roles | Done | Invite people. Set staff, doctor, pharmacist, or lab technician. Admins can still do hospital setup. |
| Patients at a hospital | Done | Grant or remove access; open the patient chart |
| Patient chart (view) | Done | Same large header and section menu as other hospital pages. Sections: overview, clinical, coverage, devices, and care. Staff view this; they do not edit it here. |
| Clinical team and schedules | Done | Add clinicians; manage schedules. Clinician detail uses the same large header and section menu. |
| Appointments | Done | Book, cancel, reschedule. The appointments page uses the same large header and section menu. |
| Visits and vitals | Done | Start a visit; record vitals and notes; order prescriptions and lab tests while the visit is open; complete. Visit and tele join pages match the hospital page layout. |
| Collection counter | Done | Search by pickup code, name, or health ID. Scan a QR from the camera. Call a code onto the waiting screen. Mark a prescription collected. Enter a lab result. Cancel or undo. Print a slip or a wall poster with a QR code. Open a waiting screen on a TV or tablet (codes only, no names). The page uses the same large header and section menu as the hospital details page. Any hospital staff can do this, including after the visit is closed. Pharmacists collect medicines. Lab technicians enter lab results. Generic staff can still do both. |
| Video join for staff | Done | Start a tele session from admin |
| Dashboard numbers | Done | See clinic metrics |
| Web languages (English, French, Lingala, Swahili) | Done | Change language in the admin portal |
| Same tools inside the **patient phone app** | Not for mobile | Patients use the app; staff use the web admin |

**Bottom line (100%):** outpatient hospital admin is ready on the web, including a patient chart staff can read, visit notes doctors can add while a visit is open, a collection counter for medicines and lab tests, and a waiting screen that shows pickup codes (not names). Opening a hospital hides the left menu so staff can focus on that hospital.

**Later (not in this %):** staff should use this same web admin on a phone. Make the screens work on a small display first. A light home-screen shortcut can follow. A full offline installable web app is not the plan.

---

## 2. Inpatient (wards, beds, admit and discharge) - 100%

Overnight stays: set up rooms and beds, put a patient in a bed, discharge them.

| Area | Status | What you can do |
|------|--------|-----------------|
| See bed board (how many free or full) | Done | Open Inpatient on a hospital in admin. The page uses the same large header as hospital details, with a section menu on the side. Beds on the ward map look like simple top-down beds. |
| Add ward, room, bed | Done | Admins can create capacity |
| Admit a patient to a bed | Done | Search patients and pick an available bed |
| Discharge (free the bed) | Done | End an active stay with optional notes |
| Edit or remove wards, rooms, or beds later | Done | Edit, deactivate, or delete; archives if history exists |
| Mark a bed "maintenance" | Done | Toggle Available or Maintenance on free beds |
| Move a patient to another bed | Done | Transfer on the active admissions board |
| Past admission history | Done | Filterable history list with load more |
| Same board in the **phone app** | Not for mobile | Unless we later build a clinician app |

**Bottom line (100%):** inpatient lifecycle is complete in admin web (capacity, maintenance, transfer, history).

---

## 3. Patient phone app - 59%

What patients see on Android and iPhone.

### Screens and features

| Area | Status | Notes |
|------|--------|-------|
| Sign up or sign in (email, phone code, voice) | Done | Includes forgot password: email code, then new password |
| Home and navigation | Done | Monochrome icons (same teal tint style as the web). Failed taps should not close the app. Some areas can be turned off with feature flags |
| Appointments (list, book, detail) | Done | Book uses your clinic and a clinician name list |
| Care, request a call, join video | Partial | Video call works on **Android**; **iPhone video** still needs more setup |
| Health records | Done | Includes pickup codes and a QR code for medicines and lab tests waiting at the hospital. Scan a wall poster at the counter to show only that hospital. When staff call your code, you get a notice and Health records say come to the counter. |
| Insurance | Done | |
| Billing and payment methods | Done | |
| Family members | Done | |
| Mental health (mood check-in) | Done | |
| Chat assistant | Partial | Works; real smart replies need cloud AI keys in production |
| Notifications list | Done | Real push alerts need Firebase and store setup |
| Settings and profile | Done | Includes My clinic: search by name or enter a short clinic code |
| Choose my clinic | Done | Patients do not need Guids; use clinic name or an RC- code from the hospital |

### Phones and devices (broader coverage)

| Area | Status | Notes |
|------|--------|-------|
| Run on **Android** phones | Done | Main day-to-day test target |
| Host API and admin web for remote Android testers | Partial | API and admin web App Services exist; admin web Staging settings point at the hosted API; finish deploy smoke and Play invite |
| Play Store internal test package id | Partial | Android id set to a stable Yindula id; upload via Play Console still needed |
| Run on **iPhone** | Partial | App can target iOS; full release checks still open |
| Same feature checked on **both** Android and iPhone before "ready" | Not started | Use the release checklist when locking a release |
| Video visit on a real Android phone | Partial | Wiring is in; confirm on hardware |
| Video visit on a real iPhone | Not started | Needs iOS video kit wiring |
| Push alerts on Android | Partial | Needs Firebase configured on the server |
| Push alerts on iPhone | Partial | Needs store and push setup, then a real device test |
| Connect **E580** or **E585** style Bluetooth bands | Partial | Scan includes ET580 and ET585 labels; heart rate or oxygen only when the band speaks standard Bluetooth health profiles |
| Live heart rate and blood oxygen in the app | Partial | Android vendor path wired; confirm on your ET580 or ET585 samples |
| Auto sync and background monitoring | Not started | Manual sync exists; background sync not finished |
| Full band health history (like the vendor companion app) | Partial | Connect and live HR/SpO₂ started; activity, sleep, and history still open |
| Activity (steps, calories, distance), sleep, stress | Not started | On the watches; not in RaphCare yet |
| Body temp, ECG, glucose-style screens from the band | Not started | Documented; clinical use of optical glucose stays gated |
| Weather and lifestyle pushes to the watch | Not for clinical v1 | Watch needs an app for weather; not a patient vital |
| Full vendor band SDK (HBand) on Android | Partial | Download libs and app bridge in place; hardware prove-out next |
| Full vendor band SDK on iPhone | Not started | Android first |
| **Y6 Pro** emergency 4G watch flow | Partial | Webhook, SMS, clinic patient chart, and hospital Devices emergency board; OEM mapping and push alerts still future |
| Windows PC build of the app | Not for BLE | Bluetooth patient devices are for phones, not the Windows target |

**Wearable feature list:** we keep a separate model-agnostic list of what a band or watch should support, even when the hardware SKU changes.

**Bottom line (59%):** the patient app screens are largely there, including choosing a clinic by name or short code. Home and Profile icons are monochrome now. Catch-up work is mostly **iPhone video**, **real push**, **wearables depth** (live vitals, full sync, background), and **testing the same flows on both phone types**.

---

## 4. Other staff and system pieces - 83%

| Area | Status | Notes |
|------|--------|-------|
| Core clinical and patient APIs | Done | Power the app and admin |
| FHIR export | Done | For interoperability |
| Emergency webhook | Done | Integration hook |
| Staging demo sign-in accounts | Done | Staging only. Sign in with demo.admin, demo.doctor, demo.pharmacy, demo.lab, or demo.patient at raphcare.com. Password is on the API as Demo:Password (default RaphCareDemo!2026). Does not wipe other hospitals. |
| Staff mental-health assessment store | Partial | List exists; deeper storage later |
| Reporting and big dashboards | Partial | Grows with product needs |

**Bottom line (83%):** core APIs, hooks, and Staging demo accounts are in; deeper mental-health storage and reporting grow with product needs.

---

## 5. Hospital plan extras - 81%

What a site gets when they pay for Hospital, not Clinic. Beds and the collection counter were already in. This step is the rest of the stay: ward notes, a bill at discharge, occupancy numbers, a nurse job, a casualty queue, today's theatre list, outbound referrals, SafeCare alerts on the hospital home, and booking a return visit when someone leaves.

| Area | Status | What you can do |
|------|--------|-----------------|
| Ward notes on the stay | Done | A nurse, doctor, or hospital admin can add a note (and optional heart rate, temperature, or oxygen) while the person is in a bed. |
| Bill at discharge | Done | On discharge, enter a nightly bed rate and any extra charge. Print that as an invoice. Mark paid in cash. |
| Discharge summary on the phone | Done | The summary the hospital writes shows in the patient's Health records as a stay. |
| Occupancy numbers | Done | See occupancy %, how many people came in or left today, and average length of stay. |
| Nurse job | Done | Invite someone as a nurse. They can write ward notes. They cannot document an outpatient visit. |
| Lab result on the phone | Partial | A completed lab already shows on that visit's health record. There is no separate "your result is ready" notice yet. |
| Casualty and triage queue | Done | Add a walk-in with a priority colour. Call a code onto a second waiting screen. The TV shows codes only, not names. |
| Theatre list | Done | Put today's operations on one board. Start, complete, or cancel a case. |
| Emergency list on the hospital home | Done | Recent SOS and fall alerts show on the hospital overview and the admin home. Full history stays on Devices. |
| Follow a referral through | Done | Log an outbound referral, then mark it accepted, completed, or cancelled. |
| Book a return visit at discharge | Done | On discharge, optionally pick a clinician and time for the next visit before the person leaves. |
| Who is on today | Not started | A simple roster for the shift. |
| Draft discharge summary with AI | Not started | Staff still write it. A draft can come later. |

**Bottom line (81%):** the stay has notes, a cash invoice, occupancy, a nurse job, a casualty queue, a theatre list, referral tracking, SafeCare alerts on the hospital home, and return-visit booking at discharge. Roster and AI draft are still open.

**Later (not in this %):** staff using the same web admin on a phone. Same note as section 1.

---

## Rollup

| Area | % | Notes |
|------|--:|-------|
| 1. Hospital admin (outpatient) | 100% | Ready on web |
| 2. Inpatient | 100% | Ready on web |
| 3. Patient phone app | 59% | iPhone video, push, wearables |
| 4. Other staff and system | 83% | Demo accounts on Staging |
| 5. Hospital plan extras | 81% | Return visit at discharge in; roster and AI draft open |
| **Overall** | **76%** | Excludes "not for mobile, clinical, or BLE" rows |

---

## What to try this week (partner)

This list is the current manual smoke for admin web and the patient phone app. Builders also keep a longer layered plan (API first, then web automation, then phone) for when scripted smoke is added.

1. **Admin web:** sign in as `demo.admin@raphcare.com` (password `RaphCareDemo!2026` unless you changed `Demo:Password`). Open **RaphCare Demo Clinic**.
2. **Admin web:** the left menu should hide on a hospital. Use All hospitals in the top bar to return to the list.
3. **Admin web:** on that hospital, open Inpatient. Add a ward, room, or bed if needed, admit someone. Add a ward note. Discharge them with a summary, a nightly rate, and (optional) a return visit for next week. Confirm occupancy numbers on the hospital dashboard.
4. **Admin web:** open Casualty. Add a walk-in (with or without a patient). Call the code and open the casualty waiting screen on another tab. Open Theatre and add today's case, then start and complete it. Open Referrals, log a referral, then accept or complete it.
5. **Admin web:** on the hospital overview (and the admin home when that hospital is selected), confirm recent SOS or fall alerts show if any exist. Open Devices for the full list.
6. **Admin web:** open a patient chart. Use the section menu for overview, clinical, coverage, devices, and care.
7. **Admin web:** book an appointment and start a visit (outpatient path). While it is in progress, add a note, a prescription with more than one medicine if you want, or a lab order (not a result).
8. **Admin web:** open Collection. Search by pickup code, name, or health ID, or scan a pickup QR. Call a code onto the waiting screen. Open the waiting screen on another tab. Mark a prescription collected, or enter a lab result. Try cancel and undo. Print a slip and a wall poster.
9. **Patient app on Android:** sign in as `demo.patient@raphcare.com`. You should get a notice that something is ready to collect. Open Health records: waiting items show a pickup code and a QR code. Scan the wall poster at the counter to show only that hospital. When staff tap Call, the app should say come to the counter and you should get a notice.
10. **Patient app on Android:** open Home, try Appointments and Devices (Bluetooth band if you have one).
11. **Patient app on iPhone (if available):** same smoke test; note anything that fails (especially video).
