# React concept → RaphCare.Mobile (design port)

**Concept app path (local):** `C:\laragon\www\raphcare-mobile-app-concept`

## Current MAUI identity: Premium soft

As of the **Premium soft** redesign, **RaphCare.Mobile** no longer targets pixel parity with the React concept.

| Layer | Source of truth |
|-------|-----------------|
| **Look and feel** | MAUI **Premium soft** tokens in `RaphCare.Mobile/Resources/Styles/` and shared controls under `Core/Common/Controls/` (softer page wash, larger radii, deeper soft shadows, richer teal/navy gradients, more breathing room). |
| **Structure** | React concept remains useful for **routes**, **screen inventory**, **copy alignment**, and **feature layout ideas**. |
| **Historical tokens** | Tables below keep the original concept → MAUI mapping for reference. Prefer live `Colors.xaml` / `Typography.xaml` / `Cards.xaml` / `Buttons.xaml` / `Inputs.xaml` when implementing UI. |

Do **not** revert Premium soft screens to concept pixel values unless product explicitly asks to restore concept parity.

**Cursor rules:** `.cursor/rules/raphcare-mobile-concept-template.mdc`, `.cursor/rules/raphcare-mobile.mdc`

---

## What the concept is (do not port literally)

| In concept | Notes for MAUI |
|------------|----------------|
| **React 18 + Vite** | Use **.NET MAUI** pages / Shell, not WebView for the main app (unless you intentionally host a web surface). |
| **react-router-dom** | Map routes to **Shell** routes / `AppNavigator`. See route table below. |
| **@tanstack/react-query** | Use **Client** + ViewModels / async commands; no React Query. |
| **shadcn/ui + Radix UI** | Recreate structure with **MAUI layouts**, **Shared Controls**, **Styles**, not Radix primitives. Visual chrome follows **Premium soft**, not shadcn defaults. |
| **lucide-react** | Use **PNG/SVG** under `Resources/Images` (`icon_*.svg`, soft fill + stroke teal; accent variant `icon_*_on_accent.svg` in white). Home quick actions, Profile menu rows, Help, and Privacy use accent wells (`AppIconWellAccent` / `ProfileMenuIconBorder` + `*OnAccent`). Soft teal wells remain for Home health metrics and device rows. Bound via `IconSource` / `MonochromeIconKeys`. Do **not** use multicolor emoji on Home / Profile / Settings / register chrome. |
| **framer-motion** | Optional subtle animations only where they stay calm and trustworthy. |
| **i18n** (`src/i18n/`) | Align copy with **`AppResources.resx`** (and future `.es.resx` etc.). Same strings as `translations.ts` where applicable. |

**Port from the concept:** screen structure, navigation map, and product copy. **Do not** treat concept colors, radii, shadows, or density as the live MAUI design target.

---

## Fonts (concept → MAUI)

Loaded in concept via Google Fonts in `src/index.css`:

| Role | Family | Weights (concept) |
|------|--------|-------------------|
| **Display** | **Space Grotesk** | 400, 500, 600, 700 |
| **Body** | **DM Sans** | 300 to 700, italic |

