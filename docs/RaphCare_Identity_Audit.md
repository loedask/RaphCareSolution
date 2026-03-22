# RaphCare Identity Audit

**Date:** March 15, 2025  
**Scope:** Full repository analysis for authentication and identity architecture.  
**Note:** Analysis only; no code was modified.

> **Update (later than this audit):** Entra user provisioning is wired via **`JwtBearerEvents.OnTokenValidated`** → **`EnsureUserExistsAsync`**. API controllers use **`[Authorize(Policy = …)]`**. For current Azure setup steps, see **[Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md)**. Sections below that claim provisioning or `[Authorize]` are unused reflect the repo **at audit time**, not necessarily today.

---

## 1. Authentication System

### Framework

- **ASP.NET Core Identity:** Not used. There is no `AddIdentity<TUser, TRole>()`, no `IdentityDbContext` from ASP.NET Identity, and no `AspNetUsers` / `AspNetRoles` tables.
- **Custom identity:** Yes. The solution uses a custom identity bounded context:
  - **Provider:** Microsoft Entra ID (Azure AD). JWTs are issued by Entra and validated by the API.
  - **User store:** `ApplicationUser` is stored in a custom `IdentityDbContext` (RaphCare.Persistence). Access is via `IApplicationUserStore` (implemented by `ApplicationUserStore` in Persistence), not ASP.NET Identity stores.
- **JWT:** Yes. JWT Bearer is the only authentication scheme:
  - Registered in `RaphCare.Identity/DependencyInjection.cs` with `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`.
  - Validation uses Entra OpenID Connect metadata (Authority, ValidIssuers, Audience); signing keys are resolved from `{Authority}/.well-known/openid-configuration`.
- **Middleware:** In `RaphCare.API/Program.cs`: `UseAuthentication()` then `UseAuthorization()`, plus `TenantResolutionMiddleware` (X-Clinic-Id) and `AuditMiddleware` (request logging).

### Configuration

- **API:** `RaphCare.API/Program.cs` registers: `AddApplication` → `AddInfrastructure` → `AddPersistence` → `AddIdentity` → `AddControllers`; then `AddAuthentication`, `AddAuthorization`, Swagger.
- **Identity options:** Bound from config section `Entra` (see `EntraOptions.SectionName = "Entra"`). API `appsettings.json` has: Authority, TenantId, ClientId, Audience. No `ValidIssuers` in config; code falls back to Authority-based issuers.
- **Startup:** No `Startup.cs`; minimal hosting with `Program.cs` in API, Web, and Mobile.

---

## 2. User Models

### Identity (RaphCare.Domain.Identity)

| Model | Purpose | Key Fields |
|-------|---------|------------|
| **ApplicationUser** | Domain user synced from Entra; used for authorization and audit | Id (Guid), EntraObjectId, Email, DisplayName, IsActive, IsDeleted, CreatedAt, UpdatedAt; nav: UserRoles, UserSessions, RefreshTokenRecords, AuditLogs, LoginAudits |
| **Role** | RBAC role | Id, Name, Description, IsDeleted, CreatedAt, UpdatedAt; nav: UserRoles, RolePermissions |
| **Permission** | Fine-grained permission (e.g. Patients.Read) | Id, Code, Name, Description, IsDeleted, CreatedAt, UpdatedAt; nav: RolePermissions |
| **UserRole** | User ↔ Role join | Id, UserId, RoleId, CreatedAt, UpdatedAt; User, Role |
| **RolePermission** | Role ↔ Permission join | Id, RoleId, PermissionId, CreatedAt, UpdatedAt; Role, Permission |
| **RefreshTokenRecord** | Refresh token tracking | Id, UserId, Token, ExpiresAt, Revoked, ReplacedByToken, CreatedAt, UpdatedAt; User |
| **UserSession** | Session / JWT tracking | Id, UserId, JwtId, ExpiresAt, Revoked, IpAddress, CreatedAt, UpdatedAt; User |
| **AuditLog** | Audit trail for actions | Id, Action, EntityName, EntityId, Changes, PerformedAt, PerformedByUserId, CreatedAt, UpdatedAt; PerformedByUser |
| **LoginAudit** | Login attempt log | Id, UserId, LoginTime, IpAddress, UserAgent, Success, CreatedAt, UpdatedAt; User |
| **AccessPolicy** | Policy (required permission) | Id, Name, Description, RequiredPermissionCode, CreatedAt, UpdatedAt |

