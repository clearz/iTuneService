using System;
using System.Runtime.InteropServices;
using System.Text;
using CredentialManagement;
using log4net;

namespace Common
{
    public static class Credentials
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Credentials));

        public static bool PersistCredentials(string username, string password)
        {
            using (var cm =
                new Credential
                {
                    Target = Constants.CredentialName,
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
                    Target = Constants.CredentialName,
                    PersistanceType = PersistanceType.LocalComputer,
                })
            {
                if (!cm.Exists() || !cm.Load()) return null;

                return Tuple.Create<string, string>(cm.Username, cm.Password);
            }
        }

        public static bool RemoveCredential()
        {
            using (var cm =
                new Credential
                {
                    Target = Constants.CredentialName,
                    PersistanceType = PersistanceType.LocalComputer,
                })
            {
                if (!cm.Exists()) return true;

                return cm.Delete();
            }
        }

        /// <summary>
        /// Sets the logon as service privilege for the given user.
        /// </summary>
        public static bool SetLogonAsServicePrivilege(DomainAuthCredentials credentials)
        {
            return (AddAccountRights(credentials, "SeServiceLogonRight") == 0L);
        }

        /// <summary>
        /// Sets the given privilege for the given account.
        /// </summary>
        public static long AddAccountRights(DomainAuthCredentials credentials, string privilegeName)
        {
            var lastError = 0L;
            var sid = IntPtr.Zero;
            var cbsid = 0;
            var domainName = new StringBuilder();
            var cbdomainLength = 0;
            var use = 0;
            var systemName = default(Win32.PermisionString);
            var policyHandle = IntPtr.Zero;
            var permission = default(Win32.PermisionString);

            try
            {
                Win32.LookupAccountName(credentials.Domain, credentials.Username, IntPtr.Zero, ref cbsid, domainName, ref cbdomainLength, ref use);
                domainName = new StringBuilder(cbdomainLength);
                sid = Marshal.AllocHGlobal(cbsid);

                var flag = Win32.LookupAccountName(credentials.Domain, credentials.Username, sid, ref cbsid, domainName, ref cbdomainLength, ref use);
                if (!flag)
                {
                    lastError = Win32.GetLastError();
                    return lastError;
                }

                var objectAttributes = new Win32.PermissionAttributes
                {
                    Length = 0,
                    RootDirectory = IntPtr.Zero,
                    Attributes = 0,
                    SecurityDescriptor = IntPtr.Zero,
                    SecurityQualityOfService = IntPtr.Zero
                };

                lastError = Win32.LsaNtStatusToWinError(Win32.LsaOpenPolicy(ref systemName, ref objectAttributes, Win32.DESIRED_ACCESS_MASK, out policyHandle));
                if (lastError != 0L)
                {
                    _logger.Error("OpenPolicy failed: " + lastError);
                    return lastError;
                }

                permission = new Win32.PermisionString
                {
                    Buffer = Marshal.StringToHGlobalUni(privilegeName),
                    Length = (ushort)(privilegeName.Length * 2),
                    MaximumLength = (ushort)((privilegeName.Length + 1) * 2)
                };

                var userRights = new[] { permission };

                lastError = Win32.LsaNtStatusToWinError(Win32.LsaAddAccountRights(policyHandle, sid, userRights, 1L));
                if (lastError != 0L)
                {
                    _logger.Error("AddAccountRights failed: " + lastError);
                }

                return lastError;
            }
            finally
            {
                if (systemName.Buffer != IntPtr.Zero) Marshal.FreeHGlobal(systemName.Buffer);
                if (policyHandle != IntPtr.Zero) Win32.LsaClose(policyHandle);
                if (permission.Buffer != IntPtr.Zero) Marshal.FreeHGlobal(systemName.Buffer);
                if (sid != IntPtr.Zero) Win32.FreeSid(sid);
            }
        }
    }
}
