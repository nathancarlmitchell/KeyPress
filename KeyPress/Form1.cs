using System.Runtime.InteropServices;

namespace KeyPress
{
    public partial class Form1 : Form, IMessageFilter
    {
        // Keys currently held down, in the order they were pressed.
        private readonly List<Keys> heldKeys = new();

        // Mouse buttons currently held down, in the order they were pressed.
        private readonly List<MouseButtons> heldButtons = new();

        // Virtual on-screen keyboard, sandwiched between the pressed/released text
        // lines. Built entirely in code (see BuildVirtualKeyboard) rather than in
        // the Designer, since it's a couple hundred small key labels laid out from
        // data. WinForms collapses left/right Shift/Ctrl/Alt to one generic KeyCode
        // (Keys.ShiftKey/ControlKey/Menu), so both visual keycaps for those map to
        // the same logical key and light up together — the same simplification the
        // rest of the app already makes for those keys.
        private readonly Dictionary<Keys, List<Label>> keyboardKeyLabels = new();
        private readonly Dictionary<MouseButtons, List<Label>> keyboardMouseButtonLabels = new();
        private Label mouseScrollUpLabel = null!;
        private Label mouseScrollDownLabel = null!;
        private readonly System.Windows.Forms.Timer scrollFlashTimer = new() { Interval = 220 };
        private Panel keyboardPanel = null!;
        private Panel keyboardGrid = null!;
        private static readonly Color KeyboardKeyBackColor = SystemColors.Window;
        private static readonly Color KeyboardKeyForeColor = SystemColors.ControlText;

        // Active touch / pen / touchpad contacts, by pointer id.
        private readonly Dictionary<uint, string> activePointers = new();

        // Wheel ticks and gestures don't "stay down", so show them as a brief notice.
        private readonly System.Windows.Forms.Timer noticeTimer = new() { Interval = 700 };
        private string transientNotice = "";

        // Second line: inputs released (keys, mouse buttons, touch, scroll) within
        // the last 1400ms, in release order. Fades out over that window.
        private readonly System.Windows.Forms.Timer releaseTimer = new() { Interval = 3000 };
        private readonly List<string> releasedItems = new();
        private readonly System.Windows.Forms.Timer fadeTimer = new() { Interval = 33 };
        private DateTime releaseShownAt;
        private Color releaseBaseColor;

        // The idle "(nothing pressed)" placeholder starts at full opacity and
        // fades down to 20% over IdleFadeDurationMs, holding there until real
        // input arrives; real held input is always shown at full strength.
        private const double IdleFadeMinOpacity = 0.2;
        private const double IdleFadeDurationMs = 3000;
        private readonly System.Windows.Forms.Timer idleFadeTimer = new() { Interval = 33 };
        private DateTime idleSince;
        private bool isIdle;
        private Color displayBaseColor;

        // Wheel ticks are discrete; "scroll stopped" fires once they pause.
        private readonly System.Windows.Forms.Timer scrollIdleTimer = new() { Interval = 300 };
        private string scrollIdleLabel = "";

        // Polls the OS cursor and reports its position relative to the Cursor
        // (per-monitor) and Trackpad boxes' own coordinate spaces.
        private readonly System.Windows.Forms.Timer cursorTimer = new() { Interval = 30 };

        // One Cursor box per connected monitor (built at startup — see
        // BuildCursorMonitorBoxes). Only the box for whichever monitor the
        // cursor is actually on shows a live position; the rest show "Not active".
        private sealed class CursorMonitorBox
        {
            public required Screen Screen { get; init; }
            public required Panel Surface { get; init; }
            public required Label CoordLabel { get; init; }
            public required Panel Dot { get; init; }
        }

        private readonly List<CursorMonitorBox> cursorMonitorBoxes = new();

        // Active touch-screen contacts: last known screen position, and the marker
        // panel shown for each inside touchSurface.
        private readonly Dictionary<uint, Point> touchScreenPoints = new();
        private readonly Dictionary<uint, Panel> touchDots = new();

        private static readonly Color[] TouchDotColors =
        {
            Color.OrangeRed,
            Color.MediumSeaGreen,
            Color.Gold,
            Color.MediumPurple,
            Color.DeepSkyBlue,
        };

        // Not readonly: RefreshHardware() re-detects these on demand.
        private bool touchScreenAvailable;
        private bool trackpadAvailable;

        // Raw pad-surface tracking: the touchpad's own HID digitizer collection,
        // read independently of the OS cursor. Only available on genuine precision
        // touchpad (PTP) hardware whose report descriptor has the expected shape.
        private IntPtr touchpadDeviceHandle = IntPtr.Zero;
        private IntPtr touchpadPreparsedData = IntPtr.Zero;
        private bool touchpadRawTrackingReady;
        private ushort touchpadContactCountLink;

        // One slot per finger contact the descriptor declares (typically up to 5
        // for a PTP touchpad), each with its own dot color reusing TouchDotColors.
        // Contact Count tells us how many are currently down; per real hardware
        // behavior, active fingers occupy the first N slots in link-collection
        // order (there's no per-slot Tip Switch check here — see the note on
        // HidP_GetButtonCaps's removal after it corrupted the native heap).
        private sealed class TouchpadContactSlot
        {
            public required ushort LinkCollection { get; init; }
            public required int XMin { get; init; }
            public required int XMax { get; init; }
            public required int YMin { get; init; }
            public required int YMax { get; init; }
            public required Panel Dot { get; init; }
            public bool Active { get; set; }
            public Point RawPoint { get; set; }
        }

        private readonly List<TouchpadContactSlot> touchpadContactSlots = new();

        // ---- Persisted settings (window size + View-menu state) ---------------

        private sealed class AppSettings
        {
            public int WindowWidth { get; set; }
            public int WindowHeight { get; set; }
            public bool Maximized { get; set; }
            public bool ShowCursor { get; set; }
            public bool ShowTrackpad { get; set; }
            public bool ShowTrackpadRaw { get; set; }
            public bool ShowTouchScreen { get; set; }

            // Default to true via the initializer (not the ?? fallback below) so
            // a settings file saved before these existed — which will be missing
            // these properties — still backfills to "on" rather than "off".
            public bool ShowKeyPressText { get; set; } = true;
            public bool ShowKeyReleaseText { get; set; } = true;
        }

        private static string SettingsFilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KeyPress",
                "settings.ini"
            );

