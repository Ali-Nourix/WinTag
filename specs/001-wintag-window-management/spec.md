# Feature Specification: WinTag Window Management

**Feature Branch**: `main` (no branch-creation hook installed; feature directory is independent)

**Created**: 2026-10-30

**Status**: Clarified — product decisions integrated; ready for technical planning

**Input**: User-provided WinTag product specification, resolved DECISIONS, technical input reserved
for the plan, and the attached Persian SRS draft 0.2 dated 2026-10-30. The current user request
and constitution v1.0.0 take precedence over older attachment wording.

WinTag is a lightweight Windows 11 tray utility for a power user managing many similar-looking
windows across monitors and virtual desktops. It adds ordered text tags and associated colors,
and provides a right-edge notch panel that lists each window once. It does not change Windows'
own Alt+Tab, Task View, or Snap Group presentation; it offers a duplicate-free alternative.

## Clarifications

### Session 2026-10-30

- Q: For tags spanning virtual desktops, gather windows or preserve their desktop placement?
  → A: Both are required, with separate configurable shortcuts and mode-specific tag pickers.
- Q: Must the user be able to change shortcuts?
  → A: Yes; every tag/picker binding is editable through settings and JSON, persists after
  restart, and rejects conflicts without losing the last working binding.
- User-authorized workflow update: continue clarify, plan and tasks consecutively with short
  phase summaries; do not implement or execute the Spike/prototype without approval.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Identify Windows by Tags and Color (Priority: P1)

As a user with several similar Comet, Notion, or other app windows, I want to assign multiple
tags and recognize each window by its primary tag's text and color, including Persian titles.

**Why this priority**: Rapid visual identification is the product's core value even without the notch.

**Independent Test**: With 20 distinguishable open windows, assign tags and identify requested
windows from the title-bar tag and color; change a primary tag and confirm both visual cues change.

**Acceptance Scenarios**:

1. **Given** an untagged eligible window, **When** I assign ordered tags «کار» and «مطالعه»,
   **Then** both are associated with the window and «کار» supplies its visible title-bar text and color.
2. **Given** that window, **When** I make «مطالعه» primary or edit its color, **Then** the title-bar
   cues reflect the new primary tag and all secondary tags remain assigned.
3. **Given** an application with a custom title bar, **When** tagging is supported safely,
   **Then** both text and color remain visible without intercepting its title-bar controls.
4. **Given** a tagged window, **When** its last tag is removed, **Then** its original visual state
   is restored and it remains available as an untagged window in the notch.

---

### User Story 2 - Automate Tags Without Losing Manual Choices (Priority: P1)

As a user, I want ordered matching rules to assign tags automatically while my explicit choices
remain authoritative and cannot be overwritten by later matching or title changes.

**Why this priority**: Automation saves repeated work only when it preserves user intent.

**Independent Test**: Configure overlapping rules, open matching windows, then manually change
one window's tags and rerun rules after changing its title. The manual tag list remains unchanged.

**Acceptance Scenarios**:

1. **Given** priority-ordered rules matching process name, window class, and title,
   **When** an eligible window appears or its matching attributes change,
   **Then** the highest-priority matching rule supplies its ordered tag list.
2. **Given** a rule-managed window, **When** I manually edit its assignments or their order,
   **Then** the entire resulting list becomes a manual override, protected from rule reruns.
3. **Given** a manual override, **When** I explicitly choose to resume automatic tagging,
   **Then** rules may replace the assignments again; deleting a tag definition is an explicit
   user edit, not a rule override.
4. **Given** an invalid regular expression, **When** I save a rule,
   **Then** I see an actionable error and my last valid rule configuration is preserved.

---

### User Story 3 - Find and Activate a Window in the Notch (Priority: P1)

As a user with many windows, I want a fast right-edge panel with Persian/English search,
text and color chips, and an optional tag grouping that never duplicates windows.

**Why this priority**: The notch removes the duplicate-list problem without replacing Windows' shell.

