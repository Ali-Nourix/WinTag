# T001 — WinTag Window Coloring Spike

**Experimental Windows 11 x64 probe, not the WinTag application.** Source + cross-build are
available. This revision has not been Windows-executed by this agent; DWM, click-through,
guardian recovery and resource acceptance remain unverified.
T001 remains unchecked and blocks T002/production scaffolding.

## مرز ایمنی

- DWM فقط روی پنجرهٔ آزمایشی متعلق به همین Spike تغییر می‌کند؛ این پنجره در فرایندی جدا
  ساخته می‌شود و رنگ اولیهٔ دقیق و معلوم دارد.
- روی Comet، Firefox، Obsidian، Notion و سایر برنامه‌های واقعی، فقط خواندن آزمایشی مقدار
  DWM و overlay بدون تغییر رنگ اصلی مجاز است. حتی موفقیت getter، تغییر DWM را فعال نمی‌کند.
- guardian قبل از تغییر رنگ شروع می‌شود و مقدار اولیه را تأیید می‌کند. تغییر DWM را خود
  guardian به‌صورت ترتیبی انجام می‌دهد؛ پاک‌سازی فقط به finally در فرایند اصلی متکی نیست.
- kill هم‌زمان فرایند اصلی و guardian، power loss و OS crash تضمین نشده‌اند. این تفاوت را
  با «آزمون موفق پاک‌سازی» اشتباه نگیرید. روی کار ذخیره‌نشده آزمایش نکنید.

## Run the framework-dependent package

Windows 11 x64 + **.NET 10 Windows Desktop Runtime x64** required. No administrator privileges.
Extract the entire ZIP into one directory, then run `WindowColoring.exe`. The runtime is NOT
included. Nothing downloads at runtime. Keep all DLL/JSON files next to the EXE. The probe is unsigned and experimental; inspect/build
the source rather than changing OS security protections to run an untrusted binary.

## Build from source

Install .NET SDK 10.0.403 (or a later 10.0.4xx patch allowed by global.json), then:

```powershell
cd spikes/WindowColoring
dotnet restore --locked-mode
dotnet build -c Release --no-restore
dotnet run -c Release --no-build
# Optional distribution build, still needs installed Windows Desktop Runtime:
dotnet publish -c Release -r win-x64 --self-contained false -o ../../artifacts/coloring-spike-win-x64
# Pure protocol checks, runnable on Windows or Linux (not native API proof):
dotnet run --project ../WindowColoring.SelfTests/WindowColoring.SelfTests.csproj -c Release
```

## Controlled DWM/crash experiment

1. Record Windows build+UBR (`winver`), theme, app version and monitor/DPI arrangement in the
   report template at `docs/validation/coloring-spike.md`.
2. Press **Launch separate controlled target + start guardian**. A separate normal WPF window
   appears. Its source-owned originals are caption `#234567`, border `#789ABC`; a unique property
   marker, PID and process birth validate its live identity. No original foreign value is guessed.
3. Press **Apply DWM colors to CONTROLLED target**. Inspect actual caption/border rendering;
   a successful HRESULT/ack is not proof that colors appeared.
4. Press **Restore controlled target via guardian**. Verify exact seeded colors return.
5. Apply again, then **FAULT INJECTION** and confirm the explicit kill dialog. Only the controller
   is abruptly terminated. The target and guardian are independent processes; the target stays
   open so its original colors can be inspected. Use external timing/video to verify <=1 second.
   No claim of that latency is made by the probe.
6. Repeat normal exit and guardian-only kill (PID displayed in controller; `Stop-Process -Id
   <guardianPid> -Force` from a separate shell). The surviving controller attempts restoration
   and disables further requests. Record failures. Close the disposable target yourself afterward.
7. Close/recreate the controlled target and confirm stale leases cannot touch another instance.
   Test memory for the whole controller+guardian set using the plan's protocol; report the extra
   disposable test-target process separately, not as part of shipped WinTag's memory claim.

