# RaphCare Dev Companion — Change Log

## Date

2026-03-07

## High-level summary of changes

Documentation was re-analyzed against the current codebase and updated so that the RaphCare Dev Companion knowledge files stay aligned with the solution. Updates focus on RaphCare.Mobile structure (Core/Features, Core/Shared, converters namespace), RaphCare.Client bearer-token support (IAccessTokenProvider, BearerTokenHandler), and Mobile auth/workflow details.

## New modules added

- None.

## Modules modified

- **RaphCare.Mobile:** Documented folder structure under Core/ (Core/Features: Auth, Home, Records, Appointments, Insurance, Settings; Core/Shared: Navigation/AppNavigator, Services/Auth, FeatureFlags, Views/UnderConstructionPage, Components; Core/Converters, Core/ViewModels). View namespaces RaphCare.Mobile.Features.*.Views and RaphCare.Mobile.Shared.Views. Converters namespace corrected to RaphCare.Mobile.Core.Converters (App.xaml).
- **RaphCare.Client:** Documented IAccessTokenProvider (Contracts) and BearerTokenHandler (Services/Base). AddRaphCareClient(..., useBearerToken: true) adds BearerTokenHandler and requires host to register IAccessTokenProvider.

## Database changes

- None.

## Auth changes

- **Mobile:** Documented Entra auth (Core.Shared.Services.Auth: EntraAuthOptions, IAuthService, EntraAuthService); SecureStorageAccessTokenProvider implements Client's IAccessTokenProvider; API client registered with useBearerToken: true so requests include Bearer token.

## Workflow updates

- **Mobile:** Documented auth flow (Landing → Register/Sign-in → Home), AppNavigator route registration and GoToFeatureAsync (feature-flag aware), UnderConstructionPage for disabled features.

## External integrations added/removed

- None.
