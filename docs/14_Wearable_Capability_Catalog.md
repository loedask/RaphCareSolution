# Wearable capability catalog (SKU-agnostic)

This document is the **product contract** for what RaphCare should support from patient wearables (health watches / bands). Capabilities are listed **by feature**, not by a single OEM model.

When hardware changes (E580 / E585 / ET580 / ET585 / future SKUs), keep this catalog. Update only:

- which SKUs claim a capability, and  
- engineering status (mobile BLE, API, clinic views).

**Related:** BLE mechanics (`docs/11_Devices_BLE_E580_E585.md`), HBand SDK path (`docs/12_HBand_SDK_Integration.md`), fleet SKUs (`docs/13_Patient_Device_Packages_and_Fleet.md`).

---

## Product goals (agreed direction)

| Goal | Meaning |
|------|---------|
| **Reliable full band data** | Match companion-app depth for in-scope metrics: live values, on-demand tests, and history sync (like H Band), without depending on the H Band app for day-to-day use. |
| **Live HR / SpO₂** | Show current heart rate and SpO₂ in the patient app while connected (and after sync when offline). |
| **Auto sync / background monitoring** | Sync stored readings when the phone is near the watch; keep a sensible background path within OS limits (Android / iOS). |

**Primary engineering path:** vendor **HBandSDK** (`VPOperateManager` handshake + health APIs) behind a RaphCare abstraction, with Plugin.BLE as a fallback for standard GATT only. See `docs/12_HBand_SDK_Integration.md`.

---

## How to read status columns

| Column | Meaning |
|--------|---------|
| **On sample watches** | Seen on current E580 / E585–class sample UI (ET580 / ET585 Device Info family). |
| **Domain** | Typed reading entity (or planned) under `RaphCare.Domain/Devices`. |
| **Patient API sync** | Patient can upload via `api/patient/devices/.../readings` (or successor). |
| **Mobile BLE** | App can obtain the value from the watch (live or history). |
| **Clinic / patient UI** | Surfaces in patient app and/or staff views. |

Statuses: **Done** / **Partial** / **Not started** / **N/A** / **Out of scope** (with note).

---

## Capability catalog

### A. Core vitals (priority 1: live + history + sync)

| Capability | On sample watches | Domain | Patient API sync | Mobile BLE | Clinic / patient UI | Notes |
|------------|-------------------|--------|------------------|------------|---------------------|-------|
| **Heart rate (live + Tap to Test + 24h history)** | Yes | `HeartRateReading` | **Partial** (batch HR points) | **Partial** (standard GATT `0x2A37` only) | **Partial** (Devices screen) | Need HBand (or decoded OEM) for reliable live + history. |
| **Blood oxygen / SpO₂ (Tap to Test + 24h)** | Yes | `PulseOximeterReading` | **Partial** (batch SpO₂) | **Partial** (standard PLX only) | **Partial** | Same as HR: proprietary path required on many firmwares. |
| **ECG + PPG modes** | Yes (ECG / PPG UI) | `ECGReading` | **Not started** | **Not started** | **Not started** | Waveform + recent bpm; medical disclaimer required. |
| **Blood pressure** | Often on HBand-class firmware (verify per SKU) | `BloodPressureReading` | **Not started** | **Not started** | **Not started** | Confirm on each sample before promising. |

### B. Activity and recovery (priority 2)

| Capability | On sample watches | Domain | Patient API sync | Mobile BLE | Clinic / patient UI | Notes |
|------------|-------------------|--------|------------------|------------|---------------------|-------|
| **Activity: steps** | Yes | **Not started** (planned entity / reading type) | **Not started** | **Not started** | **Not started** | Daily total + hourly buckets (00 / 12 / 24 graphs). |
| **Activity: calories (kcal)** | Yes | **Not started** | **Not started** | **Not started** | **Not started** | |
| **Activity: distance (km)** | Yes | **Not started** | **Not started** | **Not started** | **Not started** | |
| **Activity rings / goals** | Yes (UI rings) | **Not started** | **Not started** | **Not started** | **Not started** | Goals may be app-configured and pushed to the watch. |
| **Sleep (duration + goal)** | Yes | **Not started** | **Not started** | **Not started** | **Not started** | Sample shows duration + goal (e.g. 8.0 hr). |
| **Stress (0–100, Tap to Test, 24h)** | Yes | **Not started** | **Not started** | **Not started** | **Not started** | Consumer wellness metric; label carefully in clinic UI. |

### C. Extended body metrics (priority 3; accuracy caveats)

| Capability | On sample watches | Domain | Patient API sync | Mobile BLE | Clinic / patient UI | Notes |
|------------|-------------------|--------|------------------|------------|---------------------|-------|
| **Body temperature (°C, Tap to Test, 24h)** | Yes | **Not started** | **Not started** | **Not started** | **Not started** | Skin/ambient vs clinical core temp; document measurement context. |
| **Blood glucose (mmol/L, Tap to Test)** | Yes (watch UI) | `GlucoseReading` | **Not started** | **Not started** | **Not started** | **Non-invasive optical estimates are not clinical CGM.** Gate for research / wellness vs clinical use. |
| **Body composition** | Yes | **Not started** (or extend `WeightReading`) | **Not started** | **Not started** | **Not started** | Fat / muscle / water style estimates; OEM-specific. |
| **Blood composition / multi-analyte glance** | Yes (OEM UI) | **Not started** | **Not started** | **Not started** | **Not started** | Treat as vendor wellness bundle until validated. |
| **Health Glance (≈30s multi-indicator)** | Yes | N/A (composite trigger) | **Not started** | **Not started** | **Not started** | App or watch starts a bundle read; store component readings. |