The guardian blocks on process-handle/pipe events, not heartbeat/polling timers. Pipe endpoints
are random and current-user-only. It holds the original snapshot before acknowledging readiness.
Normal exit and broken pipe attempt idempotent restore. Simultaneous failure is not handled.

## External application overlay/readback experiment

1. Open disposable windows in Comet, Firefox, Obsidian, Notion and Win32/WPF/WinUI targets.
2. Explicitly **Refresh** and select a candidate. This list is a Spike convenience, not the
   T002 production inventory prototype; no completeness/Snap Group claim is made.
3. **READ-ONLY DWM getter probe** reports HRESULT/value for caption and border. Record the result.
   Setter-only attributes may reject getters. External DWM mutations remain DISABLED regardless.
4. Set Persian/English tag text, then **Show trial tag overlay**. The probe requests activation;
   Windows can deny it. The overlay is intentionally shown only while that exact target is
   foreground, non-minimized and uncloaked. It hides on focus loss. Full inactive-window tagging
   is not implemented by this safety-first Spike.
5. Test caption buttons, drag, resize, tabs, menus, mixed DPI, maximize/minimize, desktop switch,
   target closure and controller crash. Native transparent/layered/no-activate styles are used;
   click-through across target apps still needs physical verification.
6. Placement is deliberately simple/centered and MUST be checked for tab or caption obstruction.
   No per-app placement or final fallback decision is certified yet. Save results manually;
   the probe does not automatically export private window titles or telemetry.

## Evidence actually obtained in this environment

- Linux SDK 10.0.403: Windows x64 Release cross-build/publish succeeded (warnings treated as errors).
- Initial v0.1.0: 18 pure protocol tests passed: snapshot/command/Unicode round-trip, invalid lease fields,
  malformed/oversized messages, color parsing and Win32 COLORREF ordering.
- Windows execution: **NOT RUN**. DWM appearance, guardian IPC/process waits, real click-through,
  title-bar geometry, runtime memory/CPU and forced-exit cleanup remain unverified.
- Source uses CsWin32 0.3.367, pinned and lock-file recorded; generated interop is compile-only.
- No recurring polling, network/telemetry client, DLL injection or Explorer hook was added.
  Static source inspection is not runtime privacy/performance proof.

## Exit criteria

Follow research section 1 and the Windows checklist; do not check T001 until actual results
prove a compliant text+color strategy and cleanup, or explicitly reject a strategy and validate
its fallback. Unknown original foreign DWM values are a blocker, not permission to reset them
all to system defaults. T002 has not started.

## Build footprint

This Win32/WPF-only Spike disables unused Windows SDK.NET/WinRT framework references. The
framework-dependent package does not include the 25 MB SDK.NET runtime assembly, WinRT.Runtime,
or the .NET Desktop Runtime. This is a distribution-size optimization, NOT a measured memory
claim. See THIRD-PARTY-NOTICES.md.

## v0.1.1 — Compact badge and safe LOCAL action

The old 190x25 DIP green pill is replaced with a neutral 18-DIP-high badge, width based on
text and capped at 92 DIP, a muted 2-DIP color accent and up to eight graphemes plus ellipsis.
Text is kept legible rather than making the whole indicator nearly invisible. The full text
remains in the editor/accessibility name. A click-through overlay cannot safely implement an
interactive hover tooltip without capturing input; no such tooltip is claimed. Custom tabs,
caption controls, high contrast and the <2-second identification goal still need visual testing.

### One-button action (recommended)

1. Extract this version into a NEW local folder. Run WindowColoring.exe as an ordinary user.
2. Click **اجرای بررسی امن و ذخیرهٔ گزارش محلی** and choose a new local JSON report filename.
3. Default is READ ONLY: OS/runtime/architecture, local Windows UBR, standard-user token,
   18 shared pure-protocol smoke checks, read-only DWM composition, and one controller-only
   memory snapshot. No other applications are enumerated, changed, closed, minimized or moved.
4. Optional checkbox allows a NEW disposable companion+guardian only. The action applies and
   restores colors there, then normally closes its own companion. It does not reuse the manual
   experiment or a selected real application window. Crash injection is NEVER part of this action.
