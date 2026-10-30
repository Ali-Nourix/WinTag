# Isolated usable preview — 0.2.0

The user explicitly asked to stop the diagnostic-only loop and build usable software. An isolated
experimental preview lives under previews/, NOT the gated production src/ scaffold. T001 remains
incomplete, T002 has not been certified, and no release/Windows checklist item is marked complete.
This does not waive the constitution or declare v1 acceptance. No unproven desktop or persistent
foreign-window visual strategy is enabled. A separate fixed Spike is included as an optional tool.

## What exists

C#/.NET10 WPF with NotifyIcon/Screen, CsWin32 pinned. Hook-before-snapshot keyed inventory;
window/process-birth/generation validation before requested actions; event-driven deltas;
right-edge mouse movement and cancellable one-shot shared-edge dwell; cached monitor topology;
Persian panel/search/grouping; tags/rules/manual protection; independently editable hotkey chords
with reserve/persist/commit rollback; local config+backup/import/export; pause/tray; normal close
and minimize. Optional own-HWND badge is off by default, narrowly styled and destroyed by process
exit. Its native style bits are read back before use; physical input remains unverified.

## Known incompleteness

No layouts/autostart/full English/tag-picker parity; Gather and all cross-desktop COM disabled.
Remote/cloaked classification is provisional. Manual assignments are session-only (safe generation
identity is deliberately not guessed on restart). Rules use 50ms regex bounds but run on UI,
so large rule/window sets may affect responsiveness. Pause suppresses activation/hotkeys and
badge while inventory remains subscribed; it is not a zero-hook pause. No Windows native test
has run in this environment and no timing/memory/input/coverage acceptance is claimed.

## Evidence

- 24 model/config/rule regression checks PASS on Linux.
- Windows x64 Release cross-build/publish succeeds, warnings-as-errors, zero warnings/errors.
- Coloring Spike v0.1.3: 51 portable/real-named-pipe checks PASS, including reproduction of
  pre-connect AutoFlush InvalidOperationException and a successful post-connect handshake.
- Source has no network/telemetry client, polling timer, DWM setter or undocumented desktop COM.
  Runtime network/CPU behavior remains unmeasured. The only timer is an eligible-edge dwell.
- Product native checklist is unchanged. Exact-once Snap/UWP handling, mixed DPI, click-through,
  activation refusals, simultaneous lifecycle stress and <100MB/<100ms requirements are NOT RUN.

Next work is driven by actual preview use and measured failures, not broad pass declarations.
