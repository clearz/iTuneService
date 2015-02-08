using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security;

namespace Common
{
    /// <summary>
    /// Generic class to parse domain from username and store password.
    /// </summary>
    public class DomainAuthCredentials
    {
        public string Domain { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public DomainAuthCredentials(string domainUser, string password)
        {
            Username = domainUser;
            Password = password;
            Domain = Environment.MachineName;

            if (domainUser.Contains(@"\"))
            {
                var strArray = domainUser.Split(new[] { '\\' }, 2);
                if (strArray[0] != ".") Domain = strArray[0];
                Username = strArray[1];
            }
        }

        public bool UserEquals(string other)
        {
            return UserEquals(new DomainAuthCredentials(other, null));
        }
        public bool UserEquals(DomainAuthCredentials other)
        {
            var domain1 = Domain == "." ? Environment.MachineName : Domain;
            var domain2 = other.Domain == "." ? Environment.MachineName : other.Domain;
            return (domain1.Equals(domain2, StringComparison.CurrentCultureIgnoreCase) &&
                    Username.Equals(other.Username, StringComparison.CurrentCultureIgnoreCase));
        }

        public string ToFullUsername()
        {
            return String.Format(@"{0}\{1}", Domain, Username);
        }

        public SecureString GetPasswordAsSecureString()
        {
            var password = new SecureString();
            foreach (var c in Password)
            {
                password.AppendChar(c);
            }
            return password;
        }

        public override string ToString()
        {
            return ToFullUsername();
        }

        public static List<DomainAuthCredentials> GetLocalUsers()
        {
            var query = new SelectQuery("Win32_UserAccount");
            using (var searcher = new ManagementObjectSearcher(query))
            {
                return searcher.Get()
                               .Cast<ManagementObject>()
                               .Select(envVar => new DomainAuthCredentials(Environment.MachineName + "\\" + envVar["Name"], null))
                               .ToList();
            }
        }
    }

}