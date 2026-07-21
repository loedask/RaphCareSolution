# Devices & wearables — BLE (E580 / E585 class)

This document describes how **RaphCare.Mobile** integrates **Bluetooth Low Energy** for OEM bracelets commonly marketed as **E580** / **E585** (elder-care smart bands), and what is required to extend decoding or sync readings to the API.

**Product context:** RaphCare may ship **E585** and **E580** as part of patient packages (e.g. Health Track Plan). The **Y6 Pro** 4G emergency watch is a **different** SKU (cellular / emergency backend) — see **`docs/13_Patient_Device_Packages_and_Fleet.md`**.

**Vendor SDK (HBand):** Many of these bands match the **[HBandSDK](https://github.com/HBandSDK)** Android/iOS SDKs (`VPOperateManager`, password + personal-info handshake). That path is **not** fully implemented in C# yet; see **`docs/12_HBand_SDK_Integration.md`** for repositories, optional embedded AARs, and the recommended **binding-project** next step. Code references: `HBandSdkInfo` in the mobile app.

## REST API, generated client, and GitHub

| Topic | Status |
|--------|--------|
| **Compare with GitHub** | Automated tools **cannot** read a **private** repository (e.g. `https://github.com/loedask/RaphCareSolution`). Use `git fetch origin` and `git log` / `git diff origin/develop` locally to compare your clone with GitHub. |
| **Staff device registry** | `GET/POST/PUT api/Devices` — **`DevicesController`**, policy **`RequireProvider`** (staff JWT, not the patient app). Returns `DeviceDto` / `PagedResult<DeviceDto>`. |
| **Patient app + BLE** | **Vertical 5b:** Patient JWT endpoints **`GET/POST api/patient/devices`**, **`POST api/patient/devices/{deviceId}/readings`** — register serial + SKU, batch-upload HR / SpO₂ (`Application` → `DeviceDbContext`). After **NSwag**, wire **`RaphCare.Client`** and call from MAUI (do not duplicate DTOs in Mobile). BLE pairing remains on-device via **`WearableBleCoordinator`**. |
| **RaphCare.Client (`IClient`)** | NSwag-generated methods (`DevicesGETAsync`, `DevicesGET2Async`, …) must match OpenAPI. **`DevicesController`** now declares **`[ProducesResponseType]`** for GET responses so the next NSwag run can emit **typed** return values (previously the spec lacked response schemas and the generated client effectively discarded JSON bodies). |
| **Regenerate NSwag** | After pulling API changes, the **repository owner** regenerates **`ClientService.cs`** per solution rules — **do not** hand-edit the generated file. |

## Enabling the feature in the app (developers)

1. Set **`FeatureFlags:DevicesEnabled`** to **`true`** in `RaphCare.Mobile/appsettings.Development.json` (DEBUG builds merge this), or in your environment-specific config. Base `appsettings.json` may keep it `false` until you ship the vertical.
2. Build and deploy **Android**, **iOS**, or **Mac Catalyst** — BLE is not active on the **Windows** MAUI target in this solution.
3. From **Home**, open **Devices & wearables** (route `AppNavigator.Devices`). If the flag is off, navigation shows the under-construction page instead.

## Setting up the physical device (E580 / E585 class)

These bands are **BLE peripherals**. RaphCare does not replace the manufacturer’s full pairing flow if your unit shipped with a **vendor app** (some models require that app once for activation, time sync, or firmware). Use the following as a practical checklist.

### Before you open RaphCare

1. **Charge the bracelet** per the vendor instructions (many use a magnetic USB dock). A very low battery can prevent advertising or connections.
2. **Wake the device** — tap the screen or raise-to-wake if applicable so it is not in a deep sleep-only state.
3. On the **phone**, turn **Bluetooth** **On** in system settings (not only inside RaphCare).
4. **Stay within range** — typically 1–3 meters for a stable first connection; remove metal cases or interference if the scan list is empty.

### Permissions (first use)

- **Android:** When you tap **Scan**, the OS prompts for **Nearby devices** / **Bluetooth** (and on older versions, **Location** may be required for scanning). Grant **Allow**.
- **iOS / Mac:** Accept the **Bluetooth** permission when the system dialog appears (the app declares usage in `Info.plist`).

### In RaphCare — scan and connect

1. Open **Devices & wearables**.
2. Tap **Scan** and wait for the scan cycle to finish (up to ~30 seconds).
3. **Filtered list (default):** Only peripherals whose **advertised name** matches the patterns in `E585E580DeviceFilter` (e.g. names containing `ET580`, `ET585`, `E580`, `E585`, `YSC`, `Bracelet`, `SmartBand`) appear. If your band uses a different name, use the on-screen control to switch to **show all BLE devices**, then identify your device by name or signal strength (RSSI).
4. Tap **Connect** on the row for your device. The app connects over GATT and subscribes to **notifications** on all characteristics that support them.
5. **Heart rate:** If the firmware exposes the standard **Heart Rate** service (`0x180D`) and **Heart Rate Measurement** (`0x2A37`), the UI shows **Heart rate: N bpm**. Otherwise you will see **Raw: …** hex for engineering analysis.
6. **SpO₂ (when standard PLX is exposed):** If the band notifies **PLX Continuous Measurement** (`0x2A60`) or **PLX Spot-check** (`0x2A5F`) under the Pulse Oximeter service (`0x1822`), the app parses **IEEE-11073 SFLOAT** SpO₂ (and optional pulse) and shows **SpO₂: N %**. Many OEM bracelets use **proprietary** characteristics instead — then only **Raw** appears until you add a vendor parser or HBand SDK.

### If something does not work

| Symptom | What to try |
|--------|-------------|
| Nothing appears after Scan | Toggle phone Bluetooth off/on; move closer; use “all BLE” mode; confirm the band is awake and charged; on Android 12+, confirm Bluetooth permission is **Allowed**. |
| Device listed but Connect fails | Forget/unpair the device in **system Bluetooth settings** if it was paired only for classic Bluetooth; try again after restarting the band. |
| Connected but no heart rate | Many OEM bands use **custom** characteristics only — use **Raw** output and map payloads with vendor docs or tools like **nRF Connect** (see below). |
| Works in vendor app but not RaphCare | Vendor app may use a closed protocol or require an initial bind — complete any required step in the vendor app, then try RaphCare again. |

### Optional: engineering tools

- **nRF Connect** (mobile): Inspect advertised name, services, and UUIDs; compare with what `WearableBleCoordinator` subscribes to.
- **Android HCI snoop log** (developer options): Capture low-level BLE for protocol reverse-engineering (advanced).

## What the app does today

- **Feature flag:** `FeatureFlags:DevicesEnabled` (see `appsettings.json` / `appsettings.Development.json` and `FeatureFlagOptions`).
- **UI:** `DevicesPage` — scan, optional “show all BLE” vs **name filter** (`E585E580DeviceFilter`), connect, disconnect, last **heart rate** (if the device exposes standard **GATT Heart Rate** `0x2A37`) and **raw notify hex** for other characteristics.
- **Library:** [Plugin.BLE](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le) (`Plugin.BLE` NuGet).
- **Implementation:** `WearableBleCoordinator` (`Core/Features/Devices/Services/`) — singleton; `DevicesViewModel` is transient and unsubscribes on page leave.

## Platform support

| Platform | BLE |
|----------|-----|
| **Android** | Supported (`AndroidManifest` permissions for legacy + Android 12+ scan/connect). |
| **iOS / Mac Catalyst** | Supported (`NSBluetoothAlwaysUsageDescription` in `Info.plist`). |
| **Windows (WinUI)** | **Not** enabled in this build (`BlePlatform.IsSupported` is false); UI shows a short message. |

## OEM naming and protocol

E580/E585 devices are often sold under **varying Bluetooth advertisement names**. On-device **Device Info** may show **ET580** / **ET585** while the BLE advertisement uses the same or a shortened form. The code filters by substrings such as `ET580`, `ET585`, `E580`, `E585`, `YSC`, `Bracelet`, `SmartBand` — adjust in `E585E580DeviceFilter` when you know the exact names your fleet uses.

**Proprietary GATT:** Many bands expose **custom** services/characteristics. RaphCare subscribes to **all notify-capable** characteristics after connect. If the device exposes **standard Heart Rate Measurement**, parsers in `BleGattHeartRateParser` fill **Heart rate: N bpm**; otherwise you see **Raw: &lt;hex&gt;** for engineering work (mapping OEM payloads is vendor-specific and may require the manufacturer SDK or a capture from **nRF Connect** / **Bluetooth HCI snoop**).

## Next steps (not in this slice)

- **Full band + live + background goals:** see **`docs/14_Wearable_Capability_Catalog.md`** (product contract) and **`docs/12_HBand_SDK_Integration.md`** (vendor SDK binding).
- Expand **patient API** beyond HR / SpO₂ batches when Domain types land (activity, sleep, …).
- **Bonding / pairing** flows if the hardware requires it.
- **Background** delivery (platform-specific limits apply).

## Related files

| Area | Path |
|------|------|
| Coordinator | `RaphCare.Mobile/Core/Features/Devices/Services/WearableBleCoordinator.cs` |
| Name filter | `E585E580DeviceFilter.cs` |
| HR parser | `BleGattHeartRateParser.cs` |
| Page / VM | `Core/Features/Devices/Views/`, `ViewModels/` |
| Navigation | `AppNavigator.Devices` |
| Flags | `RaphCare.Mobile.Kernel/.../FeatureFlagOptions.cs` |
