using System;
using System.Configuration.Install;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Common;

namespace iTuneServiceManager
{
    public static class ServiceManager
    {
        private const string ServiceLocation = "./iTuneService.exe";

        public enum State
        {
            Setup,
            SetupComplete,
            Installing,
            ServiceRunning,
            ServiceStopped,
            Uninstalling,
        }

        private static State _currentState;

        public static event EventHandler<State> StateChanged;
        public static State CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState == value) return;
                _currentState = value;
                if (StateChanged != null) StateChanged(null, _currentState);
            }
        }

        public static string GetServiceiTunesPath()
        {
            var servicePathName = Service.GetServicePathName();
            if (servicePathName == null) return null;

            var m = Regex.Match(servicePathName, @"^[ ]*(?:""[^""]*""|[^ ]+)[ ]+(?:""(?<itunes>[^""]*)""|(?<itunes>[^ ]+))");

            return m.Success
                       ? m.Groups["itunes"].Value
                       : null;
        }

        public static void Install(params string[] customArgs)
        {
            // Note that all arguments must come before the service EXE location
            var argsWithLog = customArgs.Concat(new[] { GetServiceLogFile("Install"), GetInstallStateDir(), ServiceLocation }).ToArray();
            ManagedInstallerClass.InstallHelper(argsWithLog);
        }

        public static bool StartService()
        {
            var success = Service.StartService(Constants.ServiceName);
            
            if (success) CurrentState = State.ServiceRunning;

            return success;
        }

        public static bool StopService()
        {
            var success = Service.StopService(Constants.ServiceName);

            if (success) CurrentState = State.ServiceStopped;

            return success;
        }

        public static void Uninstall()
        {
            Uninstall(true);
        }

        public static void Uninstall(bool silent)
        {
            if (Service.IsServiceInstalled)
            {
                var ok = DialogResult.OK;
                if (!silent)
                {
                    ok = MessageBox.Show("Please ensure that any Services Console windows are closed before continuing.\r\n\r\nPress 'OK' when ready.", Constants.ServiceName, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                }
                if (ok != DialogResult.OK) return;

                try
                {
                    try
                    {
                        StopService();
                    }
                    catch (Exception) {}

                    // Note that all arguments must come before the service EXE location
                    ManagedInstallerClass.InstallHelper(new[] { "/u", GetServiceLogFile("Uninstall"), GetInstallStateDir(), ServiceLocation });
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not uninstall service - " + exception);
                }
            }
        }

        private static string GetServiceLogFile(string which)
        {
            // Use the same output folder as log4net log file by default
            var logPath = Util.GetLog4NetLogPath() ?? @".";

            // Note that spaces in the path are okay because of the way this
            // is passed via a string array rather than a command line.
            return string.Format(@"/LogFile={0}\Service.{1}.log", logPath.TrimEnd('\\'), which);
        }
        private static string GetInstallStateDir()
        {
            // Use the same output folder as log4net log file by default
            var logPath = Util.GetLog4NetLogPath() ?? @".";

            // Note that spaces in the path are okay because of the way this
            // is passed via a string array rather than a command line.
            return string.Format(@"/InstallStateDir={0}", logPath.TrimEnd('\\'));
        }
    }
}