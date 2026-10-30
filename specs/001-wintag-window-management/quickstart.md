# Quickstart and Validation Guide

Status: FUTURE commands, not executed and not runnable until approved tasks create the projects.
No application source, .sln or test project exists yet. Do NOT run implement based on this guide.

## Prerequisites

Windows 11 x64 with exact `winver` build+UBR recorded, .NET 10 SDK with Windows Desktop tooling,
Git, two monitors (include 100% and 150% scaling and negative-origin arrangement), two or more
virtual desktops, Comet/Firefox/Obsidian/Notion and Win32/WPF/WinUI targets. Use disposable test
documents for close/crash checks. No administrator requirement for normal use.
Development restore may use network; installed runtime must not. Local NuGet cache is required
for offline restore; do not mistake a cached development restore for proof of app privacy.

## Approval checkpoint and probe order

1. Obtain explicit implementation approval after reviewing tasks.md.
2. T001 creates/runs only the isolated coloring Spike in spikes/WindowColoring and updates
   research.md with build/app matrix, original-value capture, click-through and forced-exit results.
3. T002 creates/runs only the event enumeration prototype in spikes/WindowEnumeration and records
   exact-once inventory, cloaked remote windows, destroy/reuse and no-polling observations.
4. Reject unsafe strategies; complete foundation and build-gated adapter checks before enabling
   production visual/desktop capabilities. A disabled DWM strategy is not proof overlay-only works.

## Future build/test commands (after projects exist)

```powershell
dotnet --info
dotnet restore WinTag.sln
dotnet build WinTag.sln -c Release --no-restore
dotnet test tests/WinTag.Core.Tests/WinTag.Core.Tests.csproj -c Release
dotnet test tests/WinTag.Platform.Tests/WinTag.Platform.Tests.csproj -c Release
dotnet test tests/WinTag.Windows.Tests/WinTag.Windows.Tests.csproj -c Release
dotnet run --project src/WinTag.App/WinTag.App.csproj -c Release
```

## Validation sequence

- Start with safe visuals gated off. Create Persian tags, rules and manual-empty overrides;
  verify priority reruns cannot overwrite manual lists. Test Persian kaf/yeh/diacritics search.
- Use the proven visual strategy, then identify 20 requested windows and record each time <2 s.
- Inventory 50 windows including Snap Group members, multi-tag entries, minimized and remote
  desktop windows. Compare exact-once list in flat/group/search modes.
- Enter each monitor's physical right hot zone, cross shared segments early, dwell and scroll
  elsewhere. Verify fullscreen suppression, mixed-DPI positioning and keyboard-only use.
- Gather a distributed tag; verify moves/current desktop/restore and zero new desktops. Reset
  placements, Preserve it; verify zero moves and explicit remote selection. Rebind direct/picker
  keys in settings, restart and test; reject duplicates with old bindings remaining active.
- Send a tag/window to a new desktop with one invocation. Test unsupported capability, denied
  target and partial success; never claim whole-operation success when a member failed.
- Save layout, rearrange/resize/minimize, close one target and restore. Verify geometry within
  one logical pixel for unchanged topology and reported fallbacks for missing monitor/desktop;
  zero relaunches or lookalike matches.
- Export/import JSON, malformed imports, unknown version, interrupted save, local reload and
  Persian round-trip; previous valid settings must survive every rejected candidate.
- Pause/quit, then separately force-kill main process (`Stop-Process -Id <mainPid> -Force`).
  Guardian must survive main termination when enabled. Check original colors/zero overlays in
  <=1 s; test guardian failure and stale handles using the Spike protocol. Do not blanket-kill
  the entire process tree and then label that a tested main-only crash.
- Measure performance exactly as research section 9: 60 s warmup, 10 min idle, 100 notch opens,
  combined process memory/CPU; external sampling is measurement tooling, not app polling.
- Block network or run disconnected and inspect process traffic in local test tooling. Verify
  no app/helper/dependency runtime network access, telemetry or mapping downloads.

Record raw evidence in docs/validation/, redacting private titles by default, and summarize
in research.md plus checklists/windows-validation.md. All their cases start NOT RUN.
