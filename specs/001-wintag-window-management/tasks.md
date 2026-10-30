# Tasks: WinTag Window Management

**Input**: `specs/001-wintag-window-management/` spec, plan, research, data model and contracts.
**Prerequisites**: Approved task list plus Windows 11 evidence environment before execution.
**Status**: IMPLEMENTATION STARTED — T001 probe v0.1.3 fixes pre-connect AutoFlush; native validation pending. At explicit user request an isolated usable preview 0.2.0 exists under `previews/`, not gated production scaffolding. No task completed yet; see `docs/validation/preview-0.2.md`.
**Tests**: Explicitly requested rule/layout tests plus constitution-required safety/identity,
shortcut and manual compatibility coverage. Write tests first and observe expected failures.
**Organization**: Setup -> foundation -> P1 stories (US1/2/3/4/8) -> P2 (US5/6) -> P3 (US7).

## Format and paths

`- [ ] Tnnn [P?] [USn?] Description with exact file path`.
[P] means independent files within a declared wave AFTER listed prerequisites finish, not
permission to skip prerequisites. Setup/foundation/polish have no story label. T001 probe/self-test paths now exist; all production,
solution and T002+ test paths remain prospective. See docs/validation/coloring-spike.md for evidence.


## Phase 1: Setup (Shared Infrastructure)

**Goal**: Establish feasibility first, in the exact user-requested order; implementation approved, Windows proof still required.

**Independent test / checkpoint**: T001 and T002 record actual Windows evidence before production scaffolding; rejected strategies stay disabled.

- [ ] T001 Run the FIRST window-coloring Spike as an isolated disposable project at `spikes/WindowColoring/WindowColoring.csproj`; follow research section 1 for DWM caption/border first, mandatory text, click-through fallback, exact prior-color capture, guardian/main fault injection and total memory; record app/build matrix and what was actually proved in `specs/001-wintag-window-management/research.md` and `docs/validation/coloring-spike.md`. Do not enable unsafe DWM mutation (G1/G2/G8). (No implementation prerequisites; approval and Windows required.)
- [ ] T002 Build the SECOND task: minimal event-driven enumeration prototype at `spikes/WindowEnumeration/WindowEnumeration.csproj`; install OUTOFCONTEXT hooks before EnumWindows, merge buffered events, verify exactly-once real/minimized/remote-shell-cloaked windows versus ghosts/tools, title/destroy/reuse and no periodic refresh; document results in `docs/validation/enumeration-prototype.md` and update `specs/001-wintag-window-management/research.md` (G3). (Depends on: T001.)
- [ ] T003 Create `WinTag.sln`, `src/WinTag.Core/WinTag.Core.csproj`, `src/WinTag.Platform/WinTag.Platform.csproj`, `src/WinTag.App/WinTag.App.csproj` and the three test projects named in plan.md; use net10.0 core and net10.0-windows10.0.22000.0 Windows projects, pin SDK/package versions, review CsWin32/xUnit/conditional desktop dependencies and keep runtime offline. (Depends on: T002.)
- [ ] T004 [P] Prepare localized WPF resource dictionaries in `src/WinTag.App/Resources/Strings.fa.xaml` and `src/WinTag.App/Resources/Strings.en.xaml`, plus PerMonitorV2/asInvoker declarations in `src/WinTag.App/app.manifest`; no global render loop (NFR-005/006). (Depends on: T003.)
- [ ] T005 [P] Create evidence templates and privacy-redacted test instructions at `docs/validation/README.md`; include exact build+UBR, app versions, hardware, one-core CPU formula, all-process memory peaks and NOT RUN markers from research section 9. (Depends on: T003.)

## Phase 2: Foundational (Blocking Prerequisites)

**Goal**: Provide shared contracts, safe identity, validated config, events and guarded platform capabilities before all story work.

**Independent test / checkpoint**: Unit/fake tests pass; inventory prototype conclusions are incorporated; no persistent visuals without T001 proof; unsupported desktop capabilities disabled.