**Independent Test**: Open a mix of windows, including minimized windows, multiple tags and Snap
Group members. Open the notch on each monitor, search in both languages, navigate with the
keyboard, and activate a result. Compare its entries with the eligible-window inventory.

**Acceptance Scenarios**:

1. **Given** an enabled outer right-edge hot zone, **When** the pointer enters it,
   **Then** the notch opens without a dwell delay; scrolling elsewhere never opens it.
2. **Given** a right edge shared with another monitor, **When** I cross it before 250 ms,
   **Then** the notch stays closed; remaining in the hot zone for 250 ms makes it eligible to open.
3. **Given** 50 eligible windows including Snap Group members and windows with multiple tags,
   **When** the notch opens, **Then** each has exactly one entry with all assigned tag chips,
   whether the list is flat or grouped.
4. **Given** the open notch, **When** I type a Persian or English title/tag fragment,
   **Then** matching entries remain; arrows navigate and Enter activates/restores a selected window.
5. **Given** the open notch, **When** I press Esc or leave both the hot zone and notch panel,
   **Then** it closes; moving from the hot zone into the panel does not immediately dismiss it.
6. **Given** a fullscreen application or game on one monitor, **When** its hot zone is reached,
   **Then** automatic opening is suppressed on that monitor only.

---

### User Story 4 - Recall a Tag With a Hotkey (Priority: P1)

As a keyboard user, I want two distinct tag-recall actions: gather all members onto my current
virtual desktop, or preserve desktop placement and recall members already here while allowing
selection of members elsewhere. Each mode has separate configurable shortcuts and a tag picker.

**Why this priority**: Switching a working set must not require finding each window individually.

**Independent Test**: Assign one tag to overlapping and minimized windows on two desktops.
Test gather mode and confirm every member moves to the current desktop and is restored/raised.
Reset placements; test preserve mode and confirm no window moves, local members are restored/raised,
and selecting a remote member switches to its desktop and activates it. Repeat using both pickers.

**Acceptance Scenarios**:

1. **Given** a tag whose members span virtual desktops, **When** I press its gather shortcut
   (Ctrl+Alt+1 through Ctrl+Alt+9 by default), **Then** all movable eligible members are gathered
   onto the current desktop, minimized members restore, and the set is raised without creating
   a desktop. Inaccessible or unmovable members are individually reported.
2. **Given** the same distributed tag, **When** I press its preserve shortcut
   (Ctrl+Alt+Shift+1 through Ctrl+Alt+Shift+9 by default), **Then** only current-desktop members
   restore/raise, no member changes desktop, and remote members appear for keyboard selection.
   Selecting one switches to its existing desktop and restores/activates that window.
3. **Given** configured tags, **When** I open Ctrl+Alt+Space or Ctrl+Alt+Shift+Space,
   **Then** the picker clearly indicates gather or preserve mode respectively; Enter recalls
   the selected tag in that mode and Esc cancels without changing windows.
4. **Given** a shortcut already owned by another application or WinTag action, **When** WinTag
   tries to enable it, **Then** it reports the conflict, leaves other shortcuts operational,
   and allows the two recall modes to be reassigned independently.
5. **Given** an empty tag or preserve mode with no local members, **When** I recall it,
   **Then** an empty tag is explained without affecting unrelated windows; if remote members
   exist in preserve mode, they remain available for selection without automatic relocation.
6. **Given** either mode's direct shortcuts or picker shortcuts, **When** I change a binding
   in settings and restart WinTag, **Then** the new binding remains configured and recalls the
   intended mode; invalid/conflicting changes keep the previous working binding and report why.
7. **Given** unsupported desktop movement, **When** I invoke gather mode,
   **Then** unavailable operations are reported rather than silently substituting preserve
   mode; preserve mode remains available where desktop identification/switching is supported.

---

### User Story 5 - Send a Window or Tag to a New Desktop (Priority: P2)

