﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Common
{

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Win32
    {
        public const int MF_BYPOSITION = 0x0400;
        public const int MF_DISABLED = 0x0002;

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        public static extern IntPtr GetSystemMenu(IntPtr hwnd, int revert);

        [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
        public static extern int GetMenuItemCount(IntPtr hmenu);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        public static extern int RemoveMenu(IntPtr hmenu, int npos, int wflags);

        [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
        public static extern int DrawMenuBar(IntPtr hwnd);

        #region Logon User

        /// <remarks>
        /// See: http://www.pinvoke.net/default.aspx/advapi32.logonuser
        /// </remarks>
        public enum LogonType
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8, // Win2K or higher
            LOGON32_LOGON_NEW_CREDENTIALS = 9 // Win2K or higher
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(string lpszUsername,
                                            string lpszDomain,
                                            string lpszPassword,
                                            LogonType dwLogonType,
                                            int dwLogonProvider,
                                            out IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        #endregion

        #region For Setting Access Rights

        [DllImport("advapi32")]
        public static extern void FreeSid(IntPtr pSid);
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("advapi32.dll")]
        public static extern bool IsValidSid(IntPtr pSid);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern long LsaAddAccountRights(IntPtr policyHandle, IntPtr accountSid, PermisionString[] userRights, long countOfRights);
        [DllImport("advapi32.dll")]
        public static extern long LsaClose(IntPtr objectHandle);
        [DllImport("advapi32.dll")]
        public static extern long LsaNtStatusToWinError(long status);
        [DllImport("advapi32.dll")]
        public static extern uint LsaOpenPolicy(ref PermisionString systemName, ref PermissionAttributes objectAttributes, int desiredAccess, out IntPtr policyHandle);

        public struct PermissionAttributes
        {
            public int Length;
            public IntPtr RootDirectory;
            private readonly PermisionString ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        public struct PermisionString
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        /// <summary>
        /// For use with <see cref="LsaOpenPolicy"/>.
        /// </summary>
        /// <remarks>
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/aa374892%28v=vs.85%29.aspx
        /// </remarks>
        public const int DESIRED_ACCESS_MASK = 0x1fff;

        #endregion

        #region For Use During Service Install

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string lpMachineName,
                                                  string lpDatabaseName,
                                                  uint dwDesiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenService(IntPtr hSCManager,
                                                string lpServiceName,
                                                uint dwDesiredAccess);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct QUERY_SERVICE_CONFIG
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
        public static extern bool QueryServiceConfig(IntPtr hService,
                                                     IntPtr lpServiceConfig,
                                                     uint cbBufSize,
                                                     out uint pcbBytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig(IntPtr hService,
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
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        public const uint SERVICE_NO_CHANGE = 0xffffffffu;
        public const uint SC_MANAGER_ALL_ACCESS = 0xf003fu;
        public const uint SERVICE_ALL_ACCESS = 0xf01ffu;

        #endregion

        #region To Support Elevation

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

        public const UInt32 BCM_SETSHIELD = 0x160C;

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        public enum SHSTOCKICONID : uint
        {
            SIID_SHIELD = 77
        }

        [Flags]
        public enum SHGSI : uint
        {
            SHGSI_ICON = 0x000000100,
            SHGSI_SMALLICON = 0x000000001
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            public UInt32 cbSize;
            public IntPtr hIcon;
            public Int32 iSysIconIndex;
            public Int32 iIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSubMenu(IntPtr hMenu, int nPos);
        [DllImport("user32.dll")]
        public static extern IntPtr GetMenuItemID(IntPtr hMenu, int nPos);
        [DllImport("user32.dll")]
        public static extern int SetMenuItemBitmaps(IntPtr hMenu, IntPtr nPosition, int wFlags, IntPtr hBitmapUnchecked, IntPtr hBitmapChecked);

        #endregion
    }
}