- [ ] T006 Define the documented service/result contracts in `src/WinTag.Core/Contracts/` matching `contracts/platform-services.md`, including IWindowInventory, IVisualService, ICrashCleanupCoordinator, IVirtualDesktopService, IHotZoneService, IShortcutManager, ILayoutService, IConfigurationStore and per-target partial/fallback outcomes. (Depends on: T004, T005.)
- [ ] T007 Define shared identity/configuration records in `src/WinTag.Core/Models/`; enforce "lease: must be revalidated before every cross-process action", "uncertain identity: skip and report; never match by title alone", "schemaVersion: exactly 1; unknown future versions rejected without overwriting" and "references: all referenced tag IDs must exist" from data-model.md. (Depends on: T006.)
- [ ] T008 [P] Write failing schema/reference/Unicode/import/interrupted-save tests in `tests/WinTag.Platform.Tests/ConfigurationStoreTests.cs`; assert prior valid config survives and no runtime download (FR-024/025, G9). (Depends on: T007.)
- [ ] T009 [P] Write failing same-process HWND reuse, destroy-generation, restart-uncertainty and per-action revalidation tests in `tests/WinTag.Platform.Tests/WindowLeaseTests.cs` using synthetic inventory/marker adapters (G2/G3/G7). (Depends on: T007.)
- [ ] T010 Implement validated single-writer JSON load/save/backup/import-candidate staging in `src/WinTag.Platform/Configuration/ConfigurationStore.cs`; satisfy T008, same-volume flush/atomic replace, explicit Reload only, invariant numbers, original Unicode, last-good preservation and schema v1 (FR-024/025). (Depends on: T008, T009.)
- [ ] T011 Implement CsWin32 hooks, live-lease validation and inventory at `src/WinTag.Platform/Windows/WindowInventory.cs` and `src/WinTag.Platform/Windows/WindowLeaseValidator.cs`; root callbacks, serialize buffered startup events, classify off-desktop cloaks correctly, invalidate destroyed generations and never poll (FR-001/002/029). (Depends on: T010.)
- [ ] T012 Evaluate current maintenance/license/dependencies and pin the desktop candidate at `src/WinTag.Platform/Desktops/VirtualDesktopService.cs`; run installed-build capability probes, STA serialization, unknown-build rejection and Explorer-restart tests, recording results in `docs/validation/desktop-adapter.md` and research.md; reject unsafe/unsupported mappings without calling undocumented COM (G4). (Depends on: T011.)
- [ ] T013 Implement proven crash-cleanup handshake and process/pipe waits in `src/WinTag.Platform/Safety/CleanupCoordinator.cs` and headless entry `src/WinTag.App/Guardian/GuardianHost.cs`; enforce "original values: exact or strategy disabled" and "mutation: forbidden before guardian acknowledgement"; if DWM was rejected, keep it disabled and omit guardian runtime rather than waiving safety (FR-028/029). (Depends on: T012.)
- [ ] T014 Compose single-instance application lifetime, redacted local diagnostics, no-network policy and pause/quit cleanup in `src/WinTag.App/App.xaml.cs`; headless guardian mode must not initialize WPF; release hooks/hotkeys and restore visuals idempotently; run foundational tests before story work (FR-023/028/029). (Depends on: T013.)

## Phase 3: User Story 1 — Identify Windows by Tags and Color (P1, MVP)

**Goal**: Expose ordered manual tags and safe title text+color without depending on rules/notch.

**Independent test / checkpoint**: Tag 20 windows, reorder/delete primary, verify text+color and input transparency, then exit/crash with exact restoration.

