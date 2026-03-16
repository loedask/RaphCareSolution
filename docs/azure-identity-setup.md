# RaphCare Identity Setup (Azure)

## 1. Overview

RaphCare uses a **dual-authentication architecture** to support two distinct user types with different security and access needs.

### Authentication model

- **Staff (doctors, clinicians, administrators)**  
  Authenticate with **Microsoft Entra ID** (formerly Azure AD). Staff sign in with organizational accounts and receive Entra-issued tokens. The API validates these tokens and maps Entra roles to internal application roles.

- **Patients**  
  Authenticate via **phone OTP + API-issued JWT**. Patients do not use Entra. They verify their phone number with a one-time password, then the API creates or finds the patient and issues its own JWT for subsequent requests.

- **API layer**  
  All HTTP requests to the API use **JWT Bearer authentication**. The API accepts:
  - Entra-issued JWTs for staff (validated against Entra issuer and audience).
  - API-issued JWTs for patients (validated against a configured secret/audience).

This separation keeps staff identity in your organization’s directory while allowing simple, phone-based patient access without requiring Entra accounts for patients.

---

## 2. Azure Tenant Requirements

Before configuring identity, ensure you have:

| Requirement | Description |
|-------------|-------------|
| **Azure account** | An active Azure subscription (free tier is sufficient for app registration). |
| **Entra tenant** | A Microsoft Entra ID tenant. If your organization uses Microsoft 365 or Azure, you already have one. |
| **Permissions** | Ability to register applications in Entra ID. Typically **Application administrator** or **Cloud application administrator**, or **Global administrator**. |

To confirm you can register apps:

