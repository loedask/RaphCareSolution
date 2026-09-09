# Web and Mobile smoke plan

Plan for proving RaphCare still works after a deploy or a large change. Covers **admin Web**, **patient Mobile**, and the **shared API** both use.

Status today: plan only. No automated E2E suite yet. The engineering feature checklist row **E2E smoke script** stays open until Phase A lands.

**Related:** [`raphcare-feature-checklist.md`](raphcare-feature-checklist.md) (Cross-cutting), [`raphcare-feature-checklist-partner.md`](raphcare-feature-checklist-partner.md) (What to try this week), [`Mobile_Release_Ready_Checklist.md`](Mobile_Release_Ready_Checklist.md), [`../../09_Mobile_App_Guide.md`](../../09_Mobile_App_Guide.md), [`../../Mobile_Demo_Launch_Guide.md`](../../Mobile_Demo_Launch_Guide.md), [`../../partner-updates/raphcare-demo-accounts-v2.pdf`](../../partner-updates/raphcare-demo-accounts-v2.pdf) (sources under [`../../partner-updates/sources/`](../../partner-updates/sources/)).

Last updated: 2026-09-03

---

## Goals

1. Catch breakages early with a short, repeatable pass (not a full regression suite).
2. One API smoke that backs both Web and Mobile.
3. UI smoke for admin Web first (easier to automate).
4. Mobile: manual dual-OS path now; limited Android UI automation later.
5. Keep video, push, and wearables as separate gates where config or hardware is required.

Smoke is a **thin path**. It does not replace unit tests, release checklists, or device prove-out.

---

## Layers

| Layer | What it proves | Web | Mobile |
|-------|----------------|-----|--------|
| **A. API smoke** | Auth and main HTTP contracts against a running API | Shared | Shared |
| **B. Web UI smoke** | Admin pages load and one write or read-back works | Playwright (planned) | N/A |
| **C. Mobile smoke** | Patient app critical path on device or emulator | N/A | Manual now; Appium or MAUI UI Test later |
| **Unit / kernel** | Fast logic without a host | `RaphCare.Portal.Tests` | `RaphCare.Mobile.Tests` (Kernel only) |

Existing unit tests stay as they are. This plan does not replace `dotnet test` on Application, Client, Web, or Mobile.Kernel.

---

## Environments

| Env | Use for |
|-----|---------|
| **Local** | Developer smoke: API + Web.Host (or WASM host) + optional Android emulator |
| **Staging** | Pre-release smoke and partner "What to try" list. Demo pack accounts. |
| **Production** | Out of scope for automated smoke until product asks for a read-only health path |

Prefer Staging demo accounts when not running against a private local DB. See demo accounts doc. Default demo password is whatever `Demo:Password` is on the API (default `RaphCareDemo!2026` unless changed).

Phone OTP in Development: read the code from the API console (`[Development] SMS not sent`). See [`../../06_Key_Workflows.md`](../../06_Key_Workflows.md) and the mobile demo launch guide.

---

## Phase A - Shared API smoke (both platforms)

**Outcome:** a script or small test project that hits a running API and fails the build or CI job on first hard failure.

### Must cover

1. **Health or anonymous ping** (whatever the host already exposes).
2. **Staff email sign-in** (demo.admin or equivalent) and a token used on a protected admin call.
3. **Admin clinic board** for the demo clinic (list or detail).
4. **Inpatient slice:** list wards or admissions for that clinic (read). Optional: create then discharge only in a disposable local DB, not on shared Staging without a cleanup rule.
5. **Referral board read** for that clinic.
6. **Patient vertical:** patient email sign-in (demo.patient) and one read (health records, appointments, or collection orders).

### Rules

- Use `RaphCare.Client` shapes where practical so Web and Mobile share the same contract.
- No destructive Staging writes unless the run is tagged and cleans up (or uses a dedicated smoke clinic).
- Record base URL and which account set was used in the run log.
- When this phase ships, mark the feature checklist E2E row as partial or done per how far UI smoke has gone.

### Suggested home (when implemented)

- Prefer `scripts/` PowerShell or a small `RaphCare.Smoke.Tests` xUnit project that talks HTTP (not MAUI, not Blazor UI).
- Do not put smoke HTTP inside `RaphCare.Mobile.Tests` (Kernel-only rule).

---

## Phase B - Web UI smoke (admin)

**Outcome:** Playwright (or equivalent) against admin Web on Staging or local.

### Critical path (minimum)

