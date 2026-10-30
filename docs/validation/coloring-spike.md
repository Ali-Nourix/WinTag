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
| Pure protocol self-tests | PASS, 18 checks | No user32/dwmapi, named-pipe or recovery execution |
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
