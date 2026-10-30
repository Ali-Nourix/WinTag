# WinTag Preview 0.2.0

Experimental **usable preview**, not v1 acceptance or completion of T001–T068.
Start `WinTag.Preview.exe` normally on Windows 11 x64 with .NET 10 Windows Desktop Runtime x64.
See README.fa.md for controls, JSON shortcut editing and exact limitations.

Implemented: tray, event-driven inventory, right-edge trigger, Persian search, primary grouping,
ordered manual tags for the current session, bounded automatic rules, editable transactional
hotkeys/settings, Preserve/current-desktop recall, normal close/minimize, pause/quit, optional
small foreground-only own-window badge. No foreign DWM/styles/title modification.

Disabled/unimplemented: undocumented desktop COM, Gather/cross-desktop actions, layouts,
autostart, full English UI and durable/restart-safe manual identities. Tag picker is a searchable
window panel, not the planned two-mode picker. Native input/performance/coverage gates are open.

Settings use `%AppData%\WinTag\Preview\config.json` (isolated preview schema, not production).
No network clients, downloads, telemetry or scheduled refresh. Local fixed/removable file paths
only. No DLL injection/Explorer hooks. Badge is off by default and only modifies its own HWND.

Build: `dotnet publish previews/WinTag.Preview -c Release -r win-x64 --self-contained false`
Tests: `dotnet run --project previews/WinTag.Preview.Tests -c Release` (24 portable checks).
Native build succeeds on Linux with SDK 10.0.403, CsWin32 0.3.367 pinned. This is not native
Windows execution or proof of <100 MB / <100 ms / fail-safe input, DPI or shell compatibility.
WPF owns DPI/lifetime; WinForms is used only for NotifyIcon/Screen (targeted WFO0003 suppression).
