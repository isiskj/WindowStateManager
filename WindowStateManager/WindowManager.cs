using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.Storage;


namespace WindowStateManager
{
    public static class WindowManager
    {
        private const string KEY_X = "Window_X";
        private const string KEY_Y = "Window_Y";
        private const string KEY_W = "Window_W";
        private const string KEY_H = "Window_H";
        private const string KEY_MAX = "Window_Maximaized";

        private static bool _isClosed = false;
        private static bool _restored = false;

        /// <summary>
        /// Attach:
        /// </summary>
        /// <param name="window">Handle Window</param>
        public static void Attach(Window window)
        {
            AttachInternal(window, null);
        }

        /// <summary>
        /// Attach: window size Fixed DPI
        /// </summary>
        /// <param name="window">Handle Window</param>
        /// <param name="widthDip">Window Width</param>
        /// <param name="heightDip">Window Height</param>
        public static void Attach(Window window, double widthDip, double heightDip)
        {
            double dpi = GetDpiScale(window);
            var size = new SizeInt32(
                (int)(widthDip * dpi),
                (int)(heightDip * dpi)
            );

            AttachInternal(window, size);
        }

        /// <summary>
        /// Attach: window size Fixed Physical Pixel
        /// </summary>
        /// <param name="window">Handle Window</param>
        /// <param name="size">Window Size</param>
        public static void Attach(Window window, SizeInt32 size)
        {
            AttachInternal(window, size);
        }

        // ------------------------------
        // 内部処理
        // ------------------------------
        private static void AttachInternal(Window window, SizeInt32? fixedSize)
        {
            window.Activated += (s, e) =>
            {
                if (!_restored && !_isClosed)
                {
                    Restore(window, fixedSize);
                    _restored = true;
                }
            };

            window.Closed += (s, e) =>
            {
                _isClosed = true;
                Save(window, fixedSize != null);
            };
        }

        // ------------------------------
        // 保存
        // ------------------------------
        private static void Save(Window window, bool isSizeFixed)
        {
            // App Local Settingに保存
            var appWindow = GetAppWindow(window);
            var local = ApplicationData.Current.LocalSettings;

            local.Values[KEY_X] = appWindow.Position.X;
            local.Values[KEY_Y] = appWindow.Position.Y;
            local.Values[KEY_W] = appWindow.Size.Width;
            local.Values[KEY_H] = appWindow.Size.Height;
            local.Values[KEY_MAX] = ((OverlappedPresenter)appWindow.Presenter).State == OverlappedPresenterState.Maximized;
        }

        // ------------------------------
        // 復元
        // ------------------------------
        private static void Restore(Window window, SizeInt32? fixedSize)
        {
            var appWindow = GetAppWindow(window);
            if (appWindow == null || appWindow.Presenter == null)
            {
                return;
            }

            var local = ApplicationData.Current.LocalSettings;

            bool hasSaved =
                local.Values.ContainsKey(KEY_W) &&
                local.Values.ContainsKey(KEY_H);

            if (hasSaved)
            {
                bool isMax = (bool)local.Values[KEY_MAX];

                if (isMax)
                {
                    ((OverlappedPresenter)appWindow.Presenter).Maximize();
                    return;
                }

                int x = (int)local.Values[KEY_X];
                int y = (int)local.Values[KEY_Y];
                int w = (int)local.Values[KEY_W];
                int h = (int)local.Values[KEY_H];

                appWindow.Move(new PointInt32(x, y));

                if (fixedSize != null)
                {
                    appWindow.Resize(fixedSize.Value);
                }
                else
                {
                    appWindow.Resize(new SizeInt32(w, h));
                }

                return;
            }

            // 初回起動時：固定サイズが指定されていれば適用
            if (fixedSize != null)
            {
                appWindow.Resize(fixedSize.Value);
            }
        }

        // ------------------------------
        // AppWindow 取得
        // ------------------------------
        private static AppWindow GetAppWindow(Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        //------------------------------
        //DPI スケール取得
        //------------------------------
        private static double GetDpiScale(Window window)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            int dpi = GetDpiForWindow(hwnd);
            return dpi / 96.0;
        }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern int GetDpiForWindow(nint hwnd);
    }
}
