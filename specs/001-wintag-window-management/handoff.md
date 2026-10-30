# Documentation Handoff — WinTag

Date: 2026-10-30. Publication target: main.

- Clarify complete: no new questions; both recall modes and editable shortcuts confirmed.
- Plan complete as a conditional design with 8 generated artifacts; no Windows feasibility
  claim. Research first item is the coloring Spike. No unresolved clarification markers.
- Tasks complete: 68 unchecked tasks, 14 [P] tasks, exact paths and earlier-task dependencies.
  T001 is the coloring Spike; T002 is the minimal event-driven enumeration prototype.
- Story counts: US1 7; US2 6; US3 7; US4 7; US8 6; US5 4; US6 5; US7 6; shared 20.
- All local Markdown links, fence pairs, IDs, task format and dependency ordering reviewed.
  Requirements quality remains 16/16; this is not proof of runtime correctness.
- Windows evidence gates G1–G9 remain NOT RUN; no Windows build is certified. Exact DWM
  restoration/crash cleanup and current-build desktop wrapper compatibility block enabling
  their capabilities. Resource budget is a release gate, not a claimed result.
- No app source, test source, solution or probe projects were created; no implement command,
  compiler, unit test, prototype or Spike was run. Only Spec Kit scaffolding and documentation.
- Next step: obtain explicit implementation approval, then execute T001 and T002 in order
  on a Windows 11 test machine. Do not auto-implement based on this handoff.

## Superseding implementation update — 2026-10-30

The user approved starting implementation and proceeding despite pending Windows checks.
The earlier documentation-only snapshot above is historical, not current implementation status.
T001 probe and 18 portable protocol checks are implemented; Windows x64 cross-build/publish
succeeds. T001 remains unchecked pending actual Windows validation. T002 and all production
projects are unstarted. See `docs/validation/coloring-spike.md`; never equate compilation with
DWM/input/crash/performance correctness.

## Superseding v0.1.1 update — compact indicator / safe local action

User requested a less intrusive badge and selected LOCAL Windows diagnostics, not GitHub
Actions. Compact neutral <=92x18 DIP badge and local-report button implemented; optional
PowerShell launcher respects security policy, verifies package checksums and never elevates.
Default action is read-only except a newly selected report. Controlled companion test is opt-in,
crash tests are never automatic, no existing app is closed or moved. Reports omit private
identifiers and leave manual/native acceptance gates NotRun; no automatic upload.
37 portable checks pass; Windows x64 Release build/publish succeeds without warnings/errors.
Current interactive action/launcher/native styling NOT RUN by this agent; T001 remains open.

## Usable preview 0.2.0 and guardian fix 0.1.3

User requested immediate usable build rather than diagnostic-only iteration. Isolated preview
under previews/WinTag.Preview provides tray, edge panel, search/tags/rules/configurable shortcuts,
current-desktop Preserve, group normal close/minimize and opt-in own-window badge. No production
checklist gate is upgraded. Desktop COM/Gather, layouts, autostart and durable manual identity
remain absent/disabled. Read preview README and docs/validation/preview-0.2.md for exact limits.

New user report localizes InvalidState to GuardianSpawn. Real-pipe reproduction confirms
StreamWriter AutoFlush before connection throws. Spike v0.1.3 now waits for connection before
constructing streams; 51 portable/IPC checks pass. Windows DWM evidence remains pending.
