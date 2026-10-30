# Implementation Plan: WinTag Window Management

**Branch**: `main` (publication target; setup script resolves logical feature label
`001-wintag-window-management`, not a created Git branch) | **Date**: 2026-10-30 |
**Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-wintag-window-management/spec.md`.

## Summary

Build an offline, event-driven Windows 11 tray app with ordered multi-tags, safe title-bar
text/color, a duplicate-free right-edge notch, two independently rebindable recall modes,
virtual-desktop actions and named layouts. Use C#/.NET 10 LTS + WPF/CsWin32; retain .NET 8+
compatibility intent without starting on a nearly end-of-support runtime. No app code is part
of this phase. Research/design decisions are ready for task breakdown; actual Windows evidence
is pending and blocks enabling affected features and full-v1 release.

## Technical Context

**Language/Version**: C# with .NET 10 LTS; Windows target `net10.0-windows10.0.22000.0` for
app/platform, `net10.0` for core/unit tests; OS runtime guard Windows 11. No promise that the
minimum TFM build implies undocumented-desktop support.

**Primary Dependencies**: WPF, Microsoft.Windows.CsWin32, System.Windows.Forms.NotifyIcon,
System.Text.Json; xUnit for unit tests. Slions.VirtualDesktop core is first conditional
candidate behind an interface, not an approved any-build dependency. Pin versions in T003/T010.

**Storage**: `%AppData%\WinTag\config.json`, readable UTF-8 schema v1; last-good local backup,
local cleanup state only if needed; no service/database/network/telemetry.

**Testing**: xUnit rule/manual precedence, search, serialization, geometry and identity tests;
fake adapter tests for outcomes, shortcut replacement and crash protocol; Windows-only fault
injection plus manual multi-monitor/DPI/desktop/target-app matrix.

**Target Platform**: Windows 11, initial x64 validation on the user's exact build+UBR;
other architectures/builds are unverified until added to the tested matrix. No Windows 10 support.

**Project Type**: Native desktop/tray application with headless guardian execution mode.

**Performance Goals**: <100 ms notch input-ready with 50 windows; <=0.1% one-core idle CPU
under research protocol; total background memory <100 MB including guardian; visual cleanup
<=1 second after forced main exit. These are planned gates, not measured achievements.

**Constraints**: Event-driven; zero periodic polling; local-only; no DLL injection/Explorer hooks;
full Persian/RTL and keyboard access; preserve manual tags; reversible visual changes.

**Scale/Scope**: One local user, many windows/monitors/desktops; acceptance workload 20 windows
for identification and 50 for performance, not an artificial 50-window product limit.

## Constitution Check

*Design gate before research and rechecked after design. Evidence gates remain separate.*

| Principle | Before research | After design | Implementation/release gate |
| --- | --- | --- | --- |
| I. Lightweight/event-driven | Compatible input | PASS by design: WinEvents, mouse events, process waits; bounded dwell only | G3/G5/G8 must pass |
| II. Local-only privacy | Compatible input | PASS by design: local files/IPC, pinned dependencies, no updater | G9/offline traffic verification |
| III. Fail-safe reversibility | Risk identified, not waived | PASS as conditional design: no DWM mutation before exact-restore + guardian proof | G1/G2 block unsafe strategy; overlay-only still tested |
| IV. Persian/RTL/accessibility | Required | PASS by design: resources, FlowDirection, textual chips, keyboard/focus | Physical target-app/DPI checklist |
| V. Readable config | Required | PASS by design: versioned UTF-8 JSON, validation, transactional save/rebinding | Unit tests and interrupted-write checks |

No detected principle violation is waived. 'PASS by design' is not runtime proof. If exact
DWM restoration cannot be established, the DWM strategy is rejected, not shipped with a
best-effort reset. If no compliant title-label strategy exists, affected feature acceptance
fails. Unknown COM build remains disabled and degraded desktop mode does not satisfy full v1.

The user's latest instruction authorizes consecutive phase execution with summaries; this
satisfies phase approval checkpoints for clarify/plan/tasks, not approval to implement.

## Project Structure

### Documentation (this feature)

```text
specs/001-wintag-window-management/
├── spec.md
├── clarification-review.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/requirements.md
├── checklists/windows-validation.md
├── contracts/configuration.md
├── contracts/platform-services.md
├── contracts/interaction.md
└── tasks.md                         # Generated in the next phase only
```

### Source Code (repository root)

Planned paths only; none of these application/source/test projects is created in this run.

```text
WinTag.sln
src/
├── WinTag.Core/                     # Domain, rules, search, layouts, contracts
├── WinTag.Platform/                 # CsWin32, inventory, input, desktop adapter, config I/O
└── WinTag.App/                      # WPF/tray/settings/overlays + headless guardian mode
spikes/
├── WindowColoring/                  # T001 isolated throwaway probe
└── WindowEnumeration/               # T002 isolated event-driven prototype
tests/
├── WinTag.Core.Tests/
├── WinTag.Platform.Tests/
└── WinTag.Windows.Tests/
docs/validation/
```

**Structure Decision**: Three production projects separate testable logic, Windows interop and
WPF. Guardian uses the existing executable without WPF initialization rather than adding a
resident service. Probes are isolated temporary projects so the required first two tasks do
not depend on later solution scaffolding. Add no speculative web backend or plugin framework.

## Architecture and event flow

- App owns tray lifetime, WPF dispatcher and single-instance coordination. Core is UI/OS-free.
  Platform implements contracts in [platform-services.md](contracts/platform-services.md).
- Inventory registers hooks before EnumWindows, merges buffered events, and emits immutable
  snapshots/deltas. A serial coordinator prevents reentrant edits and stale-window actions.
- LiveWindowLease includes process birth, app/session incarnation and verified per-window
  generation/marker. Titles/class are matching attributes, never a sufficient stable identity.
- Rule engine consumes snapshot attributes; first matching rule wins, protected manual lists
  are never overwritten. Tag primary order drives color and the sole group placement.
- HotZoneService caches monitor geometry/fullscreen state and reacts only to mouse movement;
  click/keyboard entry controls panel focus. Dwell is cancellable and single-shot.
- VisualService selects per-app proven DWM+text or overlay-only. Guardian handshake and original
  value acknowledgement precede persistent mutation. Main-owned overlays die with main process.
- VirtualDesktopService does public identification where possible and a build-gated local COM
  wrapper for create/move/switch, serialized on STA. Do not blindly discard remote shell-cloaked
  real windows. Isolate failures and return partial outcomes.
- RecallCoordinator exposes Gather and Preserve. ShortcutManager reserves new chords before
  releasing old bindings and persists only a validated complete change. Settings edits and
  JSON imports update the same source of truth, including mode-specific picker bindings.
- LayoutService snapshots logical geometry + WindowPlacement and verified live leases; restore
  rechecks each lease, skips uncertain/closed windows, reports missing-monitor/desktop fallback.
- ConfigurationStore validates complete candidates then atomically commits; no background file
  polling. External edits take effect at explicit Reload or next start; local imports ask consent.

## Validation and evidence gates

See research sections 1–10 for citations and methods and
[windows-validation.md](checklists/windows-validation.md) for pending physical checks.

1. T001 runs the coloring/text/crash Spike after implementation approval; rejected DWM strategy
   may fall back to overlay-only if that independently passes all requirements.
2. T002 establishes event inventory correctness (including remote-desktop cloak distinctions),
   no recurring refresh and identity safety. Its findings update research before production work.
3. Guarded desktop adapter feasibility is tested on exact build before desktop actions are enabled.
4. Unit/fake tests cover rule order/regex/manual empty overrides, layouts, invalid JSON, stale
   identities, shortcut edit/rollback and structured partial results; Windows checks establish
   native behavior that fakes cannot prove.
5. Full release requires all SC-001–SC-015, including both shortcut modes, offline operation,
   visual reversibility and resource budgets. Capture raw results without private window titles
   unless the user explicitly enables that local diagnostic capture.

## Complexity Tracking

No constitution violations accepted; no exception table entries. Guardian complexity is required
only for persistent cross-process mutation; if DWM is rejected and overlays suffice, omit the
guardian's runtime use while retaining deterministic cleanup contracts and failure tests.
