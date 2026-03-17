# Patient Merge Event Pipeline Audit

**Date:** 2025-03-17  
**Scope:** `PatientMergedEvent` publish path, idempotency, handler discovery, side effects, and merge history consistency.

---

## 1. Event publication location

| Item | Status | Detail |
|------|--------|--------|
| Publish after persist | **Pass** | `PatientMergedEvent` is published only after `SaveChangesAsync()` succeeds. |

**Location:** `RaphCare.Persistence/PatientMergeService.cs`

- Lines 127–133: `PatientMergeHistory` is added and duplicate is soft-deleted in memory.
- Line 134: `await _unitOfWork.SaveChangesAsync(ct)` persists all changes (clinical, insurance, device, merge history).
- Line 138: `await _mediator.Publish(new PatientMergedEvent(...), ct)` runs only after a successful save.

**Conclusion:** Event is never published before database state is persisted. No risk of handlers seeing pre-commit or rolled-back state.

---

## 2. Registered handlers

| Handler | Assembly | Discovered by MediatR |
|---------|----------|------------------------|
| `PatientMergedEventLoggingHandler` | RaphCare.Application | Yes |

**MediatR registration:** `RaphCare.Application/DependencyInjection.cs`

- `AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly))` with `assembly = Assembly.GetExecutingAssembly()` (RaphCare.Application).
- `PatientMergedEventLoggingHandler` lives in `RaphCare.Application/Common/EventHandlers/` and implements `INotificationHandler<PatientMergedEvent>`.
- It is in the same assembly as the one registered, so it is discovered automatically. No manual registration required.

**Conclusion:** All current `INotificationHandler<PatientMergedEvent>` implementations are in the Application assembly and are registered via assembly scan. No handlers were found in other assemblies.

---

## 3. Merge idempotency

| Check | Before audit | After recommendation |
|-------|----------------|----------------------|
| Explicit guard against merging same duplicate twice | **Missing** | **Added** |

**Previous behavior:**  
`Patient` implements `ISoftDelete`. The persistence layer applies a global query filter so that `Patients` only returns rows with `IsDeleted == false`. So:

- First merge: duplicate is loaded, merge runs, duplicate is soft-deleted, event published.
- Second merge (same duplicate): `FindAsync(duplicatePatientId)` returned `null` (filtered out), so the code threw `"Duplicate patient not found."`

Idempotency was therefore **implicit** (retry caused an exception, not a second merge or second event). There was no explicit check such as `if (duplicate.IsDeleted) return;`.

**Change made:**  
In `PatientMergeService.MergePatientsAsync`:

- The duplicate is loaded with `IgnoreQueryFilters()` so soft-deleted rows are visible.
- If `duplicate.IsDeleted` is true, the method returns immediately (no-op). No second merge, no second `PatientMergeHistory` row, no second `PatientMergedEvent`.

**Conclusion:** The same duplicate cannot be merged twice. Duplicate events from re-invoking merge for the same pair are prevented.

---

## 4. Event side effects (handler safety)

| Handler | Mutates domain/DB? | Safe side effects |
|---------|--------------------|--------------------|
| `PatientMergedEventLoggingHandler` | No | Logging only |

**Review:** `PatientMergedEventLoggingHandler` only calls `logger.LogInformation(...)` with event data. It does not:

- Change domain or entity state.
- Call `SaveChanges` or any repository/DbContext write.
- Modify patient or merge history data.

**Conclusion:** Current handlers are limited to logging. They do not threaten merge consistency. Future handlers should only log, invalidate caches, or notify external systems, and must not mutate patient or merge data.

---

## 5. Merge history consistency

| Check | Status | Detail |
|-------|--------|--------|
| History written before event | **Pass** | `PatientMergeHistory` is added and saved in the same transaction as the merge; event is published after save. |

**Order in `PatientMergeService.MergePatientsAsync`:**

1. Related data moved from duplicate to primary (ExecuteUpdate/ExecuteDelete).
2. Duplicate soft-deleted in memory.
3. `PatientMergeHistory` row added to `_clinical.PatientMergeHistory`.
4. `SaveChangesAsync()` persists everything (including merge history).
5. `Mediator.Publish(PatientMergedEvent)` runs after successful save.

Handlers can therefore assume the merge and its audit row are committed when they run. No handler needs to create or update merge history.

---

## 6. Potential duplicate event risks

| Risk | Status | Notes |
|------|--------|--------|
| Same merge invoked twice (e.g. retry) | **Mitigated** | Explicit `if (duplicate.IsDeleted) return;` prevents second merge and second event. |
| Event published before SaveChanges | **None** | Publish is after `SaveChangesAsync()`. |
| Multiple handlers publishing same event | **None** | No handler publishes `PatientMergedEvent`. |
| Transaction rollback after publish | **None** | Single transaction committed before publish; no second transaction that could roll back. |

**Conclusion:** No remaining duplicate-event risks identified within the current merge and event pipeline.

---

## 7. Unsafe event handlers

| Handler | Status |
|---------|--------|
| `PatientMergedEventLoggingHandler` | Safe (logging only). |

No handlers that modify domain state or perform database updates were found. No unsafe event handlers identified.

---

## Summary

| # | Check | Result |
|---|--------|--------|
| 1 | Merge idempotency | Pass (explicit guard added: `if (duplicate.IsDeleted) return;`) |
| 2 | Event publish location | Pass (only after `SaveChangesAsync()`) |
| 3 | Event handler discovery | Pass (MediatR scans Application assembly; logging handler discovered) |
| 4 | Event side effects | Pass (handlers do not mutate domain/DB) |
| 5 | Merge history consistency | Pass (history inserted and saved before publish) |

**Deliverable summary**

- **Event publication location:** After `_unitOfWork.SaveChangesAsync(ct)` in `PatientMergeService.MergePatientsAsync` (line 138).
- **Registered handlers:** `PatientMergedEventLoggingHandler` (RaphCare.Application); discovered via `RegisterServicesFromAssembly(RaphCare.Application)`.
- **Duplicate event risks:** Addressed by explicit idempotency guard; no other duplicate-event risks found.
- **Unsafe handlers:** None; the only handler is logging-only.