### Organization / Clinical (link to identity)

| Model | Purpose | Identity Link |
|-------|---------|----------------|
| **Patient** | Patient aggregate (Clinical) | No ApplicationUserId; has Email, PhoneNumber; separate from login identity |
| **Provider** | Clinician/doctor | ApplicationUserId (Guid) → ApplicationUser |
| **Administrator** | Clinic admin | ApplicationUserId → ApplicationUser |
| **SupportStaff** | Non-clinical staff | ApplicationUserId → ApplicationUser |
| **Therapist** | Mental-health provider | ApplicationUserId → ApplicationUser |

There is no `IdentityUser` (ASP.NET Identity); the only login identity entity is **ApplicationUser**.

---

## 3. Database Tables

### Identity database (IdentityDbContext, connection: IdentityConnection)

Created by migration `RaphCare.Persistence/Migrations/IdentityDb/20260307180140_InitialIdentity.cs`.  
`IdentityDbContext` explicitly configures only: ApplicationUser, Role, Permission, UserRole, RolePermission; AuditLog, LoginAudit, RefreshTokenRecord, UserSession are part of the same migration (model built via conventions/snapshot).

| Table | Columns |
|-------|--------|
| **ApplicationUsers** | Id (PK, uniqueidentifier), EntraObjectId (nvarchar 100, unique), Email (nvarchar 256), DisplayName (nvarchar 200), IsActive (bit), IsDeleted (bit), CreatedAt, UpdatedAt (datetime2). Indexes: IX_ApplicationUsers_Email, IX_ApplicationUsers_EntraObjectId (unique). |
| **Roles** | Id (PK), Name (nvarchar 100), Description (nvarchar 500), IsDeleted, CreatedAt, UpdatedAt. IX_Roles_Name. |
| **Permissions** | Id (PK), Code (nvarchar 100, unique), Name, Description, IsDeleted, CreatedAt, UpdatedAt. IX_Permissions_Code, IX_Permissions_IsDeleted. |
| **UserRoles** | Id (PK), UserId (FK → ApplicationUsers), RoleId (FK → Roles), CreatedAt, UpdatedAt. Unique IX_UserRoles_UserId_RoleId. |
| **RolePermissions** | Id (PK), RoleId (FK → Roles), PermissionId (FK → Permissions), CreatedAt, UpdatedAt. Unique IX_RolePermissions_RoleId_PermissionId. |
| **AuditLog** | Id (PK), Action, EntityName, EntityId, Changes, PerformedAt, PerformedByUserId (FK → ApplicationUsers, nullable), CreatedAt, UpdatedAt. IX_AuditLog_PerformedByUserId. |
| **LoginAudit** | Id (PK), UserId (FK → ApplicationUsers), LoginTime, IpAddress, UserAgent, Success, CreatedAt, UpdatedAt. IX_LoginAudit_UserId. |
| **RefreshTokenRecord** | Id (PK), UserId (FK → ApplicationUsers), Token, ExpiresAt, Revoked, ReplacedByToken, CreatedAt, UpdatedAt. IX_RefreshTokenRecord_UserId. |
| **UserSession** | Id (PK), UserId (FK → ApplicationUsers), JwtId, ExpiresAt, Revoked, IpAddress, CreatedAt, UpdatedAt. IX_UserSession_UserId. |

There are no tables named `AspNetUsers`, `AspNetRoles`, or `Users` (the DbSet is `Users` but the table is `ApplicationUsers`).

### Clinical database (relevant to identity)

- **Patients:** Id, ClinicId, FirstName, LastName, DateOfBirth, Gender, NationalIdNumber, PhoneNumber, Email, IsActive, IsDeleted, DeletedAt, etc. (no ApplicationUserId).
- **Providers, Administrators, SupportStaff, Therapists:** Each has `ApplicationUserId` (Guid) linking to ApplicationUser in the identity DB (cross-context by ID only; no FK across databases).

---