As a user, I want one notch action to create a new virtual desktop, move a selected window or
all windows of a selected tag into it, and switch to that desktop.

**Why this priority**: Separating a working set should not require repeated desktop operations.

**Independent Test**: Select a window, then separately a tag with several members, invoke the
send action once, and inspect the newly created desktop and its contents.

**Acceptance Scenarios**:

1. **Given** a nonempty selected tag and supported desktop capabilities, **When** I invoke
   “Send tag to new desktop” once, **Then** a new desktop is created, every member moves there,
   and it becomes the active desktop without additional user commands.
2. **Given** a selected window, **When** I invoke its equivalent action,
   **Then** only that window moves into the new active desktop.
3. **Given** an unsupported desktop capability or a target that cannot move,
   **When** I invoke the action, **Then** WinTag does not claim success; it reports completed,
   skipped and failed steps and preserves the working set as far as safely possible.
4. **Given** an empty tag, **When** I invoke the action,
   **Then** no empty desktop is created and the empty selection is explained.

---

### User Story 6 - Save and Restore a Named Layout (Priority: P2)

As a user, I want a named snapshot of open windows' positions, sizes and virtual desktops that
can restore those same still-open windows without relaunching closed applications.

**Why this priority**: Reconstructing an existing workspace should not require manual arrangement.

**Independent Test**: Save a multi-monitor, multi-desktop layout, move and resize its windows,
close one, and restore it. Remaining windows return and the closed one is reported as skipped.

**Acceptance Scenarios**:

1. **Given** open eligible windows across monitors and desktops, **When** I save a named layout,
   **Then** it records each window's identity, position, size and desktop placement.
2. **Given** that layout and the same still-open windows, **When** I restore it,
   **Then** their saved positions, sizes and desktop placements are restored.
3. **Given** a saved window that has closed, **When** the layout is restored,
   **Then** it is skipped and reported, and no application is relaunched or unrelated window matched.
4. **Given** a disconnected saved monitor or removed desktop, **When** restoration cannot be exact,
   **Then** reachable placements are used where safe and the deviation is reported explicitly.

---

### User Story 7 - Manage and Configure a Working Set (Priority: P3)

As a user, I want notch actions to edit tag assignments/colors and minimize or close a tag's
windows, plus tray controls for pause, autostart, ignore entries and local settings transfer.

**Why this priority**: These complete routine management without expanding into a window manager.

**Independent Test**: Use each action on a small tagged set, including an ignored window and
one with an unsaved document. Export settings, change them, and import the valid export.

**Acceptance Scenarios**:

1. **Given** an open notch, **When** I edit a tag or its color,
   **Then** relevant chips and title-bar cues update without opening another management tool.
2. **Given** several tag members, **When** I minimize the tag,
   **Then** all eligible members minimize and unrelated windows remain unchanged.
3. **Given** unsaved content in a tag member, **When** I close the tag's windows,
   **Then** normal application close/save prompts are respected; WinTag does not force-terminate them.
4. **Given** active WinTag visuals and shortcuts, **When** I pause from the tray,
   **Then** visuals are reverted and automatic management/hot-zone opening and tag hotkeys stop;
   resume reapplies the current configuration to still-open eligible windows.
5. **Given** a valid local export, **When** I import it,
   **Then** it is validated and replaces settings only after user confirmation; invalid imports
   leave the last valid configuration intact.

---

### User Story 8 - Use WinTag Safely in Persian Across Displays (Priority: P1)

As a Persian-speaking user, I want readable right-to-left controls, mixed-direction titles,
keyboard access and correct scaling, with no runtime network activity or lasting visual damage.

**Why this priority**: Privacy, recoverability and native-language usability are release constraints.

**Independent Test**: Run the core flows with Persian UI on mixed-DPI displays, switch to English,
then exit normally and terminate the main process abruptly in separate runs. Inspect original
window appearance, input usability and absence of runtime network traffic.

**Acceptance Scenarios**:

