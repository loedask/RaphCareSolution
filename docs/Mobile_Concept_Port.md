# React concept → RaphCare.Mobile (design port)

**Concept app path (local):** `C:\laragon\www\raphcare-mobile-app-concept`

Use this doc to keep **visual parity** with the React UI when implementing **MAUI**. Update it when the concept’s `index.css` or `tailwind.config.ts` changes.

**Cursor rules:** `.cursor/rules/raphcare-mobile-concept-template.mdc`, `.cursor/rules/raphcare-mobile.mdc`

---

## What the concept is (do not port literally)

| In concept | Notes for MAUI |
|------------|----------------|
| **React 18 + Vite** | Use **.NET MAUI** pages / Shell, not WebView for the main app (unless you intentionally host a web surface). |
| **react-router-dom** | Map routes to **Shell** routes / `AppNavigator`—see route table below. |
| **@tanstack/react-query** | Use **Client** + ViewModels / async commands; no React Query. |
| **shadcn/ui + Radix UI** | Recreate **look** with **MAUI layouts**, **Shared Controls**, **Styles**—not Radix primitives. |
| **lucide-react** | Use **FontImageSource**, **PNG/SVG**, or a MAUI-compatible icon set that **matches** stroke/weight visually. |
| **framer-motion** | Optional subtle animations; match **duration/easing** where it matters (e.g. fade-up ~0.4s ease-out in Tailwind). |
| **i18n** (`src/i18n/`) | Align copy with **`AppResources.resx`** (and future `.es.resx` etc.)—same strings as `translations.ts` where applicable. |

**Port the design:** colors, typography, spacing, radii, shadows, borders, component layout, and states—not the JS framework.

---

## Fonts (concept → MAUI)

Loaded in concept via Google Fonts in `src/index.css`:

| Role | Family | Weights (concept) |
|------|--------|-------------------|
| **Display** | **Space Grotesk** | 400, 500, 600, 700 |
| **Body** | **DM Sans** | 300–700, italic |

**MAUI:** add font files under `RaphCare.Mobile/Resources/Fonts/` (or equivalent) and register in `MauiProgram` / `CreateFont` so labels match the concept. Until then, map to closest system fallback and document the gap here.

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

## Color tokens — `:root` (light) in `src/index.css`

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

**Also in XAML (no separate CSS variable):** `SurfaceColor` → `#FFFFFF`; `TextPrimaryColor` → `#141D2E`; `TextMutedColor` → `#929CAA` (lighter muted tier for captions; HSL `215 12% 62%`).

**Corner radius:** `--radius: 0.875rem` (**14px** at 16px/rem). Tailwind `rounded-lg` etc. derive from this (`md` = radius − 4px, …).

**Shadows (concept):** `--shadow-xs`, `--shadow-soft`, `--shadow-card`, `--shadow-elevated`, `--shadow-glow`, `--shadow-glow-lg` — replicate with MAUI shadow APIs or platform-specific drawables where close enough.

**Gradients:** `--gradient-brand`, `--gradient-teal`, `--gradient-card`, `--gradient-glass` — use **LinearGradientBrush** / existing **GradientButton** patterns to match angles and stops.

**Dark theme:** `.dark` block in `index.css` — when MAUI supports app dark mode, mirror the same variable set in a second resource dictionary.

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
| `/home` | Home | `HomePage` |
| `/appointments`, `/book-appointment`, `/appointment-details` | Appointments, BookAppointment, AppointmentDetails | Appointments feature |
| `/request-call`, `/consultation` | RequestCall, Consultation | Care / telehealth |
| `/records`, `/health-record-details` | HealthRecords, HealthRecordDetails | Records |
| `/devices` | ConnectedDevices | Devices |
| `/insurance` | Insurance | Insurance |
| `/family-members` | FamilyMembers | Profile / account |
| `/payment-methods`, `/billing-history`, `/upgrade-plan` | Payment*, Billing*, UpgradePlan | Billing |
| `/mental-health` | MentalHealth | Mental health |
| `/profile`, `/edit-profile`, … | Profile, EditProfile, … | Settings / profile |
| `/notifications`, `/privacy`, `/help-support` | … | Settings / shared |
| `/ai-assistant` | AIAssistant | AI |
| `*` | NotFound | Optional error page |

Shell route names in MAUI may differ; this table is for **screen inventory and parity**, not a 1:1 path string match.

---

## Screen parity checklist

Tick when the **MAUI** screen matches the concept in **layout, type scale, colors, spacing, and primary controls**.

| Screen | Concept file | MAUI target | Done |
|--------|----------------|-------------|------|
| Welcome | `Welcome.tsx` | | ☐ |
| Home | `Home.tsx` | `HomePage` | ☐ |
| Login / Register / … | `Login.tsx`, … | Auth feature | ☐ |
| … | … | … | ☐ |

*(Extend the table as you implement each flow.)*

---

## Maintenance

1. After changing **`index.css`** or **`tailwind.config.ts`** in the concept repo, update the **token tables** above and **`Colors.xaml` / `Typography.xaml`** in MAUI.
2. Keep **one source of truth** for HSL in this doc until values are copied into XAML as **explicit** `Color`.