5. Report excludes window titles, application inventories, usernames and machine names. It stays
   local and is never uploaded automatically. Existing files are never overwritten. Network
   drives, UNC/device paths and junction/symlink report directories are rejected. Choose a local
   folder not managed by cloud sync if you do not want another installed sync tool to copy it.
6. Native return-code success is labeled separately from appearance. Visual/click-through/crash,
   DPI/desktop, idle/combined-memory and traffic checks remain `NotRun`; `productionAcceptance`
   is always false. The resource snapshot is not proof of the background RAM budget.

### Transparent PowerShell launcher (optional)

The binary package also includes Run-SafeChecks.ps1 and checksums.sha256. In an ordinary
PowerShell session, from the extracted local folder:

```powershell
.\Run-SafeChecks.ps1
# Optional: requires an additional explicit yes at the prompt:
.\Run-SafeChecks.ps1 -IncludeControlledTarget
# Or choose a fresh output filename:
.\Run-SafeChecks.ps1 -ReportPath C:\Temp\WinTag-safe-report.json
```

It checks the package SHA-256 manifest before running. Checksums detect accidental corruption,
not authenticity: the package is unsigned. There is no download, administrator elevation,
execution-policy change, scheduled task, automatic crash or automatic upload. If your policy
blocks the script, use the in-app button instead; do not disable your security policy.
Source-build users can use the button directly; the packaged launcher expects the binary
package's checksum manifest, which is produced during packaging, not checked into source.

The equivalent explicit command is `WindowColoring.exe --safe-checks <new-local-report.json>`;
adding `--controlled-target` explicitly opts into the disposable target test. Exit 0 means the
limited requested checks ran without an operational error, not that native acceptance passed;
exit 2 indicates a failed/blocked check, exit 1 indicates launch/report-output failure.
The launcher/Windows action are source-reviewed and cross-compiled, NOT Windows-executed here.

### Portable test coverage

The development self-test suite now has 37 passing portable checks, including the original
18 reused in the local action plus compact Unicode labels, fixed safety flags, local report
schema, device-name rejection, atomic new-report writes and no-overwrite preservation.
None constitutes Windows API, visual, mouse-input or crash-recovery evidence.

## v0.1.2 — Stage-specific safe diagnostics

This revision improves diagnosis; it does **not** claim to fix the underlying Windows DWM
failure or reduce memory. Extract into a **fresh local folder**; never mix child processes or
binaries from different probe versions. Run normally (not Administrator). Select the optional
controlled-target checkbox and use the same safe-check button once. Share the newly saved JSON
if desired. A failed report is still useful evidence; do not retry with elevated privileges.

- JSON schema 2 identifies the entered stage: target spawn/connect/handshake/identity,
  guardian spawn/connect/handshake, controlled apply/restore and guardian/target cleanup.
- The controlled target now sends setup failures back through its private pipe; native DWM
  failures preserve HRESULTs. Guardian replies use fixed diagnostic codes, never raw error text.
- Primary apply/startup failures are no longer replaced by later cleanup failures; cleanup has
  separate results. Multiple cleanup errors are retained separately.
- No exception messages, stack traces, private paths, titles, HWNDs/PIDs, user or machine names
  are persisted. No uploads, forced crashes/kills, elevation or security-setting changes added.
- 49 portable regression checks pass and the Windows cross-build succeeds. Native execution
  of v0.1.2 is NOT RUN by the agent; the v0.1.1 reports do not certify this revision.
- Evidence received from the user: Windows 26200.8457 x64/.NET 10.0.11, v0.1.1 read-only checks
  passed; opt-in controlled DWM/cleanup failed with a generic InvalidOperationException. The
  failing substep and native HRESULT were not recorded, so the cause is still unknown.
- Reported controller memory snapshots exceeded the product's 100 MB goal. They were not
  steady-state/combined-process measurements; neither acceptance nor optimization is claimed.

Runtime/manual safety and performance gates stay open; T001 remains incomplete.