- [ ] T015 [P] [US1] Write failing ordered-tag/primary/deletion/manual-empty model tests in `tests/WinTag.Core.Tests/TagServiceTests.cs`; cover original appearance when last tag removed (FR-003/004). (Depends on: T014.)
- [ ] T016 [P] [US1] Write failing safety/strategy selection and prior-state contract tests in `tests/WinTag.Platform.Tests/VisualServiceTests.cs`; unsupported or unproven original-color capture must reject persistent mutation (FR-004/028). (Depends on: T014.)
- [ ] T017 [US1] Implement tag/assignment primitives in `src/WinTag.Core/Tags/TagService.cs` with "name: required, trimmed, nonempty, unique by normalized case-insensitive comparison", "color: #RRGGBB", "order: nonnegative integer; unique within tags" and "orderedTagIds: unique existing tag IDs, order preserved, empty allowed". (Depends on: T015, T016.)
- [ ] T018 [P] [US1] Implement the T001-approved no-activate transparent layered tag overlay in `src/WinTag.App/Overlays/TagOverlayWindow.xaml` and code-behind; genuine native click-through, mandatory RTL text, monitor/DPI tracking, no caption-control obstruction and no minimized/off-desktop display (FR-004/027). (Depends on: T017.)
- [ ] T019 [P] [US1] Implement safe per-app visual strategy selection at `src/WinTag.Platform/Visuals/WindowVisualService.cs`, using only strategies proved in T001 and guarding all persistent mutation through T013; rerun visual contract tests (FR-004/028/029). (Depends on: T017.)
- [ ] T020 [US1] Wire ordered manual tag/color editing into `src/WinTag.App/Views/TagEditorView.xaml` and `src/WinTag.App/ViewModels/TagEditorViewModel.cs`; expose tray/settings access before notch exists, persist changes and update live visuals (FR-003/004/024). (Depends on: T018, T019.)
- [ ] T021 [US1] Record US1 identification, assignment-time, primary deletion and original-style/crash results in `docs/validation/us1-tags.md`; test Comet/Firefox/Obsidian/Notion plus representative Win32/WPF/WinUI, each requested identification <2 s, assignment <3 s and <=3 commands (SC-001/009/014/015). (Depends on: T020.)

## Phase 4: User Story 2 — Automate Without Losing Manual Choices (P1)

**Goal**: Implement deterministic first-match rules with protected manual ordered lists, including empty overrides.

**Independent test / checkpoint**: Overlapping rules, title changes, reruns and WinTag restart cannot overwrite verified live manual assignments.

- [ ] T022 [US2] Write failing first-match/priority-tie/AND/regex-timeout/manual-order/manual-empty tests in `tests/WinTag.Core.Tests/RuleEngineTests.cs`, including explicit ResumeAutomatic and no-match clearing automatic only (FR-005/006). (Depends on: T021.)
- [ ] T023 [US2] Implement rule definitions/validation in `src/WinTag.Core/Rules/RuleDefinition.cs`; enforce "priority: integer; ascending value wins; listOrder resolves ties", "conditions: nonempty; AND-combined; process/class/title with explicit match mode" and unique existing ordered output tag IDs (empty allowed). (Depends on: T022.)
- [ ] T024 [US2] Implement bounded matching in `src/WinTag.Core/Rules/RuleEngine.cs`; case-insensitive process/class, title substring or explicit regex/case flag, 50 ms regex timeout off UI/hook threads, no implicit result union (FR-005, NFR-007). (Depends on: T023.)
- [ ] T025 [US2] Implement automatic/manual ownership transitions at `src/WinTag.Core/Tags/AssignmentCoordinator.cs`; enforce "Manual: immutable under rule evaluation, including an empty orderedTagIds list" and release only on explicit ResumeAutomatic (FR-006). (Depends on: T024.)
- [ ] T026 [US2] Add rule list/condition/priority editor and local errors at `src/WinTag.App/Views/RuleEditorView.xaml` and `src/WinTag.App/ViewModels/RuleEditorViewModel.cs`; preserve last valid config for invalid pattern/timeout (FR-005/024/025). (Depends on: T025.)
- [ ] T027 [US2] Connect inventory deltas/config commits/manual release to rule reevaluation in `src/WinTag.App/Coordination/RuleEventCoordinator.cs` and record `docs/validation/us2-rules.md`; no periodic rule rerun; prove SC-008. (Depends on: T026.)

## Phase 5: User Story 3 — Duplicate-Free Notch and Search (P1)

