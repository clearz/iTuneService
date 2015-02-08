using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Service
    {
        public static bool IsServiceInstalled
        {
            get
            {
                return ServiceController.GetServices().Any(controller => controller.ServiceName == Constants.ServiceName);
            }
        }
        public static ServiceControllerStatus ServiceStatus
        {
            get
            {
                foreach (var controller in ServiceController.GetServices().Where(controller => controller.ServiceName == Constants.ServiceName))
                    return controller.Status;

                return ServiceControllerStatus.Stopped;
            }
        }

        public static DomainAuthCredentials GetServiceUsername()
        {
            var query = new SelectQuery(String.Format("select name, startname from Win32_Service where name = '{0}'", Constants.ServiceName));

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject service in searcher.Get())
                {
                    return new DomainAuthCredentials((string)service["startname"], null);
                }
            }

            return null;
        }
        public static bool AuthenticateUser(DomainAuthCredentials credentials)
        {
            var loginToken = IntPtr.Zero;

            try
            {
                if (!Win32.LogonUser(credentials.Username, credentials.Domain, credentials.Password, Win32.LogonType.LOGON32_LOGON_NETWORK, 0, out loginToken))
                {
                    Logger.Instance.Log("Error while authenticating user: " + new Win32Exception(Marshal.GetLastWin32Error()));
                    return false;
                }

                new WindowsIdentity(loginToken);

                return true;
            }
            finally
            {
                if (loginToken != IntPtr.Zero) Win32.CloseHandle(loginToken);
            }
        }

        public static string GetServicePathName()
        {
            var query = new SelectQuery(String.Format("select name, pathname from Win32_Service where name = '{0}'", Constants.ServiceName));

            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject service in searcher.Get())
                {
                    return (string)service["pathname"];
                }
            }

            return null;
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
                    Logger.Instance.Log("Exception when starting!\r\n" + exception);
                    return false;
                }

                return true;
            }

            return false;
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
                        Logger.Instance.Log("Exception when stopping!\r\n" + exception);
                        throw;
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
