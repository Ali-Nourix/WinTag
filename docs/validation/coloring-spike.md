# T001 Coloring Spike — Implementation and Evidence

Date: 2026-10-30. Status: **IN PROGRESS / WINDOWS VALIDATION BLOCKED**.

## Implemented

- Isolated WPF/C# .NET 10 Windows x64 probe with CsWin32 0.3.367 and dependency lock file.
- Separate disposable WPF target with known seeded caption/border colors and live-instance marker.
- Headless guardian mode with current-user-only random local pipe, original snapshot ack,
  guardian-owned serialized mutation, process wait and EOF-triggered restoration.
- Read-only foreign-window DWM getter probe; external DWM mutation remains disabled.
- Trial Persian/English text+color overlay, native click-through/no-activate styles and WinEvent
  tracking; foreground-only visibility and target-destroy/process-birth invalidation.
- Explicit main-process kill test and guardian PID for external guardian-loss injection.
- 18 portable pure-protocol checks and Windows cross-build/publish.

## Verified here

| Check | Result | Boundary |
| --- | --- | --- |
| Portable self-tests (v0.1.1) | PASS, 37 checks | No user32/dwmapi, named-pipe or recovery execution |
| Release cross-build | PASS, 0 warnings / 0 errors | Linux compiler with Windows reference packs |
| Windows x64 publish | PASS | Framework-dependent PE apphost and managed assembly produced |
| Windows app launch | NOT RUN | No Windows session on agent machine |
| DWM actual rendering/original recovery | NOT RUN | Foreign mutation deliberately disabled |
| Overlay click-through/geometry and Persian rendering | NOT RUN | Styles compiled, not verified by input |
| Main/guardian crash cleanup and <=1 s bound | NOT RUN | IPC/fault-injection runtime unverified |
| Native multi-monitor/DPI/desktop/idle/memory | NOT RUN | No substitute synthetic PASS |

## Windows result template (fill manually)

Tester/date/commit: NOT RECORDED. OS build+UBR/architecture: NOT RECORDED.
Theme/high contrast, monitor topology and DPI: NOT RECORDED.

| Target/app version | DWM getter HRESULT | Observed DWM | Overlay text/input | Exit/crash latency | Decision |
| --- | --- | --- | --- | --- | --- |
| Controlled WPF companion | N/A: exact seeded originals | NOT RUN | NOT RUN | NOT RUN | Pending |
| Comet | NOT RUN | External mutation disabled | NOT RUN | NOT RUN | Pending |
| Firefox | NOT RUN | External mutation disabled | NOT RUN | NOT RUN | Pending |
| Obsidian | NOT RUN | External mutation disabled | NOT RUN | NOT RUN | Pending |
| Notion | NOT RUN | External mutation disabled | NOT RUN | NOT RUN | Pending |
| Representative Win32/WPF/WinUI | NOT RUN | External mutation disabled | NOT RUN | NOT RUN | Pending |

Record raw HRESULTs, observed colors, click/drag/caption outcomes, main/guardian PIDs,
end-to-end recovery timing, combined background memory/CPU and any stale-handle rejection.
Do not include private titles in committed evidence unless explicitly approved.

## Blockers and next step

T001 is not complete. It must be run on Windows 11 using the probe README. Readback of foreign
caption/border setter attributes is uncertain and is not treated as safe original capture.
Until exact capture/recovery is proved, foreign DWM stays disabled and overlay-only must prove
compliance independently. Test simple centered placement against custom tabs/caption controls.
No production source, T002 or T003 has been created. All checklist markers remain read-only.

## v0.1.1 local-action and presentation update

- User feedback: previous badge was too prominent. Prior screenshot confirms rendering only;
  no private screenshot/title is committed and no native-safety gate is upgraded.
- Compact neutral text badge: <=92x18 DIP, eight graphemes plus ellipsis, muted 2-DIP accent.
- Added one-button LOCAL diagnostics and an optional transparent PowerShell launcher. Default
  has no target/guardian and is read-only apart from the explicitly selected report file.
- Controlled DWM test is opt-in on a newly created companion only; never a selected real app.
  Normal cleanup only, no automatic kill/crash. Existing reports and window state are preserved.
- Runtime report labels return-code checks versus unrun visual/click-through/crash/DPI/desktop/
  performance/traffic gates; productionAcceptance remains false regardless of individual Pass.
- Development suite: 37 portable checks PASS; source-first tests initially failed before new
  classes existed; after implementation they pass. Current Windows cross-build has zero warnings
  and errors. PowerShell launcher/interactive diagnostics/native visual checks are NOT RUN here.
- Reports reject network/UNC/device/junction paths and contain no private window titles, user
  or machine names. SHA-256 package manifest is corruption checking, not a digital signature.
- Recheck visual identification latency and legibility on actual apps; do not equate subtle
  styling or compiled native styles with usable click-through. T001 still unchecked.

## v0.1.1 user evidence and v0.1.2 diagnostic revision

User-provided local reports dated 2026-10-30 identify Windows 26200.8457 x64, .NET 10.0.11,
and probe v0.1.1. Both report Windows/standard-user/pure-protocol/DWM-composition checks Pass.
The non-opt-in run has no operational failures. The opt-in run records
`ControlledDwmOrCleanup: Failed / InvalidOperationException`, without a substep or native
HRESULT. This does NOT establish the root cause, a successful apply, or successful restoration.
No private raw report or screenshot is committed. Controller working-set snapshots were
177381376 and 200511488 bytes: above the product goal, but not idle/combined-process acceptance.

v0.1.2 adds schema-2 entered-stage diagnostics, safe child setup/guardian failure replies,
fixed error codes with DWM HRESULT where available, and separate cleanup failures without
masking earlier errors. Read-only defaults, standard-user operation, controlled-only DWM,
no network/upload and manual runtime gates remain unchanged. Manual and automatic target
launches both consume the updated protocol. No production implementation or T002 is added.

Development verification: 49 portable checks PASS; Windows x64 Release cross-build/publish
succeeded with zero warnings/errors. v0.1.2 Windows native execution is NOT RUN here.
Next evidence needed: one opt-in safe-action JSON from v0.1.2; do not elevate or weaken security.
T001 and every Windows-validation checkbox remain unchanged/incomplete.