### D. Wellness / companion features (priority 4)

| Capability | On sample watches | Domain | Patient API sync | Mobile BLE | Clinic / patient UI | Notes |
|------------|-------------------|--------|------------------|------------|---------------------|-------|
| **Breathe (1 / 2 / 3 min guided)** | Yes | **Out of scope** for clinical store (optional later) | N/A | **Not started** (optional) | Optional patient wellness | On-watch exercise; may not need server persistence. |
| **Weather on watch** | Yes (“To get weather, go to App”) | N/A | N/A | **Not started** | N/A | **Phone → watch push** (companion duty), not a patient vital. |
| **Alarms / reminders / notifications** | Typical HBand | N/A | N/A | **Not started** | Optional | Companion config; document if product wants them. |
| **Battery level** | Typical | Optional device field | **Partial** (device registry) | **Not started** | Optional | Useful for support and empty states. |

### E. Platform behaviors (cross-cutting)

| Capability | Status today | Target |
|------------|--------------|--------|
| Scan / connect / disconnect (E580 / E585 / ET580 / ET585) | **Partial** (Plugin.BLE) | Stable pairing UX |
| HBand password + person sync handshake | **Partial** (Android JNI when AARs present) | Required for full band data |
| Live vitals on Devices (and later Home) | **Partial** (HBand detect + standard GATT fallback) | Live HR + SpO₂ while connected |
| Manual “Sync readings” + offline outbox | **Partial** (HR / SpO₂ batch) | Expand payload types; auto flush |
| Background / opportunistic sync | **Not started** / unproven | Android foreground service or WorkManager-style; iOS background BLE limits documented |
| Clinic trends / alerts on band vitals | **Partial** | Thresholds + staff views for priority metrics |

---

## Sample watch UI inventory (reference)

Captured from current hardware samples (ET580 / ET585–class UI). Use this when validating a new SKU: if the screen exists, map it to a row above.

1. Heart Rate (live bpm, Tap to Test, 24h graph)  
2. Blood Oxygen (SpO₂ %, Tap to Test, 24h)  
3. ECG / PPG (modes + start)  
4. Activity (steps, kcal, km, rings, hourly graph)  
5. Sleep (hours + goal)  
6. Stress (0–100, Tap to Test, 24h)  
7. Body Temp. (°C, Tap to Test, 24h)  
8. Blood Glucose (mmol/L, Tap to Test, 24h)  
9. Body Composition (start measurement)  
10. Blood / body component OEM screen (start measurement)  
11. Health Glance (≈30s multi-indicator)  
12. Breathe (1–3 min)  
13. Weather (requires companion app push)

Device Info on samples used names **ET585** / **ET580** (see name filter in `E585E580DeviceFilter`).

---

## Delivery phases (engineering)

### Phase 1 — Live HR / SpO₂ + reliable connect

1. **Done (Android):** HBand AARs + JNI bridge (`HBandAndroidWearableBridge`): connect → notify → `confirmDevicePwd` → `syncPersonInfo` → start HR / SpO₂.  
2. Live HR + SpO₂ into `WearableVitalsSnapshot` / Devices UI when the vendor path succeeds.  
3. Plugin.BLE remains scan + GATT fallback.  
4. **Verify** on physical ET580 / ET585; then sync existing patient API batches from live samples.  
5. iOS HBand path still open.

### Phase 2 — Auto sync / background

1. Persist last readings + history windows locally.  
2. Auto-upload when online (extend `FileVitalsSyncOutbox`).  
3. Document and implement OS-realistic background: Android (preferred for fleet), iOS constraints explicit in UX copy.  
4. Battery / Bluetooth-off empty states.

### Phase 3 — Full band catalog (priority 2 then 3)

1. Activity (steps / kcal / distance / goals), Sleep, Stress.  
2. Expand Domain + patient sync DTOs + Client + Mobile (layer order).  
3. ECG / PPG, temperature, then gated glucose / composition screens.  
4. Weather + companion pushes only if product wants watch parity with H Band lifestyle features.

### Phase 4 — Clinic value

1. Staff trends for HR / SpO₂ (then sleep / activity).  
2. Threshold alerts where clinically appropriate.  
3. Clear labeling for wellness-only OEM metrics.

---

## Clinical / product guardrails

- Optical **blood glucose**, **blood composition**, and similar OEM screens are **consumer estimates** unless a regulated device and validated pathway says otherwise. Default product stance: show with a clear non-diagnostic label, or withhold from clinic charts until approved.  
- **ECG** features need appropriate disclaimers and may have regional regulatory impact.  
- Prefer storing **units and measurement method** (on-wrist optical vs cuff vs lab) with each reading when the sync model expands.

---

## When changing watches

1. Open this catalog; check each capability row against the new Device Info / UI.  
2. Update **On sample watches** / SKU notes in `docs/13`.  
3. Adjust `E585E580DeviceFilter` (or successor) for advertisement names.  
4. Re-run nRF Connect + HBand SDK compatibility; do **not** assume GATT UUIDs stay identical.  
5. Update both feature checklists under `docs/checklist/sources/` (`raphcare-feature-checklist.md` and partner twin) and regenerate PDFs.
