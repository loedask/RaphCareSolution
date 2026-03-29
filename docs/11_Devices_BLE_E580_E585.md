# Devices & wearables — BLE (E580 / E585 class)

This document describes how **RaphCare.Mobile** integrates **Bluetooth Low Energy** for OEM bracelets commonly marketed as **E580** / **E585** (elder-care smart bands), and what is required to extend decoding or sync readings to the API.

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

E580/E585 devices are often sold under **varying Bluetooth advertisement names**. The code filters by substrings such as `E580`, `E585`, `YSC`, `Bracelet`, `SmartBand` — adjust in `E585E580DeviceFilter` when you know the exact names your fleet uses.

**Proprietary GATT:** Many bands expose **custom** services/characteristics. RaphCare subscribes to **all notify-capable** characteristics after connect. If the device exposes **standard Heart Rate Measurement**, parsers in `BleGattHeartRateParser` fill **Heart rate: N bpm**; otherwise you see **Raw: &lt;hex&gt;** for engineering work (mapping OEM payloads is vendor-specific and may require the manufacturer SDK or a capture from **nRF Connect** / **Bluetooth HCI snoop**).

## Next steps (not in this slice)

- **Patient API** to upload readings (`DeviceReading` / vitals) — follow backend → NSwag → Client → Mobile (see solution layer rules).
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
