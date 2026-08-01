# RaphCare.Mobile — structure, configuration, and testing

**Run the patient app + API (demo checklist, ports, Entra, flags):** [Mobile_Demo_Launch_Guide.md](./Mobile_Demo_Launch_Guide.md).

**Hosted Azure test API + Google Play Internal (Android):** [Mobile_Android_Test_Hosting.md](./Mobile_Android_Test_Hosting.md).

## Where code goes

| Area | Use for |
|------|---------|
| **`Core/Features/<Name>/`** | Screens, feature view models, feature-only services/models. |
| **`Core/Common/`** | Reused across features: navigation, auth services, MAUI **Controls**, **ViewModels** (e.g. `BaseViewModel`), shared views. |
| **`Core/Infrastructure/`** | Composition and DI registration (`MobileServiceCollectionExtensions`), service resolution (`MobileServiceHub`) for Shell/XAML constraints. |
| **`Blazor/`** | Razor UI hosted inside **BlazorWebView** (see `BlazorHostPage`). |
| **`RaphCare.Mobile.Kernel`** | Small **net10.0** library: `AuthResult`, feature flags types—no MAUI references. Keeps logic unit-testable without pulling MAUI workloads into test projects. |
| **`RaphCare.Client`** | HTTP, API contracts, DTOs, `IAccessTokenProvider` consumption from the app—**not** duplicated in Mobile. |

## Design port (React concept)

The UI design target is the **React + Vite** app at **`C:\laragon\www\raphcare-mobile-app-concept`**. **Match its look** (colors, type, spacing, radii, shadows, layout) in MAUI—see **`docs/Mobile_Concept_Port.md`** for tokens, fonts, route mapping, and a parity checklist. Cursor: **`.cursor/rules/raphcare-mobile-concept-template.mdc`**.

## Configuration layers (order)

Loaded in `MauiProgram` for the MAUI host:

1. `appsettings.json` (optional, tracked defaults)
2. `appsettings.Development.json` (optional, **DEBUG only**)
3. .NET **User Secrets** (optional; `UserSecretsId` in `RaphCare.Mobile.csproj`)

Later sources override earlier ones for the same keys.

### Azure Entra (API + mobile)

End-to-end portal and config steps: **[Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md)**.

### User Secrets (Entra / API / flags)

```bash
dotnet user-secrets set "Entra:ClientId" "<your-client-id>" --project RaphCare.Mobile
dotnet user-secrets set "Api:BaseAddress" "https://localhost:7001/" --project RaphCare.Mobile
dotnet user-secrets set "FeatureFlags:RecordsEnabled" "true" --project RaphCare.Mobile
```

### Voice onboarding (optional)

`POST api/onboarding/voice` requires a **clinic id**. After seeding (e.g. **ClinicalSeeder**), copy the demo clinic’s `Id` from the database and set:

```bash
dotnet user-secrets set "Onboarding:VoiceRegistrationClinicId" "<guid>" --project RaphCare.Mobile
```

Optional: `Onboarding:DefaultVoiceLanguage` (default `en-ZA` in **appsettings.json**). Without a valid clinic Guid, **VoiceSubmit** shows a configuration error instead of calling the API.

### Create account (design parity)

**RegisterOptions** matches the React concept: **Email** (Entra), **Phone** (SMS OTP + API JWT), **Voice** (phone verification first, then **in-app microphone recording** with waveform-style motion, then upload). Phone/voice use **RaphCare.Client** `IOtpAuthService` / `IVoiceOnboardingService`. Recording uses **Plugin.Maui.Audio** (`IAudioManager` / `IAudioRecorder`), registered in `MauiProgram` via `AddAudio()`. Microphone permission is requested at runtime; platform manifests include the required declarations (Android `RECORD_AUDIO`, iOS/Mac `NSMicrophoneUsageDescription`, Mac Catalyst sandbox **audio-input** entitlement, Windows **microphone** capability).

**Phone OTP in development:** The API does not send real SMS yet. With **`ASPNETCORE_ENVIRONMENT=Development`**, **`SmsService`** logs a **warning** containing the full text `Your RaphCare verification code is: …` (search the API console output for **`[Development] SMS not sent. OTP for testing`**). Use that code on **Verify phone** in the app. See **`docs/06_Key_Workflows.md`** (OTP Auth Flow).

Do **not** commit production secrets. Prefer User Secrets or your pipeline’s secret store for sensitive values.

### Feature flags

- Declared in **`appsettings.json`** under `"FeatureFlags"` (see `FeatureFlagOptions.SectionName`).
- Applied at startup via `FeatureFlags.Initialize(...)` after the app is built.
- Documented inline on `FeatureFlags` in **Kernel** (remarks + User Secrets example).

## Shell pages and dependency injection

Shell `DataTemplate` pages require a parameterless constructor. Those constructors resolve view models via **`MobileServiceHub.GetRequiredService<T>()`**, which prefers `Application.Current.Handler.MauiContext.Services` and falls back to the root provider set at startup.

## Blazor hybrid sample

- **`BlazorHostPage`** hosts `wwwroot/index.html` and the root **`Blazor/Routes.razor`** component.
- From **Home**, use **Open Blazor sample UI** to navigate to the registered route (`AppNavigator.BlazorHost`).

## Localization

- Default strings live in **`Resources/Strings/AppResources.resx`** (`NeutralLanguage` **en** in the Mobile project).
- Access via **`RaphCare.Mobile.Resources.Strings.AppResources`** (uses `ResourceManager` + `CurrentUICulture`).
- Add culture-specific `.resx` files (e.g. `AppResources.es.resx`) following [.NET MAUI localization](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/localization) guidance.

## Testing

- **`RaphCare.Mobile.Tests`** targets **net10.0** and references **`RaphCare.Mobile.Kernel` only** (avoids MAUI Resizetizer when running `dotnet test`).
- Run: `dotnet test RaphCare.Mobile.Tests/RaphCare.Mobile.Tests.csproj`

## Build quality

- Repository-wide **`Directory.Build.props`**: `AnalysisLevel=latest-recommended`.
- **`RaphCare.Mobile`**: `TreatWarningsAsErrors=true` in **Release** configurations.
