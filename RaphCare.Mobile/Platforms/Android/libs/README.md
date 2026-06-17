# HBand Android SDK binaries (optional)

This folder holds **vendor** libraries from **[HBandSDK/Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK)** (`android_sdk_source/jar_base` and `jar_core`).

- **Do not commit** `*.aar` or `*.jar` here — they are listed in `.gitignore`.
- From the **repository root**, run:

```powershell
.\tools\download-hband-android-libs.ps1
```

Then rebuild the MAUI Android target. See **`docs/12_HBand_SDK_Integration.md`** for what still needs a **binding project** before C# can call `VPOperateManager`.