**Goal**: Provide event-triggered right-edge access, one entry per window, search/grouping and complete keyboard navigation.

**Independent test / checkpoint**: Compare 50-window inventory across flat/group/search modes; all monitors, dwell, scroll-only and fullscreen cases behave exactly as specified.

- [ ] T028 [P] [US3] Write failing unique grouping/untagged/secondary-tag/Persian-normalization tests in `tests/WinTag.Core.Tests/WindowSearchTests.cs`; normalize search copy only, not original text or regex (FR-002/011/012). (Depends on: T027.)
- [ ] T029 [P] [US3] Write failing outer/shared/partial-edge geometry, cancellable dwell, wheel-only and per-monitor fullscreen state-machine tests in `tests/WinTag.Core.Tests/HotZoneTests.cs` (FR-007–010). (Depends on: T027.)
- [ ] T030 [US3] Implement cached monitor topology/fullscreen and event-driven mouse eligibility in `src/WinTag.Platform/Input/HotZoneService.cs`; "widthDip: positive finite number", "heightFraction: >0 and <=1", "dwellMs: integer >=0"; defaults 6 DIP, 0.30, 0/250 ms; fast dedicated hook with one-shot eligible dwell, no polling (G5). (Depends on: T028, T029.)
- [ ] T031 [US3] Implement unique inventory projection/grouping/search in `src/WinTag.Core/Search/WindowSearchService.cs`; primary group only, all chips searchable, original Unicode display retained (FR-002/011/012). (Depends on: T030.)
- [ ] T032 [US3] Implement reusable notch view/view-model at `src/WinTag.App/Views/NotchWindow.xaml` and `src/WinTag.App/ViewModels/NotchViewModel.cs`; text+color chips, title, empty/error states, no opening animation delaying input, right monitor placement (FR-007/011). (Depends on: T031.)
- [ ] T033 [US3] Wire mouse/focus/keyboard lifecycle and single-window activation in `src/WinTag.App/Coordination/NotchController.cs`; Esc/combined-region exit, pointer hover does not steal focus, keyboard opening stays usable, arrows/Enter/Tab actions and activation refusal reporting (FR-010/012). (Depends on: T032.)
- [ ] T034 [US3] Record exact-once Snap Group/multi-tag/minimized/remote-window and hot-zone/scroll/fullscreen checks at `docs/validation/us3-notch.md`; include 50-window <100 ms opening and mark failed native cases honestly (SC-002/003/004). (Depends on: T033.)

## Phase 6: User Story 4 — Gather/Preserve Recall and Editable Shortcuts (P1)

**Goal**: Ship both distinct recall modes and independently editable direct/picker keys with persistent, conflict-safe settings.

**Independent test / checkpoint**: Distribute tag members across two desktops; gather moves all supported members here; preserve moves none; edit all bindings, restart, verify collisions retain old working bindings.

- [ ] T035 [P] [US4] Write failing gather/preserve/remote-only/empty/partial/focus-denial tests in `tests/WinTag.Core.Tests/RecallCoordinatorTests.cs` (FR-014/016). (Depends on: T034.)
- [ ] T036 [P] [US4] Write failing independent direct/picker rebinding, duplicate chord, registration failure and save-rollback tests in `tests/WinTag.Platform.Tests/ShortcutManagerTests.cs`; assert persistence across restart (FR-013/024/025). (Depends on: T034.)
- [ ] T037 [US4] Implement RegisterHotKey/MOD_NOREPEAT transactional registration at `src/WinTag.Platform/Input/ShortcutManager.cs`; enforce "chord: unique among enabled WinTag bindings" and "mode: Gather or Preserve; no implicit mode switching on failure"; reserve replacement before releasing old binding (FR-013). (Depends on: T035, T036.)
- [ ] T038 [US4] Implement both modes in `src/WinTag.Core/Recall/RecallCoordinator.cs`; Gather moves supported members to current desktop then restores/raises, Preserve never moves and offers explicit remote selection/switch; no new desktop, one final focus target, per-window outcomes (FR-014, G6). (Depends on: T037.)
- [ ] T039 [US4] Implement mode-specific tag picker and remote-member navigation at `src/WinTag.App/Views/TagPickerWindow.xaml` and its view-model; Enter selects, Esc cancels, distinguish empty tag from remote-only Preserve (FR-013/014). (Depends on: T038.)
- [ ] T040 [US4] Implement settings shortcut editor in `src/WinTag.App/Views/ShortcutSettingsView.xaml` and its view-model; allow per-tag/per-mode/both-picker customization, Ctrl+Alt defaults Gather and Shift variants Preserve, conflict warnings and restart persistence (FR-013/024). (Depends on: T039.)
- [ ] T041 [US4] Record distributed-tag behavior, minimized restoration, OS-refused activation and all binding edit/restart/conflict cases at `docs/validation/us4-recall.md`; prove both paths of SC-005 and no silent fallback (G4/G6). (Depends on: T040.)

