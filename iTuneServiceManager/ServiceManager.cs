using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CredentialManagement;
using DialogResult = System.Windows.Forms.DialogResult;

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

        public const string ServiceName = "iTuneServer Service";
        public const string CredentialName = "iTuneServer Service Account";

        private static readonly Logger Logger = Logger.GetLogger(true);
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

        #region DLLImports

        /// <remarks>
        /// See: http://www.pinvoke.net/default.aspx/advapi32.logonuser
        /// </remarks>
        public enum LogonType
        {
            // ReSharper disable InconsistentNaming
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8, // Win2K or higher
            LOGON32_LOGON_NEW_CREDENTIALS = 9 // Win2K or higher
            // ReSharper restore InconsistentNaming
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, LogonType dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        #endregion

        public static bool IsServiceInstalled
        {
            get
            {
                return ServiceController.GetServices().Any(controller => controller.ServiceName == ServiceName);
            }
        }

        public static DomainAuthCredentials GetServiceUsername()
        {
            var query = new SelectQuery(string.Format("select name, startname from Win32_Service where name = '{0}'", ServiceManager.ServiceName));

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject service in searcher.Get())
                {
                    return new DomainAuthCredentials((string)service["startname"], null);
                }
            }

            return null;
        }

        public static string GetServiceiTunesPath()
        {
            var query = new SelectQuery(string.Format("select name, pathname from Win32_Service where name = '{0}'", ServiceManager.ServiceName));

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject service in searcher.Get())
                {
                    var m = Regex.Match((string)service["pathname"], @"^[ ]*(?:""[^""]*""|[^ ]+)[ ]+(?:""(?<itunes>[^""]*)""|(?<itunes>[^ ]+))");
                    if (m.Success) return m.Groups["itunes"].Value;
                }
            }

            return null;
        }

        public static ServiceControllerStatus ServiceStatus
        {
            get
            {
                foreach (var controller in ServiceController.GetServices().Where(controller => controller.ServiceName == ServiceName))
                    return controller.Status;

                return ServiceControllerStatus.Stopped;
            }
        }

        public static bool AuthenticateUser(DomainAuthCredentials credentials)
        {
            var loginToken = IntPtr.Zero;

            try
            {
                if (!LogonUser(credentials.Username, credentials.Domain, credentials.Password, LogonType.LOGON32_LOGON_NETWORK, 0, out loginToken))
                {
                    Logger.Log("Error while authenticating user: " + new Win32Exception(Marshal.GetLastWin32Error()));
                    return false;
                }

                new WindowsIdentity(loginToken);

                return true;
            }
            finally
            {
                if (loginToken != IntPtr.Zero) CloseHandle(loginToken);
            }
        }

        public static bool ChangeCredentials(DomainAuthCredentials credentials)
        {
            if (!AuthenticateUser(credentials))
            {
                Logger.Log("Invalid credentials");
                throw new PasswordException();
            }

            try
            {
                Permission.SetRight(credentials, "SeServiceLogonRight");
            }
            catch (Exception exception)
            {
                Logger.Log("Could not set right SeServiceLogonRight to " + credentials.ToFullUsername() + ": " + exception);
            }

            try
            {
                string str = "Win32_Service.Name='";
                var obj2 = new ManagementObject(str + ServiceName + "'");
                var args = new object[11];
                args[6] = credentials.ToFullUsername();
                args[7] = credentials.Password;
                obj2.InvokeMethod("Change", args);
            }
            catch (Exception exception2)
            {
                Logger.Log("Cannot change service credentials: " + exception2);
                return false;
            }

            RestartService();

            return true;
        }

        public static void Install(String[] args)
        {
            ManagedInstallerClass.InstallHelper(args);
        }

        public static bool PersistCredentials(string username, string password)
        {
            using (var cm =
                new Credential
                {
                    Target = CredentialName,
                    PersistanceType = PersistanceType.LocalComputer,
                    Username = username,
                    Password = password,
                })
            {
                return cm.Save();
            }
        }

        public static Tuple<string, string> RetrieveCredential()
        {
            using (var cm =
                new Credential
                {
                    Target = CredentialName,
                    PersistanceType = PersistanceType.LocalComputer,
                })
            {
                if (!cm.Exists() || !cm.Load()) return null;

                return Tuple.Create(cm.Username, cm.Password);
            }
        }

        public static bool RemoveCredential()
        {
            using (var cm =
                new Credential
                {
                    Target = CredentialName,
                    PersistanceType = PersistanceType.LocalComputer,
                })
            {
                if (!cm.Exists()) return true;

                return cm.Delete();
            }
        }

        public static bool RestartService()
        {
            return RestartService(ServiceName);
        }

        public static bool RestartService(string strServiceName)
        {
            if (!StopService(strServiceName))
            {
                return false;
            }
            return StartService(strServiceName);
        }

        public static bool StartService()
        {
            return StartService(ServiceName);
        }

        public static bool StartService(string strServiceName)
        {
            foreach (var controller in ServiceController.GetServices())
            {
                if (controller.ServiceName != strServiceName) continue;

                try
                {
                    if (controller.Status == ServiceControllerStatus.Stopped)
                    {
                        controller.Start();
                    }

                    controller.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));

                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Log("Exception when starting!\r\n" + exception);
                    return false;
                }

                CurrentState = State.ServiceRunning;
                
                return true;
            }

            return false;
        }

        public static bool StopService()
        {
            return StopService(ServiceName);
        }

        public static bool StopService(string strServiceName)
        {
            foreach (var controller in ServiceController.GetServices())
            {
                if (controller.ServiceName == strServiceName)
                {
                    try
                    {
                        if (controller.Status == ServiceControllerStatus.Running)
                        {
                            controller.Stop();
                        }

                        controller.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 1, 0));

                        if (controller.Status != ServiceControllerStatus.Stopped)
                        {
                            return false;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Log("Exception when stopping!\r\n" + exception);
                        throw;
                    }

                    CurrentState = State.ServiceStopped;

                    return true;
                }
            }
            return false;
        }

        public static void Uninstall()
        {
            Uninstall(true);
        }

        public static void Uninstall(bool Silent)
        {
            if (IsServiceInstalled)
            {
                var ok = DialogResult.OK;
                if (!Silent)
                {
                    ok = MessageBox.Show("Please ensure that any Services Console windows are closed before continuing.\r\n\r\nPress 'OK' when ready.",
                                         ServiceName, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
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
                        MessageBox.Show("This product has not been installed correctly.  Uninstall cannot complete.",
                                        ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
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