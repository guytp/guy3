

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace guy3
{
    class Win32WindowContainer : IDisposable
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const int GWL_STYLE = -16;

        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_TABSTOP = 0x00010000;

        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;
        public const uint WS_SIZEBOX = 0x00040000;


        private Form _form;
        private IntPtr _displayHandle;
        private CustomWindow _customWindow;
        private Rectangle _desiredBounds;
        private bool _boundsSet;

        public Win32Window Window { get; }
        public Win32WindowContainer(Win32Window window)
        {
            //_form = new Form();
            Window = window;
            window.BoundsChanged += OnBoundsChanged;
            /*
            _form.HandleCreated += OnHandleCreated;
            _form.Width = 1;
            _form.Height = 1;
            _form.Top = 0;
            _form.Left = 0;
            _form.Show();
            */
            _displayHandle = window.Handle;

            // Remove UI components from the apps
            uint oldStyle = GetWindowLong(Window.Handle, GWL_STYLE);
            SetWindowLong(Window.Handle, GWL_STYLE, (uint)(oldStyle & ~WS_BORDER & ~WS_CAPTION & ~WS_SYSMENU & ~WS_THICKFRAME & ~WS_DLGFRAME  & ~WS_MINIMIZE & ~WS_MINIMIZEBOX & ~WS_MAXIMIZE & ~WS_MAXIMIZEBOX & ~WS_THICKFRAME & ~WS_SIZEBOX));

            if (!SetWindowPos(Window.Handle, new IntPtr(-2), 0, 0, 0, 0, 0x0002 | 0x0001 | 0x0040))
                System.Diagnostics.Trace.WriteLine("Failed to set topmost");
        }

        private void OnBoundsChanged(object sender, EventArgs e)
        {
            if (Window.Bounds.Equals(_desiredBounds))
                return;
            UpdateBounds();
        }

        private void OnHandleCreated(object sender, System.EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("Handle created for capture");

            uint oldStyle = GetWindowLong(Window.Handle, GWL_STYLE);
            SetWindowLong(Window.Handle, GWL_STYLE, (uint)((oldStyle | WS_CHILD) & ~WS_BORDER));

            _displayHandle = _form.Handle;

            SetParent(Window.Handle, _displayHandle);



            //SetBounds(new Rectangle(_form.Top, _form.Left, _form.Width, _form.Height));
        }

        public void SetBounds(Rectangle rect)
        {
            /*
            Action act = () =>
            {
                _form.Width = rect.Width;
                _form.Height = rect.Height;
                _form.Top = rect.Top;
                _form.Left = rect.Left;
            };
            //_form.Invoke(act);
            */

            //MoveWindow(Window.Handle, 0, 0, rect.Width, rect.Height, true);
            _boundsSet = true;
            _desiredBounds = rect;
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (!_boundsSet)
                return;
            MoveWindow(_displayHandle, _desiredBounds.Left, _desiredBounds.Top, _desiredBounds.Width, _desiredBounds.Height, true);
        }

        public void Dispose()
        {
            _form.Dispose();
            _form = null;
        }
    }
}