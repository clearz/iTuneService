using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace iTuneServiceManager
{
	public static class Encryption
	{
        /// <summary>
        /// Encrypts text using the supplied password using an internal salt.
        /// </summary>
		public static string Encrypt(string clearText, string password)
		{
            var salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

			var clearBytes = Encoding.Unicode.GetBytes(clearText);
		    var pdb = new Rfc2898DeriveBytes(password, salt);
			var encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));

			return Convert.ToBase64String(encryptedData);
		}

		public static byte[] Encrypt(byte[] clearData, byte[] key, byte[] iv)
		{
		    using (var ms = new MemoryStream())
		    using (var alg = Rijndael.Create())
            {
			    alg.Key = key;
			    alg.IV = iv;

                using (var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write))
			    {
			        cs.Write(clearData, 0, clearData.Length);
			    }
			
			    var encryptedData = ms.ToArray();

			    return encryptedData;
			}
		}
	}
}