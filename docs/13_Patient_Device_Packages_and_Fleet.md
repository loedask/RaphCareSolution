# Patient device packages and fleet (RaphCare-provided hardware)

RaphCare may supply **watches and bracelets** to patients as part of subscription or care packages. This document is the **product and engineering** reference for the three SKUs in scope for the current vertical exercise, and how the **solution** is expected to support them over time.

**BLE implementation detail:** E580 and E585-class bands. See **`docs/11_Devices_BLE_E580_E585.md`** and **`docs/12_HBand_SDK_Integration.md`**.  
**Feature contract (SKU-agnostic):** what RaphCare should support from bands (HR, SpO₂, activity, sleep, and related). See **`docs/14_Wearable_Capability_Catalog.md`**.  
**Kernel SKU constants (mobile):** `RaphCare.Mobile.Kernel`, type `PatientProvisionedDeviceSkus`.

---

## Fleet overview

| # | SKU | Role | Connectivity | Primary RaphCare workflow |
|---|-----|------|--------------|---------------------------|
| 1 | **Y6 Pro** | Independent emergency health tracker | **4G** (standalone; not dependent on patient smartphone for core SOS/GPS) | **Emergency / SafeCare:** fall or alert, then backend, then caregiver or clinic |
| 2 | **E585** | BLE health monitoring watch | **Bluetooth LE** and RaphCare mobile app | **Remote monitoring:** vitals and activity in the app, then (future) **API** and clinician views |
| 3 | **E580** | Alternative BLE health bracelet (same vendor SDK family as E585) | **Bluetooth LE** and app | Same as E585. Used for **hardware diversity** testing (reliability, sensors, battery, BLE stability) |

---

## 1. Y6 Pro: Independent Emergency Health Tracker

**Approx. reference price:** about $27 (planning; not binding).

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
    then Watch detects / user invokes emergency
    then Device / carrier cloud sends signal
    then RaphCare backend receives event
    then Caregiver / clinic notified
    then Emergency response coordinated
```

**Ideal users:** Elderly, chronic patients living alone, remote monitoring programs, community health, high-risk patients.

**Example commercial package**

- **RaphCare SafeCare Plan** (or “Emergency Monitoring Package”)  
- Includes: Y6 Pro emergency watch, SOS monitoring, fall detection, location tracking, emergency call routing.
- **List prices** (South Africa and Congo): [`partner-updates/raphcare-price-list.pdf`](partner-updates/raphcare-price-list.pdf) (edit under [`partner-updates/sources/`](partner-updates/sources/)).
- **Clinic visibility:** Admin patient chart (`/admin/hospitals/{id}/patients/{patientId}`) and hospital **Devices** tab list emergency events from `GET api/clinical/patients/{patientId}/emergency-events` and `GET api/clinical/emergency-events` (requires `X-Clinic-Id`).

### Engineering status in this repository

| Area | Status |
|------|--------|
| **Mobile BLE vertical** | **Out of scope.** Y6 Pro is **not** an E580/E585 BLE bracelet; it does not use the same in-app BLE scan/connect flow. |
| **Backend ingestion (Vertical 7)** | **Implemented (MVP).** **`POST api/integrations/standalone-emergency/events`** (anonymous + HMAC when `StandaloneEmergency:WebhookSharedSecret` is set). JSON maps device by **`serialNumber`** to **`Devices`**, requires an **active `DeviceAssignment`**, persists **`DeviceEmergencyEvents`**, sends **SMS** to patient **`EmergencyContact`** phones via **`ISmsService`** (Twilio when configured). Staff: **`GET api/clinical/patients/{patientId}/emergency-events`** and clinic board **`GET api/clinical/emergency-events`**; **admin patient chart** and hospital **Devices** tab list those events. OEM-specific payload mapping and push/in-app clinic alerts remain **future** work. |
| **Documentation** | This doc and **`docs/08_External_Integrations.md`**; configure secrets in **`StandaloneEmergency`** (see API `appsettings.Development.json` sample). |

---

## 2. E585: BLE Health Monitoring Watch

**Approx. reference price:** about $20.50.

**What it is:** A **Bluetooth** health monitoring watch/bracelet that **pairs with the RaphCare mobile app**.

**Typical sensors:** Heart rate, SpO₂, sleep, steps, activity, calories (exact set depends on firmware).

**RaphCare workflow under test**

```
Sensor reading on device
    then BLE to mobile app
    then (today) Local handling / UI in app
    then (target) Sync to RaphCare API
    then Doctor / care team dashboard / trends
```

**Ideal users:** General chronic care, fitness-oriented monitoring, hypertension/diabetes programs, lifestyle tracking.

**Example commercial package**

- **RaphCare Health Track Plan** (or “Basic Health Monitoring Package”)  
- Includes: E585 watch, heart rate, oxygen, and activity monitoring, plus monthly remote check-ins (example).
- **List prices** (South Africa and Congo): [`partner-updates/raphcare-price-list.pdf`](partner-updates/raphcare-price-list.pdf) (edit under [`partner-updates/sources/`](partner-updates/sources/)).

### Engineering status in this repository

| Area | Status |
|------|--------|
| **Mobile** | **In progress.** `DevicesPage`, `WearableBleCoordinator` (Plugin.BLE scan), Android **HBand** JNI bridge for connect and live HR/SpO₂ when AARs present (`docs/12`). |
| **API** | **Patient vitals upload.** **`api/patient/devices`** (register and **`POST …/readings`**). Regenerate **NSwag**, then extend **Client** and MAUI sync. Staff **`api/Devices`** registry remains separate. |

---

## 3. E580: Alternative BLE Health Monitoring Watch

**Approx. reference price:** about $23.50.

**What it is:** Another **BLE** health bracelet, **same SDK family (HBand)** as E585.

**Why stock both E580 and E585**

- Verify **hardware reliability**, **sensor accuracy**, **battery**, and **BLE stability** across two SKUs without changing app protocol assumptions.

**Example:** Same **Health Track**-style plan; SKU may vary by region or supplier.

### Engineering status

Same as **E585** for app and API; filter and docs treat **E580** and **E585** as first-class BLE fleet targets.

---

## Cross-cutting requirements (all three)

1. **Provisioning / inventory.** Staff or operations may register **which patient received which SKU** (future: domain + API + admin UI).  
2. **Support and docs.** Patient-facing setup guides per SKU (Y6: SIM / emergency testing; E580/E585: app pairing. See **`docs/11`**).  
3. **Compliance.** Emergency and location features may require regional **telecare / medical device** review; out of scope for code comments but tracked at program level.

---

## Related documents

| Doc | Content |
|-----|---------|
| **11** | BLE E580/E585 in MAUI, permissions, troubleshooting |
| **12** | HBand SDK repos, optional AARs, binding-project next step |
| **14** | Wearable capability catalog (features survive SKU changes) |
| **10** | Twilio SMS (relevant for alerting workflows) |
| Price list | South Africa and Congo starting prices: [`partner-updates/raphcare-price-list.pdf`](partner-updates/raphcare-price-list.pdf) |
