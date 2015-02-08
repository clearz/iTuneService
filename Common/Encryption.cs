using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
	public static class Encryption
	{
        private static byte[] _salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

        /// <summary>
        /// Encrypts text using the supplied password using an internal salt.
        /// </summary>
		public static string Encrypt(string clearText, string password)
		{
			var clearBytes = Encoding.Unicode.GetBytes(clearText);
		    var pdb = new Rfc2898DeriveBytes(password, _salt);
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

        public static string Decrypt(string cipherText, string password)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            var pdb = new Rfc2898DeriveBytes(password, _salt);
            var decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            return System.Text.Encoding.Unicode.GetString(decryptedData);
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
	}
}