# Research: WinTag

Date: 2026-10-30. This is desk research and an experiment protocol, NOT Windows test evidence.
No application code or Spike was written or run. The installed Windows build is unavailable in
this Linux authoring environment. Pending experiments are implementation/release gates, not
unanswered product choices. A capability remains disabled until its gate passes.

## 1. Spike for window coloring — FIRST research item and FIRST implementation task

**Decision**: Try Windows 11 DWM caption/border color first for each target app. Always provide
visible primary-tag text; DWM color alone cannot render that label. Evaluate a small click-through
text overlay alongside DWM, or a color-plus-text overlay when custom chrome ignores DWM. Do not
rewrite another app's actual title. Final strategy is per tested app/build, never inferred from
an API success return alone.

**Rationale / documented facts**: Microsoft lists DWMWA_CAPTION_COLOR and DWMWA_BORDER_COLOR
starting at Windows 11 build 22000 and documents COLOR_DEFAULT and border COLOR_NONE sentinels.
The color attributes are described for setting; do NOT assume DwmGetWindowAttribute can retrieve
all prior values. Resetting everything to COLOR_DEFAULT is not exact restoration of an app's
custom original color. This makes original-value capture a separate pass/fail condition.
[^https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute]

**Alternatives considered**: DWM-only rejected because text is mandatory and custom chrome may
ignore it; overlay-only is the safe fallback candidate, still subject to usability/performance
and screenshot tests; DLL injection/title mutation/Explorer hooks rejected by constitution.

**Experiment protocol (NOT EXECUTED)**:
1. Record exact OS build+UBR, architecture, theme/high contrast, DPI and app version for Comet,
   Firefox, Obsidian, Notion and representative Win32/WPF/WinUI targets.
2. Establish original border/caption state including app-defined colors and default sentinels.
   Demonstrate reliable capture/restoration or mark persistent DWM mutation UNSAFE/disabled.
3. Apply DWM attributes to disposable test windows and observe active/inactive, tabbed/custom
   chrome, maximized, minimized and fullscreen behavior. Record HRESULT and actual visual result.
4. Place WPF transparent layered overlays with no activation and genuine native click-through.
   Test caption buttons, drag, resize, snap, menus, taskbar/Alt+Tab exclusion and screen readers.
   WPF IsHitTestVisible=false alone is NOT proof of cross-process click-through. Verify native
   hit testing/styles on the actual targets. RTL text must not cover caption buttons.
5. Follow location, activation, DPI, cloak and destruction events; hide overlays on minimized,
   inactive-desktop, fullscreen/unsupported targets as appropriate. No tracking polling loop.
6. Fault-inject normal exit, unhandled exception, forced main-process termination, guardian
   startup/pipe failure and guardian exit. Verify original colors and zero leftover overlays
   within 1 second of main-process termination; record actual latency. Reused HWNDs must never
   receive another window's cleanup. Test restoration after theme/color changes during tagging.
7. Measure combined main+guardian memory and idle CPU. If guardian/DWM cannot satisfy safety
   or budget, disable DWM mutation and re-evaluate overlay-only; do not lower the requirement.

**Spike proved**: Nothing yet. Documentation establishes API availability, not per-app behavior,
exact prior-value retrieval, click-through correctness, crash reversibility or resource cost.

| App/build | DWM observed | Prior value recovery | Text overlay/input | Forced-exit cleanup | Final strategy |
| --- | --- | --- | --- | --- | --- |
| Comet / unrecorded | NOT RUN | NOT RUN | NOT RUN | NOT RUN | Disabled pending evidence |
| Firefox / unrecorded | NOT RUN | NOT RUN | NOT RUN | NOT RUN | Disabled pending evidence |
| Obsidian / unrecorded | NOT RUN | NOT RUN | NOT RUN | NOT RUN | Disabled pending evidence |
| Notion / unrecorded | NOT RUN | NOT RUN | NOT RUN | NOT RUN | Disabled pending evidence |
| Win32 / WPF / WinUI | NOT RUN | NOT RUN | NOT RUN | NOT RUN | Disabled pending evidence |

## 2. Crash cleanup and visual ownership

