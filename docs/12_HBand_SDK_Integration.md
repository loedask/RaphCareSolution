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

**Patient app session rule (current):** Scan uses Plugin.BLE. **Connect** uses Plugin.BLE GATT. Probe **1.8.41** re-enabled exclusive Veepoo Connect after init harden and still force-closed on Scan → Connect. **Measure** stays gated off with exclusive Connect. Crash capture: `scripts/Capture-RaphCareAndroidLogcat.ps1` (look for `INIT-*` and `CONNECT-*` under `RaphCareHBand`).

Stable Connect checkpoint: tag **`v1.8.40`** / build **1.8.42** (Plugin.BLE Connect). Failed probe: **1.8.41**.

### Follow-up (blocked): Veepoo Connect and Measure

Do **not** flip `PreferExclusiveVendorSession` or `EnableVendorLiveMeasure` back on until this is fixed. Probe **1.8.41** (init harden) still closed the app after Scan → Connect.

#### Init audit findings (`vpprotocol-2.3.81.15` javap)

1. **`getMangerInstance(Context)`** (vendor spelling) is the full startup path: sets `mContext`, builds Inuker `BluetoothClient` (`vp_dm`), runs `vp_g()`. Bare **`getInstance()`** creates the manager **without** `BluetoothClient`. Both `isDeviceConnected` and `connectDevice` then dereference `vp_dm` with **no null check** (NPE risk, process can die).
2. Instance **`init(Context)`** is a no-op when `mContext` is already set. Call `init` only when the client is still missing after obtain.
3. The wiki **3-arg** `connectDevice(mac, IConnectResponse, INotifyResponse)` only forwards to the synchronized **4-arg** form with device name **`"none"`**. It is not a safer alternate native path. Prefer **mac+name** with the advertised watch name (HBand sample style) on the next exclusive attempt.
4. Inuker binds **`com.inuker.bluetooth.library.BluetoothService`** on first connect (`bindServiceSync`). The app manifest must declare it (`enabled=true`, `exported=false`). If bind fails, the library falls back to in-process `BluetoothServiceImpl`.
5. Dual-stack risk remains: Plugin.BLE Scan then Inuker `connectDevice` on the same radio. Exclusive Connect must stop scan, release Plugin.BLE GATT, then settle (`PostScanStopSettleBeforeVendorConnect`) before JNI connect.

#### Still blocked before flipping exclusive on

1. Capture logcat through `INIT-1`, `INIT-3`, `CONNECT-1`, `CONNECT-2`, `CONNECT-INVOKE`, and `CONNECT-3` on a crash build (`scripts/Capture-RaphCareAndroidLogcat.ps1`). Probe 1.8.41 failed without a partner logcat dump; next exclusive attempt needs that capture.
2. Compare proxy callbacks (`IConnectResponse.connectState(int, BleGattProfile, boolean)`, `INotifyResponse.notifyState(int)`) with live JNI delivery, and check dual-stack radio ownership after Plugin.BLE Scan.
3. Only then: exclusive Connect, then `startDetectHeart`, then heart callback, then Watch readings UI.

Hardening kept in the bridge (exclusive **off** again in 1.8.42): prefer `getMangerInstance`, verify `BluetoothClient`, INIT markers, Devices appear warm-up (init only), mac+name preference, scan-stop settle before vendor connect.

The on-watch Heart Rate screen (for example **071 bpm**) does not broadcast proprietary live HR to Plugin.BLE. Phone Measure needs a successful Veepoo session.

---

## Suggested next engineering steps

1. Unblock Veepoo Connect and Measure (follow-up section above; probe 1.8.41 failed). Desk steps: **`docs/checklist/sources/Wearable_Hardware_Proveout.md`**.
2. Expand bridge methods for activity, sleep, and stress (catalog Phase 3).
3. Background / auto sync (`docs/14` Phase 2).
4. iOS HBand SDK binding + shared abstraction.
5. Optional: generate a thin Android binding library if JNI maintenance becomes costly.

---

## License

HBand SDK repositories on GitHub are published under **Apache 2.0**. Keep license notices if you redistribute or embed binaries in the app package.
