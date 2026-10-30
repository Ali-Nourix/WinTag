# Data Model: WinTag

Conceptual model and validation contract, not C# source. Configuration schema starts at 1.
All persisted IDs are UUID strings except OS desktop IDs/monitor keys and explicitly serialized
live-window leases. Display strings retain original Unicode; derived search keys are not authoritative.

## Tag

Fields: id, name, color, order. `name: required, trimmed, nonempty, unique by normalized
case-insensitive comparison`. `color: #RRGGBB`. `order: nonnegative integer; unique within tags`.
Primary tag is the first tag ID in an assignment, not a global property of a tag.
Deleting a tag removes its references from rules/assignments; the next tag becomes primary.
Deleting all references leaves a manual-empty override manual. Hotkeys bind stable tag IDs,
not names; default numbers are generated from tag order unless explicitly overridden.

## ManagedWindow and LiveWindowLease

ManagedWindow: lease, originalTitle, processName/path when accessible, className, monitorKey,
desktopId when known, normalRect, currentRect, showState, DPI, lastActivatedSequence, eligibility,
cloakReason and capability limitations. Identity is distinct from matching metadata.

Lease: boot/session token, HWND serialized as a string, PID, process creation time, per-window
instance token and generation. `lease: must be revalidated before every cross-process action`.
`uncertain identity: skip and report; never match by title alone`. A verified instance marker
or equivalent tested mechanism distinguishes handle reuse within one process; destroy events
invalidate the generation immediately. Cross-app window properties require security/cleanup
review and must never be installed if their own reversal cannot be guaranteed. No safe marker
means reject that persisted lease after restart, not probabilistic matching.

Lifecycle: Discovered -> Eligible/Excluded -> Updated -> Destroyed. Minimized and remote-desktop
windows are not automatically Destroyed/Excluded. Excluded/Destroyed windows lose visuals.

## WindowAssignment

Fields: lease, orderedTagIds, source (Automatic/Manual), matchedRuleId (automatic only).
`orderedTagIds: unique existing tag IDs, order preserved, empty allowed`.
`Manual: immutable under rule evaluation, including an empty orderedTagIds list`.
Transitions: automatic match updates automatic only; manual edit locks the full list; explicit
ResumeAutomatic releases ownership and reevaluates. Closed lease invalidates live assignments;
persist only where later confident revalidation is possible. Newly opened lookalikes are new.

## Rule

Fields: id, enabled, priority, listOrder, conditions, orderedTagIds.
`priority: integer; ascending value wins; listOrder resolves ties`.
`conditions: nonempty; AND-combined; process/class/title with explicit match mode`.
`orderedTagIds: unique existing tag IDs, order preserved, empty allowed`.
Process/class are case-insensitive. Plain title is case-insensitive substring. Title regex
has explicit pattern and ignoreCase flag; invalid pattern rejects candidate configuration;
50 ms timeout isolates expensive matches and returns a reported rule failure, not partial overwrite.
First matching enabled rule produces the whole list; no match clears automatic assignments.

## HotkeyBinding

Fields: action (RecallTag/OpenTagPicker), mode (Gather/Preserve), tagId for RecallTag, modifiers,
virtualKey, enabled. `chord: unique among enabled WinTag bindings`.
`mode: Gather or Preserve; no implicit mode switching on failure`.
First-nine defaults and both picker defaults are in contracts/interaction.md. Remapping is
independent per tag/mode/picker. Persist chords, not transient OS registration IDs.
State: Candidate -> Validated -> Reserved -> Persisted/Active. Registration/persistence failure
rolls back to prior Active; no loss of a working binding because a proposed one failed.

## HotZonePreferences and MonitorState

HotZone: enabled, widthDip (default 6), heightFraction (default 0.30), outerDwellMs (0),
sharedDwellMs (250), suppressFullscreen (true), optional per-monitor override.
`widthDip: positive finite number`; `heightFraction: >0 and <=1`;
`dwellMs: integer >=0`. Monitor: stable key when available, physical bounds/work area, DPI,
adjoining right-edge segments and fullscreen state. Rebuild on display/DPI events only.
Eligibility: Outside -> EligibleOuter/Open or EligibleShared/Dwelling -> Open; leaving or
fullscreen/pause cancels dwell. A keyboard-opened panel has separate focus/dismissal state.

## Layout and LayoutEntry

Layout: id, `name: required, trimmed, nonempty, unique by normalized case-insensitive comparison`,
capturedUtc, entries. Entry: lease, monitorKey, sourceDpi, logicalNormalRect, showState,
desktopId, original desktop identity metadata for reporting (not name-only matching).
`rectangle: finite coordinates; width >0 and height >0`. Negative origins are valid.
`showState: Normal, Minimized or Maximized`. Store normal rectangle even if minimized/maximized.
Restore: verify lease -> determine available placement -> move desktop -> set normal geometry ->
restore saved show state -> record outcome. Missing or uncertain windows skip. Missing desktop
uses current, missing monitor clamps to available work area, each marked fallback. Do not create
new desktops during layout fallback or relaunch apps. OS-enforced geometry limits are deviations.

## VisualSnapshot and CleanupLease

Ephemeral/local-safety data, not ordinary config exports: target lease, supported strategy,
original border/caption attributes and known-default flags, mutation generation and guardian ack.
`original values: exact or strategy disabled`; `mutation: forbidden before guardian acknowledgement`
for persistent strategies. Applied -> Restoring -> Restored is idempotent. Expired lease never
restores onto a new instance. Any local journal contains no title by default and is not a substitute
for cleanup that survives main-process exit.

## IgnoreEntry, Configuration and OperationResult

IgnoreEntry: id, enabled, nonempty process/window matching conditions with explicit modes.
Configuration: schemaVersion, tags, rules, manualOverrides, hotkeys, hotZone, ignoreEntries,
layouts, preferences(language fa/en, autostart false by default).
`schemaVersion: exactly 1; unknown future versions rejected without overwriting`.
`references: all referenced tag IDs must exist`.
Import validates the full graph and duplicate IDs/names/chords before confirmation and replacement.
Single-instance writes retain a last-good backup; JSON uses invariant numbers and original Unicode.

OperationResult: operationId, requested targets, completed, skipped, failed, fallback details,
localizable reason codes; no remote diagnostics. A target has one terminal result. Partial
success is not success for every target. Failed create/move/switch can retain a desktop if
removing it would risk moving unrelated windows; report that instead of destructive rollback.
