<div class="doc-header">
<img src="../brand/raphcare-logo.png" alt="RaphCare" />
<div>
<h1>RaphCare feature catalog</h1>
<p class="subtitle">v1 · Portal, Ops, and the patient phone app · September 2026</p>
</div>
</div>

This is the shareable list of what RaphCare does. It is grouped by who uses it:

| Surface | Who it is for |
|---------|----------------|
| **Portal** | Hospitals and clinics. Staff open this website for day-to-day work. |
| **Ops** | Platform owners. A separate site for wearable fleet stock and assignment. |
| **Mobile** | Patients. The phone app for day-to-day care. |

It is not a progress board. Completion % lives on the partner status sheet. Use this catalog when someone asks what is in the product.

<div class="note">
<p><strong>How to sell vs what you can demo.</strong> Staging shows the full hospital workspace on the Portal so a walkthrough is easy. Commercially, Clinic is the lighter outpatient set. Hospital adds beds, the counter, casualty, theatre, and the rest of the stay. The Portal headings below follow that split. Ops is not sold as a hospital plan line; it is how RaphCare (or a partner running the platform) stocks and assigns watches.</p>
</div>

## How to read status

| Word | Meaning |
|------|---------|
| **Ready** | You can walk through it today on staging. |
| **In progress** | Partly built. Do not sell it as finished. |
| **Later** | On the list. Not in the current sale. |

Last reviewed: 6 September 2026

---

## 1. Portal (hospital website)

The site hospital staff open in a browser. Public landing, sign-in, and waiting screens sit here. After staff sign in and open a site, they get the clinic or hospital workspace. Opening a site hides the left menu so the tools have more room. All hospitals, language, and sign out stay in the top bar.

Staff jobs you can invite: hospital admin, doctor, nurse, pharmacist, lab technician, and general staff.

Patients can sign in on the web too, but they only get a short welcome. The full patient product is the phone app. There is no clinician phone app in this catalog.

### Before you open a site

| Feature | What it does | Status |
|---------|--------------|--------|
| Public landing | Choose patient or healthcare professional, then sign in or create an account. | Ready |
| Professional sign-in | Staff sign in and reach the hospital list. | Ready |
| Register a professional account | Create a staff account, then register or claim a hospital. | Ready |
| Patient sign-in on the web | Patients can sign in in the browser. After that they see a short welcome, not the full chart. Point them to the phone app for day-to-day use. | Ready |
| Collection waiting screen | Open on a TV or tablet at the pharmacy or lab counter. Shows pickup codes. Does not show names or medicines. | Ready |
| Casualty waiting screen | A second TV queue for walk-ins. Shows codes and a priority colour. Does not show names. | Ready |
| Languages | English, French, Lingala, and Swahili on the website. | Ready |
| Staff use on a phone | Same Portal website on a small screen. Top bar, forms, and tables tighten for phone and tablet. Staff can add a home-screen shortcut from the browser. Not a separate staff app. | Ready |

### On every site (Clinic and Hospital)

| Feature | What it does | Status |
|---------|--------------|--------|
| Hospital list, register, claim | Find a site, register a new one, or claim one with a short RaphCare code staff can share. | Ready |
| Hospital profile and facilities | Edit site details. Add physical rooms or virtual locations. | Ready |
| Staff invites and roles | Invite people and set their job. Admins can still do hospital setup. | Ready |
| Patients at the site | Grant or remove access. Open the patient chart. | Ready |
| Patient chart | Staff can read overview, clinical, coverage, devices, and care. They do not edit the chart here. | Ready |
| Clinical team and schedules | Add clinicians. Set when they are available. | Ready |
| Appointments | Book, cancel, or reschedule. | Ready |
| Visits and vitals | Start a visit. Record vitals and notes. Order prescriptions and lab tests while the visit is open. Complete the visit. | Ready |
| Video join for staff | Start a video session from admin. | Ready |
| Dashboard numbers | See clinic metrics for the site. | Ready |
| Hospital device fleet | See watches stocked for this site, Bluetooth MAC and activated date when known, who they are assigned to, and assign an in-stock watch to a linked patient. | Ready |

### Extra on the Hospital plan

Beds and the collection counter sit here commercially. So do the rest of the stay and the busy front of house.

