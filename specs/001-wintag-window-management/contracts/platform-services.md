# Platform Service Contracts

Documentation only; define interfaces during implementation, not in this phase.

## IWindowInventory

Start returns initial snapshot after registered-hook/buffer/snapshot reconciliation. Subscribe
emits versioned immutable upsert/remove deltas on one coordinator. Live lease revalidation is
required immediately before action. All monitor/desktop entries use the same unique lease key;
no Snap Group proxies. Retain proven off-desktop real windows, exclude ghosts/tool/UI helpers.
Stop unhooks and releases managed delegates only after pending callbacks cannot use them.
No recurring enumeration, cursor sampling or polling repair loop.

## IVisualService and ICrashCleanupCoordinator

Apply(lease, primaryTag) returns strategy/result only when eligibility, original-state capture
and safety capability are valid. For DWM, register original state and await guardian ack BEFORE
apply; never assume a getter exists or COLOR_DEFAULT is the correct original.
Remove/RevertAll are idempotent; preserve exact original styling and reject stale identities.
Guardian waits on main process/pipe signals and survives main termination; it does not start WPF.
Overlay-only still requires tested click-through, lifetime, DPI and input behavior.
No compliant strategy -> Unsupported result, not color-only success. Main/guardian loss and
simultaneous loss are separately documented in the Spike; no untested fail-safe claim.

## IVirtualDesktopService

Capabilities include identify, enumerate, notifications, create, moveForeignWindow, switch,
supportedBuild and reason. Every capability is individually gated, build+UBR/architecture
recorded. Unknown mappings reject undocumented operations before COM activation. Serialize on STA.
Create returns desktop ID; Move accepts a validated lease; Switch accepts an existing desktop ID.
Return structured result for all operations; no network resolution or Explorer injection.
User-initiated operations only; no periodic desktop inventory.

SendToNew snapshots target leases -> validate -> create once -> move individually -> switch once
if meaningful success -> report. Roll back safely where possible, never delete a desktop with
unrelated windows. Gather does not create; Preserve never moves. Disabled features explain why.

## IHotZoneService / IShortcutManager

Hot-zone events contain monitor, segment kind and trigger-eligible timestamp. Only mouse move
enters eligibility; wheel events cannot trigger. One-shot dwell cancels on exit/fullscreen/pause.
Hook callback cannot perform UI work, disk I/O, COM, regex or enumeration.
Shortcut changes validate all chords, reserve replacements, commit configuration then release
superseded registrations; rollback any failed reservation/save. RegisterHotKey errors are visible.
No keylogging, raw input storage, synthetic focus bypass, or polling for hook health.

## ILayoutService / IConfigurationStore

Capture named layout from an immutable inventory version. Restore reports exact, fallback,
skipped and failed placements; no app relaunch, no title-only identity matching.
Configuration load validates version and references before activation. Save uses a same-directory
staged file and atomic replacement with last-good backup. Confirmed import is all-or-nothing.
Explicit reload or next start reads manual file edits; no configuration polling.

## Failure contract

Common reason codes: UnsupportedBuild, AccessDenied, StaleWindow, ClosedWindow, InvalidConfig,
ShortcutConflict, RegexTimeout, MonitorMissing, DesktopMissing, ActivationRefused, PartialMove,
UnsafeVisualStrategy and CleanupUnavailable. UI localizes these; logs stay local/title-redacted.
No interface may convert a failure to unconditional success or silently change recall mode.
