# T001 — WinTag Window Coloring Spike

**Experimental Windows 11 x64 probe, not the WinTag application.** Source + cross-build are
available. No Windows UI, DWM, click-through, guardian recovery or resource test has run yet.
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
- 18 pure protocol tests passed: snapshot/command/Unicode round-trip, invalid lease fields,
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
