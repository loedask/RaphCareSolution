# Azure Entra ID Registration & Login Guide

Based on the RaphCare codebase, you need **2 application registrations** in Microsoft Entra ID (Azure AD):

| # | App registration   | Purpose |
|---|--------------------|--------|
| 1 | **RaphCare API**   | Backend API; tokens are issued *for* this app (audience). The API validates JWTs with this audience. |
| 2 | **RaphCare Mobile** | Native (public) client; users sign in through this app and get access tokens for the API. |

---

## Part 1: Register the two applications in Azure

### 1.1 Register the API app (RaphCare API)

1. Go to [Azure Portal](https://portal.azure.com) → **Microsoft Entra ID** → **App registrations** → **New registration**.
2. **Name:** `RaphCare API`.
3. **Supported account types:** choose according to your needs (e.g. “Accounts in this organizational directory only” for single tenant).
4. **Redirect URI:** leave blank (no web/login redirect for the API).
5. Click **Register**.

6. **Expose the API (so the mobile app can request tokens for it):**
   - Open the new app → **Expose an API**.
   - Click **Set** next to “Application ID URI” and set it to:  
     `api://raphcare-api`  
     (must match the mobile app’s scope `api://raphcare-api/.default` and the API’s `Entra:Audience`).
   - Click **Add a scope** (optional for simple setup):
     - Scope name: `access_as_user` (or any name).
     - Who can consent: **Admins and users** (or **Admins only**).
     - Save.  
     For “.default” usage you don’t have to create a custom scope; the Application ID URI is enough.

7. **Note these values** (you’ll use them in the API and in the mobile app’s API permissions):
   - **Application (client) ID** → use as API app’s `Entra:ClientId` in `appsettings.json` (optional for validation; audience is what matters).
   - **Directory (tenant) ID** → use as `Entra:TenantId` in the API.
   - **Application ID URI** → must be exactly `api://raphcare-api` and must be set as **`Entra:Audience`** in the API.

---

### 1.2 Register the Mobile app (RaphCare Mobile)

1. **App registrations** → **New registration**.
2. **Name:** `RaphCare Mobile`.
3. **Supported account types:** same as for the API (e.g. single tenant).
4. **Redirect URI:**
   - Platform: **Public client/native (mobile & desktop)**.
   - URI: `msal<CLIENT_ID>://auth`  
     You don’t have the Client ID yet; add this redirect **after** registration (see step 6).
5. Click **Register**.

6. **Redirect URI (finish):**
   - Go to **Authentication** → **Add a platform** → **Mobile and desktop applications**.
   - Check **Default client type** → “Yes” (treat as public client).
   - Under **Redirect URIs** add (replace `YOUR_MOBILE_CLIENT_ID` with the **Application (client) ID** of this “RaphCare Mobile” app):
     - `msalYOUR_MOBILE_CLIENT_ID://auth`
   - For MAUI you may also need platform-specific URIs (e.g. Android/iOS); add them if your docs or MSAL require.
   - Save.

7. **API permissions (so the app can get tokens for the API):**
   - **API permissions** → **Add a permission**.
   - **My APIs** → select **RaphCare API**.
   - Choose **Delegated permissions** and select the scope you exposed (e.g. `access_as_user`) or use **Application ID URI + “.default”** (e.g. `api://raphcare-api`).
   - If you only exposed the Application ID URI and no custom scope, add permission to **“Access (api://raphcare-api)”** or the **.default** scope if shown.
   - Click **Add permission**.
   - If your org requires admin consent, click **Grant admin consent for …**.

8. **Note:**
   - **Application (client) ID** of **RaphCare Mobile** → this is the **ClientId** you put in the mobile app (e.g. in `MauiProgram.cs`).
   - **Directory (tenant) ID** → same tenant as the API; use in mobile as `TenantId` (often `"common"` only for multi-tenant; for single tenant use the actual tenant ID).

---

## Part 2: Configure your solution

### 2.1 API (RaphCare.API)

Edit `RaphCare.API/appsettings.json` (and `appsettings.Development.json` if you use it):

```json
"Entra": {
  "Authority": "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0",
  "TenantId": "YOUR_TENANT_ID",
  "ClientId": "API_APP_CLIENT_ID",
  "Audience": "api://raphcare-api"
}
```

- **Authority:** `https://login.microsoftonline.com/<TenantId>/v2.0` (replace with your tenant ID).
- **TenantId:** Directory (tenant) ID of the Entra tenant.
- **ClientId:** Application (client) ID of the **RaphCare API** app (optional for JWT validation; audience is required).
- **Audience:** Must be exactly the **Application ID URI** of the API app: `api://raphcare-api`.

The API validates incoming Bearer tokens by checking that the token’s audience and issuer match this configuration.

---

### 2.2 Mobile app (RaphCare.Mobile)

Edit `RaphCare.Mobile/MauiProgram.cs` (or use a config file and bind to `EntraAuthOptions`):

```csharp
builder.Services.Configure<Core.Shared.Services.Auth.EntraAuthOptions>(options =>
{
    options.ClientId = "YOUR_MOBILE_APP_CLIENT_ID";   // RaphCare Mobile app (client) ID
    options.TenantId = "common";                      // or your tenant ID for single tenant
    options.ApiScope = "api://raphcare-api/.default";
    // Optional: set Authority if you need a custom authority (e.g. B2C)
    // options.Authority = "https://login.microsoftonline.com/YOUR_TENANT_ID/v2.0";
    // RedirectUri defaults to msal{ClientId}://auth
});
```

- **ClientId:** Application (client) ID of the **RaphCare Mobile** app registration.
- **TenantId:** `"common"` for multi-tenant; or your single tenant ID.
- **ApiScope:** Must match the API’s Application ID URI + `/.default`: `api://raphcare-api/.default`.

Redirect URI is derived as `msal{ClientId}://auth`; it must match the redirect URI you added in the **RaphCare Mobile** app registration.

---

## Part 3: User registration and login

### 3.1 Who “registers” the user?

- **Option A – Standard Entra ID (this codebase):**  
  “Registering the user” usually means **creating the user in the Entra tenant** (admin creates users or invites them). The mobile app then only does **sign-in** (login). User records in your app are created automatically by `EntraUserProvisioningService` after the first successful login (using `oid`, email, display name from the token).

- **Option B – Self-service sign-up (B2C / External ID):**  
  If you want users to **sign up themselves** (e.g. with email/password), you would use **Azure AD B2C** or **External ID** and point the mobile app’s authority and policies to that tenant. The **number of app registrations stays 2** (one for the API, one for the mobile); only the authority and possibly tenant change.

Below we assume **Option A** (standard Entra ID).

---

### 3.2 Add users in Entra (so they can log in)

1. **Azure Portal** → **Microsoft Entra ID** → **Users** → **Create user** (or **Invite external user**).
2. Fill in name, email, and password (or send invite).
3. Assign the user to groups/app roles if you use **EntraRoleMapper** (e.g. “Patient”, “Clinician”, “Administrator”) so the JWT contains the right `roles` (or groups) and your API policies (RequirePatient, RequireProvider, RequireAdmin) work.

No extra “registration” step is required in the RaphCare app: the first time the user signs in, the API provisions an `ApplicationUser` from the token.

---

### 3.3 Login flow (what the user does)

1. User opens the mobile app.
2. Taps **Sign in** (or equivalent); the app calls `IAuthService.SignInAsync()`.
3. MSAL opens the Entra sign-in page (browser or in-app).
4. User enters credentials (for a user that already exists in Entra).
5. After success, MSAL returns access and refresh tokens; the app stores them (e.g. secure storage) and uses the access token as Bearer for API calls.
6. The API validates the JWT (audience `api://raphcare-api`, issuer from Authority), then `EntraUserProvisioningService` ensures an `ApplicationUser` exists (or updates it) for that Entra `oid`.
7. User is considered “registered” in your app database and logged in; they can use the app.

---

### 3.4 Sign-up in the mobile app (EntraAuthService.SignUpWithEmailAsync)

The code has a **Sign up with email** flow that triggers an **interactive** Entra flow. In **standard Entra ID** (single/multi-tenant without B2C):

- There is no public “self-service sign-up” like in B2C; the interactive flow will typically be a **sign-in** experience. New users must be created in the tenant by an admin (or via invite) first.
- So “Sign up” in the app can mean: “Open Entra; if the user doesn’t exist, they’ll get an error; if they do, they sign in.” For true self-service sign-up you’d switch to **Azure AD B2C** or **External ID** and keep the same two app registrations (API + Mobile), but register them in the B2C/External ID tenant and set the mobile app’s **Authority** (and possibly Redirect URI) to the B2C policy URLs.

---

## Summary checklist

| Step | Action |
|------|--------|
| 1 | Create **RaphCare API** app registration; set Application ID URI to `api://raphcare-api`. |
| 2 | Create **RaphCare Mobile** app registration; add redirect URI `msal<MobileClientId>://auth`; add API permission to RaphCare API. |
| 3 | In API: set `Entra:Authority`, `Entra:TenantId`, `Entra:ClientId`, `Entra:Audience` (`api://raphcare-api`). |
| 4 | In Mobile: set `ClientId` (Mobile app), `TenantId`, `ApiScope` = `api://raphcare-api/.default`. |
| 5 | Create or invite users in Entra; assign roles/groups if needed. |
| 6 | Users sign in from the app; first login provisions them in your app via `EntraUserProvisioningService`. |

You need **2 applications** registered in Azure: **RaphCare API** and **RaphCare Mobile**. User “registration” in your system is either (1) creating users in Entra and then having them log in (standard Entra), or (2) using B2C/External ID for self-service sign-up with the same two app registrations in that tenant.
