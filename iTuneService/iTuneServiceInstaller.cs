using System;
using System.IO;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Collections;
using System.Security.Cryptography; 
using Common;

namespace iTuneService
{
    [RunInstaller(true)]
    public class iTuneServiceInstaller : Installer
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(iTuneServiceInstaller));

        ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        ServiceInstaller _serviceInstaller = new ServiceInstaller();
        String _parameters = "";
        
        public iTuneServiceInstaller()
        {
            BeforeInstall += ProjectInstaller_BeforeInstall;
            BeforeUninstall += ProjectInstaller_BeforeUninstall;

            _log.Info("iTuneService: Service Installed");

            Installers.Add(_serviceProcessInstaller);
            Installers.Add(_serviceInstaller);
        }
        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            _log.DebugFormat("Context Parameter Count={0}", Context.Parameters.Count);
            foreach (var parameter in Context.Parameters.Keys)
            {
                _log.DebugFormat("{0}={1}", parameter, Context.Parameters[parameter.ToString()]);
            }

            // Configure Account for Service Process.
            _serviceProcessInstaller.Account = ServiceAccount.User;
            _serviceProcessInstaller.Username = Context.Parameters[Constants.UserNameContextArg];

            // Decrypt password from args using today's date as a long string for the password
            var savedCreds = Credentials.RetrieveCredential();
            if (savedCreds != null)
            {
                if (string.Compare(_serviceProcessInstaller.Username, savedCreds.Item1, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    _log.Error(string.Format("Persisted credentials are for '{0}' but user passed in is '{1}'.",
                                             savedCreds.Item1,
                                             _serviceProcessInstaller.Username));
                    savedCreds = null;
                }
                else
                {
                    _log.Info(string.Format("Persisted credentials for '{0}' were successfully loaded.",
                                             savedCreds.Item1));
                }
            }
            else
            {
                _log.Error("Persisted credentials could not be loaded.");
            }

            _serviceProcessInstaller.Password = savedCreds != null ? savedCreds.Item2 : "";
            _parameters = "\"" + Context.Parameters[Constants.ITunesPathContextArg]  + "\"";

            // Configure ServiceName
            _serviceInstaller.DisplayName = Constants.ServiceName;
            _serviceInstaller.StartType = ServiceStartMode.Automatic;
            _serviceInstaller.ServiceName = Constants.ServiceName;
        }

        void ProjectInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            _serviceInstaller.ServiceName = Constants.ServiceName;
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            IntPtr hScm = Win32.OpenSCManager(null, null, Win32.SC_MANAGER_ALL_ACCESS);
            if (hScm == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                IntPtr hSvc = Win32.OpenService(hScm, _serviceInstaller.ServiceName, Win32.SERVICE_ALL_ACCESS);
                if (hSvc == IntPtr.Zero)
                    throw new Win32Exception();
                try
                {
                    Win32.QUERY_SERVICE_CONFIG oldConfig;
                    uint bytesAllocated = 8192; // Per documentation, 8K is max size.
                    IntPtr ptr = Marshal.AllocHGlobal((int)bytesAllocated);
                    try
                    {
                        uint bytesNeeded;
                        if (!Win32.QueryServiceConfig(hSvc, ptr, bytesAllocated, out bytesNeeded))
                        {
                            throw new Win32Exception();
                        }
                        oldConfig = (Win32.QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, typeof(Win32.QUERY_SERVICE_CONFIG));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

                    string newBinaryPathAndParameters = oldConfig.lpBinaryPathName + " " + _parameters;

                    if (!Win32.ChangeServiceConfig(hSvc, Win32.SERVICE_NO_CHANGE, Win32.SERVICE_NO_CHANGE, Win32.SERVICE_NO_CHANGE,
                        newBinaryPathAndParameters, null, IntPtr.Zero, null, null, null, null))
                        throw new Win32Exception();
                }
                finally
                {
                    if (!Win32.CloseServiceHandle(hSvc))
                        throw new Win32Exception();
                }
            }
            finally
            {
                if (!Win32.CloseServiceHandle(hScm))
                    throw new Win32Exception();
            }
        }
    }
}
