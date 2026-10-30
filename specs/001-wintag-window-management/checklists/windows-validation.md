# Windows Manual Validation Checklist

Every box is intentionally unchecked: NOT RUN in this Linux authoring environment.
For each case record date, tester, commit, Windows build+UBR, architecture, app version,
monitor topology/DPI, expected/observed, raw evidence path, pass/fail and capability limitations.
No checked requirement-quality box elsewhere means any physical case here has passed.

## Window/color/crash gates

- [ ] G1: Comet DWM+text/overlay, custom chrome, caption controls, active/inactive/min/max.
- [ ] G1: Firefox equivalent visual and original-state restoration matrix.
- [ ] G1: Obsidian equivalent visual and original-state restoration matrix.
- [ ] G1: Notion multiple similar/Persian titles; Win32/WPF/WinUI representative targets.
- [ ] G2: Exact original app-custom colors versus default sentinels; disable strategy if unreadable.
- [ ] G2: Normal exit, pause, unhandled exception and forced MAIN termination restore within 1 s.
- [ ] G2: Guardian cannot start, guardian exits, pipe breaks, main/guardian simultaneous loss limits.
- [ ] G2/G7: Target closes and HWND reused, including within same process; no wrong-window cleanup.

## Inventory/input/RTL

- [ ] G3: Initial snapshot/event race, title/location/minimize/destroy changes, burst/reentrancy.
- [ ] G3: 50 windows, Snap Group members, multi-tags, no proxies or duplicates; zero missing entries.
- [ ] G3: Real shell-cloaked off-desktop windows retained; ghosts/app/helper surfaces excluded.
- [ ] G5: Outer edge immediate; shared segment 250 ms; early crossing/leave cancels; wheel never opens.
- [ ] G5: Partially shared edges, negative monitor coordinates, 100/150/200% mixed scaling, topology change.
- [ ] G5: Fullscreen/game monitor suppresses its zone only; display/event changes refresh eligibility.
- [ ] G5: Event storm/hook callback latency and documented lost-hook limitation; no polling repair.
- [ ] RTL: Persian/English switch, mixed-direction titles, yeh/kaf/diacritics/ZWNJ search, labels intact.
- [ ] RTL: Complete keyboard-only flows, visible focus, contrast, automation names, Esc/pointer dismissal.

## Actions/config/layout

- [ ] G4: Installed-build wrapper license/dependency/mapping review, offline load, create/foreign move/switch/events.
- [ ] G4: Unknown build, Explorer restart, denied/elevated and pinned windows, partial moves, safe rollback.
- [ ] G6: Gather distributed tag -> current desktop, no new desktop, minimized restore, per-target refusal.
- [ ] G6: Preserve -> zero moves, current-desktop restore; remote-only picker, explicit switch, Esc cancel.
- [ ] G6: Change every direct/picker shortcut, restart persistence, collisions and persistence rollback.
- [ ] G4: One-action send window/tag -> new desktop, empty tag -> no desktop, partial outcome reporting.
- [ ] G7: Layout position/size within one logical pixel, desktop and min/max state; normalRect conversions.
- [ ] G7: Missing monitor/desktop fallback reported, closed and uncertain windows skipped; zero relaunch.
- [ ] Rules: AND, first-match/tie order, regex invalid/timeout, protected manual ordering and empty override.
- [ ] G9: JSON export/import Persian round-trip, unknown version/references/chords, interrupted atomic write.
- [ ] Management: Ignore reverts visuals; tag rename/delete/reorder; group minimize/normal close respects prompts.
- [ ] Lifecycle: Autostart opt-in, tray visibility, pause/resume, single instance, shortcut/hook release.

## Measured release criteria

- [ ] SC-001/014: 20-window identification each <2 s; tag assignment <3 s and <=3 input commands.
- [ ] G8: 100/100 notch trials <100 ms after eligibility; dwell measured separately.
- [ ] G8: 10-minute idle total CPU <=0.6 s (one-core <=0.1%); no recurring polling/animations.
- [ ] G8: Main+guardian peak private working set AND committed bytes each <100,000,000 with 50 windows.
- [ ] G9: Zero runtime network connections/telemetry, disconnected core features, no online mapping resolution.
- [ ] Release: SC-001 through SC-015 all have evidence; degraded mode is not full-v1 acceptance.
