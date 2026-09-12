# HBand Android SDK binaries (optional)

This folder holds **vendor** libraries from **[HBandSDK/Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK)**.

- **Do not commit** `*.aar` or `*.jar` here. They are listed in `.gitignore`.
- From the **repository root**, run:

```powershell
.\tools\download-hband-android-libs.ps1
```

Then rebuild the **Android** target. C# talks to `VPOperateManager` through **`Platforms/Android/HBand/HBandAndroidWearableBridge`** (JNI, `Bind=false`), not a generated binding project. The project removes default auto-bound AARs in this folder, then re-adds them with `Bind=false`. See **`docs/12_HBand_SDK_Integration.md`**.

Maven (also required; download into this folder, do not rely on AndroidMavenLibrary for mcumgr):

```powershell
.\tools\download-hband-nordic-mcumgr-libs.ps1
```

- `mcumgr-core-2.7.4.aar` / `mcumgr-ble-2.7.4.aar` / `ble-2.11.0.aar`
- Plus Maven `no.nordicsemi.android.support.v18:scanner` 1.4.2 in the csproj

**Why local AARs for mcumgr:** on 1.9.1, `AndroidMavenLibrary` for mcumgr did not merge `classes.jar` into the APK (only R stubs). Runtime still threw `ClassNotFoundException: McuMgrBleTransport`. Explicit `AndroidLibrary` Bind=false matches the Veepoo AAR path.
