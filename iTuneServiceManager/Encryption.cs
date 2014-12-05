using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace iTuneServiceManager
{
	public static class Encryption
	{
		public static string Encrypt(string clearText, string password)
		{
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
			var pdb = new Rfc2898DeriveBytes(password, new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
			byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), iv: pdb.GetBytes(16));
			return Convert.ToBase64String(encryptedData);

		}

		public static byte[] Encrypt(byte[] clearData, byte[] key, byte[] iv)
		{
			var ms = new MemoryStream();

			var alg = Rijndael.Create();
			alg.Key = key;
			alg.IV = iv;
			var cs = new CryptoStream(ms,alg.CreateEncryptor(), CryptoStreamMode.Write);
			cs.Write(clearData, 0, clearData.Length);
			cs.Close();
			byte[] encryptedData = ms.ToArray();

			return encryptedData;
		}
	}
}