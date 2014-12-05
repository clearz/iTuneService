using System;
using System.Runtime.InteropServices;
using System.Text;

namespace iTuneServiceManager
{
    public class Permission
    {
		#region DLLImports
        [DllImport("advapi32")]
        public static extern void FreeSid(IntPtr pSid);
        [DllImport("kernel32.dll")]
        private static extern int GetLastError();
        [DllImport("advapi32.dll")]
        private static extern bool IsValidSid(IntPtr pSid);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr psid, ref int cbsid, StringBuilder domainName, ref int cbdomainLength, ref int use);
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern long AddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, PermisionString[] UserRights, long CountOfRights);
        [DllImport("advapi32.dll")]
        private static extern long Close(IntPtr ObjectHandle);
        [DllImport("advapi32.dll")]
        private static extern long NTStatusToWinError(long status);
        [DllImport("advapi32.dll")]
        private static extern uint OpenPolicy(ref PermisionString SystemName, ref PermissionAttributes ObjectAttributes, int DesiredAccess, out IntPtr PolicyHandle);
		#endregion

	    #region Fields

	    private static PermisionString _systemName;
	    private const int DESIRED_ACCESS = 0x1fff;
		private static IntPtr _policyHandle = IntPtr.Zero;

	    #endregion

		#region Structs
		private struct PermissionAttributes
		{
			public int Length;
			public IntPtr RootDirectory;
			private readonly PermisionString ObjectName;
			public uint Attributes;
			public IntPtr SecurityDescriptor;
			public IntPtr SecurityQualityOfService;
		}

		private struct PermisionString
		{
			public ushort Length;
			public ushort MaximumLength;
			public IntPtr Buffer;
		}
		#endregion

		public static long SetRight(string accountName, string privilegeName)
        {
            long lastError = 0L;
            IntPtr zero = IntPtr.Zero;
            int cbsid = 0;
            var domainName = new StringBuilder();
            int cbdomainLength = 0;
            int use = 0;
            LookupAccountName(string.Empty, accountName, zero, ref cbsid, domainName, ref cbdomainLength, ref use);
            domainName = new StringBuilder(cbdomainLength);
            zero = Marshal.AllocHGlobal(cbsid);
            bool flag = LookupAccountName(string.Empty, accountName, zero, ref cbsid, domainName, ref cbdomainLength, ref use);
            if (!flag)
            {
                lastError = GetLastError();
                return lastError;
            }
            var objectAttributes = new PermissionAttributes
            {
                Length = 0,
                RootDirectory = IntPtr.Zero,
                Attributes = 0,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero
            };
            lastError = NTStatusToWinError((long)OpenPolicy(ref _systemName, ref objectAttributes, DESIRED_ACCESS, out _policyHandle));
            if (lastError != 0L)
            {
                Console.WriteLine("OpenPolicy failed: " + lastError);
            }
            else
            {
                var userRights = new [] { new PermisionString() };
                userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                userRights[0].Length = (ushort)(privilegeName.Length * 2);
                userRights[0].MaximumLength = (ushort)((privilegeName.Length + 1) * 2);
                lastError = NTStatusToWinError(AddAccountRights(_policyHandle, zero, userRights, 1L));
                if (lastError != 0L)
                {
                    Console.WriteLine("AddAccountRights failed: " + lastError);
                }
                Close(_policyHandle);
            }
            FreeSid(zero);
            return lastError;
        }


    }


}