| Feature | What it does | Status |
|---------|--------------|--------|
| Collection counter | Search by pickup code, name, or health ID. Scan a QR from the camera. Call a code onto the waiting screen. Mark a prescription collected. Enter a lab result. Cancel or undo. Print a slip or a wall poster with a QR code. Pharmacists collect medicines. Lab technicians enter results. Other staff can still do both, including after the visit is closed. | Ready |
| Bed board | See free, full, and maintenance beds. Wards look like a simple top-down map. | Ready |
| Wards, rooms, and beds | Add capacity. Edit, deactivate, or remove later. Archive when history exists. | Ready |
| Admit and discharge | Put a person in a free bed. Discharge them to free it. | Ready |
| Move a patient | Transfer an active stay to another bed. | Ready |
| Admission history | Look up past stays. | Ready |
| Ward notes | A nurse, doctor, or hospital admin can add a note, and optional heart rate, temperature, or oxygen, while the person is in a bed. | Ready |
| Bill at discharge | Enter a nightly bed rate and any extra charge. Print that as an invoice. Mark paid in cash. | Ready |
| Occupancy numbers | Occupancy %, how many people came in or left today, and average length of stay. | Ready |
| Nurse job | Invite someone as a nurse. They can write ward notes. They cannot document an outpatient visit. | Ready |
| Casualty and triage | Add a walk-in with a priority colour. Call a code onto the casualty waiting screen. | Ready |
| Theatre list | Put today's operations on one board. Start, complete, or cancel a case. | Ready |
| SafeCare on the hospital home | Recent SOS and fall alerts show on the hospital overview and the admin home. Full history stays on Devices. | Ready |
| Outbound referrals | Log a referral out of the hospital, then mark it accepted, completed, or cancelled. | Ready |
| Return visit at discharge | On discharge, optionally pick a clinician and time for the next visit before the person leaves. | Ready |
| Who is on today | Pick a day. See who is on Morning, Afternoon, or Night. Add or remove staff for a shift. | Ready |
| Draft discharge summary with AI | On discharge, start a draft when the hospital allows it. Staff edit the text, then save. The draft uses stay reason and ward vitals only, not free-text ward notes. A hospital can turn drafting off. | Ready |

---

## 2. Ops (platform owners)

A separate website from the hospital Portal. Platform owners use it to stock wearables and assign them to patients. Hospital staff do not run day-to-day clinic work here. Patients claim the assigned serial in the phone app. They cannot invent one.

| Feature | What it does | Status |
|---------|--------------|--------|
| Sign in to Ops | Platform Ops sign-in on the Ops site. | Ready |
| Use Ops on a phone | Same Ops site on a small screen. Top bar, stock form, and fleet list tighten for phone and tablet. Staff can add a home-screen shortcut from the browser. | Ready |
| Wearable fleet stock | Capture first: scan packaging barcode or QR, take a picture in the browser, or choose a photo, then confirm hospital, serial, Bluetooth MAC, and model. The picture stays in the browser. It is not uploaded. | Ready |
| Assign to a patient | Link a stocked device to a patient so they can claim it on the phone. | Ready |
| Revoke assignment | Take a watch back to stock when the patient should no longer keep it. | Ready |
| Delete stock device | Remove a mistaken or unused stock row. Only Ops can do this. Watches with history are retired instead of erased. | Ready |
| Fleet list | Browse All, In stock, or Assigned. See Bluetooth MAC and activated date when known. Assign, revoke, or delete from the same list. | Ready |

---

## 3. Mobile (patient phone app)

Android is the day-to-day test target. iPhone can run the app. Video on iPhone is still catching up.

### Account, home, and clinic

| Feature | What it does | Status |
|---------|--------------|--------|
| Sign up or sign in | Email, phone code, or voice. Forgot password by email code, then a new password. | Ready |
| Home and navigation | Home dashboard and the main areas of the app. Upcoming visits, claimed devices, synced heart rate or oxygen, and a wellness tip from mood check-ins show when you have them. | Ready |
| Choose active clinic | Set which hospital this phone uses. Search finds hospitals in the directory; it is not a membership list. Or enter a short clinic code. | Ready |
| Settings and profile | Personal info, medical info (blood type presets; allergy and chronic chips plus Other), emergency contacts (from the phone book or typed in), privacy (honest about phone-only toggles and how to ask for deletion), help, language, and change password. Patients can add a photo from Profile or Edit Profile. Insurance badge shows the plan on file. Payment methods and billing history open the right screens. Active clinic sets which hospital this phone uses. Hospitals linked to you are listed separately from directory search. | Ready |

### Care and records

| Feature | What it does | Status |
|---------|--------------|--------|
| Appointments | List, book, and open detail. Booking uses the chosen clinic and a clinician name list. | Ready |
| Health records | Visit notes, labs, prescriptions, and discharged stays. Waiting medicines or lab tests show a pickup code and a QR code. Scan a wall poster at the counter to show only that hospital. When staff tap Call, the app says come to the counter. | Ready |
| Lab result ready | When staff enter a lab result, the patient gets a notice that it is ready. The notice does not include the values. Values stay on the health record. | Ready |
| Request a call and join video | Ask for a call and join from the phone. Video works on Android. iPhone video still needs more setup. Confirm on a real Android handset before you treat it as a live clinic tool. | In progress |
| Chat assistant | Wellness questions get a reply from the hospital's cloud AI. This is not a doctor. For urgent symptoms, contact a clinician or emergency services. | Ready |
| Notifications list | In-app notices such as pickup called and lab ready. Real lock-screen push still needs store and server setup. | In progress |
| Insurance | View and add coverage. | Ready |
| Billing and payment methods | View bills and add a payment method. Live card or mobile-money collection is not on yet. | Ready |
| Family members | List, add, and open a family member. | Ready |
| Mental health | Mood check-in and related content. The Home tip follows recent check-ins. | Ready |

