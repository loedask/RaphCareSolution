# HBand SDK (E580 / E585–class bands) — integration guide

The OEM ecosystem behind many **E580 / E585** bracelets is often documented through the public **[HBandSDK](https://github.com/HBandSDK)** GitHub organization. For **commercial packages** (which patients receive) and how **Y6 Pro** differs from BLE fleet, see **`docs/13_Patient_Device_Packages_and_Fleet.md`**. RaphCare can use **two** approaches:

| Approach | Use when |
|----------|----------|
| **A. Generic BLE (current default)** | [`Plugin.BLE`](https://github.com/dotnet-bluetooth-le/dotnet-bluetooth-le) — scan, GATT connect, subscribe to notifies. Works without vendor JARs; may **not** decode proprietary payloads or may miss steps like **device password** (`confirmDevicePwd`) required by the band. See **`docs/11_Devices_BLE_E580_E585.md`**. |
| **B. Vendor HBand Android SDK** | Official **Java/Kotlin** SDK: [`VPOperateManager`](https://github.com/HBandSDK/Android_Ble_SDK/blob/master/README_EN.md) — scan, connect, **wait for `bleNotifyResponse`**, then **`confirmDevicePwd`** → **`syncPersonInfo`**, then health data APIs. Required for full compatibility on many HBand firmwares. |

**iOS:** Use **[HBandSDK/iOS_Ble_SDK](https://github.com/HBandSDK/iOS_Ble_SDK)** (Objective-C / Swift sources under `iOS_sdk_source/`). MAUI integration would use a **native binding** or **iOS binding library** — not wired in RaphCare yet.

---

## Official repositories (Apache 2.0)

| Resource | URL |
|----------|-----|
| Organization | [HBandSDK on GitHub](https://github.com/HBandSDK) |
| Android SDK | [Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK) — README: [English](https://github.com/HBandSDK/Android_Ble_SDK/blob/master/README_EN.md) |
| iOS SDK | [iOS_Ble_SDK](https://github.com/HBandSDK/iOS_Ble_SDK) |
| WeChat mini-program | [WeChat_Mini_Program_Ble_SDK](https://github.com/HBandSDK/WeChat_Mini_Program_Ble_SDK) (out of scope for native MAUI) |

---

## What the Android SDK expects (summary)

From **[README_EN.md](https://github.com/HBandSDK/Android_Ble_SDK/blob/master/README_EN.md)**:

1. **Dependencies:** `vpbluetooth` (AAR or JAR per release), **Gson**, and **`vpprotocol`** (see `android_sdk_source/jar_core/` — e.g. `vpprotocol-2.x.x.aar`).
2. **Flow:** All operations go through **`VPOperateManager.getMangerInstance()`** (use **`ApplicationContext`** to avoid leaks).
3. **Order:** **`connectDevice()`** → after **`bleNotifyResponse`** succeeds → **`confirmDevicePwd()`** (often default **`"0000"`**) → **`syncPersonInfo()`** → then heart rate / steps / etc.
4. **Concurrency:** The SDK docs warn **not** to run multiple long operations at once — serialize calls.
5. **Manifest:** Declares **`com.inuker.bluetooth.library.BluetoothService`** and (for DFU) additional services/activities — follow the Demo under `android_sdk_source/Demo/` when you enable native SDK.

Binary artifacts are under:

- [`android_sdk_source/jar_base/`](https://github.com/HBandSDK/Android_Ble_SDK/tree/master/android_sdk_source/jar_base) — e.g. `vpbluetooth-1.18.aar`, `gson-2.2.4.jar`
- [`android_sdk_source/jar_core/`](https://github.com/HBandSDK/Android_Ble_SDK/tree/master/android_sdk_source/jar_core) — e.g. `vpprotocol-2.3.48.15.aar` (large; required for protocol features)

---

## RaphCare repo: optional Android binaries (not committed)

1. Run **`tools/download-hband-android-libs.ps1`** from the solution root (requires network). This copies the **minimum** set used by `RaphCare.Mobile.csproj` when files exist:
   - `gson-2.2.4.jar`
   - `vpbluetooth-1.18.aar`
   - `vpprotocol-2.3.48.15.aar`
2. Files land in **`RaphCare.Mobile/Platforms/Android/libs/`**. That folder’s **`*.aar` / `*.jar`** are **gitignored** so binaries are not stored in git; only **`README.md`** is tracked.
3. Rebuild **Android**. MSBuild references those libraries **only if** the files exist (see `RaphCare.Mobile.csproj`).

**Embedding AARs alone does not call the SDK from C#.** You still need one of:

- **.NET Android binding library** generated from the AARs (recommended for maintainability), then reference it from the MAUI app; or  
- **JNI / `Java.Lang.Reflect`** bridge (possible but fragile — similar tradeoffs to the Agora JNI approach).

Until a binding exists, **`WearableBleCoordinator`** (Plugin.BLE) remains the **implemented** path in C#.

---

## Suggested next engineering steps (HBand path)

1. Add a **`RaphCare.Android.HBand.Binding`** (or similar) **Android binding project** targeting `vpbluetooth` + `vpprotocol` + Gson, exposing `VPOperateManager` to C#.
2. Implement **`IHBandWearableSession`** (or extend **`IWearableBleCoordinator`**) on Android only: mirror the SDK sequence (connect → notify → password → sync person → read data).
3. Merge **AndroidManifest** fragments required by the SDK (Bluetooth service, DFU activities only if you ship OTA).
4. For **iOS**, clone/bind **`iOS_Ble_SDK`** separately and expose a shared abstraction from **`RaphCare.Mobile`**.

---

## License

HBand SDK repositories on GitHub are published under **Apache 2.0** (see each repo’s `LICENSE`). Keep license notices if you redistribute or embed binaries in your app package.
