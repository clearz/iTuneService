using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmptyRecycleBin
{
    public static class Program
    {
        public const string EmptyCommand = "EMPTY";

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [Flags]
        public enum RecycleFlag : uint
        {
            /// <summary>
            /// No dialog box confirming the deletion of the objects will be displayed.
            /// </summary>
            SHERB_NOCONFIRMATION = 0x00000001,
            /// <summary>
            /// No dialog box indicating the progress will be displayed.
            /// </summary>
            SHERB_NOPROGRESSUI = 0x00000002,
            /// <summary>
            /// No sound will be played when the operation is complete. 
            /// </summary>
            SHERB_NOSOUND = 0x00000004,
        }

        const int E_UNEXPECTED = unchecked((int)0x8000FFFF);

        [DllImport("Shell32.dll")]
        public static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlag dwFlags);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length < 1 || args[0] != EmptyCommand) return 2;

            var ret = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlag.SHERB_NOSOUND | RecycleFlag.SHERB_NOCONFIRMATION);

            // If the recycle bin is already empty, then we'll get E_UNEXPECTED
            if (ret == E_UNEXPECTED) ret = 0;

            return ret;
        }
    }
}