## Phase 7: User Story 8 — Persian, RTL and Safe Cross-Display Operation (P1)

**Goal**: Complete release-critical localization, access, DPI and recovery integration before lower-priority desktop/layout actions.

**Independent test / checkpoint**: All existing workflows complete keyboard-only in Persian/English on mixed DPI; main crash leaves zero changes; runtime is offline.

- [ ] T042 [US8] Add failing bidi/original-text/language-switch and negative-monitor/DPI conversion tests at `tests/WinTag.Core.Tests/LocalizationGeometryTests.cs`; never normalize stored/display titles (FR-026/027). (Depends on: T041.)
- [ ] T043 [US8] Complete Persian/English resources, FlowDirection and mixed-direction isolation in `src/WinTag.App/Localization/LocalizationService.cs` and the two `src/WinTag.App/Resources/Strings.*.xaml` files; no clipped labels (FR-026). (Depends on: T042.)
- [ ] T044 [US8] Implement per-monitor geometry and DPI transitions in `src/WinTag.Platform/Displays/MonitorGeometryService.cs`; handle negative origins and scale overlays/notch/hot zones without relocating their physical right-edge trigger (FR-027). (Depends on: T043.)
- [ ] T045 [US8] Add automation names, contrast-aware tag text, visible focus and keyboard action routing at `src/WinTag.App/Accessibility/AccessibilityCoordinator.cs` and affected views; record `docs/validation/us8-accessibility.md` (NFR-005/006). (Depends on: T044.)
- [ ] T046 [US8] Run native main-crash/guardian-loss/stale-window tests in `tests/WinTag.Windows.Tests/VisualRecoveryTests.cs` and `docs/validation/us8-recovery.md`; <=1 s total cleanup, exact original values, no WPF guardian initialization, no strategy enabled without proof (G1/G2). (Depends on: T045.)
- [ ] T047 [US8] Audit application and all dependencies for runtime network/telemetry/mapping downloads and record offline/traffic checks at `docs/validation/us8-privacy.md`; fix any paths in `src/WinTag.App/Coordination/RuntimePolicy.cs`; do not label blocked network as proof no requests were attempted (NFR-003). (Depends on: T046.)

## Phase 8: User Story 5 — Send to New Virtual Desktop (P2)

**Goal**: Create, move target set and switch through one user action with honest partial outcomes.

**Independent test / checkpoint**: One invocation creates exactly one desktop for nonempty targets; empty creates none; unknown-build/denied/partial cases are reported safely.

- [ ] T048 [US5] Write failing create-once/empty/closed/partial-move/switch-failure/non-destructive-rollback tests at `tests/WinTag.Core.Tests/DesktopTransferTests.cs` (FR-015/016). (Depends on: T047.)
- [ ] T049 [US5] Implement one-action transfer coordinator in `src/WinTag.Core/Desktops/DesktopTransferCoordinator.cs`; snapshot/revalidate members, create once, serialize moves and final switch, retain desktop when deleting it risks unrelated windows, return structured partial results (FR-015/016). (Depends on: T048.)
- [ ] T050 [US5] Expose tag/window transfer commands and localized capability explanations at `src/WinTag.App/Coordination/DesktopActionController.cs` and notch context actions; keep unsupported operations disabled with reason (FR-015/016/020). (Depends on: T049.)
- [ ] T051 [US5] Record actual installed-build single-window/tag transfer, empty/denied targets, switch animation and partial recovery at `docs/validation/us5-transfer.md`; verify SC-006 without hiding flicker or OS limitations (G4). (Depends on: T050.)