1. **Given** Persian UI and mixed Persian/English titles, **When** I search, select and edit tags,
   **Then** text remains intact, layout is right-to-left, and all actions are keyboard accessible.
2. **Given** monitors with different scaling, **When** a window moves between them,
   **Then** tag cues and the notch remain correctly placed and labels stay readable.
3. **Given** tagged windows, **When** WinTag exits normally or its main process crashes,
   **Then** all WinTag colors and overlays are removed and original appearance is restored,
   without waiting for another WinTag launch.
4. **Given** no network connectivity, **When** I use any supported feature,
   **Then** it works locally without sign-in, telemetry or network requests.

### Edge Cases

- A window closes or changes identity while selected, being tagged, recalled, moved or restored:
  skip/report it safely; never act on a different window reusing its identity.
- A minimized window is still a valid open window. System helper surfaces, duplicate proxy
  surfaces, tool-only windows, ignored targets and WinTag's own UI are not separate list entries.
- A window has several tags: display all chips but place its single grouped entry under its
  primary tag. Untagged windows have one dedicated group. Deleting a primary tag promotes the
  next remaining tag; deleting the final tag restores the original appearance.
- Rules tie in priority or several match: configured list order breaks ties and the first
  matching rule supplies the complete ordered assignment; no implicit union of rule results.
- No rule matches: clear obsolete automatic assignments but preserve any manual override,
  including an explicitly empty manual assignment.
- Titles contain Persian/Arabic variants of yeh or kaf, diacritics, zero-width non-joiners,
  English words, digits or bidirectional punctuation: display the original title unchanged
  while forgiving search treats common equivalent forms consistently.
- A fullscreen app is present on one display, a shared edge covers only part of a monitor,
  or monitor topology changes: apply suppression and dwell only to affected hot-zone segments.
- Hot-zone entry via pointer movement is distinct from wheel scrolling. Leaving eligibility
  before the shared-edge dwell completes cancels opening. Keyboard opening is not a hover event.
- Preserve recall with only remote members opens the remote-member selection without moving
  windows or switching desktops until a member is selected. Esc cancels that selection.
  Gather with a partly unmovable tag reports partial completion rather than changing modes.
- A target rejects activation, desktop movement or visual tagging, including elevated windows:
  report the limitation without bypassing OS protection or claiming full success.
- An app ignores requested title-bar styling: title text plus color still require a safe
  supported presentation; if that cannot be achieved, report the limitation and do not leave
  persistent visual mutations. A color-only fallback is not compliant.
- Settings are malformed, partially written, contain unknown schema versions, duplicate tag
  names, invalid colors or regex patterns, or conflicting shortcuts: reject unsafe changes
  with a local explanation and preserve the last valid settings. Tag names are unique.
- Restoring after a restart must not confuse a newly opened lookalike with a captured window;
  uncertain identities are skipped and reported, not guessed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: WinTag MUST maintain an up-to-date inventory of eligible, user-facing top-level
  windows across all connected monitors and virtual desktops, including minimized windows,
  with title, application/process identity, window class, monitor and desktop when available.
- **FR-002**: The notch MUST show every eligible window exactly once. Snap Group membership
  MUST NOT create a group proxy or duplicate member entry. Ignore entries, helper/tool/proxy
  surfaces and WinTag's own UI MUST NOT appear as eligible windows.
- **FR-003**: Users MUST be able to create, rename, reorder and delete uniquely named tags,
  choose their colors, and assign an ordered list of tags to a window. The first tag is primary.
- **FR-004**: Tagged windows MUST visibly show primary-tag text and color on their title bars.
  Editing the primary tag or its color MUST update the cues; removing all tags MUST restore
  the original appearance. Cues MUST NOT block the target app's controls or input. Presentation
  MUST be compact and low-emphasis, using a narrow color accent rather than a wide filled strip,
  while keeping the text readable and the identification success criterion unchanged.
