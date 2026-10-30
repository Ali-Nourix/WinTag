# UI and Keyboard Contract

## Recall modes and rebinding

| Action | Default | Behavior |
| --- | --- | --- |
| Gather tag 1–9 | Ctrl+Alt+1..9 | Move supported members to current desktop, restore/raise |
| Preserve tag 1–9 | Ctrl+Alt+Shift+1..9 | Keep desktop placement, restore/raise local members; offer remote selection |
| Gather picker | Ctrl+Alt+Space | Choose any tag, Enter gathers, Esc cancels |
| Preserve picker | Ctrl+Alt+Shift+Space | Choose any tag, Enter preserves, Esc cancels |

Settings has a mode/action-labeled shortcut editor for every row/tag. Apply validates conflict,
keeps previous working bindings on failure and persists successful changes across restart.
Default combinations are not assumed available. Never use Win+Alt+digit as a default.
In Preserve's remote list, Enter explicitly switches to the selected existing desktop and
restores/activates that member; Esc makes no remote move or switch. No local members still
shows remote choices. Gather failure never silently substitutes Preserve.

## Notch and RTL

One reusable panel per active monitor interaction, positioned on that monitor's right edge.
6 DIP wide / 30% height centered hot zone; segment-specific dwell 0/250 ms, configurable.
Pointer entry opens without stealing keyboard focus. Clicking panel or keyboard/tray opening
focuses search. Do not dismiss a keyboard-opened panel solely because the pointer is elsewhere.
Esc closes, mouse exit from the combined panel/hot-zone area closes pointer interaction.
Fullscreen suppression is per monitor and defaults on; no wheel-only activation.

Arrows navigate, typing filters original-title/tag search keys, Enter activates selected window.
Tab/Shift+Tab traverse actionable controls; contextual action menu is keyboard accessible.
Restore prior valid focus on cancellation; tag recall deliberately transfers focus.
Groups use primary tag only; one untagged group, all chips displayed, no duplicate entries.
Persian FlowDirection=RightToLeft, English=LeftToRight; isolate mixed-direction title fragments,
keep physical right-edge activation independent from reading direction, and expose automation
names for tags/actions. No color-only status; visible focus and adequate contrast required.

## Actions and outcomes

Entries expose tag assignment/primary order, colors, send window/tag to new desktop, layout
save/restore, minimize tag and normal close tag. Normal app save prompts are not bypassed.
Tray exposes open panel/pickers, settings, autostart opt-in, pause/resume and quit.
Pause removes visuals and hotkey/hot-zone management while retaining tray access/settings.
New ignore entries revert affected visuals and remove targets from management.
Results explicitly distinguish completed, skipped, failed and fallback targets. Unsupported
builds disable affected actions with a Persian/English explanation, not a misleading success.