        // Plain "Key=Value" lines rather than JSON — the format doesn't need a
        // parser library for a handful of scalar fields, and dropping
        // System.Text.Json keeps the net48 build to just the exe.
        private static AppSettings? LoadSettings()
        {
            try
            {
                string path = SettingsFilePath;
                if (!File.Exists(path))
                {
                    return null;
                }

                var values = new Dictionary<string, string>();
                foreach (string line in File.ReadAllLines(path))
                {
                    int separatorIndex = line.IndexOf('=');
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }
                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();
                    values[key] = value;
                }

                // Only ever overwrite an AppSettings default when the key is
                // actually present and valid — a settings file saved before a
                // field existed (or a corrupt single line) should fall back to
                // that field's own default, not zero it out.
                var settings = new AppSettings();
                if (ParseInt(values, nameof(AppSettings.WindowWidth)) is int width)
                {
                    settings.WindowWidth = width;
                }
                if (ParseInt(values, nameof(AppSettings.WindowHeight)) is int height)
                {
                    settings.WindowHeight = height;
                }
                if (ParseBool(values, nameof(AppSettings.Maximized)) is bool maximized)
                {
                    settings.Maximized = maximized;
                }
                if (ParseBool(values, nameof(AppSettings.ShowCursor)) is bool showCursor)
                {
                    settings.ShowCursor = showCursor;
                }
                if (ParseBool(values, nameof(AppSettings.ShowTrackpad)) is bool showTrackpad)
                {
                    settings.ShowTrackpad = showTrackpad;
                }
                if (ParseBool(values, nameof(AppSettings.ShowTrackpadRaw)) is bool showTrackpadRaw)
                {
                    settings.ShowTrackpadRaw = showTrackpadRaw;
                }
                if (ParseBool(values, nameof(AppSettings.ShowTouchScreen)) is bool showTouchScreen)
                {
                    settings.ShowTouchScreen = showTouchScreen;
                }
                if (
                    ParseBool(values, nameof(AppSettings.ShowKeyPressText)) is bool showKeyPressText
                )
                {
                    settings.ShowKeyPressText = showKeyPressText;
                }
                if (
                    ParseBool(values, nameof(AppSettings.ShowKeyReleaseText))
                    is bool showKeyReleaseText
                )
                {
                    settings.ShowKeyReleaseText = showKeyReleaseText;
                }
                return settings;
            }
            catch
            {
                return null; // Missing, corrupt, or unreadable — just use defaults.
            }
        }

        private static int? ParseInt(Dictionary<string, string> values, string key) =>
            values.TryGetValue(key, out string? raw) && int.TryParse(raw, out int parsed)
                ? parsed
                : null;

        private static bool? ParseBool(Dictionary<string, string> values, string key) =>
            values.TryGetValue(key, out string? raw) && bool.TryParse(raw, out bool parsed)
                ? parsed
                : null;

        private void SaveSettings()
        {
            try
            {
                bool maximized = WindowState == FormWindowState.Maximized;
                Size normalSize = WindowState == FormWindowState.Normal ? Size : RestoreBounds.Size;

                var settings = new AppSettings
                {
                    WindowWidth = normalSize.Width,
                    WindowHeight = normalSize.Height,
                    Maximized = maximized,
                    ShowCursor = viewCursorMenuItem.Checked,
                    ShowTrackpad = viewTrackpadMenuItem.Checked,
                    ShowTrackpadRaw = viewTrackpadRawMenuItem.Checked,
                    ShowTouchScreen = viewTouchScreenMenuItem.Checked,
                    ShowKeyPressText = viewKeyPressTextMenuItem.Checked,
                    ShowKeyReleaseText = viewKeyReleaseTextMenuItem.Checked,
                };

                string path = SettingsFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllLines(
                    path,
                    new[]
                    {
                        $"{nameof(AppSettings.WindowWidth)}={settings.WindowWidth}",
                        $"{nameof(AppSettings.WindowHeight)}={settings.WindowHeight}",
                        $"{nameof(AppSettings.Maximized)}={settings.Maximized}",
                        $"{nameof(AppSettings.ShowCursor)}={settings.ShowCursor}",
                        $"{nameof(AppSettings.ShowTrackpad)}={settings.ShowTrackpad}",
                        $"{nameof(AppSettings.ShowTrackpadRaw)}={settings.ShowTrackpadRaw}",
                        $"{nameof(AppSettings.ShowTouchScreen)}={settings.ShowTouchScreen}",
                        $"{nameof(AppSettings.ShowKeyPressText)}={settings.ShowKeyPressText}",
                        $"{nameof(AppSettings.ShowKeyReleaseText)}={settings.ShowKeyReleaseText}",
                    }
                );
            }
            catch
            {
                // Best-effort only — never block shutdown over a settings-save failure.
            }
        }

        // The Designer's own window size, captured before any saved-settings
        // resize is applied — View > Reset restores exactly this, so it always
        // matches whatever the Designer is actually configured for.
        private readonly Size defaultWindowSize;

        public Form1()
        {
            InitializeComponent();

            // Pulled from the exe's own Win32 icon resource (embedded via the
            // csproj's ApplicationIcon) rather than a second copy baked into
            // Form1.resx as a binary resource — that binary-resource path is
            // what pulled in the System.Resources.Extensions dependency on
            // net48, so extracting it at runtime keeps this a single exe.
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            defaultWindowSize = Size;

            AddBoxIcon(trackpadSurface, BoxIconKind.Trackpad);
            AddBoxIcon(trackpadRawSurface, BoxIconKind.Trackpad);
            AddBoxIcon(touchSurface, BoxIconKind.TouchScreen);

            releaseBaseColor = releaseLabel.ForeColor;
            displayBaseColor = displayLabel.ForeColor;

            AppSettings? savedSettings = LoadSettings();
            const int MinWindowDimension = 300;
            const int MaxWindowDimension = 10000;
            if (
                savedSettings != null
                && savedSettings.WindowWidth is >= MinWindowDimension and <= MaxWindowDimension
                && savedSettings.WindowHeight is >= MinWindowDimension and <= MaxWindowDimension
            )
            {
                Size = new Size(savedSettings.WindowWidth, savedSettings.WindowHeight);
                if (savedSettings.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }

            // View menu: each item's checked state controls that box's visibility.
            // Wired once here; ApplyHardwareDetection (startup and Refresh alike)
            // only ever sets .Checked/.Visible, never re-subscribes these.
            viewCursorMenuItem.CheckedChanged += (s, e) =>
            {
                foreach (CursorMonitorBox box in cursorMonitorBoxes)
                {
                    box.Surface.Visible = viewCursorMenuItem.Checked;
                }
            };
            viewTrackpadMenuItem.CheckedChanged += (s, e) =>
                trackpadSurface.Visible = viewTrackpadMenuItem.Checked;
            viewTrackpadRawMenuItem.CheckedChanged += (s, e) =>
                trackpadRawSurface.Visible = viewTrackpadRawMenuItem.Checked;
            viewTouchScreenMenuItem.CheckedChanged += (s, e) =>
                touchSurface.Visible = viewTouchScreenMenuItem.Checked;
            viewKeyPressTextMenuItem.CheckedChanged += (s, e) =>
                displayLabel.Visible = viewKeyPressTextMenuItem.Checked;
            viewKeyReleaseTextMenuItem.CheckedChanged += (s, e) =>
                releaseLabel.Visible = viewKeyReleaseTextMenuItem.Checked;
            refreshMenuItem.Click += (s, e) => RefreshHardware();
            viewResetMenuItem.Click += (s, e) => ResetToDefaults();

            bool showKeyPressText = savedSettings?.ShowKeyPressText ?? true;
            bool showKeyReleaseText = savedSettings?.ShowKeyReleaseText ?? true;
            viewKeyPressTextMenuItem.Checked = showKeyPressText;
            displayLabel.Visible = showKeyPressText;
            viewKeyReleaseTextMenuItem.Checked = showKeyReleaseText;
            releaseLabel.Visible = showKeyReleaseText;

            // Detects touch screen / trackpad / monitors and applies the result
            // (including a first-run default of "on" for each, same as a saved
            // preference of true would) — the same path Refresh uses later.
            viewCursorMenuItem.Checked = savedSettings?.ShowCursor ?? true;
            ApplyHardwareDetection(
                preferredShowTouchScreen: savedSettings?.ShowTouchScreen ?? true,
                preferredShowTrackpad: savedSettings?.ShowTrackpad ?? true,
                preferredShowTrackpadRaw: savedSettings?.ShowTrackpadRaw ?? true
            );

            // Keep the box row centered as boxes are shown/hidden and as the
            // window is resized.
            boxesPanel.SizeChanged += (s, e) => CenterBoxesPanel();
            ClientSizeChanged += (s, e) => CenterBoxesPanel();
            CenterBoxesPanel();

            BuildVirtualKeyboard();
            ClientSizeChanged += (s, e) => CenterKeyboardGrid();
            CenterKeyboardGrid();

            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;

            // Mouse buttons are handled via the raw WM_*BUTTONDOWN/UP messages in
            // PreFilterMessage below, not WinForms MouseDown/MouseUp events — the
            // position boxes and virtual keyboard are real child controls, so a
            // click over any of them would otherwise be swallowed by that child
            // and never reach the form.

            // A message filter catches wheel / touch / gesture / button messages
            // no matter which child window they are dispatched to.
            Application.AddMessageFilter(this);

            noticeTimer.Tick += (s, e) =>
            {
                noticeTimer.Stop();
                transientNotice = "";
                UpdateDisplay();
            };

            releaseTimer.Tick += (s, e) =>
            {
                releaseTimer.Stop();
                fadeTimer.Stop();
                releasedItems.Clear();
                releaseLabel.ForeColor = releaseBaseColor;
                UpdateDisplay();
            };

            fadeTimer.Tick += (s, e) => UpdateReleaseFade();
            idleFadeTimer.Tick += (s, e) => UpdateIdleFade();

            cursorTimer.Tick += (s, e) =>
            {
                UpdateCursorMonitorBoxes();
                if (trackpadAvailable)
                {
                    UpdatePointerBox(trackpadSurface, trackpadCoordLabel, trackpadDot);
                }
            };
            cursorTimer.Start();

            scrollIdleTimer.Tick += (s, e) =>
            {
                scrollIdleTimer.Stop();
                AddRelease(scrollIdleLabel + " stopped");
            };

            scrollFlashTimer.Tick += (s, e) =>
            {
                scrollFlashTimer.Stop();
                SetScrollIndicatorActive(mouseScrollUpLabel, false);
                SetScrollIndicatorActive(mouseScrollDownLabel, false);
            };

            Deactivate += (s, e) =>
            {
                heldKeys.Clear();
                heldButtons.Clear();
                activePointers.Clear();
                foreach (Keys key in keyboardKeyLabels.Keys)
                {
                    HighlightVirtualKey(key, false);
                }
                foreach (MouseButtons button in keyboardMouseButtonLabels.Keys)
                {
                    HighlightVirtualMouseButton(button, false);
                }
                ClearAllTouches();
                noticeTimer.Stop();
                transientNotice = "";
                scrollIdleTimer.Stop();
                scrollFlashTimer.Stop();
                SetScrollIndicatorActive(mouseScrollUpLabel, false);
                SetScrollIndicatorActive(mouseScrollDownLabel, false);
                releaseTimer.Stop();
                fadeTimer.Stop();
                releasedItems.Clear();
                releaseLabel.ForeColor = releaseBaseColor;
                UpdateDisplay();
            };

            ClearAllTouches();
            UpdateDisplay();
        }

        // ---- Win32 interop -------------------------------------------------------

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_XBUTTONUP = 0x020C;

        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_MOUSEHWHEEL = 0x020E;

        private const int WM_POINTERUPDATE = 0x0245;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;
        private const int WM_POINTERLEAVE = 0x024A;

        private const int WM_GESTURE = 0x0119;

        private const int GID_BEGIN = 1;
        private const int GID_END = 2;
        private const int GID_ZOOM = 3;
        private const int GID_PAN = 4;
        private const int GID_ROTATE = 5;
        private const int GID_TWOFINGERTAP = 6;
        private const int GID_PRESSANDTAP = 7;

        private const int GF_INERTIA = 0x00000002;

        private enum PointerInputType
        {
            Pointer = 0x00000001,
            Touch = 0x00000002,
            Pen = 0x00000003,
            Mouse = 0x00000004,
            TouchPad = 0x00000005,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTUREINFO
        {
            public uint cbSize;
            public uint dwFlags;
            public uint dwID;
            public IntPtr hwndTarget;
            public short ptsLocationX;
            public short ptsLocationY;
            public uint dwInstanceID;
            public uint dwSequenceID;
            public ulong ullArguments;
            public uint cbExtraArgs;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGestureInfo(
            IntPtr hGestureInfo,
            ref GESTUREINFO pGestureInfo
        );

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPointerType(uint pointerId, out PointerInputType pointerType);

        private const int SM_DIGITIZER = 94;
        private const int NID_INTEGRATED_TOUCH = 0x01;
        private const int NID_EXTERNAL_TOUCH = 0x02;

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        private static bool IsTouchScreenPresent()
        {
            int digitizer = GetSystemMetrics(SM_DIGITIZER);
            return (digitizer & (NID_INTEGRATED_TOUCH | NID_EXTERNAL_TOUCH)) != 0;
        }

        // A precision touchpad enumerates as a raw HID device under usage page
        // "Digitizer" (0x0D), usage "Touch Pad" (0x05) — the standard way Windows
        // itself identifies trackpad hardware.
        private const int RIM_TYPEHID = 2;
        private const uint RIDI_DEVICEINFO = 0x2000000B;
        private const ushort HID_USAGE_PAGE_DIGITIZER = 0x0D;
        private const ushort HID_USAGE_DIGITIZER_TOUCHPAD = 0x05;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICELIST
        {
            public IntPtr hDevice;
            public uint dwType;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct RID_DEVICE_INFO
        {
            [FieldOffset(0)]
            public int cbSize;

            [FieldOffset(4)]
            public int dwType;

            [FieldOffset(8)]
            public RID_DEVICE_INFO_HID hid;

            [FieldOffset(8)]
            public RID_DEVICE_INFO_KEYBOARD keyboard; // sizes the union correctly
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RID_DEVICE_INFO_HID
        {
            public int dwVendorId;
            public int dwProductId;
            public int dwVersionNumber;
            public ushort usUsagePage;
            public ushort usUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RID_DEVICE_INFO_KEYBOARD
        {
            public int dwType;
            public int dwSubType;
            public int dwKeyboardMode;
            public int dwNumberOfFunctionKeys;
            public int dwNumberOfIndicators;
            public int dwNumberOfKeysTotal;
        }

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceList(
            IntPtr pRawInputDeviceList,
            ref uint puiNumDevices,
            uint cbSize
        );

        [DllImport("user32.dll")]
        private static extern uint GetRawInputDeviceInfo(
            IntPtr hDevice,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize
        );

        private static bool IsTrackpadPresent() => TryFindTouchpadDevice(out _);

        private static bool TryFindTouchpadDevice(out IntPtr hDevice)
        {
            hDevice = IntPtr.Zero;

            int listItemSize = Marshal.SizeOf<RAWINPUTDEVICELIST>();
            uint deviceCount = 0;
            if (
                GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)listItemSize) != 0
                || deviceCount == 0
            )
            {
                return false;
            }

            IntPtr listBuffer = Marshal.AllocHGlobal((int)deviceCount * listItemSize);
            IntPtr infoBuffer = Marshal.AllocHGlobal(Marshal.SizeOf<RID_DEVICE_INFO>());
            try
            {
                uint fetched = GetRawInputDeviceList(
                    listBuffer,
                    ref deviceCount,
                    (uint)listItemSize
                );
                if (fetched == unchecked((uint)-1))
                {
                    return false;
                }

                int infoSize = Marshal.SizeOf<RID_DEVICE_INFO>();
                for (int i = 0; i < fetched; i++)
                {
                    var device = Marshal.PtrToStructure<RAWINPUTDEVICELIST>(
                        listBuffer + i * listItemSize
                    );
                    if (device.dwType != RIM_TYPEHID)
                    {
                        continue;
                    }

                    uint cb = (uint)infoSize;
                    Marshal.WriteInt32(infoBuffer, 0, infoSize); // RID_DEVICE_INFO.cbSize
                    if (
                        GetRawInputDeviceInfo(device.hDevice, RIDI_DEVICEINFO, infoBuffer, ref cb)
                        == unchecked((uint)-1)
                    )
                    {
                        continue;
                    }

                    var info = Marshal.PtrToStructure<RID_DEVICE_INFO>(infoBuffer);
                    if (
                        info.dwType == RIM_TYPEHID
                        && info.hid.usUsagePage == HID_USAGE_PAGE_DIGITIZER
                        && info.hid.usUsage == HID_USAGE_DIGITIZER_TOUCHPAD
                    )
                    {
                        hDevice = device.hDevice;
                        return true;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(listBuffer);
                Marshal.FreeHGlobal(infoBuffer);
            }

            return false;
        }

        // ---- Raw touchpad-surface tracking (bypasses the OS cursor entirely) ---

        private const int WM_INPUT = 0x00FF;
        private const uint RID_INPUT = 0x10000003;
        private const uint RIDEV_INPUTSINK = 0x00000100;
        private const uint RIDI_PREPARSEDDATA = 0x20000005;

        // X/Y live under the Generic Desktop usage page (0x01), not Digitizer
        // (0x0D), even inside a Digitizer collection — confirmed against a real
        // touchpad's descriptor (see touchpad_debug.log investigation): each
        // finger's contact collection declared Contact Identifier under 0x0D but
        // X/Y under 0x01. This is standard HID practice (X/Y are Generic Desktop
        // usages reused across device types), not a device-specific quirk.
        private const ushort HID_USAGE_PAGE_GENERIC_DESKTOP = 0x01;
        private const ushort HID_USAGE_GENERIC_X = 0x30;
        private const ushort HID_USAGE_GENERIC_Y = 0x31;
        private const ushort HID_USAGE_DIGITIZER_CONTACT_COUNT = 0x54;

        private const int HidP_Input = 0;
        private const int HIDP_STATUS_SUCCESS = 0x00110000;

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RAWINPUTDEVICE
        {
            public ushort usUsagePage;
            public ushort usUsage;
            public uint dwFlags;
            public IntPtr hwndTarget;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public ushort[] Reserved;
            public ushort NumberLinkCollectionNodes;
            public ushort NumberInputButtonCaps;
            public ushort NumberInputValueCaps;
            public ushort NumberInputDataIndices;
            public ushort NumberOutputButtonCaps;
            public ushort NumberOutputValueCaps;
            public ushort NumberOutputDataIndices;
            public ushort NumberFeatureButtonCaps;
            public ushort NumberFeatureValueCaps;
            public ushort NumberFeatureDataIndices;
        }

        // Matches Win32 HIDP_VALUE_CAPS. UsageMin doubles as the plain "Usage" for
        // the (overwhelmingly common) non-range case — same union offset either way.
        [StructLayout(LayoutKind.Sequential)]
        private struct HIDP_VALUE_CAPS
        {
            public ushort UsagePage;
            public byte ReportID;
            public byte IsAlias;
            public ushort BitField;
            public ushort LinkCollection;
            public ushort LinkUsage;
            public ushort LinkUsagePage;
            public byte IsRange;
            public byte IsStringRange;
            public byte IsDesignatorRange;
            public byte IsAbsolute;
            public byte HasNull;
            public byte Reserved;
            public ushort BitSize;
            public ushort ReportCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ushort[] Reserved2;
            public uint UnitsExp;
            public uint Units;
            public int LogicalMin;
            public int LogicalMax;
            public int PhysicalMin;
            public int PhysicalMax;
            public ushort UsageMin;
            public ushort UsageMax;
            public ushort StringMin;
            public ushort StringMax;
            public ushort DesignatorMin;
            public ushort DesignatorMax;
            public ushort DataIndexMin;
            public ushort DataIndexMax;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterRawInputDevices(
            RAWINPUTDEVICE[] pRawInputDevices,
            uint uiNumDevices,
            uint cbSize
        );

        [DllImport("user32.dll")]
        private static extern uint GetRawInputData(
            IntPtr hRawInput,
            uint uiCommand,
            IntPtr pData,
            ref uint pcbSize,
            uint cbSizeHeader
        );

        [DllImport("hid.dll")]
        private static extern int HidP_GetCaps(IntPtr preparsedData, out HIDP_CAPS capabilities);

        [DllImport("hid.dll")]
        private static extern int HidP_GetValueCaps(
            int reportType,
            [Out] HIDP_VALUE_CAPS[] valueCaps,
            ref ushort valueCapsLength,
            IntPtr preparsedData
        );

        [DllImport("hid.dll")]
        private static extern int HidP_GetUsageValue(
            int reportType,
            ushort usagePage,
            ushort linkCollection,
            ushort usage,
            out uint usageValue,
            IntPtr preparsedData,
            byte[] report,
            uint reportLength
        );

        // Releases the previous raw-tracking state (if any) before re-detecting,
        // so RefreshHardware doesn't leak the native preparsed-data allocation
        // when re-running this against a possibly different touchpad.
        private void ResetTouchpadRawTracking()
        {
            if (touchpadPreparsedData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(touchpadPreparsedData);
                touchpadPreparsedData = IntPtr.Zero;
            }
            touchpadDeviceHandle = IntPtr.Zero;
            touchpadRawTrackingReady = false;

            foreach (TouchpadContactSlot slot in touchpadContactSlots)
            {
                trackpadRawSurface.Controls.Remove(slot.Dot);
                slot.Dot.Dispose();
            }
            touchpadContactSlots.Clear();
        }

        // Finds the touchpad's Digitizer HID collection, reads its report
        // descriptor, and locates every finger contact's X/Y value caps plus the
        // packet's contact-count value. If the descriptor doesn't have the
        // expected shape, raw tracking is simply left unavailable.
        private void SetupTouchpadRawTracking()
        {
            if (!TryFindTouchpadDevice(out IntPtr hDevice))
            {
                return;
            }

            uint size = 0;
            GetRawInputDeviceInfo(hDevice, RIDI_PREPARSEDDATA, IntPtr.Zero, ref size);
            if (size == 0)
            {
                return;
            }

            IntPtr preparsed = Marshal.AllocHGlobal((int)size);
            if (
                GetRawInputDeviceInfo(hDevice, RIDI_PREPARSEDDATA, preparsed, ref size)
                == unchecked((uint)-1)
            )
            {
                Marshal.FreeHGlobal(preparsed);
                return;
            }

            bool gotCaps = HidP_GetCaps(preparsed, out HIDP_CAPS caps) == HIDP_STATUS_SUCCESS;
            if (!gotCaps || caps.NumberInputValueCaps == 0)
            {
                Marshal.FreeHGlobal(preparsed);
                return;
            }

            ushort valueCapsLength = caps.NumberInputValueCaps;
            var valueCaps = new HIDP_VALUE_CAPS[valueCapsLength];
            if (
                HidP_GetValueCaps(HidP_Input, valueCaps, ref valueCapsLength, preparsed)
                != HIDP_STATUS_SUCCESS
            )
            {
                Marshal.FreeHGlobal(preparsed);
                return;
            }

            var xByLink = new Dictionary<ushort, HIDP_VALUE_CAPS>();
            var yByLink = new Dictionary<ushort, HIDP_VALUE_CAPS>();
            ushort? contactCountLink = null;

            foreach (HIDP_VALUE_CAPS cap in valueCaps)
            {
                // Single-usage caps (the normal case for per-contact X/Y) store the
                // usage at the same offset a range's UsageMin would occupy.
                if (cap.UsagePage == HID_USAGE_PAGE_GENERIC_DESKTOP)
                {
                    switch (cap.UsageMin)
                    {
                        case HID_USAGE_GENERIC_X:
                            xByLink[cap.LinkCollection] = cap;
                            break;
                        case HID_USAGE_GENERIC_Y:
                            yByLink[cap.LinkCollection] = cap;
                            break;
                    }
                }
                else if (
                    cap.UsagePage == HID_USAGE_PAGE_DIGITIZER
                    && cap.UsageMin == HID_USAGE_DIGITIZER_CONTACT_COUNT
                )
                {
                    contactCountLink ??= cap.LinkCollection;
                }
            }

            List<ushort> contactLinks = xByLink
                .Keys.Where(yByLink.ContainsKey)
                .OrderBy(link => link)
                .ToList();

            if (contactLinks.Count == 0 || contactCountLink == null)
            {
                Marshal.FreeHGlobal(preparsed);
                return;
            }

            touchpadDeviceHandle = hDevice;
            touchpadPreparsedData = preparsed;
            touchpadContactCountLink = contactCountLink.Value;

            // One dot color per slot, reusing the same palette the Touch Screen
            // box uses — capped to however many colors exist, in the (unlikely)
            // event a descriptor declares more contacts than that.
            int slotCount = Math.Min(contactLinks.Count, TouchDotColors.Length);
            for (int i = 0; i < slotCount; i++)
            {
                ushort link = contactLinks[i];
                var dot = new Panel
                {
                    BackColor = TouchDotColors[i],
                    Size = new Size(10, 10),
                    Visible = false,
                };
                trackpadRawSurface.Controls.Add(dot);
                dot.BringToFront();

                touchpadContactSlots.Add(
                    new TouchpadContactSlot
                    {
                        LinkCollection = link,
                        XMin = xByLink[link].LogicalMin,
                        XMax = xByLink[link].LogicalMax,
                        YMin = yByLink[link].LogicalMin,
                        YMax = yByLink[link].LogicalMax,
                        Dot = dot,
                    }
                );
            }

            touchpadRawTrackingReady = true;

            var rid = new RAWINPUTDEVICE
            {
                usUsagePage = HID_USAGE_PAGE_DIGITIZER,
                usUsage = HID_USAGE_DIGITIZER_TOUCHPAD,
                dwFlags = RIDEV_INPUTSINK,
                hwndTarget = Handle,
            };
            RegisterRawInputDevices(new[] { rid }, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT && touchpadRawTrackingReady)
            {
                HandleRawTouchpadInput(m.LParam);
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            ResetTouchpadRawTracking();
            base.OnFormClosed(e);
        }

        private void HandleRawTouchpadInput(IntPtr hRawInput)
        {
            uint headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
            uint size = 0;
            GetRawInputData(hRawInput, RID_INPUT, IntPtr.Zero, ref size, headerSize);
            if (size == 0)
            {
                return;
            }

            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetRawInputData(hRawInput, RID_INPUT, buffer, ref size, headerSize) != size)
                {
                    return;
                }

                var header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);
                if (header.dwType != RIM_TYPEHID || header.hDevice != touchpadDeviceHandle)
                {
                    return;
                }

                int hidHeaderOffset = (int)headerSize;
                int dwSizeHid = Marshal.ReadInt32(buffer, hidHeaderOffset);
                int dwCount = Marshal.ReadInt32(buffer, hidHeaderOffset + 4);
                IntPtr rawDataStart = buffer + hidHeaderOffset + 8;

                if (dwSizeHid <= 0 || dwCount <= 0)
                {
                    return;
                }

                for (int i = 0; i < dwCount; i++)
                {
                    byte[] report = new byte[dwSizeHid];
                    Marshal.Copy(rawDataStart + i * dwSizeHid, report, 0, dwSizeHid);
                    ParseTouchpadReport(report);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void ParseTouchpadReport(byte[] report)
        {
            if (
                HidP_GetUsageValue(
                    HidP_Input,
                    HID_USAGE_PAGE_DIGITIZER,
                    touchpadContactCountLink,
                    HID_USAGE_DIGITIZER_CONTACT_COUNT,
                    out uint contactCount,
                    touchpadPreparsedData,
                    report,
                    (uint)report.Length
                ) != HIDP_STATUS_SUCCESS
            )
            {
                return;
            }

            // Active fingers occupy the first contactCount slots in link-collection
            // order — there's no per-slot Tip Switch check here (see the note by
            // TouchpadContactSlot on why), so this is a reasonable approximation
            // rather than a guarantee for every possible descriptor.
            for (int i = 0; i < touchpadContactSlots.Count; i++)
            {
                TouchpadContactSlot slot = touchpadContactSlots[i];

                if (i >= contactCount)
                {
                    slot.Active = false;
                    continue;
                }

                int xStatus = HidP_GetUsageValue(
                    HidP_Input,
                    HID_USAGE_PAGE_GENERIC_DESKTOP,
                    slot.LinkCollection,
                    HID_USAGE_GENERIC_X,
                    out uint x,
                    touchpadPreparsedData,
                    report,
                    (uint)report.Length
                );
                int yStatus = HidP_GetUsageValue(
                    HidP_Input,
                    HID_USAGE_PAGE_GENERIC_DESKTOP,
                    slot.LinkCollection,
                    HID_USAGE_GENERIC_Y,
                    out uint y,
                    touchpadPreparsedData,
                    report,
                    (uint)report.Length
                );

                slot.Active = xStatus == HIDP_STATUS_SUCCESS && yStatus == HIDP_STATUS_SUCCESS;
                if (slot.Active)
                {
                    slot.RawPoint = new Point((int)x, (int)y);
                }
            }

            UpdateTouchpadSurfaceDisplay();
        }

        private void UpdateTouchpadSurfaceDisplay()
        {
            int activeCount = 0;
            Point firstActivePoint = default;

            foreach (TouchpadContactSlot slot in touchpadContactSlots)
            {
                slot.Dot.Visible = slot.Active;
                if (!slot.Active)
                {
                    continue;
                }

                activeCount++;
                if (activeCount == 1)
                {
                    firstActivePoint = slot.RawPoint;
                }

                slot.Dot.Location = ScaleToBox(
                    slot.RawPoint,
                    Rectangle.FromLTRB(slot.XMin, slot.YMin, slot.XMax, slot.YMax),
                    trackpadRawSurface.ClientSize,
                    slot.Dot.Size
                );
            }

            trackpadRawCoordLabel.Text = activeCount switch
            {
                0 => "Not touching",
                1 => $"X: {firstActivePoint.X}, Y: {firstActivePoint.Y}",
                _ => $"{activeCount} touching",
            };
        }

        // ---- Message pump hook -------------------------------------------------

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                    OnMouseButtonDown(MouseButtons.Left);
                    break;
                case WM_LBUTTONUP:
                    OnMouseButtonUp(MouseButtons.Left);
                    break;
                case WM_RBUTTONDOWN:
                    OnMouseButtonDown(MouseButtons.Right);
                    break;
                case WM_RBUTTONUP:
                    OnMouseButtonUp(MouseButtons.Right);
                    break;
                case WM_MBUTTONDOWN:
                    OnMouseButtonDown(MouseButtons.Middle);
                    break;
                case WM_MBUTTONUP:
                    OnMouseButtonUp(MouseButtons.Middle);
                    break;
                case WM_XBUTTONDOWN:
                    OnMouseButtonDown(XButtonFromWParam(m.WParam));
                    break;
                case WM_XBUTTONUP:
                    OnMouseButtonUp(XButtonFromWParam(m.WParam));
                    break;

                case WM_MOUSEWHEEL:
                case WM_MOUSEHWHEEL:
                {
                    int delta = (short)((long)m.WParam >> 16 & 0xFFFF);
                    bool vertical = m.Msg == WM_MOUSEWHEEL;
                    ShowNotice(
                        vertical
                            ? (delta > 0 ? "Mouse Wheel Up" : "Mouse Wheel Down")
                            : (delta > 0 ? "Mouse Wheel Right" : "Mouse Wheel Left")
                    );
                    scrollIdleLabel = vertical ? "Mouse Wheel" : "Mouse Wheel (horizontal)";
                    scrollIdleTimer.Stop();
                    scrollIdleTimer.Start();
                    if (vertical)
                    {
                        FlashVirtualScroll(delta > 0);
                    }
                    break;
                }

                case WM_POINTERDOWN:
                {
                    uint id = (uint)((long)m.WParam & 0xFFFF);
                    string label = PointerLabel(id);
                    activePointers[id] = label;
                    if (label == "Touch")
                    {
                        TrackTouchPoint(id, GetPointerScreenPosition(m.LParam));
                    }
                    UpdateDisplay();
                    break;
                }

                case WM_POINTERUPDATE:
                {
                    uint id = (uint)((long)m.WParam & 0xFFFF);
                    if (touchDots.ContainsKey(id))
                    {
                        TrackTouchPoint(id, GetPointerScreenPosition(m.LParam));
                    }
                    break;
                }

                case WM_POINTERUP:
                case WM_POINTERLEAVE:
                {
                    uint id = (uint)((long)m.WParam & 0xFFFF);
                    if (activePointers.TryGetValue(id, out string? label))
                    {
                        activePointers.Remove(id);
                        // Report "stopped" once the last contact of this kind lifts.
                        if (!activePointers.ContainsValue(label))
                        {
                            AddRelease(label + " stopped");
                        }
                        UpdateDisplay();
                    }
                    RemoveTouchPoint(id);
                    break;
                }

                case WM_GESTURE:
                {
                    var info = new GESTUREINFO { cbSize = (uint)Marshal.SizeOf<GESTUREINFO>() };
                    if (GetGestureInfo(m.LParam, ref info))
                    {
                        string? name = GestureName(info.dwID);
                        if (name != null)
                        {
                            if ((info.dwFlags & GF_INERTIA) != 0)
                            {
                                name += " (inertia)";
                            }
                            ShowNotice(name);
                        }
                    }
                    // Fall through to DefWindowProc, which closes the gesture handle.
                    break;
                }
            }

            // Never consume the message.
            return false;
        }

        // Pointer messages carry screen coordinates packed into lParam, same layout
        // as mouse messages (low word = x, high word = y).
        private static Point GetPointerScreenPosition(IntPtr lParam)
        {
            int x = unchecked((short)((long)lParam & 0xFFFF));
            int y = unchecked((short)((long)lParam >> 16 & 0xFFFF));
            return new Point(x, y);
        }

        private static string PointerLabel(uint id)
        {
            if (GetPointerType(id, out PointerInputType type))
            {
                return type switch
                {
                    PointerInputType.Touch => "Touch",
                    PointerInputType.Pen => "Pen",
                    PointerInputType.TouchPad => "Touchpad",
                    PointerInputType.Mouse => "Mouse",
                    _ => "Pointer",
                };
            }
            return "Touch";
        }

        private static string? GestureName(uint id) =>
            id switch
            {
                GID_ZOOM => "Gesture: Pinch Zoom",
                GID_PAN => "Gesture: Pan",
                GID_ROTATE => "Gesture: Rotate",
                GID_TWOFINGERTAP => "Gesture: Two-Finger Tap",
                GID_PRESSANDTAP => "Gesture: Press & Tap",
                _ => null,
            };

        private void ShowNotice(string text)
        {
            transientNotice = text;
            noticeTimer.Stop();
            noticeTimer.Start();
            UpdateDisplay();
        }

        // Add / refresh an entry on the second "released" line and (re)start its window.
        private void AddRelease(string label)
        {
            releasedItems.Remove(label);
            releasedItems.Add(label);

            releaseShownAt = DateTime.UtcNow;
            releaseLabel.ForeColor = releaseBaseColor;
            releaseTimer.Stop();
            releaseTimer.Start();
            fadeTimer.Start();

            UpdateDisplay();
        }

        // Fades releaseLabel's text color from full strength down to
        // IdleFadeMinOpacity (not all the way to invisible) as the release
        // window elapses; the item is removed outright once the window ends.
        private void UpdateReleaseFade()
        {
            double fraction =
                (DateTime.UtcNow - releaseShownAt).TotalMilliseconds / releaseTimer.Interval;
            fraction = Clamp(fraction, 0.0, 1.0);

            double blend = fraction * (1.0 - IdleFadeMinOpacity);
            releaseLabel.ForeColor = Lerp(releaseBaseColor, BackColor, blend);

            if (fraction >= 1.0)
            {
                fadeTimer.Stop();
            }
        }

        // Fades displayLabel's "(nothing pressed)" text from full opacity down to
        // IdleFadeMinOpacity over IdleFadeDurationMs, then holds there.
        private void UpdateIdleFade()
        {
            double fraction = (DateTime.UtcNow - idleSince).TotalMilliseconds / IdleFadeDurationMs;
            fraction = Clamp(fraction, 0.0, 1.0);

            // fraction 0 -> full opacity (blend 0, i.e. displayBaseColor itself);
            // fraction 1 -> IdleFadeMinOpacity (blend (1 - min) toward the background).
            double blend = fraction * (1.0 - IdleFadeMinOpacity);
            displayLabel.ForeColor = Lerp(displayBaseColor, BackColor, blend);

            if (fraction >= 1.0)
            {
                idleFadeTimer.Stop();
            }
        }

        private static Color Lerp(Color from, Color to, double t) =>
            Color.FromArgb(
                (int)(from.R + (to.R - from.R) * t),
                (int)(from.G + (to.G - from.G) * t),
                (int)(from.B + (to.B - from.B) * t)
            );

        // net48's Math class predates Math.Clamp (added in .NET Core 2.0).
        private static double Clamp(double value, double min, double max) =>
            value < min ? min
            : value > max ? max
            : value;

        private static int Clamp(int value, int min, int max) =>
            value < min ? min
            : value > max ? max
            : value;

        // ---- Keyboard / mouse-button events ----------------------------------

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!heldKeys.Contains(e.KeyCode))
            {
                heldKeys.Add(e.KeyCode);
                UpdateDisplay();
            }
            HighlightVirtualKey(e.KeyCode, true);

            // Don't let the key also type into the display.
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            heldKeys.Remove(e.KeyCode);
            HighlightVirtualKey(e.KeyCode, false);
            AddRelease(e.KeyCode.ToString());
        }

        // Driven by the raw WM_*BUTTONDOWN/UP messages in PreFilterMessage rather
        // than WinForms MouseDown/MouseUp events, so a click lands here no matter
        // which child control (a position box, a keyboard key, the mouse cluster)
        // it was actually targeted at.
        private void OnMouseButtonDown(MouseButtons button)
        {
            if (!heldButtons.Contains(button))
            {
                heldButtons.Add(button);
                HighlightVirtualMouseButton(button, true);
                UpdateDisplay();
            }
        }

        private void OnMouseButtonUp(MouseButtons button)
        {
            if (heldButtons.Remove(button))
            {
                HighlightVirtualMouseButton(button, false);
                AddRelease(MouseButtonName(button));
            }
        }

        // HIWORD(wParam) is 1 for XBUTTON1, 2 for XBUTTON2 on WM_XBUTTONDOWN/UP.
        private static MouseButtons XButtonFromWParam(IntPtr wParam) =>
            ((long)wParam >> 16 & 0xFFFF) == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2;

        // Maps a point onto a box acting as a miniature of some coordinate space
        // (the whole screen, or a touchpad's raw sensor grid): the dot's position
        // within the box is proportional to where the point sits within bounds, so
        // the bottom-right of that space lands at the bottom-right of the box,
        // regardless of where the box itself is.
        private static Point ScaleToBox(
            Point point,
            Rectangle bounds,
            Size clientSize,
            Size dotSize
        )
        {
            double fx = bounds.Width == 0 ? 0 : (double)(point.X - bounds.Left) / bounds.Width;
            double fy = bounds.Height == 0 ? 0 : (double)(point.Y - bounds.Top) / bounds.Height;

            int x = (int)Math.Round(fx * (clientSize.Width - dotSize.Width));
            int y = (int)Math.Round(fy * (clientSize.Height - dotSize.Height));

            return new Point(
                Clamp(x, 0, clientSize.Width - dotSize.Width),
                Clamp(y, 0, clientSize.Height - dotSize.Height)
            );
        }

        // boxesPanel auto-sizes to fit only its currently-visible children (a
        // FlowLayoutPanel excludes hidden ones from both size and layout), so
        // re-centering it here after every such change keeps the whole row
        // centered under the menu regardless of how many boxes are shown or how
        // wide the window is.
        private void CenterBoxesPanel()
        {
            // Cap how wide the row is allowed to grow so a narrow window wraps
            // it onto additional rows instead of overflowing past the edge.
            boxesPanel.MaximumSize = new Size(Math.Max(1, ClientSize.Width), 0);
            boxesPanel.Left = Math.Max(0, (ClientSize.Width - boxesPanel.Width) / 2);
        }

        // ---- Virtual keyboard ---------------------------------------------------

        // One keyboard-unit's pixel size (a standard 1u key). Row widths below are
        // expressed as multiples of this, matching real keyboard key proportions
        // (e.g. Tab = 1.5u, Enter = 2.25u, Space = 6.25u) so the layout reads as an
        // actual keyboard rather than an arbitrary grid.
        private const int KeyUnit = 26;
        private const int KeyGap = 3;

        // Builds the on-screen keyboard from data (function row through the bottom
        // row, plus an arrow cluster) and drops it into a Dock.Bottom panel sitting
        // directly above releaseLabel — since releaseLabel is added to the form
        // earlier (in the Designer), adding keyboardPanel afterward here means it
        // claims its Dock.Bottom slice from whatever space releaseLabel left,
        // landing it just above releaseLabel rather than competing for the same
        // strip. displayLabel (Dock.Fill) ends up occupying whatever's left above
        // both, which is exactly "between" the two text lines.
        private void BuildVirtualKeyboard()
        {
            keyboardGrid = new Panel { BackColor = Color.Transparent };

            int rowHeight = KeyUnit - KeyGap;
            int y = 0;

            AddKeyRow(
                y,
                rowHeight,
                ("Esc", 1f, Keys.Escape),
                ("", 0.5f, Keys.None),
                ("F1", 1f, Keys.F1),
                ("F2", 1f, Keys.F2),
                ("F3", 1f, Keys.F3),
                ("F4", 1f, Keys.F4),
                ("", 0.5f, Keys.None),
                ("F5", 1f, Keys.F5),
                ("F6", 1f, Keys.F6),
                ("F7", 1f, Keys.F7),
                ("F8", 1f, Keys.F8),
                ("", 0.5f, Keys.None),
                ("F9", 1f, Keys.F9),
                ("F10", 1f, Keys.F10),
                ("F11", 1f, Keys.F11),
                ("F12", 1f, Keys.F12)
            );
            y += KeyUnit;

            AddKeyRow(
                y,
                rowHeight,
                ("`", 1f, Keys.Oemtilde),
                ("1", 1f, Keys.D1),
                ("2", 1f, Keys.D2),
                ("3", 1f, Keys.D3),
                ("4", 1f, Keys.D4),
                ("5", 1f, Keys.D5),
                ("6", 1f, Keys.D6),
                ("7", 1f, Keys.D7),
                ("8", 1f, Keys.D8),
                ("9", 1f, Keys.D9),
                ("0", 1f, Keys.D0),
                ("-", 1f, Keys.OemMinus),
                ("=", 1f, Keys.Oemplus),
                ("Back", 2f, Keys.Back)
            );
            y += KeyUnit;

            AddKeyRow(
                y,
                rowHeight,
                ("Tab", 1.5f, Keys.Tab),
                ("Q", 1f, Keys.Q),
                ("W", 1f, Keys.W),
                ("E", 1f, Keys.E),
                ("R", 1f, Keys.R),
                ("T", 1f, Keys.T),
                ("Y", 1f, Keys.Y),
                ("U", 1f, Keys.U),
                ("I", 1f, Keys.I),
                ("O", 1f, Keys.O),
                ("P", 1f, Keys.P),
                ("[", 1f, Keys.OemOpenBrackets),
                ("]", 1f, Keys.OemCloseBrackets),
                ("\\", 1.5f, Keys.OemPipe)
            );
            y += KeyUnit;

            AddKeyRow(
                y,
                rowHeight,
                ("Caps", 1.75f, Keys.CapsLock),
                ("A", 1f, Keys.A),
                ("S", 1f, Keys.S),
                ("D", 1f, Keys.D),
                ("F", 1f, Keys.F),
                ("G", 1f, Keys.G),
                ("H", 1f, Keys.H),
                ("J", 1f, Keys.J),
                ("K", 1f, Keys.K),
                ("L", 1f, Keys.L),
                (";", 1f, Keys.OemSemicolon),
                ("'", 1f, Keys.OemQuotes),
                ("Enter", 2.25f, Keys.Enter)
            );
            y += KeyUnit;

            int shiftRowY = y;
            AddKeyRow(
                y,
                rowHeight,
                ("Shift", 2.25f, Keys.ShiftKey),
                ("Z", 1f, Keys.Z),
                ("X", 1f, Keys.X),
                ("C", 1f, Keys.C),
                ("V", 1f, Keys.V),
                ("B", 1f, Keys.B),
                ("N", 1f, Keys.N),
                ("M", 1f, Keys.M),
                (",", 1f, Keys.Oemcomma),
                (".", 1f, Keys.OemPeriod),
                ("/", 1f, Keys.OemQuestion),
                ("Shift", 2.75f, Keys.ShiftKey)
            );
            y += KeyUnit;

            int bottomRowY = y;
            AddKeyRow(
                y,
                rowHeight,
                ("Ctrl", 1.25f, Keys.ControlKey),
                ("Win", 1.25f, Keys.LWin),
                ("Alt", 1.25f, Keys.Menu),
                ("Space", 6.25f, Keys.Space),
                ("Alt", 1.25f, Keys.Menu),
                ("Win", 1.25f, Keys.RWin),
                ("Menu", 1.25f, Keys.Apps),
                ("Ctrl", 1.25f, Keys.ControlKey)
            );
            y += KeyUnit;

            // Arrow cluster to the right of the main block, aligned with the
            // bottom two rows (the standard inverted-T arrangement).
            const float mainBlockWidth = 15f;
            const float arrowGap = 0.5f;
            int arrowX = (int)Math.Round((mainBlockWidth + arrowGap) * KeyUnit);
            AddKeyRow(
                arrowX,
                shiftRowY,
                rowHeight,
                ("", 1f, Keys.None),
                ("↑", 1f, Keys.Up),
                ("", 1f, Keys.None)
            );
            AddKeyRow(
                arrowX,
                bottomRowY,
                rowHeight,
                ("←", 1f, Keys.Left),
                ("↓", 1f, Keys.Down),
                ("→", 1f, Keys.Right)
            );

            // Mouse cluster (buttons + scroll wheel), just above the arrow
            // cluster on the right side — same column span as the arrows
            // below, so the two clusters read as one grouped block.
            BuildMouseCluster(arrowX, shiftRowY);

            keyboardGrid.Size = new Size(arrowX + 3 * KeyUnit, y);

            keyboardPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = keyboardGrid.Height + 14,
                BackColor = Color.Transparent,
            };
            keyboardPanel.Controls.Add(keyboardGrid);
            Controls.Add(keyboardPanel);
        }

        // Lays out one row of keys starting at the left edge (x = 0).
        private void AddKeyRow(
            int y,
            int rowHeight,
            params (string Text, float Width, Keys Key)[] keys
        ) => AddKeyRow(0, y, rowHeight, keys);

        // Lays out one row of keys starting at an arbitrary x (used for the arrow
        // cluster, which starts to the right of the main block).
        private void AddKeyRow(
            int startX,
            int y,
            int rowHeight,
            params (string Text, float Width, Keys Key)[] keys
        )
        {
            int x = startX;
            foreach ((string text, float width, Keys key) in keys)
            {
                int pixelWidth = (int)Math.Round(width * KeyUnit);
                if (key != Keys.None)
                {
                    var label = new Label
                    {
                        Text = text,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = KeyboardKeyBackColor,
                        ForeColor = KeyboardKeyForeColor,
                        Font = new Font("Segoe UI", 7.5F),
                        Location = new Point(x, y),
                        Size = new Size(pixelWidth - KeyGap, rowHeight),
                    };
                    keyboardGrid.Controls.Add(label);

                    if (!keyboardKeyLabels.TryGetValue(key, out List<Label>? labels))
                    {
                        labels = new List<Label>();
                        keyboardKeyLabels[key] = labels;
                    }
                    labels.Add(label);
                }
                x += pixelWidth;
            }
        }

        // Highlights (or un-highlights) every visual keycap mapped to a given
        // Keys value — more than one keycap can share a value (e.g. both Shift
        // keys), since WinForms itself can't tell left and right apart for
        // Shift/Ctrl/Alt (KeyEventArgs.KeyCode reports the generic ShiftKey /
        // ControlKey / Menu regardless of which physical key was pressed).
        private void HighlightVirtualKey(Keys key, bool pressed)
        {
            if (!keyboardKeyLabels.TryGetValue(key, out List<Label>? labels))
            {
                return;
            }

            foreach (Label label in labels)
            {
                label.BackColor = pressed ? Color.Red : KeyboardKeyBackColor;
                label.ForeColor = pressed ? Color.White : KeyboardKeyForeColor;
            }
        }

        // Builds a mouse-shaped cluster — left/middle/right buttons flanking a
        // center scroll-wheel strip (scroll-up indicator over the middle
        // button over scroll-down) — inside a bordered "body" panel so the
        // group reads as one device rather than three loose keys. Sized to
        // the same 3-unit column width as the arrow cluster and sat directly
        // on top of it (bottomY is the arrow cluster's own top edge).
        private void BuildMouseCluster(int x, int bottomY)
        {
            const int rows = 3;
            int height = rows * KeyUnit;
            int top = bottomY - height;
            int cellSize = KeyUnit - KeyGap;

            var body = new Panel
            {
                Location = new Point(x, top),
                Size = new Size(3 * KeyUnit - KeyGap, height - KeyGap),
                BackColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
            };
            keyboardGrid.Controls.Add(body);

            Label leftButton = CreateMouseCellLabel(
                "L",
                new Point(0, 0),
                new Size(cellSize, height - KeyGap)
            );
            body.Controls.Add(leftButton);
            RegisterMouseButtonLabel(MouseButtons.Left, leftButton);

            Label rightButton = CreateMouseCellLabel(
                "R",
                new Point(2 * KeyUnit, 0),
                new Size(cellSize, height - KeyGap)
            );
            body.Controls.Add(rightButton);
            RegisterMouseButtonLabel(MouseButtons.Right, rightButton);

            mouseScrollUpLabel = CreateMouseCellLabel(
                "▲",
                new Point(KeyUnit, 0),
                new Size(cellSize, cellSize)
            );
            body.Controls.Add(mouseScrollUpLabel);

            Label middleButton = CreateMouseCellLabel(
                "M",
                new Point(KeyUnit, KeyUnit),
                new Size(cellSize, cellSize)
            );
            body.Controls.Add(middleButton);
            RegisterMouseButtonLabel(MouseButtons.Middle, middleButton);

            mouseScrollDownLabel = CreateMouseCellLabel(
                "▼",
                new Point(KeyUnit, 2 * KeyUnit),
                new Size(cellSize, cellSize)
            );
            body.Controls.Add(mouseScrollDownLabel);
        }

        private static Label CreateMouseCellLabel(string text, Point location, Size size) =>
            new()
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = KeyboardKeyBackColor,
                ForeColor = KeyboardKeyForeColor,
                Font = new Font("Segoe UI", 7.5F),
                Location = location,
                Size = size,
            };

        private void RegisterMouseButtonLabel(MouseButtons button, Label label)
        {
            if (!keyboardMouseButtonLabels.TryGetValue(button, out List<Label>? labels))
            {
                labels = new List<Label>();
                keyboardMouseButtonLabels[button] = labels;
            }
            labels.Add(label);
        }

        // Highlights (or un-highlights) every visual button-cap mapped to a
        // given MouseButtons value.
        private void HighlightVirtualMouseButton(MouseButtons button, bool pressed)
        {
            if (!keyboardMouseButtonLabels.TryGetValue(button, out List<Label>? labels))
            {
                return;
            }

            foreach (Label label in labels)
            {
                label.BackColor = pressed ? Color.Red : KeyboardKeyBackColor;
                label.ForeColor = pressed ? Color.White : KeyboardKeyForeColor;
            }
        }

        // Wheel ticks are discrete (no down/up pair), so the scroll-direction
        // indicator briefly flashes red instead of tracking a held state —
        // scrollFlashTimer clears it shortly after the last tick.
        private void FlashVirtualScroll(bool up)
        {
            SetScrollIndicatorActive(up ? mouseScrollUpLabel : mouseScrollDownLabel, true);
            SetScrollIndicatorActive(up ? mouseScrollDownLabel : mouseScrollUpLabel, false);
            scrollFlashTimer.Stop();
            scrollFlashTimer.Start();
        }

        private static void SetScrollIndicatorActive(Label label, bool active)
        {
            label.BackColor = active ? Color.Red : KeyboardKeyBackColor;
            label.ForeColor = active ? Color.White : KeyboardKeyForeColor;
        }

        // Keeps the keyboard centered horizontally (and vertically within its own
        // panel) as the window is resized — the grid's own size is fixed, but
        // keyboardPanel stretches to the full window width via Dock.Bottom.
        private void CenterKeyboardGrid()
        {
            keyboardGrid.Left = Math.Max(
                0,
                (keyboardPanel.ClientSize.Width - keyboardGrid.Width) / 2
            );
            keyboardGrid.Top = Math.Max(
                0,
                (keyboardPanel.ClientSize.Height - keyboardGrid.Height) / 2
            );
        }

        // Reports the OS cursor's position relative to whichever monitor the app
        // window is currently on (not the whole multi-monitor virtual desktop),
        // and plots it, scaled, inside the box. Queried fresh every call so
        // dragging the window to a different monitor picks up immediately. Used
        // by the Trackpad box — a trackpad moves the OS cursor exactly like a
        // mouse does, and Windows doesn't expose which device last moved it.
        private void UpdatePointerBox(Panel surface, Label coordText, Panel dot)
        {
            Point screenPoint = Cursor.Position;
            Rectangle monitorBounds = Screen.FromControl(this).Bounds;
            Point relative = new(
                screenPoint.X - monitorBounds.Left,
                screenPoint.Y - monitorBounds.Top
            );
            coordText.Text = $"X: {relative.X}, Y: {relative.Y}";

            dot.Visible = true;
            dot.Location = ScaleToBox(screenPoint, monitorBounds, surface.ClientSize, dot.Size);
        }

        // ---- Hardware / monitor detection ---------------------------------------

        // File > Refresh: re-runs the same startup detection against whatever is
        // plugged in right now. Each box's current checkbox state is preserved
        // as the "preferred" value if that hardware was already available (so a
        // deliberate uncheck sticks), but hardware that was unavailable before
        // gets the same "default to on" treatment a first-ever launch would give
        // it, rather than staying forced off just because it used to be absent.
        private void RefreshHardware()
        {
            ApplyHardwareDetection(
                preferredShowTouchScreen: touchScreenAvailable
                    ? viewTouchScreenMenuItem.Checked
                    : true,
                preferredShowTrackpad: trackpadAvailable ? viewTrackpadMenuItem.Checked : true,
                preferredShowTrackpadRaw: trackpadAvailable ? viewTrackpadRawMenuItem.Checked : true
            );
        }

        // View > Reset: restores the window to its default size (un-maximizing
        // first if needed) and re-detects hardware, turning every box on if its
        // device is currently connected — the same "on if available" default a
        // brand-new install would show, discarding whatever was checked before.
        private void ResetToDefaults()
        {
            WindowState = FormWindowState.Normal;
            Size = defaultWindowSize;

            viewKeyPressTextMenuItem.Checked = true;
            displayLabel.Visible = true;
            viewKeyReleaseTextMenuItem.Checked = true;
            releaseLabel.Visible = true;

            viewCursorMenuItem.Checked = true;
            ApplyHardwareDetection(
                preferredShowTouchScreen: true,
                preferredShowTrackpad: true,
                preferredShowTrackpadRaw: true
            );
        }

        // Re-detects touch screen, trackpad (+ its raw pad-surface tracking), and
        // connected monitors; applies the greyed-out/enabled visuals and updates
        // each box's View-menu checked state and visibility. preferredShowX is
        // ANDed with that hardware's freshly-detected availability — hardware
        // that's gone is always forced off no matter what was preferred.
        private void ApplyHardwareDetection(
            bool preferredShowTouchScreen,
            bool preferredShowTrackpad,
            bool preferredShowTrackpadRaw
        )
        {
            touchScreenAvailable = IsTouchScreenPresent();
            touchSurface.Enabled = touchScreenAvailable;
            touchSurface.BackColor = touchScreenAvailable
                ? SystemColors.Window
                : SystemColors.Control;
            // touchCoordLabel is only otherwise updated by real touch events (no
            // poll loop backs it like Cursor/Trackpad), so refresh it explicitly
            // here — unconditionally, since it needs to change either direction
            // (e.g. "No touch screen detected" -> "No touch" once one appears).
            ClearAllTouches();

            trackpadAvailable = IsTrackpadPresent();
            trackpadSurface.Enabled = trackpadAvailable;
            trackpadSurface.BackColor = trackpadAvailable
                ? SystemColors.Window
                : SystemColors.Control;
            if (!trackpadAvailable)
            {
                trackpadCoordLabel.Text = "No trackpad detected";
            }

            ResetTouchpadRawTracking();
            if (trackpadAvailable)
            {
                SetupTouchpadRawTracking();
            }
            trackpadRawSurface.Enabled = touchpadRawTrackingReady;
            trackpadRawSurface.BackColor = touchpadRawTrackingReady
                ? SystemColors.Window
                : SystemColors.Control;
            if (touchpadRawTrackingReady)
            {
                UpdateTouchpadSurfaceDisplay();
            }
            else
            {
                trackpadRawCoordLabel.Text = trackpadAvailable
                    ? "Raw tracking unavailable"
                    : "No trackpad detected";
            }

            RebuildCursorMonitorBoxes();
            foreach (CursorMonitorBox box in cursorMonitorBoxes)
            {
                box.Surface.Visible = viewCursorMenuItem.Checked;
            }

            bool showTrackpad = preferredShowTrackpad && trackpadAvailable;
            viewTrackpadMenuItem.Checked = showTrackpad;
            trackpadSurface.Visible = showTrackpad;

            bool showTrackpadRaw = preferredShowTrackpadRaw && trackpadAvailable;
            viewTrackpadRawMenuItem.Checked = showTrackpadRaw;
            trackpadRawSurface.Visible = showTrackpadRaw;

            bool showTouchScreen = preferredShowTouchScreen && touchScreenAvailable;
            viewTouchScreenMenuItem.Checked = showTouchScreen;
            touchSurface.Visible = showTouchScreen;

            CenterBoxesPanel();
        }

        // ---- Cursor boxes, one per monitor --------------------------------------

        // Disposes the current per-monitor Cursor boxes (if any) and rebuilds
        // them from a fresh Screen.AllScreens read. Used both at startup and by
        // File > Refresh, so a monitor plugged/unplugged since launch is picked
        // up without restarting the app.
        private void RebuildCursorMonitorBoxes()
        {
            foreach (CursorMonitorBox box in cursorMonitorBoxes)
            {
                boxesPanel.Controls.Remove(box.Surface);
                box.Surface.Dispose(); // also disposes its title/coord/dot children
            }
            cursorMonitorBoxes.Clear();

            BuildCursorMonitorBoxes();
        }

        // Which kind of device a position box represents — drives the small
        // corner glyph in DrawBoxIcon so the boxes are distinguishable at a
        // glance without reading their title text.
        private enum BoxIconKind
        {
            Monitor,
            Trackpad,
            TouchScreen,
        }

        // Drops a small device-type glyph in a box's bottom-left corner.
        // Surfaces here are all fixed-size (never resized after creation), so
        // a one-time Location computed from the current ClientSize is enough
        // — no Anchor needed.
        private static void AddBoxIcon(Panel surface, BoxIconKind kind)
        {
            const int iconWidth = 18;
            const int iconHeight = 14;
            const int margin = 6;

            var icon = new Panel
            {
                Size = new Size(iconWidth, iconHeight),
                Location = new Point(margin, surface.ClientSize.Height - iconHeight - margin),
                BackColor = Color.Transparent,
            };
            icon.Paint += (s, e) => DrawBoxIcon(e.Graphics, kind, icon.ClientRectangle);
            surface.Controls.Add(icon);
        }

        // Draws a tiny monochrome glyph for each device kind: a monitor is a
        // screen on a stand, a trackpad is a flat pad with a click-button
        // divider near the bottom, and a touch screen is a screen with a
        // fingertip touch ring in the middle.
        private static void DrawBoxIcon(Graphics g, BoxIconKind kind, Rectangle bounds)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var pen = new Pen(SystemColors.GrayText, 1.25f);

            switch (kind)
            {
                case BoxIconKind.Monitor:
                {
                    var screenRect = new Rectangle(
                        bounds.Left,
                        bounds.Top,
                        bounds.Width - 1,
                        bounds.Height - 5
                    );
                    g.DrawRectangle(pen, screenRect);
                    int midX = bounds.Left + bounds.Width / 2;
                    g.DrawLine(pen, midX, screenRect.Bottom, midX, bounds.Bottom - 1);
                    g.DrawLine(pen, midX - 4, bounds.Bottom - 1, midX + 4, bounds.Bottom - 1);
                    break;
                }

                case BoxIconKind.Trackpad:
                {
                    var padRect = new Rectangle(
                        bounds.Left,
                        bounds.Top,
                        bounds.Width - 1,
                        bounds.Height - 1
                    );
                    g.DrawRectangle(pen, padRect);
                    int dividerY = bounds.Bottom - 4;
                    g.DrawLine(pen, bounds.Left + 2, dividerY, bounds.Right - 3, dividerY);
                    break;
                }

                case BoxIconKind.TouchScreen:
                {
                    var screenRect = new Rectangle(
                        bounds.Left,
                        bounds.Top,
                        bounds.Width - 1,
                        bounds.Height - 1
                    );
                    g.DrawRectangle(pen, screenRect);
                    Point center = new(
                        bounds.Left + bounds.Width / 2,
                        bounds.Top + bounds.Height / 2
                    );
                    const int ringRadius = 3;
                    g.DrawEllipse(
                        pen,
                        center.X - ringRadius,
                        center.Y - ringRadius,
                        ringRadius * 2,
                        ringRadius * 2
                    );
                    using var dotBrush = new SolidBrush(SystemColors.GrayText);
                    g.FillEllipse(dotBrush, center.X - 1, center.Y - 1, 2, 2);
                    break;
                }
            }
        }

        // Builds one "Cursor" box per connected display (Screen.AllScreens),
        // ordered left-to-right by physical position, and inserts them ahead of
        // the other boxes so Cursor stays first in the row.
        private void BuildCursorMonitorBoxes()
        {
            Screen[] screens = Screen
                .AllScreens.OrderBy(s => s.Bounds.Left)
                .ThenBy(s => s.Bounds.Top)
                .ToArray();

            for (int i = 0; i < screens.Length; i++)
            {
                Screen screen = screens[i];
                string title = screen.Primary
                    ? $"Cursor - Monitor {i + 1} (Primary)"
                    : $"Cursor - Monitor {i + 1}";

                var titleLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = SystemColors.GrayText,
                    Height = 22,
                    Text = title,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                var coordLabel = new Label
                {
                    Dock = DockStyle.Top,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Height = 28,
                    Text = "Not active",
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                var dot = new Panel
                {
                    BackColor = Color.RoyalBlue,
                    Size = new Size(8, 8),
                    Visible = false,
                };
                var surface = new Panel
                {
                    BackColor = SystemColors.Window,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(10, 0, 10, 0),
                    Size = new Size(240, 120),
                };
                surface.Controls.Add(dot);
                surface.Controls.Add(coordLabel);
                surface.Controls.Add(titleLabel);
                AddBoxIcon(surface, BoxIconKind.Monitor);

                boxesPanel.Controls.Add(surface);
                boxesPanel.Controls.SetChildIndex(surface, i);

                cursorMonitorBoxes.Add(
                    new CursorMonitorBox
                    {
                        Screen = screen,
                        Surface = surface,
                        CoordLabel = coordLabel,
                        Dot = dot,
                    }
                );
            }
        }

        // Shows a live, monitor-relative position only in the box for whichever
        // monitor the cursor is actually on; every other monitor's box reports
        // "Not active" since the cursor can only be on one screen at a time.
        private void UpdateCursorMonitorBoxes()
        {
            Point screenPoint = Cursor.Position;

            foreach (CursorMonitorBox box in cursorMonitorBoxes)
            {
                if (box.Screen.Bounds.Contains(screenPoint))
                {
                    Point relative = new(
                        screenPoint.X - box.Screen.Bounds.Left,
                        screenPoint.Y - box.Screen.Bounds.Top
                    );
                    box.CoordLabel.Text = $"X: {relative.X}, Y: {relative.Y}";
                    box.Dot.Visible = true;
                    box.Dot.Location = ScaleToBox(
                        screenPoint,
                        box.Screen.Bounds,
                        box.Surface.ClientSize,
                        box.Dot.Size
                    );
                }
                else
                {
                    box.CoordLabel.Text = "Not active";
                    box.Dot.Visible = false;
                }
            }
        }

        // ---- Touch-screen positions --------------------------------------------

        private void TrackTouchPoint(uint id, Point screenPoint)
        {
            touchScreenPoints[id] = screenPoint;

            if (!touchDots.TryGetValue(id, out Panel? dot))
            {
                dot = new Panel
                {
                    Size = new Size(12, 12),
                    BackColor = TouchDotColors[touchDots.Count % TouchDotColors.Length],
                };
                touchDots[id] = dot;
                touchSurface.Controls.Add(dot);
                dot.BringToFront();
            }

            UpdateTouchDisplay();
        }

        private void RemoveTouchPoint(uint id)
        {
            touchScreenPoints.Remove(id);
            if (touchDots.TryGetValue(id, out Panel? dot))
            {
                touchDots.Remove(id);
                touchSurface.Controls.Remove(dot);
                dot.Dispose();
            }

            UpdateTouchDisplay();
        }

        private void ClearAllTouches()
        {
            touchScreenPoints.Clear();
            foreach (Panel dot in touchDots.Values)
            {
                touchSurface.Controls.Remove(dot);
                dot.Dispose();
            }
            touchDots.Clear();
            UpdateTouchDisplay();
        }

        private void UpdateTouchDisplay()
        {
            Rectangle screenBounds = SystemInformation.VirtualScreen;
            foreach ((uint id, Point screenPoint) in touchScreenPoints)
            {
                Panel dot = touchDots[id];
                dot.Visible = true;
                dot.Location = ScaleToBox(
                    screenPoint,
                    screenBounds,
                    touchSurface.ClientSize,
                    dot.Size
                );
            }

            touchCoordLabel.Text = touchScreenPoints.Count switch
            {
                0 when !touchScreenAvailable => "No touch screen detected",
                0 => "No touch",
                1 => FormatSingleTouch(touchScreenPoints.Values.First()),
                _ => $"{touchScreenPoints.Count} touches",
            };
        }

        private static string FormatSingleTouch(Point screenPoint) =>
            $"X: {screenPoint.X}, Y: {screenPoint.Y}";

        // ---- Rendering --------------------------------------------------------

        private void UpdateDisplay()
        {
            IEnumerable<string> parts = heldKeys
                .Select(k => k.ToString())
                .Concat(heldButtons.Select(MouseButtonName))
                .Concat(PointerSummary());

            if (transientNotice.Length != 0)
            {
                parts = parts.Append(transientNotice);
            }

            string text = string.Join(" + ", parts);
            bool nothingPressed = text.Length == 0;
            displayLabel.Text = nothingPressed ? "(nothing pressed)" : text;

            if (nothingPressed)
            {
                if (!isIdle)
                {
                    // Just went idle — restart the fade from full opacity.
                    isIdle = true;
                    idleSince = DateTime.UtcNow;
                    idleFadeTimer.Start();
                }
                UpdateIdleFade();
            }
            else
            {
                isIdle = false;
                idleFadeTimer.Stop();
                displayLabel.ForeColor = displayBaseColor;
            }

            releaseLabel.Text =
                releasedItems.Count == 0 ? "" : "Released: " + string.Join(" + ", releasedItems);
        }

        private IEnumerable<string> PointerSummary()
        {
            foreach (var group in activePointers.Values.GroupBy(v => v).OrderBy(g => g.Key))
            {
                yield return group.Count() == 1 ? group.Key : $"{group.Key} x{group.Count()}";
            }
        }

        private static string MouseButtonName(MouseButtons button) =>
            button switch
            {
                MouseButtons.Left => "Mouse Left",
                MouseButtons.Right => "Mouse Right",
                MouseButtons.Middle => "Mouse Middle",
                MouseButtons.XButton1 => "Mouse X1",
                MouseButtons.XButton2 => "Mouse X2",
                _ => "Mouse " + button,
            };
    }
}