### Phones, alerts, and watches

| Feature | What it does | Status |
|---------|--------------|--------|
| Android app | Build and run on Android phones. | Ready |
| iPhone app | The project can target iPhone. Full release checks and iPhone video are still open. | In progress |
| Push alerts on Android | App can register. Delivery needs Firebase on the server, then a real-device test. | In progress |
| Push alerts on iPhone | Needs store and push setup, then a real-device test. | In progress |
| Claim a watch | Patient claims the serial Ops already assigned. They can type it or photograph the packaging barcode or QR. They cannot invent a serial. Bluetooth Connect is blocked until claim, and only the locked MAC is accepted after the first pair. Leaving Devices keeps the link when the band stays nearby. | Ready |
| E580 or E585 style band | Scan and connect after claim. Open Watch readings and Measure now for heart rate and oxygen. Measure retries when the watch radio is busy after Connect. Confirm on your sample watches. | In progress |
| Watch readings screen | Dedicated screen for heart rate and oxygen, with room for more measures later. | Ready |
| Live heart rate and oxygen | Wired on Android. Hardware prove-out still needed. | In progress |
| Auto sync in the background | Manual sync exists. Background sync is not finished. | Later |
| Activity, sleep, stress | On the watches. Not in RaphCare yet. | Later |
| Body temperature, ECG, glucose-style screens | Documented. Not in the app yet. Optical glucose stays gated for clinical use. | Later |
| Y6 Pro emergency watch | SOS and fall can reach the clinic chart, the hospital Devices board, and SMS to contacts. Watch-app polish and lock-screen alerts are still open. | In progress |
| Play Store internal test | Android id is set. Upload in Play Console is still needed. | In progress |

---

## 4. Shared pieces (behind the three surfaces)

These are not a fourth product. They make Portal, Ops, and Mobile work together.

| Feature | What it does | Status |
|---------|--------------|--------|
| Staging demo clinic | Fake staff, a platform Ops login, and a patient on staging so you can walk the screens. Does not wipe other hospitals. | Ready |
| Records for other systems | Export a patient's file in a standard health format so another system can read it. | Ready |
| Emergency hook | An outside system can send an SOS or fall event into RaphCare. | Ready |
| Photos and voice files | A photo from the phone and voice sign-up audio are stored privately. | Ready |
| Staff mental-health assessments | A list exists for staff. Deeper storage comes later. | In progress |
| Reporting beyond the site dashboard | Grows with product needs. | In progress |
| Network (several sites) | Quoted. Shared patient index and group reporting switch on with that plan. | Later |

---

## 5. On the list, not in the current sale

Ideas for after the Hospital plan board. AI would still only draft or suggest. A person always checks before anything is saved. A hospital can turn AI drafting off. We are not building US insurance form-fighting, and we are not building an AI that acts as the patient's only doctor.

| Feature | What it would do | Status |
|---------|------------------|--------|
| Open work board | One place for overdue referrals, missed return visits, medicines not yet collected, and labs still waiting. | Later |
| Visit or ward note draft with AI | Start text from vitals and stay facts. Staff edit. | Later |
| Reminders for visits and pickup | In-app first. Text or call later when the site is set up for that. | Later |
| Plain-language help on labs and vitals | The phone chat explains results and band readings in simple words, and tells people to ask their clinician when unsure. | Later |
| Casualty priority hint | Suggest a colour from the complaint and vitals. Staff confirm. | Later |
| Discharge checklist on the phone | After leaving: medicines, return visit, and warning signs in plain language. | Later |
| Imaging report draft | Optional partner link for X-ray style drafts. A radiologist edits. | Later |

---

## Clinic vs Hospital (commercial reminder)

| Plan | What they get |
|------|----------------|
| Clinic | One site. Outpatient admin, appointments, visits, patient chart. Up to 8 staff. |
| Hospital | Everything in Clinic, plus beds, ward notes, collection counter, waiting screens, discharge invoice, occupancy, casualty, theatre, outbound referrals, nurse job, roster, return visit at discharge, and AI discharge draft. Up to 25 staff. |
| Network | Several sites. Quoted. Shared patient index and reporting as we switch that on. |

Patients use the phone app. Hospital staff use the Portal. Platform owners use Ops for wearable fleet. There is no clinician phone app in this catalog.

Prices sit on the separate price list.
