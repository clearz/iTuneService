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
        ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        ServiceInstaller _serviceInstaller = new ServiceInstaller();
        String _parameters = "";

        public iTuneServiceInstaller()
        {
            BeforeInstall += ProjectInstaller_BeforeInstall;
            BeforeUninstall += ProjectInstaller_BeforeUninstall;

            Log.Write("iTuneService: Service Installed");

            Installers.Add(_serviceProcessInstaller);
            Installers.Add(_serviceInstaller);
        }
        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            foreach (var parameter in Context.Parameters.Keys)
            {
                Log.Write(String.Format("{0}={1}", parameter, Context.Parameters[parameter.ToString()]));
            }
            Log.Write(Context.Parameters.Count.ToString());

            // Configure Account for Service Process.
            _serviceProcessInstaller.Account = ServiceAccount.User;
            _serviceProcessInstaller.Username = Context.Parameters["UserName"];

            // Decrypt password from args using today's date as a long string for the password
            var passU = Decrypt(Context.Parameters["EncryptedPassword"], DateTime.Now.ToLongDateString());
            _serviceProcessInstaller.Password = passU;
            _parameters = "\"" + Context.Parameters["ITunesPath"]  + "\"";

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

            IntPtr hScm = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (hScm == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                IntPtr hSvc = OpenService(hScm, _serviceInstaller.ServiceName, SERVICE_ALL_ACCESS);
                if (hSvc == IntPtr.Zero)
                    throw new Win32Exception();
                try
                {
                    QUERY_SERVICE_CONFIG oldConfig;
                    uint bytesAllocated = 8192; // Per documentation, 8K is max size.
                    IntPtr ptr = Marshal.AllocHGlobal((int)bytesAllocated);
                    try
                    {
                        uint bytesNeeded;
                        if (!QueryServiceConfig(hSvc, ptr, bytesAllocated, out bytesNeeded))
                        {
                            throw new Win32Exception();
                        }
                        oldConfig = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(ptr, typeof(QUERY_SERVICE_CONFIG));
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr);
                    }

                    string newBinaryPathAndParameters = oldConfig.lpBinaryPathName + " " + _parameters;

                    if (!ChangeServiceConfig(hSvc, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE, SERVICE_NO_CHANGE,
                        newBinaryPathAndParameters, null, IntPtr.Zero, null, null, null, null))
                        throw new Win32Exception();
                }
                finally
                {
                    if (!CloseServiceHandle(hSvc))
                        throw new Win32Exception();
                }
            }
            finally
            {
                if (!CloseServiceHandle(hScm))
                    throw new Win32Exception();
            }
        }

        public static byte[] Decrypt(byte[] cipherData, byte[] key, byte[] IV)
        {
            using (var ms = new MemoryStream())
            using (var alg = Rijndael.Create())
            {
                alg.Key = key;
                alg.IV = IV;

                using (var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherData, 0, cipherData.Length);
                }
                
                var decryptedData = ms.ToArray();

                return decryptedData;
            }
        }

        public static string Decrypt(string cipherText, string password)
        {
            var salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

            var cipherBytes = Convert.FromBase64String(cipherText);
            var pdb = new Rfc2898DeriveBytes(password, salt);
            var decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenSCManager(
            string lpMachineName,
            string lpDatabaseName,
            uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr OpenService(
            IntPtr hSCManager,
            string lpServiceName,
            uint dwDesiredAccess);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct QUERY_SERVICE_CONFIG
        {
            public uint dwServiceType;
            public uint dwStartType;
            public uint dwErrorControl;
            public string lpBinaryPathName;
            public string lpLoadOrderGroup;
            public uint dwTagId;
            public string lpDependencies;
            public string lpServiceStartName;
            public string lpDisplayName;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool QueryServiceConfig(
            IntPtr hService,
            IntPtr lpServiceConfig,
            uint cbBufSize,
            out uint pcbBytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ChangeServiceConfig(
            IntPtr hService,
            uint dwServiceType,
            uint dwStartType,
            uint dwErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            string lpDependencies,
            string lpServiceStartName,
            string lpPassword,
            string lpDisplayName);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseServiceHandle(
            IntPtr hSCObject);

        private const uint SERVICE_NO_CHANGE = 0xffffffffu;
        private const uint SC_MANAGER_ALL_ACCESS = 0xf003fu;
        private const uint SERVICE_ALL_ACCESS = 0xf01ffu;
    }
}
