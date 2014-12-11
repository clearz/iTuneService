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
        private static extern long AddAccountRights(IntPtr policyHandle, IntPtr accountSid, PermisionString[] userRights, long countOfRights);
        [DllImport("advapi32.dll")]
        private static extern long Close(IntPtr objectHandle);
        [DllImport("advapi32.dll")]
        private static extern long LsaNtStatusToWinError(long status);
        [DllImport("advapi32.dll")]
        private static extern uint LsaOpenPolicy(ref PermisionString systemName, ref PermissionAttributes objectAttributes, int desiredAccess, out IntPtr policyHandle);
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

        private const int DESIRED_ACCESS = 0x1fff;

        public static long SetRight(string accountName, string privilegeName)
        {
            var lastError = 0L;
            var sid = IntPtr.Zero;
            var cbsid = 0;
            var domainName = new StringBuilder();
            var cbdomainLength = 0;
            var use = 0;
	        var systemName = default (PermisionString);
		    var policyHandle = IntPtr.Zero;
            var permission = default(PermisionString);

		    try
		    {
		        LookupAccountName(string.Empty, accountName, IntPtr.Zero, ref cbsid, domainName, ref cbdomainLength, ref use);
		        domainName = new StringBuilder(cbdomainLength);
		        sid = Marshal.AllocHGlobal(cbsid);

		        var flag = LookupAccountName(string.Empty, accountName, sid, ref cbsid, domainName, ref cbdomainLength, ref use);
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

		        lastError = LsaNtStatusToWinError(LsaOpenPolicy(ref systemName, ref objectAttributes, DESIRED_ACCESS, out policyHandle));                
		        if (lastError != 0L)
		        {
		            Console.WriteLine("OpenPolicy failed: " + lastError);
		            return lastError;
		        }
		        
		        permission = new PermisionString
		        {
		            Buffer = Marshal.StringToHGlobalUni(privilegeName),
		            Length = (ushort)(privilegeName.Length * 2),
		            MaximumLength = (ushort)((privilegeName.Length + 1) * 2)
		        };

		        var userRights = new[] {permission};

		        lastError = LsaNtStatusToWinError(AddAccountRights(policyHandle, sid, userRights, 1L));
		        if (lastError != 0L)
		        {
		            Console.WriteLine("AddAccountRights failed: " + lastError);
		        }

		        return lastError;
		    }
		    finally
		    {
		        if (systemName.Buffer != IntPtr.Zero) Marshal.FreeHGlobal(systemName.Buffer);
		        if (policyHandle != IntPtr.Zero) Close(policyHandle);
		        if (permission.Buffer != IntPtr.Zero) Marshal.FreeHGlobal(systemName.Buffer);
		        if (sid != IntPtr.Zero) FreeSid(sid);
		    }
        }
    }
}
