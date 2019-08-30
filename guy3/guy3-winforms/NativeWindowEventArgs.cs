using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace guy3
{
    class NativeWindowEventArgs : EventArgs
    {
        public Win32Window Window { get; }
        public NativeWindowEventArgs(Win32Window window)
        {
            Window = window;
        }
    }
}
