# Azure / Microsoft Entra setup: API + Mobile

This is the **single place** to configure **Microsoft Entra ID** (Azure AD) so **RaphCare.API** and **RaphCare.Mobile** work together for staff sign-in. Patient flows (phone OTP + API-issued JWT) are described in [04_Authentication_Authorization.md](./04_Authentication_Authorization.md)—not covered step-by-step here.

**Related docs:** [04_Authentication_Authorization.md](./04_Authentication_Authorization.md) (architecture), [09_Mobile_App_Guide.md](./09_Mobile_App_Guide.md) (mobile dev notes).

---

## 1. What you register in Azure

You need **two app registrations** in the same Entra tenant:

| App registration   | Purpose |
|-------------------|---------|
| **RaphCare API**  | Resource that exposes scopes; tokens are issued **for** this API (`aud` claim). The API validates JWTs against this audience. |
| **RaphCare Mobile** | Public (native) client; users sign in here; MSAL requests delegated access to the API. |

---

## 2. Register the API application

1. [Azure Portal](https://portal.azure.com) → **Microsoft Entra ID** → **App registrations** → **New registration**.
2. **Name:** `RaphCare API` (or your name).
3. **Supported account types:** e.g. single tenant, or B2C/External ID tenant if you use those.
4. **Redirect URI:** leave blank for a pure API resource.
5. **Register**, then note **Application (client) ID** and **Directory (tenant) ID**.

### Expose an API

1. Open **RaphCare API** → **Expose an API**.
2. Set **Application ID URI** to match what the API expects, e.g. **`api://raphcare-api`** (must match `Entra:Audience` in the API `appsettings`).
3. **Add a scope** (delegated) used by the mobile app, e.g.:
   - **Scope name:** `access_as_user` (matches the sample mobile config `api://raphcare-api/access_as_user`).
   - **Who can consent:** Admins and users (or per your policy).
4. Save. Full scope value: `api://raphcare-api/access_as_user`.

Alternatively you can use **`.../.default`** in the client if you prefer; then align `Entra:ApiScope` in the mobile app and ensure the API registration exposes the API correctly.

### App roles (recommended for `[Authorize]` policies)

Controllers use policies such as **RequireProvider** / **RequireAdmin**, which map to **role** claims. In the **RaphCare API** registration:

1. **App roles** → create roles whose **Value** matches what the API expects, e.g. `Administrator`, `Clinician`, `Patient`.
2. **Enterprise applications** → your API app → **Users and groups** → assign users (or groups) to those app roles.

Without role assignment, the user may authenticate (200 on token validation) but receive **403** on role-protected endpoints.

---

## 3. Register the mobile application

1. **App registrations** → **New registration**.
2. **Name:** `RaphCare Mobile`.
3. **Supported account types:** same tenant model as the API.
4. **Redirect URI:** **Public client/native** — after creation, set:
   - **`msal{MOBILE_CLIENT_ID}://auth`**  
     Replace `{MOBILE_CLIENT_ID}` with this app’s **Application (client) ID**.

5. **Authentication** → enable **Allow public client flows** if you use interactive MSAL on device.

### API permissions

1. **API permissions** → **Add a permission** → **My APIs** → **RaphCare API**.
2. Add **Delegated** permission for the scope you exposed (e.g. `access_as_user`).
3. **Grant admin consent** if required.

### Platform redirect URIs (MAUI)

- **Windows (WinUI):** the app uses **`http://localhost`** as redirect (see `EntraAuthOptions.GetRedirectUri()`).
  - In the portal, add **Mobile and desktop** → `http://localhost` (or the specific loopback URI you configure) for local Windows debugging.
- **Android / iOS:** follow [Microsoft identity platform redirect URIs for mobile](https://learn.microsoft.com/en-us/azure/active-directory/develop/reply-url) and add the MSAL-generated / intent-filter URIs your build uses.

---

## 4. Configure RaphCare.API

### `appsettings.json` (`Entra` section)

Bind to **`EntraOptions`** (`RaphCare.Identity`). Example:

```json
"Entra": {
  "Authority": "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0",
  "TenantId": "YOUR_TENANT_ID",
  "ClientId": "YOUR_API_APP_CLIENT_ID",
  "Audience": "api://raphcare-api"
}
```

| Key | Meaning |
|-----|--------|
| **Authority** | Issuer base for token validation; typically `https://login.microsoftonline.com/{TenantId}/v2.0` for single-tenant workforce. |
| **TenantId** | Directory (tenant) ID. |
| **ClientId** | API app’s client ID (optional for pure validation; useful for diagnostics and consistency). |
| **Audience** | Must match the **Application ID URI** (or the `aud` claim Entra puts on access tokens for your API), e.g. `api://raphcare-api`. |
| **ValidIssuers** | Optional array; if omitted, issuers are derived from **Authority**. For **Azure AD B2C**, set explicit issuers for each user-flow policy. |

Use **user-secrets** or environment variables in development; do not commit production secrets.

```bash
cd RaphCare.API
dotnet user-secrets set "Entra:TenantId" "YOUR_TENANT_ID"
dotnet user-secrets set "Entra:Authority" "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0"
dotnet user-secrets set "Entra:Audience" "api://raphcare-api"
dotnet user-secrets set "Entra:ClientId" "YOUR_API_CLIENT_ID"
```

### How the API handles sign-in (readiness)

There is **no** `/login` endpoint. The client obtains a token from Entra; the API only **validates** the JWT.

| Capability | Status |
|------------|--------|
| JWT Bearer authentication | Registered in **`RaphCare.Identity`** `AddIdentity` (`AddJwtBearer`). **Authority** and **Audience** are set on `JwtBearerOptions` so OIDC metadata and signing keys load from Entra. |
| Token validation | Signature, issuer, audience, lifetime (see `DependencyInjection.cs`). |
| User provisioning | **`OnTokenValidated`** calls **`IUserProvisioningService.EnsureUserExistsAsync`** (`EntraUserProvisioningService`) so an **`ApplicationUser`** row is created/updated from the token (`oid`, email, name). |
| Authorization | **`[Authorize(Policy = "RequireProvider")]`** (and similar) on API controllers; policies require roles **`Administrator`**, **`Clinician`**, **`Patient`** matching role claims in the token. |

**Important:** Entra must issue **role** claims the API understands (via **app roles** assignment or claim mapping). Otherwise users get **403** on protected controllers even with a valid token.

---

## 5. Configure RaphCare.Mobile

### `appsettings.json` (`Entra` section)

Binds to **`EntraAuthOptions`** (`RaphCare.Mobile`). Example (align with your portal):

```json
"Entra": {
  "ClientId": "YOUR_MOBILE_APP_CLIENT_ID",
  "TenantId": "YOUR_TENANT_ID",
  "ApiScope": "api://raphcare-api/access_as_user",
  "RedirectUri": "msalYOUR_MOBILE_APP_CLIENT_ID://auth",
  "SelfServicePasswordResetUrl": "https://passwordreset.microsoftonline.com/",
  "B2CPasswordResetAuthority": "",
  "B2CPasswordResetScopes": "openid",
  "ExternalSignUpUrl": ""
}
```

| Key | Meaning |
|-----|--------|
| **ClientId** | **RaphCare Mobile** app registration client ID. |
| **TenantId** | Same tenant as the API (or `common` / B2C-specific per MSAL docs). |
| **ApiScope** | Delegated scope the mobile app requests (must match an exposed scope on the API app). |
| **RedirectUri** | Optional; default is `msal{ClientId}://auth`. **WinUI** uses `http://localhost` in code—register that URI in Azure for desktop debugging. |
| **SelfServicePasswordResetUrl** | Browser URL for “Forgot password?” when **B2CPasswordResetAuthority** is empty (workforce SSPR or B2C user-flow “Run now” link). |
| **B2CPasswordResetAuthority** | If set, “Forgot password?” runs an in-app MSAL interactive flow against this policy authority instead of opening the browser URL. |
| **ExternalSignUpUrl** | If set, **Create Account** / **Sign up** opens this URL (e.g. B2C sign-up user flow) instead of in-app registration pages. |

User secrets (example):

```bash
cd RaphCare.Mobile
dotnet user-secrets set "Entra:ClientId" "YOUR_MOBILE_CLIENT_ID"
dotnet user-secrets set "Entra:TenantId" "YOUR_TENANT_ID"
dotnet user-secrets set "Entra:ApiScope" "api://raphcare-api/access_as_user"
```

Set **`Api:BaseAddress`** to your running API URL (e.g. `https://localhost:7001/`). The HTTP client attaches **`Authorization: Bearer`** from secure storage when **`useBearerToken: true`** (see `MobileServiceCollectionExtensions`).

---

## 6. Azure AD B2C / External ID (optional)

The same **two-app** pattern applies in a B2C tenant: one registration exposes **`api://...`** scopes; the native app requests those scopes. Differences:

- **Authority** in the API and **Authority** / policy hosts in MSAL for the mobile app must use your **B2C** endpoints (e.g. `https://{tenant}.b2clogin.com/...`).
- Set **`Entra:ValidIssuers`** in the API to every issuer string your policies emit, if they differ from the default Authority-derived issuers.
- Use **`ExternalSignUpUrl`** / **`SelfServicePasswordResetUrl`** / **`B2CPasswordResetAuthority`** on the mobile app as documented above; paste **Run now** links from the Azure portal where appropriate.

---

## 7. End-to-end check

1. Start **RaphCare.API** (HTTPS).
2. Run **RaphCare.Mobile**, sign in with an Entra user that has the correct **app role** assignments.
3. Trigger an API call from the app (or use Swagger with a token from MSAL).
4. Expect **401** if the token is missing/wrong audience/wrong issuer; **403** if the token is valid but **roles** are missing; **200** when roles match the controller policy.

Decode a sample access token at [jwt.ms](https://jwt.ms): confirm **`aud`** equals **`Entra:Audience`**, **`iss`** matches your tenant/policy, and **`roles`** (or `scp`) are present as expected.

---

## 8. Troubleshooting

| Symptom | What to check |
|--------|----------------|
| **invalid_audience** / audience mismatch | API **`Entra:Audience`** equals the API’s Application ID URI; mobile **`ApiScope`** requests that API’s scope; token **`aud`** matches. |
| **Issuer mismatch** | **`Entra:Authority`** / **`ValidIssuers`** match token **`iss`** (include `/v2.0` where used). B2C: add policy-specific issuers. |
| **Signature / metadata errors** | API **`JwtBearerOptions.Authority`** is set (see `RaphCare.Identity`); machine can reach `login.microsoftonline.com` or `b2clogin.com`. |
| **401 with valid token** | Request includes **`Authorization: Bearer`**. |
| **403 after 200 auth** | User lacks **app role** in Entra for the API; policies require **Administrator** / **Clinician** / **Patient**. |
| **MSAL redirect_uri mismatch** | Portal redirect URIs match **`msal{ClientId}://auth`** and platform-specific URIs; Windows: **`http://localhost`**. |
| **Mobile cannot reach API** | **`Api:BaseAddress`**, TLS/localhost trust, firewall. |

---

## 9. Checklist

| Step | Action |
|------|--------|
| 1 | Create **RaphCare API** registration; set Application ID URI **`api://raphcare-api`** (or your chosen URI and align **Audience**). |
| 2 | Expose delegated scope (e.g. **`access_as_user`**); define **app roles** and assign users. |
| 3 | Create **RaphCare Mobile** registration; redirect **`msal{MobileClientId}://auth`** (+ **`http://localhost`** for WinUI dev). |
| 4 | Grant mobile app **delegated** permission to the API scope; **admin consent**. |
| 5 | Configure **API** `Entra` (Authority, TenantId, Audience, …). |
| 6 | Configure **Mobile** `Entra` + **`Api:BaseAddress`**. |
| 7 | Sign in on mobile; verify API calls succeed and roles allow the intended controllers. |

---

*Older duplicate material was consolidated from `azure-identity-setup.md` into this guide; patient OTP architecture remains in [04_Authentication_Authorization.md](./04_Authentication_Authorization.md).*
