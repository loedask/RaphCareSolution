<div class="doc-header">
<img src="brand/raphcare-logo.png" alt="RaphCare" />
<h1>RaphCare partner update (4 Sep 2026)</h1>
</div>

Here is what landed today. The short version: **cloud AI is on** for staging. The patient chat assistant can give a real reply, and staff can draft a discharge summary with AI. Release **1.6.0** also includes the Android crash fixes that were sitting in 1.5.1. Overall partner checklist completion is about **82%**.

## 1. Cloud AI on staging

The server now uses a low-cost Azure OpenAI model (`gpt-4.1-mini`, pay-as-you-go). You are billed for tokens when someone chats or drafts. There is no idle hourly fee. A $15 a month budget alert emails `raphcare@yindula.com`. That is an alert, not a hard shutoff.

What you can try:

- **Patient app:** open the chat assistant and send a short wellness question. You should get a real reply plus the medical disclaimer. This is not a doctor. For urgent symptoms, contact a clinician or emergency services.
- **Admin web:** on a hospital with the AI discharge toggle on, admit someone, add a vitals-only ward note, open discharge, tap Draft with AI. You should get editable draft text, not the "not connected" message. Staff must edit before save. The draft uses stay reason and ward vitals only, not free-text ward notes. Clinical text goes only to the Azure OpenAI resource we configured, not to public ChatGPT.

A hospital can still turn drafting off under Organization details.

## 2. Android app (1.6.0)

Sideload file name: `RaphCare-v1.6.0+10.apk`. It talks to the staging API, not localhost. Copy it to the phone (USB, Drive, or email). Allow install from that source if Android asks. If an older RaphCare install blocks it, uninstall the old copy first. This build is for sideload testing, not the Play Store.

This APK also keeps the 1.5.1 stability work: Book appointment (and several other screens) should stay open instead of closing the app.

A one-page phone note is here: [`mobile-update-v1.6.0.pdf`](../mobile-updates/mobile-update-v1.6.0.pdf).

## 3. Feature catalog

There is a shareable list of what RaphCare does, grouped as portal, ops, and mobile: [`raphcare-feature-catalog.pdf`](raphcare-feature-catalog.pdf). It is not a progress board. Completion % stays on the partner status sheet.

## 4. Demo clinic and accounts (unchanged)

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

A one-page sheet with the same logins is here: [`raphcare-demo-accounts.pdf`](raphcare-demo-accounts.pdf). Keep that sheet private. It has the password.

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
| Patient phone app | 61% |
| Other staff and system | 83% |
| Hospital plan extras (stay and counter) | 100% |
| **Overall** | **82%** |

## Try this week

1. Sign in as hospital admin. Open **RaphCare Demo Clinic**.
2. Admit someone (or use **Demo Inpatient**). Add a ward note with vitals. On discharge, tap Draft with AI, edit the text, then save.
3. On the phone, sign in as the demo patient. Open the chat assistant and send a short wellness question. You should get a real reply.
4. From Home, tap Book appointment. The form should open and stay open.
