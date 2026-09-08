# HBand SDK (E580 and E585 class bands): integration guide

The OEM ecosystem behind many **E580** and **E585** bracelets is often documented through the public **[HBandSDK](https://github.com/HBandSDK)** GitHub organization. For commercial packages (which patients receive) and how **Y6 Pro** differs from BLE fleet, see **`docs/13_Patient_Device_Packages_and_Fleet.md`**. Capability goals: **`docs/14_Wearable_Capability_Catalog.md`**.

RaphCare uses **two** approaches:

| Approach | Use when |
|----------|----------|
| **A. Generic BLE** | [`Plugin.BLE`](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le): scan, GATT connect, subscribe to notifies. Fallback when HBand AARs are missing or handshake fails. See **`docs/11_Devices_BLE_E580_E585.md`**. |
| **B. Vendor HBand Android SDK (Phase 1 in app)** | Official **Java** SDK via **`VPOperateManager`**, called from C# with **JNI reflection** (`HBandAndroidWearableBridge`, same pattern as Agora). Flow: connect, then notify, then `confirmDevicePwd`, then `syncPersonInfo`, then `startDetectHeart` and `startDetectSPO2H`. |

**iOS:** Use **[HBandSDK/iOS_Ble_SDK](https://github.com/HBandSDK/iOS_Ble_SDK)**. Not wired yet (`UnavailableHBandWearableBridge` on non-Android).

---

## Official repositories (Apache 2.0)

| Resource | URL |
|----------|-----|
| Organization | [HBandSDK on GitHub](https://github.com/HBandSDK) |
| Android SDK | [Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK) (English README: [README_EN.md](https://github.com/HBandSDK/Android_Ble_SDK/blob/master/README_EN.md)) |
| iOS SDK | [iOS_Ble_SDK](https://github.com/HBandSDK/iOS_Ble_SDK) |
| API wiki | [VeepooSDK Android API Document](https://github.com/HBandSDK/Android_Ble_SDK/wiki/VeepooSDK-Android-API-Document) |

---

## What the Android SDK expects (summary)

1. **Dependencies:** `vpbluetooth`, `vpprotocol`, JieLi and abpartool companion AARs (see download script), Nordic scanner Maven artifact, AndroidX LocalBroadcastManager. **Gson** comes from the MAUI / GoogleGson graph. Do **not** embed `gson-*.jar` (D8 duplicate-type failure).
2. **Flow:** `VPOperateManager.getInstance().init(context)`, then `connectDevice(mac, name, …)`. After notify success: `confirmDevicePwd("0000", …)`, then `syncPersonInfo`, then health APIs.
3. **Concurrency:** Serialize long operations.
4. **Manifest:** `com.inuker.bluetooth.library.BluetoothService` (declared in `Platforms/Android/AndroidManifest.xml`). Use `tools:replace="android:label"` when AAR manifests conflict.

---

## RaphCare repo: enable the vendor path

1. From the solution root (network required):

```powershell
.\tools\download-hband-android-libs.ps1
```

2. Files land in **`RaphCare.Mobile/Platforms/Android/libs/`** (gitignored `*.aar` / `*.jar`).
3. Rebuild **Android**. `RaphCare.Mobile.csproj` removes default auto-bound `Platforms/Android/libs/*.aar` items, then re-adds them with **`Bind="false"`** when `vpbluetooth-1.20.aar` is present (JNI only). Without that remove step, the Android SDK binds JieLi AARs and the build fails. The protocol AAR filename currently tracks `vpprotocol-2.3.81.15.aar` from HBand `jar_core`.
4. At runtime, `WearableBleCoordinator` prefers **`IHBandWearableBridge`** when `IsAvailable` and a BLE MAC was captured from scan; otherwise Plugin.BLE GATT.

### C# surface

| Type | Role |
|------|------|
| `IHBandWearableBridge` | Shared contract (connect/handshake, live HR, live SpO₂, disconnect) |
| `HBandAndroidWearableBridge` | Android JNI implementation |
| `UnavailableHBandWearableBridge` | Non-Android / no-op |
| `WearableBleCoordinator` | Scan (Plugin.BLE) + HBand connect when available |

A full **.NET Android binding project** remains optional later for typed APIs. Phase 1 uses JNI intentionally (large `vpprotocol` AAR).

**Patient app session rule (current):** when the AARs are present, **Connect** uses an exclusive Veepoo handshake (no Plugin.BLE GATT fallback for that attempt). **Measure** only calls `startDetectHeart` on that session. Do not hand the radio from Plugin.BLE to Veepoo mid-Measure. Crash capture: `scripts/Capture-RaphCareAndroidLogcat.ps1`.

---

## Suggested next engineering steps

1. Device test on ET580 / ET585: scan, connect, confirm live HR and SpO₂ in Devices UI, then Sync readings. Desk steps: **`docs/checklist/sources/Wearable_Hardware_Proveout.md`**.
2. Expand bridge methods for activity, sleep, and stress (catalog Phase 3).
3. Background / auto sync (`docs/14` Phase 2).
4. iOS HBand SDK binding + shared abstraction.
5. Optional: generate a thin Android binding library if JNI maintenance becomes costly.

---

## License

HBand SDK repositories on GitHub are published under **Apache 2.0**. Keep license notices if you redistribute or embed binaries in the app package.