## 4. Existing Authentication Flows

### Login

- **No API login endpoint.** There is no `/auth/login`, `/account/login`, or similar. Authentication is entirely external (Entra).
- **Mobile:** User signs in via Microsoft Entra ID using MSAL in `EntraAuthService`:
  - `SignInAsync()` calls `AcquireTokenInteractive(scopes)` and stores access token (and expiry) in secure storage.
  - Token is then supplied to the API via `IAccessTokenProvider` / `BearerTokenHandler`.

### Registration

- **No API registration endpoint.** No `/auth/register` or `/account/register`.
- **Mobile:** “Create account” leads to `RegisterOptionsPage` → `RegisterEmailPage`; `RegisterEmailViewModel` calls `IAuthService.SignUpWithEmailAsync(email, password)`, which triggers Entra interactive sign-up (e.g. B2C/External ID). On success, navigation goes to `VerifyEmailPage`. No local user creation on the API at this step.

### User provisioning (intended but not wired)

- **Design:** After JWT validation, `EntraUserProvisioningService.EnsureUserExistsAsync(principal)` should create or update an `ApplicationUser` from Entra claims (oid, email, name).
- **Gap:** There is no call to `EnsureUserExistsAsync` in the API pipeline. No `JwtBearerEvents.OnTokenValidated`, no middleware, and no filter invokes it. So ApplicationUser rows are never created or updated from Entra on request. The service is registered but unused.

### Authorization (API)

- **Policies:** In `Program.cs`: `RequireAdmin` (role Administrator), `RequireProvider` (Administrator, Clinician), `RequirePatient` (Patient, Administrator, Clinician).
- **Usage:** No controller or action in the audited API code uses `[Authorize]` or `[Authorize(Policy = "...")]`. So all current API endpoints are effectively unsecured from an authorization perspective; JWT is validated but roles are not enforced on endpoints.

---

## 5. JWT Implementation

### Token generation

- **No token generation in the API.** Tokens are not issued by RaphCare. The API only validates JWTs issued by Microsoft Entra ID.
- **No local JWT creation:** No `GenerateJwt`, `TokenService`, `JwtSecurityToken`, or `SigningCredentials` for issuing tokens. `EntraTokenValidator` and `JwtSecurityTokenHandler` are used only for **validation** (e.g. in non-pipeline scenarios), not generation.

### Token validation (API)

- **Location:** `RaphCare.Identity/DependencyInjection.cs` configures `AddJwtBearer` with:
  - Authority from config (or `https://login.microsoftonline.com/{TenantId}/v2.0`).
  - Audience and ValidIssuers from config (or derived from Authority).
  - `TokenValidationParameters`: ValidateIssuer, ValidateAudience, ValidateLifetime, ValidateIssuerSigningKey; ClockSkew 2 minutes; `IssuerSigningKeyResolver` loads keys from Entra OpenID metadata.

### Mobile

- **Acquisition:** MSAL `AcquireTokenInteractive` / `AcquireTokenSilent` in `EntraAuthService`.
- **Storage:** Access token and expiry stored with `SecureStorage.Default.SetAsync("access_token", ...)` and `"expires_on"` (Shared `EntraAuthService`); Features version also has `RefreshTokenKey` and refresh handling.
- **API calls:** `SecureStorageAccessTokenProvider` implements `IAccessTokenProvider`; `BearerTokenHandler` in RaphCare.Client adds `Authorization: Bearer {token}` to requests when `useBearerToken: true`.

---

## 6. OTP / SMS

### Services

- **Interface:** `RaphCare.Application/Common/Interfaces/ISmsService.cs` — `SendSmsAsync(phoneNumber, message)`.
- **Implementation:** `RaphCare.Infrastructure/Services/SmsService.cs` — placeholder; only logs. Comment: “TODO: Integrate with Twilio, Africa's Talking, or similar.”

### Controllers / Endpoints

- **None.** No OTP, verification code, or phone verification endpoints. No references to Otp, VerificationCode, PhoneVerification, or OTP in controllers.

### Entities / Tables

- **Domain:** `SMSLog` (Communication) — PhoneNumber, Message, SentAt, Successful. Used for logging, not for OTP storage.
- **No** Otp, VerificationCode, or PhoneVerification entities or tables.