1. Sign in to [Azure Portal](https://portal.azure.com).
2. Go to **Microsoft Entra ID** → **App registrations**.
3. If you can create a new registration, you have the required permissions.

---

## 3. Register the API Application

The **RaphCare API** is represented in Entra ID as a single-tenant application that exposes scopes and optionally app roles for staff.

### Steps

1. In **Azure Portal**, go to **Microsoft Entra ID** → **App registrations**.
2. Click **New registration**.
3. Use this configuration:

   | Field | Value |
   |-------|--------|
   | **Name** | `RaphCare API` |
   | **Supported account types** | **Accounts in this organizational directory only** (Single tenant) |
   | **Redirect URI** | Leave blank (no redirect needed for a backend API) |

4. Click **Register**.

### After creation

Note these values; you will use them in the API and mobile app configuration:

- **Application (client) ID** — e.g. `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- **Directory (tenant) ID** — e.g. `yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy`

You can find them on the app’s **Overview** page in the Azure Portal.

---

## 4. Configure API Permissions

Configure the API app so the mobile (or other) client can request tokens for your API.

### Expose an API

1. Open the **RaphCare API** app registration.
2. Go to **Expose an API**.
3. Next to **Application ID URI**, click **Set** (if not set). Accept the default or use:  
   `api://raphcare-api`  
   (or `api://<client-id-of-this-app>`).
4. Under **Scopes**, click **Add a scope**:
   - **Scope name:** `access`
   - **Who can consent:** Admins and users (or Admins only, per policy)
   - **Display name:** e.g. `Access RaphCare API`
   - **Description:** e.g. `Allows the app to call the RaphCare API`
5. Save. The full scope value will be: **`api://raphcare-api/access`** (or with your Application ID URI).

### Add app roles (optional)

To map Entra roles to internal roles (e.g. Administrator, Clinician):

1. In the same app registration, go to **App roles** → **Create app role**.
2. Create roles, for example:

   | Display name | Value | Allowed member types |
   |--------------|--------|----------------------|
   | Administrator | `Administrator` | Users/Groups |
   | Clinician | `Clinician` | Users/Groups |

3. Save each role.

You will use these role claims in the API for authorization (see **Section 9. Role Mapping**).

---

## 5. Register the Mobile Application

Register a second app for the mobile (or desktop) client that will sign in staff and call the API.

### Steps

1. **Microsoft Entra ID** → **App registrations** → **New registration**.
2. Use this configuration:

   | Field | Value |
   |-------|--------|
   | **Name** | `RaphCare Mobile` |
   | **Supported account types** | **Accounts in this organizational directory only** (same tenant as the API) |
   | **Redirect URI** | **Public client/native (mobile & desktop)** → `msal{CLIENT_ID}://auth` |

   Replace `{CLIENT_ID}` with the **Application (client) ID** of this new app (you can set it after creation and then update the redirect URI).

3. Click **Register**.
4. After creation, go to **Authentication**:
   - Under **Platform configurations**, ensure **Mobile and desktop applications** is added with redirect URI `msal{CLIENT_ID}://auth`.
   - Enable **Mobile and desktop flows** as needed (e.g. allow public client flows if you use device code or native client secret-less flows).

### Grant the mobile app permission to the API

1. In the **RaphCare Mobile** app registration, go to **API permissions** → **Add a permission**.
2. Choose **My APIs** → select **RaphCare API**.
3. Under **Delegated permissions**, select the scope **access** (e.g. `api://raphcare-api/access`).
4. Click **Add permissions**.
5. If your tenant requires admin consent, click **Grant admin consent for [your organization]**.

---

## 6. Configure Authentication Flow

The mobile app obtains tokens for staff using the **Microsoft Authentication Library (MSAL)** and the OAuth 2.0 / OpenID Connect flows (e.g. interactive login, or device code for desktop).

### Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| **ClientId** | Application (client) ID of **RaphCare Mobile** | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` |
| **TenantId** | Directory (tenant) ID (same for both apps) | `yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy` |
| **ApiScope** | Scope exposed by the API | `api://raphcare-api/access` |

### Example configuration (mobile app)

Values are typically read from a config file or environment; never ship production secrets in the app binary.

```json
{
  "Entra": {
    "ClientId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "TenantId": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy",
    "ApiScope": "api://raphcare-api/access",
    "RedirectUri": "msalaaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa://auth"
  }
}
```

The mobile app uses **ClientId**, **TenantId**, and **RedirectUri** to sign in the user, and requests **ApiScope** so the issued token includes the scope claim required by the API.

---

## 7. Configure the API

The API validates Entra-issued JWTs using the JWT Bearer authentication middleware.

### appsettings.json (example)

Use placeholders and replace with your tenant and API app details. Do not commit real tenant/client IDs to source control in production; use Key Vault or environment variables.

```json
{
  "Entra": {
    "Authority": "https://login.microsoftonline.com/{TenantId}",
    "Audience": "api://raphcare-api"
  }
}
```

- **Authority** — Issuer of` the token (Entra tenant). Replace `{TenantId}` with your **Directory (tenant) ID**.
- **Audience** — The **Application ID URI** of the **RaphCare API** app (e.g. `api://raphcare-api` or `api://<api-client-id>`). The token’s `aud` claim must match this.

### JwtBearer configuration

Typical setup in the API (e.g. in `Program.cs` or `Startup.cs`):

- Add `Microsoft.AspNetCore.Authentication.JwtBearer` and (optionally) `Microsoft.Identity.Web`.
- Configure `AddAuthentication(JwtBearerDefaults.AuthenticationScheme)` and `AddJwtBearer(...)` using:
  - **Authority**: `https://login.microsoftonline.com/{TenantId}`
  - **Audience**: `api://raphcare-api`
- Optionally validate the `roles` claim for app roles (Administrator, Clinician).

The middleware will:
- Validate signature using Entra’s OpenID Connect metadata.
- Validate `iss`, `aud`, and expiration.
- Attach the user (and role claims) to `HttpContext.User` for authorization.

---

## 8. Patient Authentication

Patients **do not use Microsoft Entra ID**. They authenticate with:

1. **Phone number** — as the user identifier.
2. **OTP (one-time password)** — sent via SMS or another channel to that number.
3. **API-issued JWT** — after successful OTP verification, the API creates or finds the patient and issues its own JWT for subsequent calls.

### Flow

1. **Send OTP** — Client calls an endpoint (e.g. `POST /auth/patient/send-otp`) with the phone number. The API generates an OTP, stores it (e.g. in cache or DB with expiry), and sends it via SMS.
2. **Verify OTP** — Client calls (e.g. `POST /auth/patient/verify-otp`) with phone number and OTP. The API verifies the OTP.
3. **Create/find user** — The API creates an **ApplicationUser** (or equivalent) for the patient if one does not exist, or finds the existing one linked to that phone number.
4. **Issue JWT** — The API signs a JWT (using its own secret or key) with claims such as `sub`, `phone_number`, and perhaps `role: Patient`, and returns it to the client.
5. **Subsequent requests** — The client sends this JWT in the `Authorization: Bearer <token>` header. The API validates this token with a separate configuration (e.g. a different audience or issuer) so patient JWTs are distinct from Entra tokens.

This keeps patient identity and MFA (OTP) outside Entra while still using JWTs for API access.

---

## 9. Role Mapping

Entra **app roles** are mapped to internal application roles used for authorization (e.g. in controllers or policies).

| Azure app role (Entra) | Internal role (RaphCare) |
|------------------------|---------------------------|
| Administrator          | Administrator             |
| Clinician              | Clinician                 |

The API reads the `roles` claim (or `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`) from the validated Entra JWT and maps it to your internal role model. Ensure the **value** of the app role in Entra (e.g. `Administrator`) matches what the API expects. Patient tokens use a separate path (e.g. a fixed “Patient” role set when the API issues the JWT).

---

## 10. Local Development

Use **user-secrets** (or environment variables) so Entra and other secrets are not in `appsettings.json` in the repo.

### Example: dotnet user-secrets

From the API project directory:

```bash
dotnet user-secrets set "Entra:TenantId" "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"
dotnet user-secrets set "Entra:ClientId" "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
dotnet user-secrets set "Entra:Audience" "api://raphcare-api"
```

If your config uses **Authority**:

```bash
dotnet user-secrets set "Entra:Authority" "https://login.microsoftonline.com/yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"
```

User-secrets override `appsettings.json`, so you can keep placeholder values in the file and real values only in secrets. For the mobile app, use a dev config file or environment-specific settings that are not committed.

---

## 11. Security Best Practices

| Practice | Description |
|----------|-------------|
| **Do not commit secrets** | Never commit tenant IDs, client IDs, client secrets, or JWT signing keys to source control. Use user-secrets, environment variables, or Key Vault. |
| **Use Azure Key Vault in production** | Store API configuration (e.g. Entra settings, patient JWT secret) in Azure Key Vault and reference them from the API (e.g. via managed identity or client secret). |
| **Rotate JWT secrets** | For API-issued patient JWTs, use a strong secret or certificate and rotate it periodically; have a strategy to invalidate or short-expire old tokens. |
| **Restrict API scopes** | Expose only the scopes and app roles you need; avoid broad “full access” scopes. Grant mobile app only the delegated permissions required. |
| **Admin consent** | Use “Admins only” consent for sensitive scopes or roles if appropriate for your organization. |
| **HTTPS only** | Use HTTPS in all environments; do not use HTTP for token or API traffic. |

---

## 12. Troubleshooting

Common issues and how to diagnose them:

| Issue | Cause | What to check |
|-------|--------|----------------|
| **Invalid audience** | Token `aud` does not match API configuration. | Ensure **Entra:Audience** in the API matches the **Application ID URI** of the RaphCare API app (e.g. `api://raphcare-api`). Ensure the client is requesting the correct scope (e.g. `api://raphcare-api/access`). |
| **Token issuer mismatch** | Issuer (`iss`) in the token does not match the authority. | Ensure **Entra:Authority** is `https://login.microsoftonline.com/{TenantId}` with the correct **TenantId**. Check for typos or wrong tenant (e.g. common vs single-tenant). |
| **Missing scopes** | Token does not contain the expected scope or role. | In Azure, confirm the **RaphCare Mobile** app has the **access** delegated permission and that the user (or admin) has consented. For roles, confirm the user is assigned the app role in **Enterprise applications** → **RaphCare API** → **Users and groups**. |
| **401 Unauthorized** | Request rejected by authentication middleware. | Confirm the request sends `Authorization: Bearer <token>`. Decode the JWT (e.g. at [jwt.ms](https://jwt.ms)) and verify `aud`, `iss`, and `exp`. Ensure the API is using the same Authority and Audience as the token. Check logs for “invalid signature” or “invalid audience” messages. |

### Quick checks

1. **Decode the JWT** at [jwt.ms](https://jwt.ms) and verify `aud`, `iss`, `exp`, and `roles` or `scp`.
2. **Compare with API config**: Authority should match `iss` (e.g. `https://login.microsoftonline.com/{tenantid}/v2.0`), and Audience should match `aud`.
3. **Verify consent**: In **API permissions** for the mobile app, ensure the scope is granted and (if required) admin consent is done.

---

With this guide, a new developer can register the Entra applications, configure the API and mobile app, run the API locally with user-secrets, and understand how staff (Entra) and patient (OTP + API JWT) authentication fit together.
