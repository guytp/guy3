using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace guy3
{
    class Win32
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd,
            StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
        ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumDesktopWindows(IntPtr hDesktop,
            EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumDelegate enumProc, IntPtr lParam);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        private const int WS_CHILD = 0x40000000;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern ulong GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        // Define the callback delegate's type.
        private delegate bool EnumDelegate(IntPtr hWnd, int lParam);
        private static List<IntPtr> WindowHandles = new List<IntPtr>();
        private static List<string> WindowTitles = new List<string>();
        private static List<RECT> Sizes = new List<RECT>();
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        // Return a list of the desktop windows' handles and titles.
        public static Win32Window[] GetDesktopWindowHandlesAndTitles()
        {
            List<Win32Window> windows = new List<Win32Window>();
            WindowHandles.Clear();
            WindowTitles.Clear();
            Sizes.Clear();

            if (EnumWindows(FilterCallback, IntPtr.Zero))
            {
                for (int i = 0; i < WindowHandles.Count; i++)
                {
                    RECT r = Sizes[i];
                    Rectangle bounds = new Rectangle
                    {
                        X = r.Left,
                        Y = r.Top,
                        Width = r.Right - r.Left,
                        Height = r.Bottom - r.Top
                    };
                    windows.Add(new Win32Window(WindowHandles[i], WindowTitles[i], bounds));
                }
            }
            return windows.ToArray();
        }

        // We use this function to filter windows.
        // This version selects visible windows that have titles.
        private static bool FilterCallback(IntPtr hWnd, int lParam)
        {
            // Get the window's title.
            StringBuilder sb_title = new StringBuilder(1024);
            int length = GetWindowText(hWnd, sb_title, sb_title.Capacity);
            string title = sb_title.ToString();


            // If the window is visible and has a title, save it.
            bool validTitle = title != "NVIDIA GeForce Overlay" && !title.Contains("Visual Studio") && title != "Program Manager";// (title.Contains("Notepad") || title.Contains("Paint") || title.Contains("MINGW64"));
            if (IsWindowVisible(hWnd) && string.IsNullOrEmpty(title) == false && validTitle)
            {
                WindowHandles.Add(hWnd);
                WindowTitles.Add(title);
                RECT rct;
                GetWindowRect(hWnd, out rct);
                Sizes.Add(rct);
            }

            // Return true to indicate that we
            // should continue enumerating windows.
            return true;
        }

        List<Win32Window> _windows = new List<Win32Window>();
        public event EventHandler<NativeWindowEventArgs> WindowAdded;
        public event EventHandler<NativeWindowEventArgs> WindowUpdated;
        public event EventHandler<NativeWindowEventArgs> WindowRemoved;
        public void ScanForWindowUpdates()
        {
            Win32Window[] windows = GetDesktopWindowHandlesAndTitles();
            foreach (Win32Window window in windows)
            {
                Win32Window existing = _windows.FirstOrDefault(w => w.Handle == window.Handle);
                if (existing == null)
                {
                    _windows.Add(window);
                    WindowAdded?.Invoke(this, new NativeWindowEventArgs(window));
                }
                else if (!existing.Equals(window))
                {
                    existing.UpdateFrom(window);
                    WindowUpdated?.Invoke(this, new NativeWindowEventArgs(window));
                }
            }
            List<Win32Window> toRemove = new List<Win32Window>();
            foreach (Win32Window window in _windows)
                if (!windows.Any(w => w.Handle == window.Handle))
                    toRemove.Add(window);
            foreach (Win32Window window in toRemove)
            {
                _windows.Remove(window);
                WindowRemoved?.Invoke(this, new NativeWindowEventArgs(window));
            }
        }
    }
}
