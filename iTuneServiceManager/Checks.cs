using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace JTunesServiceManager
{
    public class Checks
    {
        // Methods
        public static void CheckWriteInstallDate()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", true);
                if (Convert.ToInt64(key.GetValue("InitCachePoint")) == 0x15fdL)
                {
                    try
                    {
                        long num2 = DateTime.Now.ToBinary();
                        key.SetValue("InitCachePoint", num2, RegistryValueKind.QWord);
                    }
                    catch (Exception)
                    {
                    }
                }
                key.Close();
            }
            catch (Exception)
            {
            }
        }

        private static void CreateLocalRegistryKeys()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ClearZDeV", false);
                if (key == null)
                {
                    key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ClearZDeV");
                    if (key != null)
                    {
                        key.Close();
                    }
                }
                else
                {
                    key.Close();
                }
                key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                if (key == null)
                {
                    key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\ClearZDeV\JTunes");
                    if (key != null)
                    {
                        key.Close();
                    }
                }
                else
                {
                    key.Close();
                }
            }
            catch (Exception exception)
            {
                Logger.Write("Error Creating Registry Keys - " + exception.ToString());
            }
        }

        public static bool EULAAccepted(ClientType clientType)
        {
            if (GetAgreedEULAVersion != ConfigurationStatic.CurrentEULAVersion)
            {
                return (clientType != ClientType.WHSv1);
            }
            return true;
        }

        private static string GetLocalUserString(string str)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                string str2 = "";
                if (key != null)
                {
                    str2 = Convert.ToString(key.GetValue(str, ""));
                    key.Close();
                }
                return str2;
            }
            catch (Exception exception)
            {
                Logger.Write("get_" + str + " - " + exception.ToString());
                return "";
            }
        }

        private static string GetProgIDLocalServer(string str)
        {
            string str2 = str;
            try
            {
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(str2 + @"\CLSID", false);
                string str3 = Convert.ToString(key.GetValue(""));
                key.Close();
                key = Registry.ClassesRoot.OpenSubKey(@"CLSID\" + str3 + @"\LocalServer32", false);
                if (key == null)
                {
                    key = Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\CLSID\" + str3 + @"\LocalServer32", false);
                }
                string str4 = Convert.ToString(key.GetValue(""));
                key.Close();
                return str4;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void SetAgreedEULAVersion(int val)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", true);
                if (key == null)
                {
                    Logger.Write("Could not open iHomeServer reg key");
                }
                else
                {
                    key.SetValue("EULAAgreed", val, RegistryValueKind.DWord);
                    key.Close();
                }
            }
            catch (Exception exception)
            {
                Logger.Write("Error writing EULA reg key: " + exception.ToString());
            }
        }

        private static void SetLocalUserString(string keyName, string value)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", true);
                if (key == null)
                {
                    CreateLocalRegistryKeys();
                }
                else
                {
                    key.Close();
                }
                key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", true);
                key.SetValue(keyName, value, RegistryValueKind.String);
                key.Close();
            }
            catch (Exception exception)
            {
                Logger.Write("set_HostingControlColumns - " + exception.ToString());
            }
        }

        public static bool ValidInstall(ClientType clientType)
        {
            return (((iHomeServerServiceInstalled && (iTunesInstallLocation != "")) && (QuickTimeInstallLocation != "")) && EULAAccepted(clientType));
        }

        // Properties
        public static int GetAgreedEULAVersion
        {
            get
            {
                int defaultValue = 0;
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    int num2 = Convert.ToInt32(key.GetValue("EULAAgreed", defaultValue));
                    key.Close();
                    return num2;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        public static string GetCulture
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    string str = Convert.ToString(key.GetValue("Culture", ""));
                    key.Close();
                    return str;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public static int GetDatabaseMaxSize
        {
            get
            {
                int defaultValue = 0x200;
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    int num2 = Convert.ToInt32(key.GetValue("DatabaseMaxSize", defaultValue));
                    key.Close();
                    return num2;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        public static string GetPostRunProcess
        {
            get
            {
                string defaultValue = "";
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    string str2 = Convert.ToString(key.GetValue("PostRunProcess", defaultValue));
                    key.Close();
                    return str2;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        public static string GetPreRunProcess
        {
            get
            {
                string defaultValue = "";
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    string str2 = Convert.ToString(key.GetValue("PreRunProcess", defaultValue));
                    key.Close();
                    return str2;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        public static int GetRecentWriteDelay
        {
            get
            {
                int defaultValue = 15;
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    int num2 = Convert.ToInt32(key.GetValue("RecentWriteDelay", defaultValue));
                    key.Close();
                    return num2;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
        }

        public static int GetTCPPort
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    int num = Convert.ToInt32(key.GetValue("ServerPort", 0));
                    key.Close();
                    return num;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public static string GetWindowsVersionFriendlyName
        {
            get
            {
                string defaultValue = "";
                RegistryKey key = null;
                try
                {
                    key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    string str2 = Convert.ToString(key.GetValue("ProductName", defaultValue));
                    key.Close();
                    return str2;
                }
                catch (Exception)
                {
                    if (key != null)
                    {
                        key.Close();
                    }
                    return defaultValue;
                }
            }
        }

        public static string HostingControlColumns
        {
            get
            {
                return GetLocalUserString("HostingControlColumns");
            }
            set
            {
                SetLocalUserString("HostingControlColumns", value);
            }
        }

        public static bool iHomeServerServiceInstalled
        {
            get
            {
                if (ConfigurationStatic.DEBUG_ForceReinstallation)
                {
                    return false;
                }
                return ServiceControllerB.IsServiceInstalled;
            }
        }

        public static DirectoryInfo iHomeServerServiceInstallFolder
        {
            get
            {
                FileInfo info = new FileInfo(ServiceInstallLocation);
                return new DirectoryInfo(info.DirectoryName);
            }
        }

        public static string ServiceInstallLocation
        {
            get
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\ClearZDeV\JTunes", false);
                    string str = Convert.ToString(key.GetValue("InstallPath", ""));
                    key.Close();
                    return str;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public static string iTunesControlColumns
        {
            get
            {
                return GetLocalUserString("iTunesControlColumns");
            }
            set
            {
                SetLocalUserString("iTunesControlColumns", value);
            }
        }

        public static string iTunesInstallLocation
        {
            get
            {
                return GetProgIDLocalServer("iTunes.Application");
            }
        }

        public static string QuickTimeInstallLocation
        {
            get
            {
                return GetProgIDLocalServer("QuickTimePlayerLib.QuickTimePlayerApp");
            }
        }
    }
    public enum ClientType
    {
        Generic,
        Console,
        WHSv1,
        WHS2011
    }



}
