# Web admin on a phone

**Status:** Portal and Ops work on phone and tablet: responsive layout plus a thin home-screen install (web app manifest and icons). Desktop remains the primary staff target. This is not a full offline PWA.

## Goal

Staff should be able to use the **same** web admin on a phone (and tablet), not only a desktop browser.

This stays the Blazor WASM portal (`RaphCare.Web` hosted by `RaphCare.Portal.Host`). It is **not** a clinician MAUI app and **not** the patient phone app.

Patients keep **RaphCare.Mobile**. Staff keep the web admin. Do not duplicate hospital ops into Mobile unless product later scopes a clinician app.

## What shipped

1. **Responsive admin UI.** Layouts, tables, top bar, forms, and hospital / fleet screens tighten under 768px and 480px. Keep polishing edge pages as you hit them on a phone.
2. **Thin install.** `site.webmanifest`, 192/512 icons, theme color, and Apple touch icon on Portal and Ops. Staff can use the browser Add to Home Screen / Install app flow. Hosts already map `.webmanifest` to the right content type.
3. **Not a true PWA** as the product story. No service worker, no offline clinical writes, no background sync queue, no staff listing in Play / App Store, no “admin app” that staff confuse with the patient app.

## Why not a full PWA

- Blazor WASM is a large download. Caching the shell can wait until we have a clear update path.
- Sign-in redirects and Agora video are easy to break in standalone / installed browser mode (camera, microphone). Today staff use email and password; if Entra is turned on later, re-test install redirects then.
- Service workers often leave people on an old build after you ship.

If we add a service worker later, keep it online-first: cache static assets only, with a clear update path.

## Current state

- Responsive admin UI is in place on Portal and Ops.
- Thin home-screen install is available (manifest + icons). No service worker yet.
- After adding a home-screen shortcut, confirm email/password sign-in still works. Re-check video join on a phone if you use that flow.

## Related

- Engineering checklist: Step 1 phone-usable admin.
- Partner checklist: Portal and Ops on a phone.
- Structure: [`02_Solution_Structure.md`](./02_Solution_Structure.md) (`RaphCare.Web` / `RaphCare.Portal.Host`).
- Patient app: [`09_Mobile_App_Guide.md`](./09_Mobile_App_Guide.md).
