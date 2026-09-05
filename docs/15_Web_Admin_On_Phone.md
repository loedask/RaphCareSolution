# Web admin on a phone

**Status:** Portal and Ops have phone and tablet breakpoints (top bar, forms, swipeable tables, hospital deck, fleet). Thin home-screen install is still later. Desktop remains the primary staff target.

## Goal

Staff should be able to use the **same** web admin on a phone (and tablet), not only a desktop browser.

This stays the Blazor WASM portal (`RaphCare.Web` hosted by `RaphCare.Portal.Host`). It is **not** a clinician MAUI app and **not** the patient phone app.

Patients keep **RaphCare.Mobile**. Staff keep the web admin. Do not duplicate hospital ops into Mobile unless product later scopes a clinician app.

## What to build when we pick this up

Order matters more than PWA branding.

1. **Responsive admin UI** (done for the main chrome). Layouts, tables, top bar, forms, and hospital / fleet screens tighten under 768px and 480px. Keep polishing edge pages as you hit them on a phone.
2. **Thin install** after the UI works on a phone. Web app manifest, icons, name, and optionally caching the WASM shell so repeat visits on clinic Wi-Fi are faster. `RaphCare.Portal.Host` already maps `.webmanifest` to the right content type; nothing serves a manifest today.
3. **Not a true PWA** as the product story. No offline clinical writes, no background sync queue, no staff listing in Play / App Store, no “admin app” that staff confuse with the patient app.

A responsive admin UI plus that thin install matters more than a true PWA.

## Why not a full PWA first

- Blazor WASM is a large download. Caching the shell helps. Caching **saves** (admissions, vitals, chart edits) is a clinical risk if staff think something persisted when it only queued.
- Entra sign-in and Agora video are easy to break in standalone / installed browser mode (redirects, camera, microphone).
- Service workers often leave people on an old build after you ship.

If we add a service worker later, keep it online-first: cache static assets only, with a clear update path.

## Current state

- Responsive admin UI is in place on Portal and Ops: phone breakpoints tighten the top bar, stack forms and actions, keep tables swipeable, and make hospital deck rails / segment tabs scroll horizontally.
- Not a PWA yet: no manifest, no service worker, no install tags.
- Thin home-screen install still later.

## Related

- Engineering checklist: Step 1 phone-usable admin `(partial)`.
- Partner checklist: Portal and Ops on a phone is Partial under hospital admin.
- Structure: [`02_Solution_Structure.md`](./02_Solution_Structure.md) (`RaphCare.Web` / `RaphCare.Portal.Host`).
- Patient app: [`09_Mobile_App_Guide.md`](./09_Mobile_App_Guide.md).
