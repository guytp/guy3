using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guy3
{
    class Win32Window
    {
        public IntPtr Handle { get; }
        public string Title { get; private set; }
        public Rectangle Bounds { get; private set; }

        public event EventHandler BoundsChanged;
        public Win32Window(IntPtr handle, string title, Rectangle bounds)
        {
            Handle = handle;
            Title = title;
            Bounds = bounds;
        }

        public override bool Equals(object obj)
        {
            Win32Window w = obj as Win32Window;
            if (w == null)
                return false;
            if (w == this)
                return true;
            return w.Title.Equals(Title) && w.Handle.Equals(Handle) && w.Bounds.Equals(Bounds);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + Handle.GetHashCode();
                hash = (hash * 7) + (Title == null ? 0 : Title.GetHashCode());
                hash = (hash * 7) + Bounds.GetHashCode();
                return hash;
            }
        }

        internal void UpdateFrom(Win32Window window)
        {
            Title = window.Title;
            bool boundsChanged = !window.Bounds.Equals(Bounds);
            Bounds = window.Bounds;
            if (boundsChanged)
                BoundsChanged.Invoke(this, new EventArgs());
        }
    }
}