1. Sign in as demo.admin.
2. Open hospital list; open **RaphCare Demo Clinic** (or configured clinic).
3. Hospital detail loads (header and section rail).
4. **Inpatient** page loads.
5. **Referrals** page loads; optional: log a referral then mark accepted or cancelled (only with cleanup policy).
6. **Collection** page loads (search UI visible).
7. Sign out or session clear.

### Stretch (same suite, separate tags)

- Casualty: add walk-in, open waiting screen token URL.
- Theatre: list today's cases.
- Emergency card on hospital overview when alerts exist.
- Language switch (en to fr) does not blank the shell.

### Rules

- Stable selectors: prefer roles, labels, and `data-testid` only if the Web app already uses them; do not invent a second design system for tests.
- Run headed locally; headless in CI when the env is up.
- Skip the job cleanly when Staging URL or secrets are missing (do not fail unrelated PRs).

---

## Phase C - Mobile smoke (patient app)

Mobile UI automation is slower and more fragile than Web. Split into three steps.

### C1 - Manual dual-OS checklist (now)

Use this as the phone half of partner "What to try," plus a few builder checks.

**Android (required for every Staging or release candidate build):**

1. Install the TestHosting or Staging-pointing build.
2. Sign in as `demo.patient@raphcare.com`.
3. Home loads without a crash.
4. Health records: waiting collection items show a pickup code when Staging has one.
5. Appointments list or empty state loads.
6. Settings or profile: Active clinic search or code entry loads (empty search does not list every hospital).
7. Devices screen opens (band connect is a separate hardware gate: [`Wearable_Hardware_Proveout.md`](Wearable_Hardware_Proveout.md)).

**iPhone (when a device or Mac build is available):**

1. Same steps 2 through 6.
2. Note video separately (see gates below).

**Optional adb check (Android):** install APK, launch main activity, confirm process stays up for a short wait. Does not replace the checklist above.

### C2 - Android UI automation (next)

- Appium or `Microsoft.Maui.Controls.Testing` against an emulator.
- Automate only 3 to 5 steps: launch, sign-in (or inject test token if product allows), Home, one vertical, Settings.
- Keep BLE, camera scan, and real Agora calls out of the default job.

### C3 - iOS UI automation (later)

- Needs a Mac agent and signed build.
- Mirror the Android critical path when the agent exists.
- Until then, C1 on a physical iPhone is enough for dual-OS smoke notes.

---

## Separate gates (not default smoke)

| Gate | Why separate |
|------|----------------|
| **Telehealth video** | Agora keys, Android vs iOS kit wiring |
| **Push** | Firebase or APNs config |
| **Wearables live vitals** | Hardware on desk. Walk [`Wearable_Hardware_Proveout.md`](Wearable_Hardware_Proveout.md) on a real E580 or E585. |
| **Play or App Store upload** | Store consoles, not API health |

Document pass or fail in release notes. Do not block Phase A on these.

---

## Suggested run order

1. `dotnet build RaphCareSolution.slnx --warnaserror`
2. Unit tests you already trust for the change area
3. **Phase A** API smoke against the target env
4. **Phase B** Web UI smoke (when the suite exists); until then, walk the partner admin steps by hand
5. **Phase C1** Mobile manual (Android always; iOS when available)
6. Separate gates only if the change touched video, push, or devices

---

## Mapping to existing checklists

| Checklist item | How this plan helps |
|----------------|---------------------|
| Engineering: E2E smoke script (admin inpatient + one patient mobile vertical) | Phase A is the first close; B and C deepen it |
| Partner: What to try this week | Manual Web + Mobile path until automation exists |
| Mobile release readiness | C1 before calling a build ready; C2 or C3 optional automation |
| Dual-OS same-feature check | C1 on both phones; tracked on partner Phones and devices table |

When Phase A ships, update both feature checklists (status, %, Last reviewed) and regenerate PDFs with `scripts/Export-RaphCareFeatureChecklistPdf.ps1`.

---

## Out of scope for v1 smoke

- Full visual polish vs locked Premium soft (see [`../../Mobile_Concept_Port.md`](../../Mobile_Concept_Port.md))
- Clinician mobile app (out of product scope)
- Load or performance testing
- Chaos or multi-tenant isolation suites
- Editing generated NSwag clients as part of smoke

---

## Implementation backlog (when work starts)

1. Choose Phase A vehicle (script vs `RaphCare.Smoke.Tests`) and wire Staging base URL via env or user secrets.
2. Implement read-mostly API checks; add guarded write tests for local only.
3. Add Playwright project for admin Web critical path; gate on secrets.
4. Paste C1 into release notes template or a short `SMOKE.md` run log if helpful.
5. Spike Appium or MAUI UI Test on Android emulator; keep the suite tiny.
6. Mark checklist rows as automation lands; keep partner wording plain.