## Phase 9: User Story 6 — Named Layouts (P2)

**Goal**: Persist and restore safely identified still-open windows with exact normal geometry/state and reported fallback.

**Independent test / checkpoint**: Unchanged topology restores size/position within one logical pixel and desktop/state; closed/uncertain skip, missing topology reports fallback, no relaunch.

- [ ] T052 [US6] Write failing layout Unicode/version/geometry/min-max round-trip, stale lease, missing window/monitor/desktop and no-relaunch tests at `tests/WinTag.Core.Tests/LayoutSerializationTests.cs` and `tests/WinTag.Core.Tests/LayoutRestoreTests.cs` (FR-017–019). (Depends on: T051.)
- [ ] T053 [US6] Implement layout records/capture/serialization at `src/WinTag.Core/Layouts/LayoutService.cs`; "name: required, trimmed, nonempty, unique by normalized case-insensitive comparison", "rectangle: finite coordinates; width >0 and height >0", "showState: Normal, Minimized or Maximized"; store source DPI, normal rectangle, desktop and live lease (FR-017). (Depends on: T052.)
- [ ] T054 [US6] Implement Windows placement restore at `src/WinTag.Platform/Layouts/WindowPlacementAdapter.cs`; validate leases immediately, restore desktop/normal geometry/show state, skip uncertainty, report missing-topology/app-minimum-size deviations, never relaunch or guess by title (FR-018/019, G7). (Depends on: T053.)
- [ ] T055 [US6] Add named layout save/replace-confirm/delete/restore UI at `src/WinTag.App/Views/LayoutManagerView.xaml` and view-model, reachable from notch; show exact/fallback/skipped/failed results (FR-017–020). (Depends on: T054.)
- [ ] T056 [US6] Record real mixed-DPI/negative-origin/min-max/desktop restore, close-and-reopen lookalike, missing monitor/desktop results at `docs/validation/us6-layouts.md`; verify SC-007 within one logical pixel on unchanged topology (G7). (Depends on: T055.)

## Phase 10: User Story 7 — Group Actions, Tray and Settings (P3)

**Goal**: Complete ignore, normal-close/minimize, autostart, pause/resume and confirmed local settings transfer.

**Independent test / checkpoint**: Group actions leave unrelated windows unchanged and respect save prompts; ignore/pause remove visuals; valid import round-trips and failed import preserves settings.

- [ ] T057 [US7] Write failing ignore/tag-action/save-prompt/pause/import-confirmation tests at `tests/WinTag.Core.Tests/GroupManagementTests.cs` and `tests/WinTag.Platform.Tests/SettingsTransferTests.cs` (FR-020–025). (Depends on: T056.)
- [ ] T058 [US7] Implement process/window ignore matching and exclusion cleanup at `src/WinTag.Core/Windows/IgnoreService.cs`; newly ignored targets revert visuals and disappear from every management target set; invalid patterns preserve config (FR-022). (Depends on: T057.)
- [ ] T059 [US7] Implement eligible-target minimize and normal close at `src/WinTag.Platform/Windows/GroupActionService.cs`; normal app save/cancel respected, never force terminate, revalidate live leases and report per-target outcomes (FR-020/021). (Depends on: T058.)
- [ ] T060 [US7] Complete NotifyIcon tray lifecycle, autostart opt-in and pause/resume wiring at `src/WinTag.App/Tray/TrayController.cs` and `src/WinTag.Platform/Startup/AutostartService.cs`; pause reverses visuals and disables management keys/hooks while tray remains usable (FR-023/028). (Depends on: T059.)
- [ ] T061 [US7] Complete settings ignore/hot-zone/language preferences and confirmed export/import/reload at `src/WinTag.App/Views/SettingsWindow.xaml` and view-model; validate full candidates with ConfigurationStore, persist readable JSON and independent edited keys (FR-024/025). (Depends on: T060.)
- [ ] T062 [US7] Record group close/save prompts, minimize, ignore cleanup, tray/autostart/pause/resume and Persian JSON transfer at `docs/validation/us7-management.md`; invalid import and interrupted writes preserve last-good data (SC-013). (Depends on: T061.)

