# KeyPress

A Windows Forms utility that shows live input from your keyboard, mouse, touchscreen, and
trackpad — useful for testing input devices, demoing what a device reports, or debugging input
handling in other apps.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)

## Download

KeyPress targets .NET Framework 4.8, which ships with Windows itself — nothing to download or
install beyond the app on a normal Windows 11 machine.

- **[dist/KeyPress-Setup.exe](dist/KeyPress-Setup.exe)** — installer. Run it, follow the wizard; it
  adds a Start Menu shortcut and a normal Add/Remove Programs entry. See
  [Installing](#installing) below.
- **[dist/KeyPress-Portable.zip](dist/KeyPress-Portable.zip)** — portable. No install, no admin
  rights, nothing written outside its own folder (except the small settings file described below).
  See [Running the portable version](#running-the-portable-version) below.

Both are built from the same source in this repo (see [Building](#building) for how to regenerate
them yourself).

### Installing

1. Download `dist/KeyPress-Setup.exe`.
2. Run it. Windows may show a SmartScreen prompt since the installer isn't code-signed — click
   **More info > Run anyway** if so.
3. Follow the wizard (it'll ask whether to install just for you or for all users, and offers an
   optional desktop shortcut).
4. Launch KeyPress from the Start Menu. Uninstall later from Settings > Apps, like any other
   installed program.

### Running the portable version

1. Download `dist/KeyPress-Portable.zip`.
2. Extract it anywhere — a folder, a USB drive, a network share. Keep `KeyPress.exe` and
   `KeyPress.exe.config` together in the same folder; the `.config` file declares which .NET
   Framework version to run against.
3. Run `KeyPress.exe` directly. No installer, no admin rights, and nothing is written to the
   system other than the same `%LOCALAPPDATA%\KeyPress\settings.ini` the installed version uses
   (window size and View-menu toggles) — delete that folder to fully reset or remove all trace of
   having run it.
4. To update, just replace `KeyPress.exe`/`KeyPress.exe.config` with a newer extract; settings
   carry over since they live outside the app's own folder.

## What it shows

**Held input** — the big centered line lists every key, mouse button, and pointer (touch/pen/
touchpad) currently held down. When nothing is pressed it fades from full opacity down to a dim
20% and stays there, so the idle state isn't distracting.

**Released input** — a second line lists what was just released (keys, mouse buttons, touch
contacts, scroll-wheel activity), fading out over ~1.4 seconds.

**Virtual keyboard overlay** — sits between the two text lines and highlights each key in red the
instant it's detected as pressed. Covers the full layout: main alphanumeric block, arrow cluster,
a navigation block (Insert/Home/PageUp over Delete/End/PageDown), and a numpad. A mouse cluster
sits just above the arrow keys: Left/Right buttons flank a center strip with
scroll-up/middle-click/scroll-down, all highlighting the same way (scroll indicators flash
briefly, since wheel ticks are discrete rather than held).

**Position boxes**, shown across the top and toggleable individually from the View menu, each
marked with a small icon for its device type:

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
`%LOCALAPPDATA%\KeyPress\settings.ini` (a plain `Key=Value` text file, not JSON).

## Building

Requires the .NET SDK (targets `net48`, .NET Framework — already installed on any Windows machine)
and Windows (WinForms).

```
dotnet build
```

Or open `KeyPress.sln` in Visual Studio and run.

### Regenerating the distributables in `dist/`

```
dotnet publish KeyPress\KeyPress.csproj -c Release -p:PublishProfile=FolderProfile
```

produces the portable build at `KeyPress\bin\Release\V3\` (`KeyPress.exe` + `KeyPress.exe.config`)
— zip those two files together for `dist/KeyPress-Portable.zip`. Then, with the free
[Inno Setup](https://jrsoftware.org/isinfo.php) compiler installed:

```
ISCC.exe installer\KeyPress.iss
```

produces `installer\Output\KeyPress-Setup.exe` — copy that to `dist/KeyPress-Setup.exe`.

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
