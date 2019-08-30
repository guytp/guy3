using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace guy3
{
    class DisplayItem
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public Win32WindowContainer Container { get; }
        public Rectangle Bounds { get; private set; }

        public DisplayItem(Win32WindowContainer container)
        {
            Container = container;
        }

        internal void SetBounds(Rectangle rect)
        {
            if (Bounds.Equals(rect))
                return;
            Container.SetBounds(rect);
            Bounds = rect;
        }
    }
}