**Decision**: Overlay windows belong to the main process. Persistent DWM changes additionally
require a minimal headless guardian launched before mutation. Main and guardian communicate
through a current-user-only named pipe; guardian waits on a main-process handle/pipe events,
not polling or heartbeat timers. Main sends original attribute snapshots and receives an
acknowledgement BEFORE mutation, so a crash between acknowledgement and apply is safe to undo.
Guardian validates the live-window lease before restoration. Normal cleanup is idempotent.
If guardian fails or exits, main restores visuals immediately and disables DWM mutation.

**Rationale**: An exit/finally handler cannot run after forced termination. Main-owned overlays
have an OS-owned lifetime; cross-process attribute changes may outlive their owner. A guardian
can survive a main crash, but simultaneous main+guardian failure is NOT a proved guarantee.
Until the Spike bounds and demonstrates the promised crash model, persistent mutation stays off.
Power failure/OS failure is not represented as a tested main-process crash.

**Alternatives considered**: Next-launch recovery rejected; injecting cleanup into targets
rejected; a polling watchdog rejected. Native guardian could lower memory but adds language/
build complexity, so begin with a headless .NET process and measure the total budget first.

**Uncertainty / gate**: Capture original DWM attributes, guardian survivability, non-racy live
identity and OS cleanup of overlays are NOT RUN. A journal may retain non-title cleanup data
locally for diagnostics; it cannot substitute for live crash cleanup.

## 3. Runtime and UI foundation

**Decision**: C# / .NET 10 LTS, satisfying the requested .NET 8+; WPF with per-monitor-v2 DPI
manifest, FlowDirection and localized resources. Use CsWin32 for Win32 declarations,
System.Text.Json for UTF-8 JSON and System.Windows.Forms.NotifyIcon only for the tray.
Use a separate net10.0 core library and Windows app/platform layer; guardian is a headless
entry mode of the Windows executable and MUST NOT initialize WPF in guardian mode.
Pin actual package/SDK patch versions in implementation after dependency review.

**Rationale**: On the research date .NET 8 support ends 2026-11-10, while .NET 10 LTS runs
through 2028-11-14, so starting a new project on 8 would immediately create an upgrade burden.
[^https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core]

**Alternatives considered**: .NET 8 compatible but near end of support; WinUI adds no needed
capability; Electron incompatible with the lightweight goal. WPF transparent-window and mixed
DPI behavior still require physical Windows validation; selecting WPF is not performance proof.

## 4. Window inventory and the second-task prototype

**Decision**: Register SetWinEventHook OUTOFCONTEXT|SKIPOWNPROCESS on a dedicated message-loop
thread before initial EnumWindows, buffer events while taking the snapshot, then reconcile
idempotently into one dictionary keyed by a live lease. Root managed callback delegates until
unhooking; callbacks enqueue small records and return. Serialize updates on one coordinator.
Handle create/destroy/show/hide/name/location, foreground/minimize and desktop/display changes.
Coalesce bursty location events only while events exist; do not schedule recurring refresh.

**Rationale**: Out-of-context hooks do not inject callbacks into other processes; Microsoft
requires a message loop and warns about managed callbacks and reentrancy.
[^https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook]

**Filtering**: Exclude tool-only, owned helper/proxy, ghost and WinTag surfaces; retain minimized
real windows. Important correction to a naive 'skip all cloaked' filter: real windows on another
virtual desktop may be shell-cloaked. Keep a proven real application window in the inventory
when desktop membership explains its cloak, but hide its title overlay off-desktop. Reject app/
helper cloaks and ghosts. If classification is uncertain, report a capability gap rather than
silently losing all remote windows. Never invent Snap Group proxy entries.
Microsoft documents shell/app/inherited cloak reasons and hiding windows on inactive desktops.
[^https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute]
[^https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager]

**Uncertainty / gate**: T002 must validate inventory accuracy across desktop switches and HWND
reuse without treating every cloak as a ghost. Process start time+HWND is insufficient against
same-process HWND reuse: use destroy-event generation plus a per-window instance marker where
allowed; reject uncertain persisted leases after restart. Denied marker access means no unsafe
persistent mutation and no guessed restore. Window titles are not identity keys.

## 5. Virtual desktop wrapper evaluation

**Decision**: Isolate all build-dependent operations behind IVirtualDesktopService and test
Slions.VirtualDesktop core package as the first .NET candidate. NO final compatibility claim
or unconditional package approval yet. Use only pinned, locally bundled interfaces: no runtime
GUID downloads, no network version checks and no Explorer code injection.

