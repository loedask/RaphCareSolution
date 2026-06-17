# Authentication & Authorization

**Azure portal + appsettings for API and mobile:** [Azure_Entra_Registration_Guide.md](./Azure_Entra_Registration_Guide.md)

## Identity Setup

- **Provider:** Microsoft Entra ID (Azure AD) for staff; phone OTP + API-issued JWT for patients.
- **Configuration:** Entra options are bound from configuration section `Entra` (e.g. Authority, Audience, TenantId). Section name constant: `EntraOptions.SectionName = "Entra"`. API-issued JWTs use section `Jwt` (JwtOptions: Issuer, Audience, Secret).
- **User storage:** ApplicationUser is stored in IdentityDbContext (Persistence). Users are provisioned or updated from Entra claims after each successful Entra JWT validation via `IUserProvisioningService`, or created/updated on OTP verify (phone stored in Email, EntraObjectId empty) with Patient role.
- **Key types:** ApplicationUser (EntraObjectId, Email, DisplayName), Role, Permission, UserRole, RolePermission, OtpCode. No ASP.NET Core Identity identity store; custom IApplicationUserStore implemented in RaphCare.Persistence (ApplicationUserStore) against IdentityDbContext.

## JWT Strategy

- **Scheme:** JWT Bearer (`JwtBearerDefaults.AuthenticationScheme`). Registered in RaphCare.Identity's `AddIdentity` with `AddAuthentication(JwtBearer).AddJwtBearer(...)`.
- **Validation:** Entra metadata (OpenID Connect) is used to validate signature and claims. Configured via EntraOptions (Authority, etc.).
- **Token source:** Staff: clients obtain tokens from Entra (e.g. MSAL or Entra login flow); API validates the Bearer token. Patients: after OTP verify, API issues a JWT via `ITokenService.GeneratePatientToken` (TokenService, JwtOptions); no token issuance for staff in the API.
- **Post-validation:** After Entra JWT validation, `JwtBearerEvents.OnTokenValidated` (in `RaphCare.Identity` `AddIdentity`) calls `IUserProvisioningService.EnsureUserExistsAsync` (`EntraUserProvisioningService`) so an `ApplicationUser` is created or updated from `oid`, email, and display name. OTP verify flow provisions or finds user by phone (Email), assigns Patient role, writes LoginAudit, returns API-issued token.

## Role Definitions

- **Administrator** — Full system access.
- **Clinician** — Clinical access (provider role).
- **Patient** — Patient portal access.

Roles are seeded by IdentitySeeder (Administrator, Clinician, Patient). EntraRoleMapper maps Entra roles/groups from the JWT (e.g. `roles` claim) to these internal role names for authorization.

## Permission Handling

- **Seeded permissions:** Patients.Read, Patients.Write, Visits.Read, Visits.Write, Admin.All (stored in IdentityDbContext Permissions table; linked via RolePermission to Roles).
- **Usage:** Authorization is role-based in the API (RequireAdmin, RequireProvider, RequirePatient). Permission checks can be extended in application behaviors or controllers; controllers currently use role-based policies.

## Authorization Policies (API)

- **RequireAdmin:** Role "Administrator".
- **RequireProvider:** Roles "Administrator" or "Clinician".
- **RequirePatient:** Roles "Patient", "Administrator", or "Clinician".

Policies are registered in Program.cs with `AddAuthorization(options => { ... })`. Controllers or handlers apply these policies as needed; exact application (e.g. `[Authorize(Policy = "RequireProvider")]`) is per endpoint.

## Tenant / Clinic Scope

- **Header:** `X-Clinic-Id` (required for API paths under `/api/`). Enforced by TenantResolutionMiddleware.
- **Storage:** Resolved clinic ID is stored in HttpContext.Items (key: TenantResolutionMiddleware.ClinicIdItemKey). Non-API and Swagger paths are skipped. Missing or invalid Guid returns 400.

## Phone OTP Auth (API)

- **Endpoints:** POST api/auth/otp/send (SendOtpCommand; rate-limited per phone, returns 429 if exceeded), POST api/auth/otp/verify (VerifyOtpCommand; returns 200 with `{ success, token }` or 400). AuthController is [AllowAnonymous].
- **Flow:** Send: IOtpService.GenerateOtpAsync stores hashed code in OtpCodes, ISmsService sends code. Verify: IOtpService.ValidateOtpAsync marks OTP used; find or create ApplicationUser (Email = phone), assign Patient role, LoginAudit, ITokenService.GeneratePatientToken returns JWT for patient sessions.

## Mobile Client Auth

- **Entra login:** RaphCare.Mobile uses Microsoft Entra ID for sign-in. Core.Shared.Services.Auth: EntraAuthOptions (ClientId, TenantId, ApiScope), IAuthService, EntraAuthService. Configured in MauiProgram; tokens obtained via Entra flow.
- **API requests:** SecureStorageAccessTokenProvider (Core.Features.Auth.Services) implements RaphCare.Client.Contracts.IAccessTokenProvider and is registered in MauiProgram. AddRaphCareClient(..., useBearerToken: true) registers BearerTokenHandler so every API request includes the current access token from IAccessTokenProvider.
