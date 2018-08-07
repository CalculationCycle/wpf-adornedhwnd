using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace ChildWindowInDll
{
    public class DllIntf
    {
        const string childWinDll = "CppWin32ChildWindow.dll";

        [DllImport(childWinDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int NewChildWindow(int parentHwnd);


        [DllImport(childWinDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool DeleteChildWindow(int childHwnd);

    }
}