- **FR-005**: Users MUST be able to define ordered automatic rules using process name, window
  class, title text or regular expressions, including combinations. All conditions within a
  rule MUST match; first matching rule by priority and list order supplies its ordered tags.
- **FR-006**: Rules MUST reevaluate for new windows, relevant attribute changes and explicit
  user reruns. Manual assignment changes MUST protect the full resulting list and its order,
  including an empty list, until the user explicitly resumes automatic tagging.
- **FR-007**: The notch MUST open at the centered right-edge hot zone of each monitor. Default
  dimensions MUST be 6 logical pixels wide at 100% scaling and 30% of that monitor's height.
  Width, height, enabled state, delays and fullscreen suppression MUST be configurable.
- **FR-008**: Outer-edge opening MUST have no dwell delay. A segment shared with another
  monitor MUST require 250 ms continuous dwell by default. Leaving the zone MUST cancel dwell.
- **FR-009**: A fullscreen application/game on a monitor MUST suppress automatic hot-zone
  opening on that monitor by default without suppressing unaffected monitors. Scrolling a
  window without qualifying pointer entry/dwell MUST never trigger opening.
- **FR-010**: The notch MUST close on Esc or when the pointer leaves the combined notch/hot-zone
  area after pointer interaction. Movement from the hot zone into the panel MUST remain usable.
  A keyboard-opened panel MUST remain usable until dismissal or interaction outside it.
- **FR-011**: Each notch entry MUST display its title, primary color and assigned text-tag chips.
  Grouping MUST be optional; grouped entries MUST appear under their primary tag only, with a
  separate untagged group. Other tag chips MUST remain visible and searchable.
- **FR-012**: Typing in the notch MUST filter by title or any assigned tag in Persian or English;
  arrows MUST navigate, Enter MUST activate/restore the selected window, and all exposed
  actions MUST be keyboard accessible with visible focus.
- **FR-013**: The first nine tags in configured order MUST have independently configurable
  shortcuts for two recall modes: gather defaults to Ctrl+Alt+1 through Ctrl+Alt+9 and preserve
  defaults to Ctrl+Alt+Shift+1 through Ctrl+Alt+Shift+9. Ctrl+Alt+Space MUST open the gather tag
  picker and Ctrl+Alt+Shift+Space the preserve tag picker. Each picker MUST indicate its mode,
  use Enter to select and Esc to cancel. Users MUST be able to change every direct-tag and
  picker shortcut independently through the settings UI and readable configuration file.
  Changes MUST persist across restarts; invalid or conflicting assignments MUST produce an
  actionable warning without discarding the last working binding or breaking other bindings.
  Win+Alt+digit MUST NOT be a default shortcut.
- **FR-014**: Gather recall MUST move all movable eligible tag members onto the current desktop,
  restore minimized members, and raise the working set without creating a desktop. Preserve
  recall MUST leave every member on its existing desktop, restore/raise current-desktop members,
  and expose remote members for keyboard selection; choosing one MUST switch to its existing
  desktop and restore/activate it without moving it. Neither mode may close/minimize unrelated
  windows. Refused movements, activations or switches MUST be individually reported, with no
  silent substitution of one recall mode for the other.
- **FR-015**: One notch action MUST create a new virtual desktop, move the selected window or
  all members of a selected tag there, and switch to it. Empty selections MUST NOT create a
  desktop. The operation MUST not require separate create, move or switch commands from the user.
- **FR-016**: Unsupported desktop operations and partial failures MUST be explained locally
  with completed, failed and skipped targets; WinTag MUST attempt safe recovery and MUST NOT
  claim atomic success when Windows prevents completion or rollback.
- **FR-017**: Users MUST be able to save, name, replace with confirmation, delete and restore
  layouts capturing each eligible open window's identity, position, size, monitor and virtual
  desktop, with minimized/maximized state retained for meaningful restoration.
- **FR-018**: Restore MUST act only on confidently identified still-open captured windows,
  restore position, size and desktop, skip/report missing or uncertain matches, and never
  launch closed applications. Unrelated currently open windows MUST be left unchanged.