## Phase 11: Polish and Cross-Cutting Release Gates

**Goal**: Prove all requested acceptance outcomes without weakening safety/resource/privacy constraints.

**Independent test / checkpoint**: All SC-001–SC-015 have raw Windows evidence; no NOT RUN gate is treated as pass; full release still requires separate approval.

- [ ] T063 [P] Write end-user Persian/English setup/keyboard/configuration guidance and scope/limitations in `docs/user-guide.fa.md` and `docs/user-guide.en.md`; document both modes and every editable binding (FR-013/026). (Depends on: T062.)
- [ ] T064 [P] Create measurement harness/runbook at `tests/WinTag.Windows.Tests/PerformanceAcceptanceTests.cs` and `docs/validation/performance.md`; record 100 opens all <100 ms, 10 min idle <=0.6 s one-core CPU, combined private-working-set AND committed peaks <100,000,000 bytes with 50 windows (G8). (Depends on: T062.)
- [ ] T065 Re-run exit/crash/reused-handle/guardian failure and no-network regression checks at `docs/validation/release-safety.md`, then mark only actually passing cases in `specs/001-wintag-window-management/checklists/windows-validation.md` (G1/G2/G9). (Depends on: T063, T064.)
- [ ] T066 Complete full app/build/multi-monitor/DPI/virtual-desktop compatibility matrix at `docs/validation/compatibility.md`; update `specs/001-wintag-window-management/research.md` with proved methods, rejected strategies and remaining API limits (SC-015). (Depends on: T065.)
- [ ] T067 Prepare offline Windows packaging instructions at `docs/distribution.md` and platform policy in `src/WinTag.App/WinTag.App.csproj`; no updater/telemetry, no admin prompt by default, include only tested build mappings and required licenses. (Depends on: T066.)
- [ ] T068 Audit requirement-to-test coverage and run quickstart acceptance at `docs/validation/acceptance-report.md`; check every FR/NFR/SC, record full versus degraded capability, block full-v1 release for any failed/unrun gate, and obtain final human release approval. (Depends on: T067.)

## Dependencies and Execution Order

Primary ordered gate chain:

```text
T001 coloring Spike -> T002 event prototype -> T003 scaffold
-> (T004 || T005) -> T006 contracts -> T007 models -> (T008 || T009)
-> T010 config -> T011 inventory -> T012 desktop feasibility -> T013 safety -> T014 app lifetime
-> US1 -> US2 -> US3 -> US4 -> US8 -> US5 -> US6 -> US7
-> (T063 || T064) -> T065 -> T066 -> T067 -> T068
```

The conservative numbered execution order avoids shared WPF composition/resource conflicts.
Independent story tests may also be developed against the stable foundation contracts with
fixtures, but do not integrate incomplete story services or mutate shared composition files
in parallel. Native desktop/visual failures cannot be made 'green' by substituting fakes.
T001 may finish with a rejected DWM strategy; continue only with a proven compliant fallback.
T012 may finish with desktop capabilities disabled; full-desktop feature acceptance still fails
until supported-build evidence exists. Neither is an exception to the constitution.

### Story dependencies and independent validation

