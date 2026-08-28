# HBand Android SDK binaries (optional)

This folder holds **vendor** libraries from **[HBandSDK/Android_Ble_SDK](https://github.com/HBandSDK/Android_Ble_SDK)**.

- **Do not commit** `*.aar` or `*.jar` here — they are listed in `.gitignore`.
- From the **repository root**, run:

```powershell
.\tools\download-hband-android-libs.ps1
```

Then rebuild the **Android** target. C# talks to `VPOperateManager` through **`Platforms/Android/HBand/HBandAndroidWearableBridge`** (JNI, `Bind=false`), not a generated binding project. See **`docs/12_HBand_SDK_Integration.md`**.
