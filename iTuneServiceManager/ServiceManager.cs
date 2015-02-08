using System;
using System.Configuration.Install;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Common;

namespace iTuneServiceManager
{
    public static class ServiceManager
    {
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

        public static void Install(String[] args)
        {
            ManagedInstallerClass.InstallHelper(args);
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

        public static void Uninstall(bool Silent)
        {
            if (Service.IsServiceInstalled)
            {
                var ok = DialogResult.OK;
                if (!Silent)
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
                    catch (Exception)
                    { }

                    var location = "./iTuneService.exe";
                    if (location == "")
                    {
                        MessageBox.Show("This product has not been installed correctly.  Uninstall cannot complete.", Constants.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        ManagedInstallerClass.InstallHelper(new[] { "/u", location });
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not uninstall service - " + exception);
                }
            }
        }
    }
}