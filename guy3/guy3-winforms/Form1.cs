using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace guy3
{
    public partial class Form1 : Form
    {

        private readonly Win32 _win32 = new Win32();
        private bool _isInvalidated = false;
        private List<DisplayItem> _displayItems = new List<DisplayItem>();

        public Form1()
        {
            InitializeComponent();
            _win32.WindowAdded += OnWindowAdded;
            _win32.WindowRemoved += OnWindowRemoved;
            _win32.WindowUpdated += OnWindowUpdated;
            KeyboardInterceptor kbd = new KeyboardInterceptor();
            KeyboardInterceptor.WindowsEnterPressed += OnWindowsEnterPressed;

            VisibleChanged += OnVis;
        }

        private void OnVis(object sender, EventArgs e)
        {
            if (!Visible)
                return;

            VisibleChanged -= OnVis;

            Thread thr = new Thread(Win32UpdateThread)
            {
                IsBackground = true,
                Name = "Win32 Scanner"
            };
            thr.Start();
        }

        private void OnWindowUpdated(object sender, NativeWindowEventArgs e)
        {
            Log("Upd: " + e.Window.Handle + "   Title: " + e.Window.Title);
            _isInvalidated = true;
        }

        private void OnWindowsEnterPressed(object sender, EventArgs e)
        {
            Log("Starting terminal");
            System.Diagnostics.Process.Start(@"C:\Program Files\Git\git-bash.exe", "--cd-to-home");
        }

        private void Log(string text)
        {
            Action act = () =>
            {
                LogText.Text = text + Environment.NewLine + LogText.Text;
            };
            if (InvokeRequired)
                Invoke(act);
            else
                act();
        }
        private void OnWindowRemoved(object sender, NativeWindowEventArgs e)
        {
            Log("Rem: " + e.Window.Handle + "   Title: " + e.Window.Title);
            DisplayItem toRemove = _displayItems.FirstOrDefault(di => di.Container.Window.Handle == e.Window.Handle);
            if (toRemove != null)
            {
                _displayItems.Remove(toRemove);
                _isInvalidated = true;
            }
        }

        private void OnWindowAdded(object sender, NativeWindowEventArgs e)
        {
            Log("New: " + e.Window.Handle + "   Title: " + e.Window.Title);
            _displayItems.Add(new DisplayItem(new Win32WindowContainer(e.Window)));
            _isInvalidated = true;
        }

        private void Win32UpdateThread()
        {
            while (true)
            {
                _win32.ScanForWindowUpdates();
                UpdateUiIfRequired();
                Thread.Sleep(10);
            }
        }

        private void UpdateUiIfRequired()
        {
                if (!_isInvalidated || _displayItems.Count == 0)
                    return;

                Rectangle bounds = Screen.PrimaryScreen.Bounds;

                // For now just horizontally stack
                int widthPerApp = bounds.Width / _displayItems.Count;
                int heightPerApp = bounds.Height;
                int rows = 1;
                while (widthPerApp < 300)
                {
                    rows++;
                    heightPerApp = bounds.Height / rows;
                    widthPerApp = bounds.Width / (_displayItems.Count / rows);
                }
                int x = 0;
                int y = 0;
                foreach (DisplayItem item in _displayItems)
                {
                    Rectangle r = new Rectangle(x, y, widthPerApp, heightPerApp);
                    item.SetBounds(r);
                    x += widthPerApp;
                    Log("Setting " + item.Container.Window.Handle + " / " + item.Container.Window.Title + " to be at " + r);
                    if (x >= bounds.Width)
                    {
                        x = 0;
                        y += heightPerApp;
                    }
                }
            _isInvalidated = false;
        }

        // Layotu better if manhy apps (multirple rows)
        // Quicker window detection - i.e. with console - rather than waiting for it to load
        // Set TOP (not topmost)

        // Prevent Alt+Space
        // Exclude hidden apps/windows (i.e.  Google Backup & Sync, MS Teams, etc.)
        // On a window minimising/maximising restore it
        // Exclude popups and non-root level windows (i.e. chrome tabs)
        // Cope with full screen (i.e. games)
        // Dispose of everything throughout code and on close
        // Callback hooks rather than loop
        // Nested container for different layout types with key press for H/V change
        // Support adding UI decorations (i.e. titles) and transparencies - use CusotmWindow/NativeWindow
    }
}
