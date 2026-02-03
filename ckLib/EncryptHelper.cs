#define ENCRYPT
using System.Security.Cryptography;

/// <summary>
/// Summary description for EncryptHelper.
/// </summary>
namespace ckLib
{
	public class EncryptHelper
	{
		//8 bytes randomly selected for both the Key and the Initialization Vector
		//the IV is used to encrypt the first block of text so that any repetitive 
		//patterns are not apparent
		byte[] KEY_64 = new byte[8]
			{
			71, 72, 97, 99,   2, 68, 123, 94
			};
		byte[] IV_64 = new byte[8]
			{
			12, 67, 123, 78, 45, 23, 87, 2
			};

		public EncryptHelper()
		{
		} // end EncryptHelper
		public string Encrypt(string toEncrypt)
		{
#if ENCRYPT
			if (toEncrypt.Length > 0)
			{
				using var cryptoProvider = DES.Create();
				cryptoProvider.Key = KEY_64;
				cryptoProvider.IV = IV_64;
				using var ms = new MemoryStream();
				using var cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(), CryptoStreamMode.Write);
				using var sw = new StreamWriter(cs);

				sw.Write(toEncrypt);
				sw.Flush();
				cs.FlushFinalBlock();
				ms.Flush();

				//convert back to a string
				return Convert.ToBase64String(ms.GetBuffer(), 0, Convert.ToInt32(ms.Length));
			}
			return "";
#else
		return toEncrypt;
#endif
		} // end Encrypt

		public string Decrypt(string toDecrypt)
		{
#if ENCRYPT
			try
			{
				if (toDecrypt.Length > 0)
				{
					using var cryptoProvider = DES.Create();
					cryptoProvider.Key = KEY_64;
					cryptoProvider.IV = IV_64;

					//convert from string to byte array
					Byte[] buffer = Convert.FromBase64String(toDecrypt);

					using var ms = new MemoryStream(buffer);
					using var cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(), CryptoStreamMode.Read);
					using var sr = new StreamReader(cs);

					return sr.ReadToEnd();
				} // end if
			}
			catch (Exception)
			{
			} // end try
			return "";
#else
		return toDecrypt;
#endif
		} // end Decrypt
	} // end class EncryptHelper
}