using System;
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
using System.Threading;
using System.Windows.Forms;

namespace iTuneServiceManager
{
    public static class ServiceManager
    {
        public const string ServiceName = "iTuneServer Service";

        private static readonly Logger Logger = Logger.GetLogger(true);

        #region DLLImports

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

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

        public static ServiceControllerStatus ServiceStatus
        {
            get
            {
                foreach (var controller in ServiceController.GetServices().Where(controller => controller.ServiceName == ServiceName))
					return controller.Status;
	            
	            return ServiceControllerStatus.Stopped;
            }
        }

        public static bool AuthenticateUser(string domainUser, string password)
        {
            var loginToken = IntPtr.Zero;
            var credentials = new DomainAuthCredentials(domainUser, password);

            try
            {
                if (!LogonUser(credentials.Username, credentials.Domain, credentials.Password, 4, 0, out loginToken))
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

        public static bool ChangeCredentials(string username, string password)
        {
            if (!AuthenticateUser(username, password))
            {
                Logger.Log("Invalid credentials");
                throw new PasswordException();
            }

            try
            {
                Permission.SetRight(username, "SeServiceLogonRight");
            }
            catch (Exception exception)
            {
                Logger.Log("Could not set right SeServiceLogonRight to " + username + ": " + exception);
            }

            try
            {
                string str = "Win32_Service.Name='";
                var obj2 = new ManagementObject(str + ServiceName + "'");
                var args = new object[11];
                args[6] = username;
                args[7] = password;
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
                    {}
                    
                    var location = "./iTuneService.exe";
                    if (location == "")
                    {
                        MessageBox.Show("This product has not been installed correctly.  Uninstall cannot complete.",
                                        ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        ManagedInstallerClass.InstallHelper(new[] {"/u", location});
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Could not uninstall service - " + exception);
                }
            }
        }

        private class DomainAuthCredentials
        {
            public string Domain { get; private set; }
            public string Password { get; private set; }
            public string Username { get; private set; }

            public DomainAuthCredentials(string domainUser, string password)
            {
                Username = domainUser;
                Password = password;
                Domain = ".";

                if (domainUser.Contains(@"\"))
                {
                    var strArray = domainUser.Split(new[] {'\\'}, 2);
                    Domain = strArray[0];
                    Username = strArray[1];
                }
            }
        }
    }
}