- **FR-019**: If saved monitors or desktops no longer exist, restore MUST keep windows reachable
  on available displays/desktops, report each fallback, and never represent a fallback as exact.
- **FR-020**: Notch actions MUST support manual assignment/primary-tag editing, tag-color
  editing, layout save/restore, and minimize/close all windows of a selected tag.
- **FR-021**: Close-tag MUST request normal app closure and respect save prompts/cancellation;
  it MUST NOT force-kill a process. Minimize-tag MUST affect only the tag's eligible members.
- **FR-022**: A configurable ignore list MUST exclude matching processes/windows from tagging,
  listing and management actions; newly ignored windows MUST have WinTag visuals reverted.
- **FR-023**: A tray icon MUST expose open-notch/tag-picker access, settings, autostart opt-in,
  pause/resume and quit. Pause MUST restore visuals and suspend automatic tagging, hot-zone
  opening and tag shortcuts while keeping tray controls available.
- **FR-024**: Tags, order, rules, overrides for identifiable still-open windows, hotkeys, ignore
  entries, preferences and layouts MUST persist in readable local UTF-8 JSON at
  `%AppData%\WinTag\config.json`, with a documented configuration version.
- **FR-025**: Users MUST be able to export/import the local configuration. Import MUST validate
  before replacement, require confirmation and preserve previous valid data on failure.
  Invalid settings MUST produce actionable local errors; saves MUST not leave truncated data.
- **FR-026**: The complete UI MUST provide Persian right-to-left and English modes, preserve
  original Unicode text and mixed-direction titles, and never rely on color alone for tag identity.
- **FR-027**: Multi-monitor placement and mixed scaling MUST work for title-bar cues, notch
  positioning, hot-zone geometry, saved layouts and keyboard focus without clipped controls.
- **FR-028**: Normal exit, pause, disabling visuals and main-process crash MUST remove WinTag
  overlays and restore exact prior window styling without requiring a later WinTag launch.
  Cleanup MUST be safe if a target closes or its identity is reused during recovery.
- **FR-029**: Unsupported or inaccessible targets MUST fail safely with a local explanation.
  WinTag MUST use least privilege and MUST NOT inject code into other apps, modify system files,
  or hook Explorer. No unsupported visual strategy may be enabled without crash-cleanup evidence.

### Non-Functional Requirements

- **NFR-001 — Responsiveness**: With 50 eligible windows, the notch MUST be visible and ready
  for input in less than 100 ms after qualifying entry or completed configured dwell.
- **NFR-002 — Idle cost**: Operation MUST be event-driven with no recurring polling. Idle CPU
  MUST be near zero; the plan MUST define the measurement protocol and numerical acceptance
  threshold before implementation. Total background RAM across all WinTag processes MUST be
  below 100 MB, including any safety helper.
- **NFR-003 — Privacy**: Every runtime component MUST be local-only: no telemetry, runtime
  network connections, cloud storage, remote diagnostics, accounts or automatic update checks.
- **NFR-004 — Recoverability**: Normal exit and abrupt main-process termination MUST both
  restore original visuals. Recovery on the next launch alone is insufficient. Configuration
  writes MUST preserve a last-known-good file, including when interrupted.
- **NFR-005 — Accessibility**: Every operation MUST be usable with the keyboard. Text labels,
  legible contrast, visible focus and correct Persian/RTL reading order MUST accompany color.
- **NFR-006 — Compatibility**: Verification MUST include Comet, Firefox, Obsidian, a Notion
  multi-window scenario, and representative Win32, WPF and WinUI applications on Windows 11.
  Supported Windows builds and capability limitations MUST be recorded from actual testing.
- **NFR-007 — Predictability**: Manual precedence, rule ordering, duplicate elimination and
  missing-window restore behavior MUST be reproducible and covered by documented tests.