**Conclusion:** OTP and phone-based authentication are not implemented; only an SMS sending abstraction exists as a placeholder.

---

## 7. Email Authentication

### Email registration

- **API:** No email registration endpoint. No local user or account creation by email on the API.
- **Mobile:** Registration is via Entra (B2C/External ID) from `RegisterEmailViewModel`; “email registration” is Entra sign-up, then navigation to `VerifyEmailPage`.

### Login endpoints

- **API:** No login endpoint. Clients use Entra; API only validates Bearer token.
- **Mobile:** Sign-in via `SignInViewModel` → `IAuthService.SignInAsync()` (Entra interactive).

### Password reset

- **Not implemented in the repo.** No password reset endpoints, handlers, or UI. Entra would handle password reset if configured in the tenant.

### Email verification

- **API:** No email verification endpoint or token.
- **Mobile:** `VerifyEmailPage` / `VerifyEmailViewModel` — UI prompt only; “verify via Entra”; no resend or verification API in the app (resend noted as “typically done from the verification email link”).

### Email service

- **Interface:** `IEmailService` — `SendEmailAsync(to, subject, body)`.
- **Implementation:** `EmailService` in Infrastructure — placeholder (logs only). Comment: “TODO: Integrate with SendGrid, SMTP, or Azure Communication Services”.

---

## 8. Controllers / Endpoints

### API (RaphCare.API)

All controllers use `[ApiController]` and `[Route("api/[controller]")]`:

| Controller | Route prefix | Examples (from audit) |
|------------|--------------|------------------------|
| PatientsController | api/Patients | GET api/Patients/{id}, GET api/Patients, POST api/Patients, PUT api/Patients/{id} |
| AppointmentsController | api/Appointments | |
| ClinicalController | api/Clinical | |
| DevicesController | api/Devices | |
| InsuranceController | api/Insurance | |
| BillingController | api/Billing | |
| MentalHealthController | api/MentalHealth | |
| TelemedicineController | api/Telemedicine | |
| AIController | api/AI | |
| ReportingController | api/Reporting | |

**No** AuthController, AccountController, or UsersController. No routes under `/auth/`, `/account/`, or `/users/` for login, register, OTP, or verify.

### Swagger

- Swagger is configured with a Bearer (JWT) security scheme; description references “JWT Bearer token from Microsoft Entra ID.”

---

## 9. Azure Integration

| Service | Status | Notes |
|--------|--------|------|
| **Azure AD / Microsoft Entra ID** | In use | JWT validation and intended user provisioning. Config: Authority, TenantId, ClientId, Audience. Mobile uses MSAL (Microsoft.Identity.Client). |
| **Azure Communication Services** | Not used | Mentioned in comments (EmailService, TeleSessionService) as future integration; no SDK or implementation. |
| **Azure Key Vault** | Not used | No references in code or config. |
| **Azure Storage** | Not used | No references. |
| **Azure OpenAI** | Not used | AIService comment: “TODO: Integrate with Azure OpenAI / GPT”; no SDK. |

---

## 10. Security Features

| Feature | Status | Details |
|---------|--------|---------|
| **MFA / 2FA** | Not implemented | No MultiFactor, TwoFactor, or MFA logic in code. Could be enforced by Entra only if configured in tenant. |
| **Device recognition** | Not implemented | No device fingerprint, device ID, or device binding in auth flow. |
| **Audit logging** | Partial | **AuditMiddleware:** logs Path, UserId (from claims: NameIdentifier or sub), Timestamp to ILogger. **AuditLog table:** exists for entity-level audit (Action, EntityName, EntityId, PerformedByUserId); no code found that writes to it in this audit. **LoginAudit table:** exists (UserId, LoginTime, IpAddress, UserAgent, Success); no code found that writes to it. |
| **Session tracking** | Table only | **UserSession** table exists (UserId, JwtId, ExpiresAt, Revoked, IpAddress). No middleware or code observed that creates or updates UserSession records. |
| **Refresh token storage** | Table only | **RefreshTokenRecord** table exists. API does not issue refresh tokens; Entra issues tokens. No code found that persists or validates refresh tokens in this DB. |
| **Tenant (clinic) scope** | Implemented | TenantResolutionMiddleware requires `X-Clinic-Id` (valid Guid) for `/api/` requests; value stored in HttpContext.Items. |

