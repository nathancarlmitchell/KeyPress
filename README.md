# KeyPress

A Windows Forms utility that shows live input from your keyboard, mouse, touchscreen, and
trackpad — useful for testing input devices, demoing what a device reports, or debugging input
handling in other apps.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)

## What it shows

**Held input** — the big centered line lists every key, mouse button, and pointer (touch/pen/
touchpad) currently held down. When nothing is pressed it fades from full opacity down to a dim
20% and stays there, so the idle state isn't distracting.

**Released input** — a second line lists what was just released (keys, mouse buttons, touch
contacts, scroll-wheel activity), fading out over ~1.4 seconds.

**Position boxes**, shown across the top and toggleable individually from the View menu:

- **Cursor** — one box per connected monitor, each showing the OS cursor's position relative to
  that monitor's own top-left corner. Only the monitor the cursor is actually on shows a live
  position; the rest read "Not active."
- **Touch Screen** — tracks touchscreen contacts via `WM_POINTER` messages, with one colored dot
  per active finger.
- **Trackpad** — the OS cursor's position again, scoped to whichever monitor the app window is
  currently on (a trackpad moves the same cursor a mouse does — Windows doesn't expose which
  device last moved it, so this and Cursor necessarily show the same underlying signal).
- **Trackpad Surface (raw)** — reads the touchpad's own HID digitizer collection directly via
  `RegisterRawInputDevices` + `HidP_*` parsing, independent of the OS cursor entirely. Shows the
  physical finger position(s) on the pad's own sensor grid, with one colored dot per active
  contact. Only available on genuine Precision Touchpad (PTP) hardware whose report descriptor has
  the expected shape.

Any box for hardware that isn't detected is greyed out automatically.

## Menus

- **File > Refresh** — re-runs hardware and monitor detection on demand (e.g. after plugging in a
  trackpad or a new display), without restarting the app.
- **View** — a checkbox per box (Cursor, Touch Screen, Trackpad, Trackpad Surface, Key Press Text,
  Key Release Text) plus **Reset**, which restores the default window size and re-enables/disables
  boxes based on what's currently detected.

Window size and every View-menu checkbox are remembered between runs, in
`%LOCALAPPDATA%\KeyPress\settings.json`.

## Building

Requires the .NET 9 SDK and Windows (WinForms).

```
dotnet build
```

Or open `KeyPress.sln` in Visual Studio and run.

## Known limitations

- **Touch that starts outside the window**: Windows' pointer-capture model gives full move data
  only to the window that was under a contact at the moment it went down. A finger dragged in from
  outside the window (or from another app) only produces hit-test notifications, not real
  coordinates — this is an OS capture rule (the same one governing mouse drags), not something an
  app can opt out of.
- **Raw trackpad tracking**: only works for hardware exposing a standard Precision Touchpad HID
  Digitizer collection. Active-contact detection assumes fingers occupy the first *N* declared
  contact slots (where *N* is the reported contact count) rather than checking each slot's own Tip
  Switch bit — reading button capabilities via `HidP_GetButtonCaps` triggered a native heap
  corruption crash on real hardware and was removed rather than risk it again blind.
- **Multi-monitor Cursor/monitor enumeration** happens once at startup; use File > Refresh after
  changing your monitor or trackpad setup rather than restarting.
