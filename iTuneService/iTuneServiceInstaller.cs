using System;
using System.IO;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Collections;
using System.Security.Cryptography; 


namespace iTuneService
{
    [RunInstaller(true)]
    public class iTuneServiceInstaller : Installer
    {
        ServiceProcessInstaller _serviceProcessInstaller1 = new ServiceProcessInstaller();
        ServiceInstaller _serviceInstaller1 = new ServiceInstaller();
        String _parameters = "";

        public iTuneServiceInstaller()
        {
            BeforeInstall += ProjectInstaller_BeforeInstall;
            BeforeUninstall += ProjectInstaller_BeforeUninstall;

            Log.Write("iTuneService: Service Installed");

            Installers.Add(_serviceProcessInstaller1);
            Installers.Add(_serviceInstaller1);
        }
        void ProjectInstaller_BeforeInstall(object sender, InstallEventArgs e)
        {
            foreach (var parameter in Context.Parameters.Keys)
            {
                Log.Write(String.Format("{0}={1}", parameter, Context.Parameters[parameter.ToString()]));
            }
            Log.Write(Context.Parameters.Count.ToString());

            // Configure Account for Service Process.
            _serviceProcessInstaller1.Account = ServiceAccount.User;
            _serviceProcessInstaller1.Username = Context.Parameters["UserName"];
            String passU = Decrypt(Context.Parameters["EncryptedPassword"], DateTime.Now.ToLongDateString());
            _serviceProcessInstaller1.Password = passU;
            _parameters = "\"" + Context.Parameters["ITunesPath"]  + "\"";

            // Configure ServiceName
            _serviceInstaller1.DisplayName = "iTuneServer Service";
            _serviceInstaller1.StartType = ServiceStartMode.Automatic;
            _serviceInstaller1.ServiceName = "iTuneServer Service";
        }

        void ProjectInstaller_BeforeUninstall(object sender, InstallEventArgs e)
        {
            _serviceInstaller1.ServiceName = "iTuneServer Service";
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            IntPtr hScm = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (hScm == IntPtr.Zero)
                throw new Win32Exception();
            try
            {
                IntPtr hSvc = OpenService(hScm, _serviceInstaller1.ServiceName, SERVICE_ALL_ACCESS);
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

        public static byte[] Decrypt(byte[] cipherData,  byte[] Key, byte[] IV)
        {

            var ms = new MemoryStream();

            var alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;

            var cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        public static string Decrypt(string cipherText, string Password)
        {

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
			var pdb = new Rfc2898DeriveBytes(Password, new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

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
