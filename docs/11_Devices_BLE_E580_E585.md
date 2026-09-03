# Devices and wearables: BLE (E580 and E585 class)

This document describes how **RaphCare.Mobile** integrates **Bluetooth Low Energy** for OEM bracelets commonly marketed as **E580** or **E585** (elder-care smart bands), and what is required to extend decoding or sync readings to the API.

**Product context:** RaphCare may ship **E585** and **E580** as part of patient packages (for example Health Track Plan). The **Y6 Pro** 4G emergency watch is a different SKU (cellular emergency backend). See **`docs/13_Patient_Device_Packages_and_Fleet.md`**.

**Vendor SDK (HBand):** Many of these bands match the **[HBandSDK](https://github.com/HBandSDK)** Android and iOS SDKs (`VPOperateManager`, password and personal-info handshake). Android Phase 1 is a JNI bridge (`HBandAndroidWearableBridge`) when the vendor AARs are present. See **`docs/12_HBand_SDK_Integration.md`**. Code references: `HBandSdkInfo` in the mobile app.

Desk hardware steps: **`docs/checklist/sources/Wearable_Hardware_Proveout.md`**.

## REST API, generated client, and GitHub

| Topic | Status |
|--------|--------|
| **Compare with GitHub** | Automated tools **cannot** read a **private** repository (for example `https://github.com/loedask/RaphCareSolution`). Use `git fetch origin` and `git log` or `git diff origin/develop` locally to compare your clone with GitHub. |
| **Staff device registry** | `GET/POST/PUT api/Devices`. **`DevicesController`**, policy **`RequireProvider`** (staff JWT, not the patient app). Returns `DeviceDto` or `PagedResult<DeviceDto>`. |
| **Patient app and BLE** | **Vertical 5b:** Patient JWT endpoints **`GET/POST api/patient/devices`**, **`POST api/patient/devices/{deviceId}/readings`**. Register serial and SKU, then batch-upload HR and SpO₂ (`Application` to `DeviceDbContext`). After **NSwag**, wire **`RaphCare.Client`** and call from MAUI (do not duplicate DTOs in Mobile). BLE pairing remains on-device via **`WearableBleCoordinator`**. |
| **RaphCare.Client (`IClient`)** | NSwag-generated methods (`DevicesGETAsync`, `DevicesGET2Async`, and similar) must match OpenAPI. **`DevicesController`** now declares **`[ProducesResponseType]`** for GET responses so the next NSwag run can emit **typed** return values (previously the spec lacked response schemas and the generated client effectively discarded JSON bodies). |
| **Regenerate NSwag** | After pulling API changes, the **repository owner** regenerates **`ClientService.cs`** per solution rules. **Do not** hand-edit the generated file. |

## Enabling the feature in the app (developers)

1. Set **`FeatureFlags:DevicesEnabled`** to **`true`** in `RaphCare.Mobile/appsettings.Development.json` (DEBUG builds merge this), or in your environment-specific config. Base `appsettings.json` may keep it `false` until you ship the vertical.
2. Build and deploy **Android**, **iOS**, or **Mac Catalyst**. BLE is not active on the **Windows** MAUI target in this solution.
3. From **Home**, open **Devices & wearables** (route `AppNavigator.Devices`). If the flag is off, navigation shows the under-construction page instead.

## Setting up the physical device (E580 and E585 class)

These bands are **BLE peripherals**. RaphCare does not replace the manufacturer's full pairing flow if your unit shipped with a **vendor app** (some models require that app once for activation, time sync, or firmware). Use the following as a practical checklist.

### Before you open RaphCare

1. **Charge the bracelet** per the vendor instructions (many use a magnetic USB dock). A very low battery can prevent advertising or connections.
2. **Wake the device.** Tap the screen or raise-to-wake if applicable so it is not in a deep sleep-only state.
3. On the **phone**, turn **Bluetooth** **On** in system settings (not only inside RaphCare).
4. **Stay within range.** Typically 1 to 3 meters for a stable first connection. Remove metal cases or interference if the scan list is empty.

### Permissions (first use)

- **Android:** When you tap **Scan**, the OS prompts for **Nearby devices** or **Bluetooth** (and on older versions, **Location** may be required for scanning). Grant **Allow**.
- **iOS and Mac:** Accept the **Bluetooth** permission when the system dialog appears (the app declares usage in `Info.plist`).

### In RaphCare: scan and connect

1. Open **Devices & wearables**.
2. Tap **Scan** and wait for the scan cycle to finish (up to about 30 seconds).
3. **Filtered list (default):** Only peripherals whose **advertised name** matches the patterns in `E585E580DeviceFilter` (for example names containing `ET580`, `ET585`, `E580`, `E585`, `YSC`, `Bracelet`, `SmartBand`) appear. If your band uses a different name, use the on-screen control to switch to **show all BLE devices**, then identify your device by name or signal strength (RSSI).
4. Tap **Connect** on the row for your device. On Android, if the vendor SDK is in the APK, the app runs the HBand handshake. Otherwise it connects over GATT and subscribes to **notifications** on all characteristics that support them.
5. **Heart rate:** Vendor live detect, or the standard **Heart Rate** service (`0x180D`) and **Heart Rate Measurement** (`0x2A37`). The UI shows **Heart rate: N bpm**. Otherwise you will see **Raw:** hex for engineering analysis.
6. **SpO₂:** Vendor live detect, or **PLX Continuous Measurement** (`0x2A60`) or **PLX Spot-check** (`0x2A5F`) under the Pulse Oximeter service (`0x1822`). The app parses **IEEE-11073 SFLOAT** SpO₂ (and optional pulse) and shows **SpO₂: N %**. Many OEM bracelets use **proprietary** characteristics instead. Then only **Raw** appears until the vendor path is active.

### If something does not work

| Symptom | What to try |
|--------|-------------|
| Nothing appears after Scan | Toggle phone Bluetooth off and on; move closer; use "all BLE" mode; confirm the band is awake and charged; on Android 12+, confirm Bluetooth permission is **Allowed**. |
| Device listed but Connect fails | Forget or unpair the device in **system Bluetooth settings** if it was paired only for classic Bluetooth; try again after restarting the band. |
| Connected but no heart rate | Many OEM bands use **custom** characteristics only. Use **Raw** output and map payloads with vendor docs or tools like **nRF Connect** (see below), or confirm the HBand AARs were packaged. |
| Works in vendor app but not RaphCare | Vendor app may use a closed protocol or require an initial bind. Complete any required step in the vendor app, then try RaphCare again. |

### Optional: engineering tools

- **nRF Connect** (mobile): Inspect advertised name, services, and UUIDs; compare with what `WearableBleCoordinator` subscribes to.
- **Android HCI snoop log** (developer options): Capture low-level BLE for protocol reverse-engineering (advanced).

## What the app does today

- **Feature flag:** `FeatureFlags:DevicesEnabled` (see `appsettings.json` / `appsettings.Development.json` and `FeatureFlagOptions`).
- **UI:** `DevicesPage`: scan, optional "show all BLE" vs **name filter** (`E585E580DeviceFilter`), connect, disconnect, last **heart rate** (vendor or standard **GATT Heart Rate** `0x2A37`) and **raw notify hex** for other characteristics.
- **Library:** [Plugin.BLE](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le) (`Plugin.BLE` NuGet) for scan. Android vendor path when AARs exist.
- **Implementation:** `WearableBleCoordinator` (`Core/Features/Devices/Services/`): singleton; `DevicesViewModel` is transient and unsubscribes on page leave.

## Platform support

| Platform | BLE |
|----------|-----|
| **Android** | Supported (`AndroidManifest` permissions for legacy + Android 12+ scan/connect). HBand JNI when libs are present. |
| **iOS / Mac Catalyst** | Supported (`NSBluetoothAlwaysUsageDescription` in `Info.plist`). Generic BLE only. |
| **Windows (WinUI)** | **Not** enabled in this build (`BlePlatform.IsSupported` is false); UI shows a short message. |

## OEM naming and protocol

E580 and E585 devices are often sold under **varying Bluetooth advertisement names**. On-device **Device Info** may show **ET580** or **ET585** while the BLE advertisement uses the same or a shortened form. The code filters by substrings such as `ET580`, `ET585`, `E580`, `E585`, `YSC`, `Bracelet`, `SmartBand`. Adjust `E585E580DeviceFilter` when you know the exact names your fleet uses.

**Proprietary GATT:** Many bands expose **custom** services and characteristics. RaphCare subscribes to **all notify-capable** characteristics after a generic connect. If the device exposes **standard Heart Rate Measurement**, parsers in `BleGattHeartRateParser` fill **Heart rate: N bpm**; otherwise you see **Raw:** hex for engineering work (mapping OEM payloads is vendor-specific and may require the manufacturer SDK or a capture from **nRF Connect** or **Bluetooth HCI snoop**).

## Next steps (not in this slice)

- **Full band, live, and background goals:** see **`docs/14_Wearable_Capability_Catalog.md`** (product contract) and **`docs/12_HBand_SDK_Integration.md`** (vendor SDK).
- Expand **patient API** beyond HR and SpO₂ batches when Domain types land (activity, sleep, and related).
- **Bonding and pairing** flows if the hardware requires it.
- **Background** delivery (platform-specific limits apply).

## Related files

| Area | Path |
|------|------|
| Hardware prove-out | `docs/checklist/sources/Wearable_Hardware_Proveout.md` |
| Coordinator | `RaphCare.Mobile/Core/Features/Devices/Services/WearableBleCoordinator.cs` |
| Name filter | `E585E580DeviceFilter.cs` |
| HR parser | `BleGattHeartRateParser.cs` |
| Page / VM | `Core/Features/Devices/Views/`, `ViewModels/` |
| Navigation | `AppNavigator.Devices` |
| Flags | `RaphCare.Mobile.Kernel/.../FeatureFlagOptions.cs` |