**MAUI:** bundled as variable TTFs (OFL, from [google/fonts](https://github.com/google/fonts)) under `RaphCare.Mobile/Resources/Fonts/`, with `OFL-*.txt` license copies. Registered in `MauiProgram.ConfigureFonts`: alias **`SpaceGrotesk`** → `SpaceGrotesk-VariableFont_wght.ttf`, **`DMSans`** → `DMSans-VariableFont_opsz_wght.ttf`, **`DMSansItalic`** → `DMSans-Italic-VariableFont_opsz_wght.ttf`. Default **Label** / **Button** / **Entry** / **Editor** use **DM Sans** via `App.xaml`; display styles (`TitleLarge`, `SectionTitle`, …) and `AuthTitleLabel` set **Space Grotesk**. Premium soft slightly enlarges display sizes vs the original concept scale.

---

## Typography scale (from `tailwind.config.ts`)

Use these as the target for **MAUI styles** (convert `rem` → device-independent units, e.g. 1rem ≈ 16).

| Token | Size / line-height | Weight / letter-spacing |
|-------|--------------------|-------------------------|
| `text-page-title` | 1.75rem / 2.125rem | 700, -0.02em |
| `text-section-title` | 1.25rem / 1.625rem | 600, -0.01em |
| `text-body-lg` | 1rem / 1.5rem | (default) |
| `text-body-md` | 0.875rem / 1.375rem | |
| `text-caption` | 0.75rem / 1rem | |
| `text-micro` | 0.625rem / 0.875rem | |

---

## Color tokens (historical concept `:root` light in `src/index.css`)

Values are **HSL components** (no `hsl()` wrapper): use `hsl({value})` in CSS. **MAUI** mirrors them in `RaphCare.Mobile/Resources/Styles/Colors.xaml` as **`Color`** resources. Hex below is **sRGB** from the same HSL components (standard `hsl()` conversion); use these literals in XAML.

| Semantic token | CSS variable (light) | MAUI `x:Key` | Hex |
|----------------|----------------------|--------------|-----|
| Background | `--background` | `BackgroundColor` | `#F9FAFB` |
| Foreground | `--foreground` | `ForegroundColor` | `#141D2E` |
| Card | `--card` | `CardColor` | `#FFFFFF` |
| Card foreground | `--card-foreground` | `CardForegroundColor` | `#141D2E` |
| Popover | `--popover` | `PopoverColor` | `#FFFFFF` |
| Popover foreground | `--popover-foreground` | `PopoverForegroundColor` | `#141D2E` |
| Primary | `--primary` | `PrimaryColor`, `TealColor`, `RingColor` | `#1B9BBB` |
| Primary foreground | `--primary-foreground` | `PrimaryForegroundColor` | `#FFFFFF` |
| Primary (pressed / darker) | *(derived for MAUI)* | `PrimaryDarkColor` | `#16819C` |
| Teal light / focus accents | `--teal-light` | `PrimaryLightColor`, `TealLightColor` | `#41C8C8` |
| Secondary | `--secondary` | `SecondaryColor` | `#F2F5F8` |
| Secondary foreground | `--secondary-foreground` | `SecondaryForegroundColor` | `#242E42` |
| Muted | `--muted` | `MutedColor` | `#EDF0F2` |
| Muted foreground | `--muted-foreground` | `MutedForegroundColor`, `TextSecondaryColor` | `#768293` |
| Accent | `--accent` | `AccentColor` | `#35B69C` |
| Accent foreground | `--accent-foreground` | `AccentForegroundColor` | `#FFFFFF` |
| Destructive | `--destructive` | `DestructiveColor`, `ErrorColor`, `HealthCriticalColor` | `#DB4343` |
| Destructive foreground | `--destructive-foreground` | `DestructiveForegroundColor` | `#FFFFFF` |
| Border | `--border` | `BorderColor` | `#E7EBEE` |
| Input | `--input` | `InputColor` | `#E7EBEE` |
| Navy | `--navy` | `NavyColor`, `GradientStartColor` | `#121C30` |
| Navy light | `--navy-light` | `NavyLightColor`, `GradientMidColor` | `#23324D` |
| Teal glow | `--teal-glow` | `TealGlowColor` | `#78E2DF` |
| Calm | `--calm` | `CalmColor` | `#F2F4F7` |
| Calm accent | `--calm-accent` | `CalmAccentColor` | `#7B6EB9` |
| Calm soft | `--calm-soft` | `CalmSoftColor` | `#ECE9F2` |
| Health normal | `--health-normal` | `HealthNormalColor`, `SuccessColor` | `#37BE7F` |
| Health attention | `--health-attention` | `HealthAttentionColor`, `WarningColor` | `#F6A823` |
| Health critical | `--health-critical` | `HealthCriticalColor` | `#DB4343` |
| Sidebar background | `--sidebar-background` | `SidebarBackgroundColor` | `#FAFAFA` |
| Sidebar foreground | `--sidebar-foreground` | `SidebarForegroundColor` | `#3F3F46` |
| Sidebar primary | `--sidebar-primary` | `SidebarPrimaryColor` | `#18181B` |
| Sidebar primary fg | `--sidebar-primary-foreground` | `SidebarPrimaryForegroundColor` | `#FAFAFA` |
| Sidebar accent | `--sidebar-accent` | `SidebarAccentColor` | `#F4F4F5` |
| Sidebar accent fg | `--sidebar-accent-foreground` | `SidebarAccentForegroundColor` | `#18181B` |
| Sidebar border | `--sidebar-border` | `SidebarBorderColor` | `#E5E7EB` |
| Sidebar ring | `--sidebar-ring` | `SidebarRingColor` | `#3B82F6` |
| Gradient brand end | `--teal` (with navy stops) | `GradientEndColor` | `#1B9BBB` |
| Gradient card | `--gradient-card` stops | `GradientCardStartColor`, `GradientCardEndColor` | `#FFFFFF`, `#F9FAFB` |

**Also in XAML (no separate CSS variable):** `SurfaceColor`; `TextPrimaryColor`; `TextMutedColor` (lighter muted tier for captions). Live Premium soft hex values are in `Colors.xaml` and may differ from the concept column above.

**Corner radius (Premium soft):** shared cards and buttons use about **16 to 20** device units (larger than concept `--radius: 0.875rem`).

**Shadows (Premium soft):** softer, taller card and CTA shadows via MAUI `Shadow` on `CardBorder`, `CardView`, `GradientButton`, and hero CTAs. Concept CSS shadow names remain historical only.

**Gradients:** navy → teal brand washes and teal → accent CTAs via **LinearGradientBrush** / **GradientButton**.

**Dark theme:** not the default. If added later, define a second resource dictionary rather than copying concept `.dark` blindly.

---

## React routes ↔ MAUI mapping (reference)

| Concept path | Page (file) | Suggested MAUI alignment |
|--------------|-------------|-------------------------|
| `/` | Welcome | Auth / welcome flow |
| `/login` | Login | Auth |
| `/register` | Register | Auth |
| `/email-register`, `/email-verify` | EmailRegister, EmailVerify | Auth |
| `/phone-register`, `/phone-verify` | PhoneRegister, PhoneVerify | Auth |
| `/account-created` | AccountCreated | Auth |
| `/voice-register` | VoiceRegister | Auth / onboarding |
| `/account-created` | AccountCreated | `AccountCreatedPage` |
| `/home` | Home | `HomePage` |
| `/appointments`, `/book-appointment`, `/appointment-details` | Appointments, BookAppointment, AppointmentDetails | `AppointmentsPage`, `BookAppointmentPage`, `AppointmentDetailPage` (patient API) |
| `/request-call`, `/consultation` | RequestCall, Consultation | `RequestCallPage` → `CareTelehealthPage` / `TelehealthJoinPage` |
| `/records`, `/health-record-details` | HealthRecords, HealthRecordDetails | Records |
| `/devices` | ConnectedDevices | `DevicesPage` (BLE E580/E585-class; see `docs/11_Devices_BLE_E580_E585.md`; fleet/packages: `docs/13_Patient_Device_Packages_and_Fleet.md`) |
| `/insurance` | Insurance | Insurance |
| `/family-members` | FamilyMembers | Profile / account |
| `/personal-information`, `/change-password`, `/language`, `/medical-information`, `/emergency-contacts` | Profile sub-pages | `PersonalInformationPage`, `ChangePasswordPage`, `LanguageSettingsPage`, `MedicalInformationPage`, `EmergencyContactsPage` |
| `/payment-methods`, `/billing-history`, `/upgrade-plan` | Payment*, Billing*, UpgradePlan | `BillingPage` (combined hub) |
| `/mental-health` | MentalHealth | Mental health |
| `/profile`, `/edit-profile`, … | Profile, EditProfile, … | `SettingsPage` (hub), `EditProfilePage` |
| `/notifications`, `/privacy`, `/help-support` | … | `NotificationsPage` (patient API), `PrivacyPage`, `HelpSupportPage` |
| `/ai-assistant` | AIAssistant | `AiAssistantPage` (chat bubbles with in-session history; patient API chat) |
| `*` | NotFound | Optional error page |

Shell route names in MAUI may differ; this table is for **screen inventory**, not a 1:1 path string match.

---

## Screen parity checklist

Tick when the **MAUI** screen is product-complete (layout, type, colors, spacing, primary controls) under the **Premium soft** identity. Concept files are structural references only.

| Screen | Concept file | MAUI target | Done |
|--------|----------------|-------------|------|
| Welcome | `Welcome.tsx` | `LandingPage` (Premium soft) | ☑ |
| Home | `Home.tsx` | `HomePage` (Premium soft dashboard) | ☑ |
| Register (3 methods) | `Register.tsx` | `RegisterOptionsPage` (Email / Phone / Voice cards) | ☑ |
| Email register | `EmailRegister.tsx` | `RegisterEmailPage` | ☑ |
| Phone register / verify | `PhoneRegister.tsx`, verify | `RegisterPhonePage`, `VerifyPhonePage` | ☑ |
| Voice register | `VoiceRegister.tsx` (full animated mock) | `RegisterVoiceIntroPage` then phone then `VoiceSubmitPage` (mic record + level bars + API) | ☑ |
| Account created | `AccountCreated.tsx` | `AccountCreatedPage` (shared welcome after phone OTP, email verify sign-in, or voice profile success) | ☑ |
| Login / … | `Login.tsx`, … | `SignInPage` (Premium soft), `VerifyEmailPage` | ☑ |
| Devices | `ConnectedDevices.tsx` | `DevicesPage` | ☑ |
| Family members | `FamilyMembers.tsx` | `FamilyMembersPage` (+ add / detail) | ☑ |
| Mental health | `MentalHealth.tsx` | `MentalHealthPage` (API content + mood check-in) | ☑ |
| Profile hub | `Profile.tsx` | `SettingsPage` + `ProfileHubViewModel` (Premium soft) | ☑ |
| Edit profile | `EditProfile.tsx` | `EditProfilePage` (local store + JWT hints) | ☑ |
| Privacy | `Privacy.tsx` | `PrivacyPage` (local toggles, delete flow) | ☑ |
| Help & support | `HelpSupport.tsx` | `HelpSupportPage` | ☑ |
| Notifications | (concept notifications) | `NotificationsPage` (list, mark read, push registration API) | ☑ |
| AI assistant | `AIAssistant` | `AiAssistantPage` (bubble thread; last 8 turns as model context; `POST api/patient/ai-assistant/chat`, no auto PHI) | ☑ |
| Appointments | `Appointments.tsx`, `BookAppointment.tsx`, `AppointmentDetails.tsx` | `AppointmentsPage`, `BookAppointmentPage`, `AppointmentDetailPage` | ☑ |
| Health records | `HealthRecords.tsx`, `HealthRecordDetails.tsx` | `RecordsPage`, `HealthRecordDetailPage` | ☑ |
| Insurance | `Insurance.tsx` | `InsurancePage` (+ add / detail) | ☑ |
| Billing | `PaymentMethods.tsx`, `BillingHistory.tsx`, `UpgradePlan.tsx` | `BillingPage`, `AddPaymentMethodPage` | ☑ |
| Request call | `RequestCall.tsx` | `RequestCallPage` | ☑ |
| Consultation | `Consultation.tsx` | `TelehealthJoinPage` (full-screen in-call UI; Agora on Android) | ☑ |
| Personal information | `PersonalInformation.tsx` | `PersonalInformationPage` | ☑ |
| Change password | `ChangePassword.tsx` | `ChangePasswordPage` (email: `api/patient/account/change-password`; Entra: SSPR) | ☑ |
| Language | `Language.tsx` | `LanguageSettingsPage` | ☑ |
| Medical information | `MedicalInformation.tsx` | `MedicalInformationPage` (`api/patient/medical-info`) | ☑ |
| Emergency contacts | `EmergencyContacts.tsx` | `EmergencyContactsPage` (`api/patient/emergency-contacts`; add from phone contacts or enter manually) | ☑ |

*(Extend the table as you implement each flow.)*

---

## Maintenance

1. After changing **`index.css`** or **`tailwind.config.ts`** in the concept repo, update the **token tables** above and **`Colors.xaml` / `Typography.xaml`** in MAUI.
2. Keep **one source of truth** for HSL in this doc until values are copied into XAML as **explicit** `Color`.
