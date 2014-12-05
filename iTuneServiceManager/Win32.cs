using System;
using System.Runtime.InteropServices;

namespace iTuneServiceManager
{
	public class Win32
	{
		public const int MF_BYPOSITION = 0x0400;
		public const int MF_DISABLED = 0x0002;

		[DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
		public static extern IntPtr GetSystemMenu(IntPtr hwnd, int revert);

		[DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
		public static extern int GetMenuItemCount(IntPtr hmenu);

		[DllImport("user32.dll", EntryPoint = "RemoveMenu")]
		public static extern int RemoveMenu(IntPtr hmenu, int npos, int wflags);

		[DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
		public static extern int DrawMenuBar(IntPtr hwnd);
	}
}