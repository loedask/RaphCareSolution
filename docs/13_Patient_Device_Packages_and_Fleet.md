# Patient device packages & fleet (RaphCare-provided hardware)

RaphCare may supply **watches and bracelets** to patients as part of subscription or care packages. This document is the **product + engineering** reference for the three SKUs in scope for the current vertical exercise, and how the **solution** is expected to support them over time.

**BLE implementation detail:** E580 / E585-class bands — see **`docs/11_Devices_BLE_E580_E585.md`** and **`docs/12_HBand_SDK_Integration.md`**.  
**Kernel SKU constants (mobile):** `RaphCare.Mobile.Kernel` — `PatientProvisionedDeviceSkus`.

---

## Fleet overview

| # | SKU | Role | Connectivity | Primary RaphCare workflow |
|---|-----|------|--------------|---------------------------|
| 1 | **Y6 Pro** | Independent emergency health tracker | **4G** (standalone; not dependent on patient smartphone for core SOS/GPS) | **Emergency / SafeCare** — fall → alert → backend → caregiver / clinic |
| 2 | **E585** | BLE health monitoring watch | **Bluetooth LE** + RaphCare mobile app | **Remote monitoring** — vitals/activity → app → (future) **API** → clinician views |
| 3 | **E580** | Alternative BLE health bracelet (same vendor SDK family as E585) | **Bluetooth LE** + app | Same as E585 — **hardware diversity** testing (reliability, sensors, battery, BLE stability) |

---

## 1. Y6 Pro — Independent Emergency Health Tracker

**Approx. reference price:** ~$27 (planning; not binding).

**What it is:** A **4G standalone** medical tracker watch. It can operate **without** a smartphone for core emergency features.

**Key capabilities (typical):**

- 4G SIM connectivity  
- GPS tracking  
- SOS panic button  
- Fall detection  
- Emergency call  
- Voice communication  

**RaphCare workflow under test**

```
Patient falls or triggers SOS
    → Watch detects / user invokes emergency
    → Device / carrier cloud sends signal
    → RaphCare backend receives event
    → Caregiver / clinic notified
    → Emergency response coordinated
```

**Ideal users:** Elderly, chronic patients living alone, remote monitoring programs, community health, high-risk patients.

**Example commercial package**

- **RaphCare SafeCare Plan** (or “Emergency Monitoring Package”)  
- Includes: Y6 Pro emergency watch, SOS monitoring, fall detection, location tracking, emergency call routing.

### Engineering status in this repository

| Area | Status |
|------|--------|
| **Mobile BLE vertical** | **Out of scope** — Y6 Pro is **not** an E580/E585 BLE bracelet; it does not use the same in-app BLE scan/connect flow. |
| **Backend ingestion** | **Roadmap** — requires a defined channel (device OEM cloud webhook, MQTT, partner API, etc.), **patient/device identity**, **alert** entities, and **notification** paths (SMS, push, Twilio, etc.). Not implemented as an end-to-end vertical here. |
| **Documentation** | This doc locks **product intent** so API/device teams can design **`api/patient/...` or `api/devices/telemetry`** contracts later. |

---

## 2. E585 — BLE Health Monitoring Watch

**Approx. reference price:** ~$20.50.

**What it is:** A **Bluetooth** health monitoring watch/bracelet that **pairs with the RaphCare mobile app**.

**Typical sensors:** Heart rate, SpO₂, sleep, steps, activity, calories (exact set depends on firmware).

**RaphCare workflow under test**

```
Sensor reading on device
    → BLE to mobile app
    → (today) Local handling / UI in app
    → (target) Sync to RaphCare API
    → Doctor / care team dashboard / trends
```

**Ideal users:** General chronic care, fitness-oriented monitoring, hypertension/diabetes programs, lifestyle tracking.

**Example commercial package**

- **RaphCare Health Track Plan** (or “Basic Health Monitoring Package”)  
- Includes: E585 watch, HR / oxygen / activity monitoring, monthly remote check-ins (example).

### Engineering status in this repository

| Area | Status |
|------|--------|
| **Mobile** | **In progress** — `DevicesPage`, `WearableBleCoordinator` (Plugin.BLE), name filter includes **E585**; optional **HBand** native SDK path documented in **`docs/12_HBand_SDK_Integration.md`**. |
| **API** | **Patient vitals upload** — not wired yet; staff **`api/Devices`** registry is separate. See **`docs/11_Devices_BLE_E580_E585.md`**. |

---

## 3. E580 — Alternative BLE Health Monitoring Watch

**Approx. reference price:** ~$23.50.

**What it is:** Another **BLE** health bracelet, **same SDK family (HBand)** as E585.

**Why stock both E580 and E585**

- Verify **hardware reliability**, **sensor accuracy**, **battery**, and **BLE stability** across two SKUs without changing app protocol assumptions.

**Example:** Same **Health Track**-style plan; SKU may vary by region or supplier.

### Engineering status

Same as **E585** for app and API; filter and docs treat **E580** and **E585** as first-class BLE fleet targets.

---

## Cross-cutting requirements (all three)

1. **Provisioning / inventory** — Staff or operations may register **which patient received which SKU** (future: domain + API + admin UI).  
2. **Support & docs** — Patient-facing setup guides per SKU (Y6: SIM / emergency testing; E580/E585: app pairing — see **`docs/11`**).  
3. **Compliance** — Emergency and location features may require regional **telecare / medical device** review; out of scope for code comments but tracked at program level.

---

## Related documents

| Doc | Content |
|-----|---------|
| **11** | BLE E580/E585 in MAUI, permissions, troubleshooting |
| **12** | HBand SDK repos, optional AARs, binding-project next step |
| **10** | Twilio SMS (relevant for alerting workflows) |