| Story | Required integration baseline | Independent result |
| --- | --- | --- |
| US1 | Foundation; proven visual strategy | Ordered tags and safe title text/color without notch/rules |
| US2 | US1 tag/assignment primitives | Rules vs protected manual lists, tested without notch |
| US3 | Inventory/tag services | Single-entry searchable notch, tested with fixture-assigned tags |
| US4 | Inventory, tags, desktop adapter; panel/picker integration | Gather and Preserve tested directly and with both pickers |
| US8 | All current P1 views plus safety foundation | Persian/RTL/keyboard/DPI/crash/privacy before P2 |
| US5 | Tags/inventory and proven desktop capabilities | Single-window or fixture-tag send action, no layout dependency |
| US6 | Inventory/geometry/desktop/config | Named restore without any rule/close-action dependency |
| US7 | Existing management UI/config services | Ignore/group actions/settings/lifecycle on isolated fixture set |

### Parallel execution examples (after prerequisites)

- Setup: T004 localized resources/manifest and T005 evidence guide after T003.
- Foundation: T008 configuration tests and T009 identity tests after T007.
- US1: T015 tag tests || T016 visual tests after foundation; T018 overlay || T019 platform visual
  service after T017, then integrate T020. They edit distinct projects/files and share contracts only.
- US2: run rule matching and assignment-ownership test cases in parallel within T022; production
  tasks remain serial because they share rule/assignment semantics. No false [P] marker added.
- US3: T028 search/group tests || T029 hot-zone tests after US2; later UI/controller integration serial.
- US4: T035 recall tests || T036 shortcut transaction tests after US3; UI/state integration serial.
- US8: localization and DPI test cases may run in parallel after US4; resource/view edits stay
  serialized to avoid shared-file conflicts. No independent implementation pair claimed.
- US5: transfer test cases use independent fake instances; native create/switch runs are serial
  on one interactive desktop session so they do not interfere with each other.
- US6: serialization and restore cases can run in parallel with isolated temp configs/desktops;
  production capture/restore integration is ordered. No [P] marker across dependent code tasks.
- US7: group/ignore and config-import cases can run with isolated fixtures; lifecycle/native
  save-prompt checks run serially. No parallel edits to shared settings/tray composition.
- Polish: T063 guides || T064 performance harness after story work, then native safety/matrix serial.

## Requirement traceability

| Requirement group | Primary task coverage |
| --- | --- |
| FR-001/002 | T002, T009, T011, T028, T031–034 |
| FR-003/004 | T001, T015–021 |
| FR-005/006 | T022–027 |
| FR-007–012 | T028–034, T042–045 |
| FR-013/014 | T035–041; editable keys/settings/restart/conflicts explicitly covered |
| FR-015/016 | T012, T048–051 |
| FR-017–019 | T052–056 |
| FR-020/021/022 | T057–059, T062 |
| FR-023 | T014, T060, T062 |
| FR-024/025 | T007/008/010, T036/040, T052/053, T057/061/062 |
| FR-026/027 | T004, T018, T042–045 |
| FR-028/029 | T001/009/011/013/014, T016/019, T046, T065 |
| NFR-001/002 | T001/002/005, T030/032/034, T064 |
| NFR-003/004 | T008/010/013/014, T046/047/065 |
| NFR-005/006 | T004/021/033/042–046/063/066 |
| NFR-007/008 | T022–027, T035/038/048–051, T052–056 |
| SC-001–015 | T021/027/034/041/046/047/051/056/062/064–068; see quickstart/checklist |

## Implementation Strategy

1. Implementation and proceeding with open Windows checks were explicitly approved. T001 is
   in progress; no task is checked off just because code compiles or docs exist.
2. Once approved, execute T001 then T002 in order on Windows and review feasibility evidence.
3. Complete foundation, then US1 as a minimal identification MVP with manual tags, title text/
   color and crash safety. No notch/rules required for this first useful increment.
4. Add remaining P1 flows, including both recall modes and US8 safety/localization validation.
5. Add P2 desktop transfer/layouts, then P3 management. Keep independent test fixtures for each.
6. Perform cross-cutting resource/safety/offline/compatibility acceptance; any unrun gate blocks
   full-v1 release. Commit logical changes, preserve originals, and never run the implement
   command automatically because tasks.md exists.
