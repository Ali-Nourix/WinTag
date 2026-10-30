# Configuration File Contract

Location: `%AppData%\WinTag\config.json`. Encoding UTF-8, readable indented JSON, schemaVersion 1.
The repository does not contain a generated user configuration or real private window titles.

| Root field | Meaning and validation |
| --- | --- |
| schemaVersion | Exactly 1, reject unknown version and preserve existing file |
| tags | Ordered Tag records; unique IDs/names; color #RRGGBB |
| rules | Rule records, explicit priority/list order/modes; nonempty AND conditions |
| manualOverrides | Verified live leases + ordered tag IDs; empty list remains protected |
| hotkeys | Action, mode, optional tagId, modifiers, virtualKey, enabled; unique active chords |
| hotZone | enabled, widthDip=6, heightFraction=0.30, outerDwellMs=0, sharedDwellMs=250, suppressFullscreen=true |
| ignoreEntries | Explicit enabled process/window conditions; cannot include malformed patterns |
| layouts | Named records with verified identity, source DPI, normal rectangle, state, desktop |
| preferences | language fa/en; autostart false by default; optional monitor overrides |

Refer to data-model.md for exact field constraints and state transitions. No unknown tag
references, duplicate IDs, non-finite rectangles, negative dimensions, invalid colors/regex or
reused enabled chords may be silently accepted. Display strings preserve Persian; normalized
search keys never replace original strings. A future migration must be explicit and tested.

Settings UI and JSON use the same validation. Shortcut editing covers Gather and Preserve
independently plus both picker bindings. Proposed registrations are tested before replacing
working ones; restart retains successful mappings. Import confirmation describes replacement,
not an undocumented merge. Export excludes ephemeral guardian protocol/journals.

Write protocol: serialize validated full candidate to same-volume temp, flush, atomic replace,
retain last-good backup; do not overwrite the valid file on failed import/write. Single-instance
ownership serializes edits. Explicit Reload validates external edits and applies them as a
transaction; no background polling. Startup with invalid active file explains the problem and
uses the valid backup only if one validates, otherwise safe defaults without deleting evidence.

Runtime is offline, including schema help and validation. No downloading mappings/schema files.