- **NFR-008 — Smooth desktop transfer**: A supported one-action transfer MUST avoid unnecessary
  intermediate focus/desktop changes or repeated visible show/hide cycles; manual verification
  MUST record any OS-imposed flicker or partial completion rather than claiming it absent.

### Key Entities *(include if feature involves data)*

- **Managed Window**: A live, eligible user window with a safe identity, title, application,
  class, current monitor/desktop, geometry, visibility state and ordered assignments.
- **Tag**: A persistent identity, unique human-readable name, color and user-controlled order.
- **Window Assignment**: An ordered tag list for a managed window, its primary tag and its
  automatic/manual ownership state. Explicit manual removal can leave an empty protected list.
- **Rule**: Enabled state, priority/list order, matching conditions, and ordered output tags.
- **Hot-Zone Preferences**: Right-edge geometry, delay values, monitor applicability, enabled
  state and fullscreen suppression setting.
- **Layout / Layout Entry**: A named collection of captured window identities, positions,
  sizes, monitor/desktop placements and window states, independent of application relaunch.
- **Configuration**: Versioned local settings containing the entities above, shortcuts,
  ignored targets, UI language, autostart preference and other behavior preferences.
- **Operation Result**: Completed, skipped and failed targets with local explanations for
  restore, desktop transfer, activation or group actions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: In a recorded test with 20 tagged open windows, the intended user identifies
  each requested window in less than 2 seconds using visible tag text and color. Record
  per-request times, target names and test conditions; do not substitute an average.
- **SC-002**: With 50 eligible windows, the notch is usable in less than 100 ms after trigger
  eligibility in every recorded trial, excluding the separately measured shared-edge dwell.
- **SC-003**: Comparing the notch with the eligible-window inventory produces zero missing
  windows and zero duplicate entries, including Snap Group members and multi-tag windows,
  in flat, grouped and filtered views.
- **SC-004**: The default 6-pixel-at-100%-scaling by 30%-height hot zone opens on every monitor;
  outer edges add no dwell, and shared segments require 250 ms. All scroll-only and early
  shared-edge-crossing test cases produce zero unintended openings. Fullscreen suppression
  affects only the occupied monitor.
- **SC-005**: Both recall modes pass distributed-tag tests through their separate shortcuts
  and pickers. Gather moves all movable eligible members to the current desktop and restores/
  raises them with zero new desktops. Preserve moves zero windows, restores/raises all eligible
  local members, and lets the user select a remote member to switch to its existing desktop
  and restore/activate it. Each refused movement/activation/switch is reported; unrelated
  windows are never minimized or closed and the two modes never silently substitute. Every
  direct-tag and picker binding can be changed independently in settings and survives restart;
  rejected conflicting bindings leave the previous working bindings intact.
- **SC-006**: In supported desktop-transfer tests, a single user invocation creates a new
  desktop, moves exactly the selected live target set and switches to it. Empty selections
  create zero desktops; unsupported/partial cases report outcomes without false success.
- **SC-007**: With unchanged display topology, a saved layout restores the position and size
  of each matched still-open window within one logical pixel and restores its desktop and
  window state. Closed windows are skipped/reported with zero relaunches or false matches.
- **SC-008**: Manual ordered tag lists, including empty overrides, remain unchanged in every
  rule rerun, title-change and priority-change test until the user explicitly releases them.
- **SC-009**: After normal exit, pause and forced termination of the main process in separate
  tests, zero WinTag-owned visual changes remain and original styling is restored without
  restarting WinTag. Recovery latency is recorded and its bound defined during safety research.
- **SC-010**: All listed user actions are completed using only the keyboard in Persian and
  English UI, including search, tag picker, grouping, assignment, layout and group actions;
  mixed-direction labels remain readable on all test displays.
- **SC-011**: Background memory remains below 100 MB for the complete WinTag process set with
  50 windows; idle CPU meets the near-zero threshold and observation protocol fixed in the plan,
  with no recurring window or cursor polling.
