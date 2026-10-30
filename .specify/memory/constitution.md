# WinTag Constitution

## Core Principles

### I. Lightweight, Event-Driven Operation

WinTag MUST be event-driven and consume near-zero CPU while idle. Window lifecycle,
activation, title, geometry, and monitor changes MUST be handled through event notifications,
not periodic enumeration, cursor sampling, or recurring polling timers. Initial enumeration
and bounded, event-triggered reconciliation are permitted. A one-shot dwell timer is permitted
only while a pointer is eligible to open a shared-edge hot zone; it MUST be cancelled when
eligibility ends. Idle operation MUST NOT depend on a continuously active render loop.

The notch MUST open in less than 100 ms with 50 eligible windows, measured after the
configured dwell interval where applicable. Total WinTag background RAM, including any
cleanup helper, MUST remain below 100 MB. Planning MUST define reproducible measurement
conditions and the numeric interpretation of near-zero idle CPU before implementation.
These constraints prevent a window-management utility from competing with users' apps.

### II. Local-Only Privacy

The shipped application and any helper MUST operate entirely on the local device and MUST
NOT initiate network connections, telemetry, analytics, remote logging, cloud synchronization,
or automatic update checks. Window titles, process information, rules, tags, and layouts
MUST NOT leave the device. Import and export MUST be explicit user actions on local files.
No account, online service, or network availability may be required at runtime.

Development-time retrieval of tools, source code, and dependencies is outside this runtime
restriction; it MUST NOT introduce runtime network behavior. Diagnostics MUST remain local
and MUST avoid collecting window titles unless explicitly requested by the user.

### III. Fail-Safe Visual Reversibility

All WinTag-owned visual changes MUST be reverted on normal exit AND on application crash.
Before changing another window's caption or border attributes, WinTag MUST retain enough
original state to restore those exact attributes, including system-default values. Overlays
MUST be removable without modifying the target application. Stale or reused window handles
MUST NOT cause cleanup to modify a different window. Cleanup MUST cover pause or disabling
visual tagging as well as exit, and MUST be idempotent.

A normal shutdown handler alone MUST NOT be accepted as crash safety. Before persistent
cross-process visual mutation is enabled, research and fault-injection tests MUST demonstrate
a cleanup path that survives abrupt termination of the main process. If safe cleanup cannot
be demonstrated for a visual strategy, that strategy MUST remain disabled; the requirement
MUST NOT be silently weakened to best-effort cleanup on next launch. Host shutdown and power
loss behavior MUST be documented separately from application-crash evidence.

WinTag MUST NOT inject DLLs, modify system files, hook Explorer, or destabilize other apps.
Unavailable or unsupported OS capabilities MUST fail gracefully with a local explanation
and without leaving partial visual changes. This protects the user's existing workspace.

### IV. Persian, RTL, and Accessible Interaction

The complete interface MUST support Persian and right-to-left layout, with an English
option. Persian window titles, tags, search input, configuration text, and mixed Persian /
English content MUST survive display, editing, and serialization without corruption.
Keyboard navigation, focus behavior, Esc dismissal, and tag selection MUST work without a
mouse. Monitor scaling and mixed-DPI setups MUST NOT clip labels or misplace input targets.

Color MUST always be accompanied by a visible text tag; color alone MUST NOT communicate
tag identity. Contrast and keyboard focus MUST remain discernible. Validation MUST include
Persian and mixed-direction titles in Comet, Firefox, and Obsidian, plus representative
Win32, WPF, and WinUI applications. Localization and accessibility are release requirements,
not optional polish.

### V. Human-Readable Local Configuration

Tags, their order and colors, rules and priorities, hotkeys, ignore entries, layouts, and
configurable hot-zone behavior MUST be stored in readable UTF-8 JSON at
`%AppData%\WinTag\config.json`. The format MUST have a documented schema version, preserve
Persian text, and support local export/import and manual editing without proprietary tools.

Writes MUST be atomic or otherwise preserve a last-known-good configuration. Invalid input,
invalid regular expressions, and unsupported schema versions MUST produce actionable local
errors without destroying valid settings. Import MUST be validated before replacing data.
Schema changes MUST provide documented migration or explicit incompatibility handling.
Manual tag assignments MUST never be overwritten by rule evaluation; persistence semantics
and window identity across sessions MUST be explicit in the specification and design.

## Product Boundaries and Quality Standards

- WinTag is a Windows 11 tray utility, not a replacement window manager or Snap Layouts engine.
  Windows 10, other operating systems, cloud sync, app relaunch, and non-right-edge notches
  are out of scope for v1.
- Each eligible window MUST appear exactly once in the notch, including Snap Group members.
  Grouping by tag MUST NOT duplicate windows with multiple tags.
- Layout restoration MUST operate only on still-open windows, restore position, size, and
  virtual desktop, skip missing windows, and report them without relaunching applications.
- Normal operation MUST use least privilege. Unsupported or elevated targets MUST be handled
  safely and reported rather than bypassing OS security restrictions.
- Build-dependent and undocumented Windows APIs MUST be isolated behind replaceable
  interfaces, capability-checked on the actual Windows build, and covered by graceful
  degradation behavior. No compatibility claim may rely on documentation alone.
- Technical framework choices belong in the implementation plan and MUST satisfy these
  principles; adopting a dependency does not exempt it from privacy or resource limits.

## Workflow and Evidence Gates

1. Follow `constitution -> specify -> clarify -> plan -> tasks`. Stop after every phase,
   present a short summary, and obtain user approval before continuing. The supplied product
   decisions are authoritative over older attachment wording. Ask only genuinely new questions.
2. Do not generate application code, prototypes, or tests during these documentation phases.
   Spec Kit's own generated tooling is permitted. Do not run `speckit.implement`; any future
   implementation requires explicit approval after the task list is reviewed.
3. The first research item and first implementation task MUST be the window-coloring Spike:
   evaluate DWM caption/border attributes first, then transparent click-through title-bar
   overlays for applications that ignore DWM. Include text-tag visibility and crash cleanup
   in the evidence. The second task MUST be minimal event-driven window enumeration.
4. Research MUST record every API uncertainty, Windows build and app versions tested, observed
   results, and remaining limitations. Proposed experiments MUST be marked unexecuted until
   actually run; a Linux authoring environment is not evidence of Windows compatibility.
5. Plans MUST include a constitution compliance gate, explicit measurement methods, unit-test
   coverage for rules/manual precedence and layout serialization, and a manual checklist for
   multiple monitors, mixed DPI, virtual desktops, target apps, and exit/crash cleanup.
6. Tasks MUST trace back to requirements and mark independent work with `[P]` only when there
   are no unmet dependencies or shared-file conflicts. Failed safety or privacy gates block
   the affected implementation rather than being waived without a constitution amendment.

## Governance

This constitution governs the specification, plan, tasks, implementation, and reviews.
Every plan and change review MUST explicitly check all five principles and record supporting
evidence or an unverified gate. An unverified platform claim MUST remain visibly unverified.

Amendments require a written rationale, affected requirements and artifacts, migration or
remediation implications, and explicit user approval. Consumers MUST be checked for impact;
versioned Spec Kit templates MUST NOT be changed by the constitution workflow. Conflicts
MUST be surfaced rather than silently resolved against a principle.

Use semantic versioning: MAJOR for incompatible principle removal or redefinition, MINOR
for new principles or materially expanded governance, and PATCH for non-semantic wording
clarifications. Record the amendment date and sync impact for each revision. Version 1.0.0
is the initial adoption, not a revision of a previously ratified constitution.

**Version**: 1.0.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-10-30