---

## 11. Mobile App Authentication

- **Project:** .NET MAUI (RaphCare.Mobile).
- **Token storage:** `SecureStorage.Default.SetAsync("access_token", result.AccessToken)` and `SetAsync("expires_on", ...)` in `EntraAuthService`. Features version also stores refresh token under `RefreshTokenKey`.
- **Login service:** `IAuthService` / `EntraAuthService` (Core.Shared.Services.Auth): SignUpWithEmailAsync, SignInAsync, SignOutAsync, GetAccessTokenAsync, IsAuthenticatedAsync. Uses MSAL `PublicClientApplication` with ClientId, Authority, RedirectUri from `EntraAuthOptions`.
- **API auth headers:** `SecureStorageAccessTokenProvider` implements `RaphCare.Client.Contracts.IAccessTokenProvider`; `BearerTokenHandler` calls `GetAccessTokenAsync()` and sets `Authorization: Bearer {token}`. Client is registered with `AddRaphCareClient(..., useBearerToken: true)` in MauiProgram.
- **Config:** Mobile `appsettings.json`: Entra (ClientId, TenantId, ApiScope), Api:BaseAddress. No redirect URI in config; code uses `msal{ClientId}://auth` or `http://localhost` for WinUI.

---

## 12. Gaps (for unified identity)

### Email login

- No API-side “email + password” login; everything goes through Entra. If you want a unified story that includes classic email/password on the API, you need either an API login that issues or validates something (e.g. custom JWT or Entra-on-behalf) or a dedicated “email/password” provider in Entra (e.g. B2C local account). Today: email login exists only via Entra in the mobile app.

### Phone OTP login

- No OTP generation, storage, or verification. No endpoints for send-OTP or verify-OTP. `ISmsService` is a placeholder. You need: OTP entity/store, send and verify endpoints, and a way to link OTP verification to a session or token (e.g. Entra B2C custom policy or API-issued token).

### Voice registration

- No voice or biometric registration flow in the repo. Would require new UX, storage of voice/biometric data, and integration with identity (e.g. Entra or custom).

### Azure identity integration

- **Already in place:** Entra for JWT validation and (designed) user provisioning.
- **Gaps:**
  - **User provisioning not invoked:** `EnsureUserExistsAsync` is never called (no OnTokenValidated, no middleware). So ApplicationUser is never created/updated from Entra on first/successful login.
  - **Role mapping not applied:** `EntraRoleMapper` exists and maps `roles`/`groups` claims to role names, but nothing in the pipeline assigns those to ApplicationUser or uses them for RequireAdmin/RequireProvider/RequirePatient on the API (and no controllers use [Authorize]).
  - **Optional:** Sync Entra groups to UserRoles in DB, or rely only on JWT roles and enforce via [Authorize(Roles = "...")] / policy once provisioning is wired.

### Other gaps

- **Authorization:** No `[Authorize]` on API controllers; policies exist but are unused.
- **LoginAudit / UserSession / RefreshTokenRecord:** Tables exist but are not populated by current code.
- **AuditLog:** Entity and table exist; no observed writes from application code (only middleware logs to ILogger).
- **AccessPolicy:** Domain entity only; not used in authorization or DB in the audited code.

---

## Summary

RaphCare uses **Microsoft Entra ID** as the only identity provider: the API validates Entra JWTs and uses a custom **ApplicationUser** and **IdentityDbContext** (no ASP.NET Identity). The mobile app uses **MSAL** for sign-in and stores the access token in **SecureStorage**; the API client sends it as a **Bearer** token. There are **no** API auth endpoints (login/register/OTP/verify); no **OTP or phone auth**; no **email/password or email verification** on the API; and **user provisioning** and **role usage** are designed but not wired (no post-validation provisioning, no [Authorize] on endpoints). **Audit/session/refresh** tables exist but are not written to. Filling these gaps is required for a unified identity system that includes email, phone OTP, and consistent Azure (Entra) integration.
