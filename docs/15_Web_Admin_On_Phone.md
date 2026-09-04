# Web admin on a phone

**Status:** product direction, not scheduled work. Hospital ops on the web remain complete for desktop use.

## Goal

Staff should be able to use the **same** web admin on a phone (and tablet), not only a desktop browser.

This stays the Blazor WASM portal (`RaphCare.Web` hosted by `RaphCare.Portal.Host`). It is **not** a clinician MAUI app and **not** the patient phone app.

Patients keep **RaphCare.Mobile**. Staff keep the web admin. Do not duplicate hospital ops into Mobile unless product later scopes a clinician app.

## What to build when we pick this up

Order matters more than PWA branding.

1. **Responsive admin UI** (the real work). Layouts, tables, side nav, forms, and tele join must be usable on a small screen. There is already a 768px breakpoint that stacks the sidebar and hides search. That is not phone-ready hospital ops.
2. **Thin install** after the UI works on a phone. Web app manifest, icons, name, and optionally caching the WASM shell so repeat visits on clinic Wi-Fi are faster. `RaphCare.Portal.Host` already maps `.webmanifest` to the right content type; nothing serves a manifest today.
3. **Not a true PWA** as the product story. No offline clinical writes, no background sync queue, no staff listing in Play / App Store, no “admin app” that staff confuse with the patient app.

A responsive admin UI plus that thin install matters more than a true PWA.

## Why not a full PWA first

- Blazor WASM is a large download. Caching the shell helps. Caching **saves** (admissions, vitals, chart edits) is a clinical risk if staff think something persisted when it only queued.
- Entra sign-in and Agora video are easy to break in standalone / installed browser mode (redirects, camera, microphone).
- Service workers often leave people on an old build after you ship.

If we add a service worker later, keep it online-first: cache static assets only, with a clear update path.

## Current state

- Not a PWA: no manifest, no service worker, no install tags in `RaphCare.Web/wwwroot/index.html`.
- Admin is a desktop-first shell (`AdminLayout`) with limited narrow-width CSS.

## Related

- Engineering checklist: later / optional row under Step 1.
- Partner checklist: “Later” note under hospital admin.
- Structure: [`02_Solution_Structure.md`](./02_Solution_Structure.md) (`RaphCare.Web` / `RaphCare.Portal.Host`).
- Patient app: [`09_Mobile_App_Guide.md`](./09_Mobile_App_Guide.md).
