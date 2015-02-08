using System;
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
        public static extern long AddAccountRights(IntPtr policyHandle, IntPtr accountSid, PermisionString[] userRights, long countOfRights);
        [DllImport("advapi32.dll")]
        public static extern long Close(IntPtr objectHandle);
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
    }
}