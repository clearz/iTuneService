using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace Common
{
    public static class Util
    {
        /// <summary>
        /// Gets the directory name of the first <see cref="FileAppender"/> in the
        /// list of appenders configured for log4net. Returns null if not found.
        /// </summary>
        public static string GetLog4NetLogPath()
        {
            var hierarchy = ((Hierarchy)LogManager.GetRepository());
            var rootAppender = hierarchy.Root.Appenders.OfType<FileAppender>()
                                        .FirstOrDefault();

            return rootAppender != null
                       ? Path.GetDirectoryName(rootAppender.File)
                       : null;
        }

        #region UAC / Elevation Support

        private static Action<bool> _appSetSingleInstance;
        private static bool? _isRunAsAdmin = null;

        /// <summary>
        /// Registers a callback that can be used to suppress the application's single
        /// instance behavior. This allows us to restart the app without cycling back
        /// to the current process.
        /// </summary>
        public static void RegisterSetAppSingleInstance(Action<bool> appShutdown)
        {
            _appSetSingleInstance = appShutdown;
        }

        /// <summary>
        /// Whether or not we're currently running with admin privileges.
        /// </summary>
        public static bool IsRunAsAdmin()
        {
            if (_isRunAsAdmin == null)
            {
                var principle = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                _isRunAsAdmin = principle.IsInRole(WindowsBuiltInRole.Administrator);
            }
            return _isRunAsAdmin.Value;
        }

        /// <summary>
        /// Adds a UAC icon to a button if we're not running as admin.
        /// </summary>
        public static void CheckSetUACIcon(ButtonBase button)
        {
            if (IsRunAsAdmin()) return;
            button.FlatStyle = FlatStyle.System;
            Win32.SendMessage(button.Handle, Win32.BCM_SETSHIELD, 0, (IntPtr)1);
        }

        /// <summary>
        /// Adds a UAC icon to a menu item if we're not running as admin.
        /// </summary>
        public static void CheckSetUACIcon(ToolStripMenuItem menuItem)
        {
            if (IsRunAsAdmin()) return;

            Win32.SHSTOCKICONINFO iconResult = new Win32.SHSTOCKICONINFO();
            iconResult.cbSize = (uint)Marshal.SizeOf(iconResult);

            Win32.SHGetStockIconInfo(
                Win32.SHSTOCKICONID.SIID_SHIELD,
                Win32.SHGSI.SHGSI_ICON | Win32.SHGSI.SHGSI_SMALLICON,
                ref iconResult);

            menuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            menuItem.Image = Bitmap.FromHicon(iconResult.hIcon);
        }

        /// <summary>
        /// Relaunches the aplication as administrator. If the user cancels the
        /// elevation dialog, then returns false.
        /// </summary>
        /// <param name="args">Command line arguments to pass to the new instance.</param>
        public static bool RelaunchAsAdmin(string args)
        {
            // Disable single instance so new process can start
            if (_appSetSingleInstance != null) _appSetSingleInstance(false);

            // Launch itself as administrator
            var psi =
                new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Application.ExecutablePath,
                    Arguments = args,
                    Verb = "runas"
                };

            try
            {
                Process.Start(psi);
            }
            catch
            {
                // Reenable single instance after failure
                if (_appSetSingleInstance != null) _appSetSingleInstance(true);

                // User refused to allow privileges elevation.
                // Do nothing and return directly ...
                return false;
            }

            // Quit self
            Application.Exit();
            return true;
        }

        #endregion

    }
}
