# RaphCare.Mobile design lock and screen map

**Live look and feel:** **Premium soft** in MAUI.  
**Do not** use the old React Vite mockup as a visual target.

| Layer | Source of truth |
|-------|-----------------|
| **Look and feel** | `RaphCare.Mobile/Resources/Styles/` (`Colors.xaml`, `Typography.xaml`, `Cards.xaml`, `Buttons.xaml`, `Inputs.xaml`) and shared controls under `Core/Common/Controls/` |
| **Screen inventory** | Tables below (MAUI pages and Shell routes) |
| **Copy** | `AppResources.resx` (and locale satellites) |
| **Architecture** | `.cursor/rules/raphcare-mobile.mdc`, `docs/09_Mobile_App_Guide.md` |

**Cursor rules:** `.cursor/rules/raphcare-mobile.mdc`, `.cursor/rules/raphcare-mobile-concept-template.mdc` (archive note only).

---

## Premium soft (locked)

Calm patient-app chrome: soft page wash, teal and navy brand, larger radii (about 16 to 20), gentler shadows, more breathing room. Keep healthcare readable. Prefer shared styles over one-off colors in page XAML.

**Fonts (MAUI):** Space Grotesk for display titles; DM Sans for body. Registered in `MauiProgram.ConfigureFonts`.

**Gradients:** navy to teal brand washes; teal to accent for primary CTAs (`GradientButton`, `LinearGradientBrush`).

**Dark mode:** not the default. If added later, add a second resource dictionary from the live Premium soft palette, not from any old mockup theme.

---

## Old React mockup (archive only)

Local path (optional, outside this repo): `C:\laragon\www\raphcare-mobile-app-concept`.

That Vite app was an early layout sketch. It is **not** the design system. Do **not**:

- Match its colors, type scale, radii, shadows, or density
- Sync `Colors.xaml` or `Typography.xaml` from its `index.css` / Tailwind config
- Mark mobile work “done” based on mockup pixel parity

You may glance at it only for historical screen names when reading old notes. Prefer the MAUI inventory below.

---

## Feature area ↔ MAUI screens

| Area | MAUI targets |
|------|----------------|
| Welcome / auth | `LandingPage`, `SignInPage`, `RegisterOptionsPage`, `RegisterEmailPage`, `VerifyEmailPage`, `RegisterPhonePage`, `VerifyPhonePage`, `RegisterVoiceIntroPage`, `VoiceSubmitPage`, `AccountCreatedPage`, `ForgotPasswordPage` |
| Home | `HomePage` |
| Appointments | `AppointmentsPage`, `BookAppointmentPage`, `AppointmentDetailPage` |
| Care / video | `RequestCallPage`, `CareTelehealthPage`, `TelehealthJoinPage` |
| Records | `RecordsPage`, `HealthRecordDetailPage` |
| Devices | `DevicesPage` (BLE; see `docs/11_Devices_BLE_E580_E585.md`, `docs/13_Patient_Device_Packages_and_Fleet.md`) |
| Insurance | `InsurancePage`, add / detail pages |
| Family | `FamilyMembersPage`, add / detail |
| Mental health | `MentalHealthPage` |
| Profile / settings | `SettingsPage` (`ProfileHubViewModel`), `EditProfilePage`, `PersonalInformationPage`, `ChangePasswordPage`, `LanguageSettingsPage`, `MedicalInformationPage`, `EmergencyContactsPage`, `SelectClinicPage`, `PrivacyPage`, `HelpSupportPage`, `HelpFaqPage`, `SupportMessagePage` |
| Billing | `BillingPage`, `AddPaymentMethodPage` |
| Notifications | `NotificationsPage` |
| AI assistant | `AiAssistantPage` |

Shell route names may differ from folder names; use `AppNavigator` as the runtime map.

---

## Screen completeness checklist

Tick when the MAUI screen is product-complete under **Premium soft** (usable flow, real data where APIs exist, acceptable loading / empty / error). Not mockup parity.

| Screen | MAUI target | Done |
|--------|-------------|------|
| Welcome | `LandingPage` | ☑ |
| Home | `HomePage` | ☑ |
| Register (3 methods) | `RegisterOptionsPage` | ☑ |
| Email register | `RegisterEmailPage` | ☑ |
| Phone register / verify | `RegisterPhonePage`, `VerifyPhonePage` | ☑ |
| Voice register | `RegisterVoiceIntroPage` then `VoiceSubmitPage` | ☑ |
| Account created | `AccountCreatedPage` | ☑ |
| Sign in | `SignInPage`, `VerifyEmailPage` | ☑ |
| Devices | `DevicesPage` | ☑ |
| Family members | `FamilyMembersPage` (+ add / detail) | ☑ |
| Mental health | `MentalHealthPage` | ☑ |
| Profile hub | `SettingsPage` | ☑ |
| Edit profile | `EditProfilePage` | ☑ |
| Privacy | `PrivacyPage` | ☑ |
| Help and support | `HelpSupportPage` | ☑ |
| Notifications | `NotificationsPage` | ☑ |
| AI assistant | `AiAssistantPage` | ☑ |
| Appointments | `AppointmentsPage`, `BookAppointmentPage`, `AppointmentDetailPage` | ☑ |
| Health records | `RecordsPage`, `HealthRecordDetailPage` | ☑ |
| Insurance | `InsurancePage` (+ add / detail) | ☑ |
| Billing | `BillingPage`, `AddPaymentMethodPage` | ☑ |
| Request call | `RequestCallPage` | ☑ |
| Consultation | `TelehealthJoinPage` | ☑ |
| Personal information | `PersonalInformationPage` | ☑ |
| Change password | `ChangePasswordPage` | ☑ |
| Language | `LanguageSettingsPage` | ☑ |
| Medical information | `MedicalInformationPage` | ☑ |
| Emergency contacts | `EmergencyContactsPage` | ☑ |

---

## Maintenance

1. Change visuals in **`Resources/Styles/`** and shared controls first, then hero screens as needed.
2. Keep this screen map updated when you add or retire a patient screen.
3. Do not reintroduce the React mockup as a design dependency.