**Desk evidence**: Slion's README advertises create/switch, move other windows and notifications,
but explicitly warns that undocumented interface IDs change and startup can crash if the OS
mapping is wrong. It describes runtime compilation of a local COM interop DLL, which is not
permission to inject it into another process. Source props identify 6.9.2; inspected app.config
contains mappings through 26100.0000 plus 22621/22631 revisions. Those mappings do NOT establish
support for later cumulative revisions or the user's installed build. Maintenance evidence is
limited to these inspected source artifacts; present-day release activity is not established.
[^https://github.com/Slion/VirtualDesktop/blob/e400d71d1573e843363ae529f59ae72f6779cd03/README.md]
[^https://github.com/Slion/VirtualDesktop/blob/e400d71d1573e843363ae529f59ae72f6779cd03/src/Directory.Build.props]
[^https://github.com/Slion/VirtualDesktop/blob/e400d71d1573e843363ae529f59ae72f6779cd03/src/VirtualDesktop/app.config]

**Alternative evaluated**: MScholtes/VirtualDesktop is a C# CLI/reference, not an equivalent
in-process .NET library dependency. Its README version 1.21 (2025-08-11) records changed GUIDs
and an animated-switch concurrency crash fix. Useful as an independent comparison of build
handling, not selected as a subprocess invoked per window. Grabacr07 upstream is marked
unmaintained by Slion and is not the preferred candidate. Native DLL wrappers add deployment
cost and do not remove undocumented-API risk.
[^https://github.com/MScholtes/VirtualDesktop/blob/a725cbd3cdb9e977678eeaf034a7cc96d2e74bc6/README.md]

**Public API boundary**: IVirtualDesktopManager exposes identify/current-desktop checks and a
move method, but not desktop create/switch; public-only cannot satisfy the complete product.
Do not interpret that move method as unrestricted movement of all other applications.
[^https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nn-shobjidl_core-ivirtualdesktopmanager]

**Acceptance gate (NOT RUN)**: On exact installed build+UBR and architecture, review package
license/dependencies, build mappings, offline behavior and memory, then probe in disposable
Windows session: enumerate/identify/create/move foreign HWND/switch/notifications, elevation,
Explorer restart, unsupported build and partial failures. Serialize COM calls on a dedicated
STA; do not concurrently animate switches. Reject unknown build mappings before instantiating
undocumented interfaces. If rejected, disabled create/move/switch actions explain why; safe
current-desktop tagging/listing can work, but this degraded mode is not full-v1 acceptance.

## 6. Event-driven right-edge trigger

**Decision**: WH_MOUSE_LL on its own message-loop thread, lightweight arithmetic for WM_MOUSEMOVE
only, then enqueue eligibility changes and CallNextHookEx immediately. Wheel messages never
open the notch. A cancellable single-shot 250 ms timer is allowed only during eligible shared-
edge dwell. No GetCursorPos sampling loop or recurring timer. Cache monitor physical rectangles;
convert 6 logical pixels by that monitor's DPI and calculate genuinely adjoining edge segments.
Fullscreen suppression updates on foreground/location/show/display events, not polling.

**Rationale**: The low-level hook is not injected into other processes; it requires a message
loop, must return quickly, and may be silently removed on timeout. Microsoft also recommends
considering raw input. This risk must be tested, not solved with a periodic hook-health poll.
[^https://learn.microsoft.com/en-us/windows/win32/winmsg/lowlevelmouseproc]

**Alternatives considered**: Raw input with event-driven cursor lookup is an allowed contingency
if hook reliability/latency fails; periodic lightweight tracking is rejected because no polling
is a stronger user constraint. Do not modify system LowLevelHooksTimeout.

**Uncertainty / gate**: Dedicated-thread hook overhead, partial shared edges, fullscreen detection,
remote-session input and exact WPF focus/dismissal need manual evidence. A known lost-hook
failure must expose a recoverable local error; do not promise all silent loss can be detected.

## 7. Recall, focus and configurable shortcuts

**Decision**: RegisterHotKey+MOD_NOREPEAT. Gather: Ctrl+Alt+1..9 / Ctrl+Alt+Space picker.
Preserve: Ctrl+Alt+Shift+1..9 / Ctrl+Alt+Shift+Space picker. User may rebind each in settings/JSON.
Use temporary unique registration IDs to reserve replacement chords before unregistering old
bindings; update persistence after validation and roll back registrations on save failure.
Reject duplicate WinTag bindings and respect OS failures. Do not promise conflict-free defaults.
[^https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey]

Gather snapshots membership, moves supported members to the current desktop, restores and
raises them in deterministic most-recent-use order, with one final foreground target. Preserve
never moves windows: restore/raise local members and list remote members for explicit selection/
switch. No program can give all windows keyboard focus simultaneously. Foreground requests can
be denied by Windows even when documented conditions appear met; return per-target results,
never use synthetic input/AttachThreadInput tricks to defeat foreground restrictions.
[^https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow]

## 8. Layouts, config, rules and identity

**Decision**: System.Text.Json, schemaVersion=1, UTF-8 readable/indented, original Persian text,
same-directory temporary write + flush + atomic replace with last-good backup. Single-instance
writer; explicit reload/import/edit events, no file polling. Reject unknown versions and invalid
references before commit. Rule regex has an explicit 50 ms timeout per match and executes off
hook/UI threads; preserve prior valid config and report timeout. Search normalization is
separate from display and regex. No database, remote persistence or auto-update component.

Layouts store logical work-area-relative geometry plus source DPI/monitor and WindowPlacement
state. Recheck live leases before action. Closed or uncertain identities skip, even lookalikes.
Missing desktop falls back to current; missing display fits available work area and reports.
Application-enforced minimum size is a reported deviation, not a claimed exact restore.

**Uncertainty / gate**: Verify coordinate conversions for negative origins and mixed DPI,
minimized/maximized normal rectangles, desktop IDs after Explorer restart, same-process HWND
reuse, and atomic-write interruption on Windows. Do not persist a title-only matching heuristic.

## 9. Numeric performance and evidence protocol

**Decision**: Planning acceptance interpretation of 'near zero': over a 10-minute motionless
idle interval after 60 s warmup, total CPU time of all WinTag processes <=0.6 s (<=0.1% of ONE
logical core, not divided by machine core count). No idle polling timer, scheduled recurring
work or permanent animation. Separately report active pointer/event-storm cost.

Memory: combined private working set AND private committed bytes each <100,000,000 bytes,
main+guardian, 50 managed windows and closed notch, sampled by external test tooling once per
second for 10 minutes. External measurement sampling is not application polling. Report peaks;
shared-runtime counting methodology is documented, never exclude guardian or count only UI heap.

Notch: preloaded reusable lightweight panel, no opening animation delaying usability; measure
trigger-eligible timestamp to visible+input-ready with a monotonic clock, 100 trials with 50
windows, each <100 ms (report maximum and distribution; p95 alone cannot hide failures).
Shared-edge dwell is measured separately (250 ms target with timer scheduling error recorded).
Cleanup target: <=1 second from detected main exit, verify visual absence from process exit;
record both detection latency and total end-to-end latency, fail if total exceeds 1 second.
All hardware, OS, DPI, app versions and tools accompany results. No figures above are results.

## 10. Remaining API uncertainty register

| ID | Uncertainty | Required experiment | Current status / fallback |
| --- | --- | --- | --- |
| G1 | DWM exact readback and custom chrome/text placement | Section 1 Spike | NOT RUN; no persistent mutation |
| G2 | Main crash, guardian loss and reuse-safe restoration | Sections 1–2 fault injection | NOT RUN; DWM disabled |
| G3 | WinEvents coverage, cloak classification, app-frame ghosts | T002 inventory prototype | NOT RUN; no completeness claim |
| G4 | Installed-build COM wrapper support and maintenance suitability | Section 5 validation | NOT RUN; desktop operations disabled |
| G5 | Low-level hook reliability, monitor geometry, fullscreen | Section 6 manual checks | NOT RUN; no polling fallback |
| G6 | Focus denial, minimized restore and mixed privilege | Section 7 recall matrix | NOT RUN; report individual failures |
| G7 | Mixed-DPI geometry and stable live identities | Section 8 layout checks | NOT RUN; skip uncertain matches |
| G8 | WPF + guardian total memory/idle/open latency | Section 9 protocol | NOT RUN; block release if exceeded |
| G9 | Config interruption and offline dependency behavior | Import/write/traffic tests | NOT RUN; preserve last-good config |

All technical choices have a documented conditional decision and fallback. This research is
complete as a design artifact, not complete as platform feasibility proof. No Windows build
is currently certified and no package compatibility claim is finalized.