- **SC-012**: Runtime verification records zero application-initiated network connections
  and no external telemetry; all supported features work while the device is offline.
- **SC-013**: Valid configuration export/import round-trips tags, order, colors, rules, layouts,
  hotkeys and Persian text without semantic loss; every malformed/unsupported import test
  preserves the last valid settings. Interrupted saves do not destroy that configuration.
- **SC-014**: A window can be tagged from the available management UI in less than 3 seconds
  and at most 3 clicks or keyboard commands, excluding entry of a newly created tag name.
- **SC-015**: The compatibility checklist records actual results for every named target app,
  mixed-DPI multi-monitor use, virtual desktops, normal exit and crash cleanup; unexecuted
  Windows checks are explicitly marked pending and are not counted as passing evidence.

## Assumptions

- **Authority and technical separation**: The user's supplied decisions override unresolved
  markers in the older SRS. Title-bar presentation technique, framework, OS interop choices,
  and desktop compatibility research belong in the plan, not in this product specification.
  This document does not claim any Windows experiments have run.
- **Platform baseline**: Scope is Windows 11 only. A particular minimum release/build has not
  been promised; the supported build matrix follows actual capability testing in the plan.
  An undocumented desktop capability may be unavailable while safe core features remain usable.
- **Live-window identity**: Manual overrides apply to the same identifiable live window and
  survive rule reruns and WinTag restarts only when that identity can still be established.
  Closing and reopening an app creates a new window; v1 does not promise that manual tags
  follow newly created lookalikes. Rules provide repeatable tagging for new windows.
- **Rule defaults**: Process-name and class text matching are case-insensitive; plain-title
  matching is substring-based and case-insensitive where applicable. Regular-expression
  matching is explicitly selected and its case behavior is configurable. Conditions combine
  with AND, while rule priority/list order selects the first match. These are reviewable defaults.
- **Search defaults**: Title/tag search uses a case-insensitive substring over a normalized
  search copy, treating common Persian/Arabic yeh/kaf variants consistently and ignoring
  diacritics and zero-width non-joiners for matching; original display/storage text is preserved.
  This forgiving search does not silently rewrite regex patterns or change regex semantics.
- **Cross-desktop recall — confirmed decision**: The user explicitly requested both gather
  (A) and preserve (B) with different shortcuts. Neither is an exclusive global preference.
  The original Ctrl+Alt shortcuts retain gather behavior; adding Shift supplies preserve-mode
  defaults for both direct tag shortcuts and the picker. Exact added key bindings are
  configurable defaults, not reserved or guaranteed conflict-free Windows combinations.
- **Panel access**: Pointer opening does not steal focus from the active app merely on hover;
  clicking into the panel or keyboard/tray opening gives it keyboard focus for type-to-search.
  Keyboard-opened panels are not immediately closed because the pointer is elsewhere.
- **Layout fallbacks**: If the saved desktop no longer exists, use the current desktop and
  report the fallback; if a monitor is missing, fit the window into an available work area
  and report it. Exact restoration is promised only for unchanged topology and available targets.
- **Autostart and pause**: Autostart is opt-in. Pausing retains settings but reverses visuals
  and suspends management behavior; quitting also unregisters WinTag-owned shortcuts and input
  monitoring. Resume does not relaunch apps or recover windows that have closed.
- **Close semantics**: Group closure uses normal app close behavior. App-owned save prompts
  can prevent or delay completion and must not be bypassed.
- **Out of scope v1**: Relaunching closed apps, tiling/window-manager replacement, replacing
  Snap Layouts, altering system Alt+Tab/Task View duplicates, cloud sync, Windows 10, other
  operating systems, modifying system files, hooking Explorer, and notch placement on other edges.
- **Validation environment**: Windows-specific performance, visual, multi-monitor and desktop
  checks require real Windows 11 environments; documentation authored here is not proof of them.
  Power loss/OS crash is distinguished from termination of WinTag's main process.
