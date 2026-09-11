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

**Patient app session rule (current):** Daily Connect stays **Plugin.BLE** (**1.8.42**). Probe **1.8.46** adds an engineer-only **vendor-native scan** path (`UseVeepooNativeScanProbe`: Veepoo `startScanDevice` then `connectDevice`, no Plugin.BLE scan/GATT for that session). Hybrid exclusive Connect (`PreferExclusiveVendorSession`) stays **off** after five dual-stack crashes. Crash breadcrumbs are consumed on **Devices and Watch readings** (shared `TryConsumeVendorConnectCrashMessage`) so auto-reconnect cannot wipe the step code.

### What public sources say (working pattern)

Official HBand / Veepoo docs and sample ([HBandSDK/Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK)):

1. Use **only** `VPOperateManager` for scan and connect (`startScanDevice` then `connectDevice`). Do not run a second BLE stack in parallel.
2. Prefer `getMangerInstance(getApplicationContext())`.
3. Wait for notify success before `confirmDevicePwd` / `syncPersonInfo`.
4. Device does not support overlapping async operations.

We found no public MAUI app that mixes **Plugin.BLE Scan** with Veepoo `connectDevice` successfully. Dual-stack (Plugin.BLE Scan + Veepoo Connect) crashed across probes **1.8.39** through **1.8.45**. Probe **1.8.46** removes Plugin.BLE from the radio path for the diagnostic session only.

Crash capture (when USB works): `scripts/Capture-RaphCareAndroidLogcat.ps1`. Prefer wireless adb when Huawei MTP is flaky. Primary signal: on-device breadcrumb (`IVendorConnectStepProbe` / `FileVendorConnectStepProbe`) on Devices **or** Watch readings after relaunch. Secondary: tombstone / logcat.

Stable Connect checkpoint: **1.8.42**. Hybrid exclusive probes: **1.8.43** through **1.8.45**. Single-stack scan probe: **1.8.46** (button CanExecute fix **1.8.47**).

### Follow-up: Veepoo single-stack scan probe (1.8.46)

1. Install `RaphCare-v1.8.46+58.apk`. Keep `RaphCare-v1.8.42+54.apk` for rollback.
2. Do **not** run Plugin.BLE Scan in the same session as the diagnostic.
3. Devices → **Try vendor scan (diagnostic)**.
4. If the process dies, reopen Devices or Watch readings and read the red step (`SCAN-*` or `CONNECT-*`).
5. If Connect survives, try Measure on Watch readings.
6. Report the step code or Measure result; turn `UseVeepooNativeScanProbe` off for partner builds until Scan→Connect is proven.

`PreferExclusiveVendorSession` remains **false**. Do not re-enable hybrid Connect permutations.

Inspect log / breadcrumb for:

1. `SCAN-INVOKE` without `SCAN-RESULT` → native scan abort.
2. `CONNECT-2` / `CONNECT-INVOKE` without `CONNECT-3` → `connectDevice` abort after vendor scan.
3. `HANDSHAKE-OK` then Measure sample → single-stack path works.

#### Init audit findings (`vpprotocol-2.3.81.15` javap)

1. **`getMangerInstance(Context)`** (vendor spelling) is the full startup path: sets `mContext`, builds Inuker `BluetoothClient` (`vp_dm`), runs `vp_g()`. Bare **`getInstance()`** creates the manager **without** `BluetoothClient`. Both `isDeviceConnected` and `connectDevice` then dereference `vp_dm` with **no null check** (NPE risk, process can die).
2. Instance **`init(Context)`** is a no-op when `mContext` is already set. Call `init` only when the client is still missing after obtain.
3. The wiki **3-arg** `connectDevice(mac, IConnectResponse, INotifyResponse)` only forwards to the synchronized **4-arg** form with device name **`"none"`**. Prefer **mac+name** with the advertised watch name.
4. Scan APIs: `startScanDevice(SearchResponse)`, `startScanDevice(int, SearchResponse)`, `stopScanDevice()`. Search callbacks: `onSearchStarted`, `onDeviceFounded(SearchResult)`, `onSearchStopped`, `onSearchCanceled`.
5. Inuker binds **`com.inuker.bluetooth.library.BluetoothService`** on first connect. Declared in the app manifest.

#### Capture checklist for probe 1.8.46

1. Install `RaphCare-v1.8.46+58.apk`. Keep `RaphCare-v1.8.42+54.apk` for rollback.
2. Optional wireless adb: `adb tcpip 5555` then `adb connect <phone-ip>:5555`, then `scripts/Capture-RaphCareAndroidLogcat.ps1`.
3. Devices → **Try vendor scan (diagnostic)** (not the normal Scan button).
4. If Connected, Watch readings → Measure.
5. On crash: note the in-app step code; optionally `adb pull /data/tombstones/` after relaunch.

Hardening already in the bridge: prefer `getMangerInstance`, verify `BluetoothClient`, INIT/SCAN/CONNECT markers, Devices appear warm-up (init only), mac+name `connectDevice`, shared crash-probe consume on Devices and Watch readings.

The on-watch Heart Rate screen (for example **071 bpm**) does not broadcast proprietary live HR to Plugin.BLE. Phone Measure needs a successful Veepoo session.

---

## Suggested next engineering steps

1. Run probe 1.8.46 single-stack scan on hardware; use in-app breadcrumb as primary evidence. Desk steps: **`docs/checklist/sources/Wearable_Hardware_Proveout.md`**.
2. If single-stack survives, gate Measure for partners and retire hybrid exclusive flags permanently.
3. If single-stack still aborts, narrow to AAR/JNI packaging (dual-stack falsified).
4. Expand bridge methods for activity, sleep, and stress (catalog Phase 3).
5. Background / auto sync (`docs/14` Phase 2).
6. iOS HBand SDK binding + shared abstraction.

---

## License

HBand SDK repositories on GitHub are published under **Apache 2.0**. Keep license notices if you redistribute or embed binaries in the app package.
