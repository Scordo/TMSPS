using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TMSPS.TRC.BL.Common
{
    public static class CryptoHelper
    {
        public const string DEFAULT_SALT = "-=TRC=--=TRC=-";
        public static readonly Encoding DEFAULT_ENCODING = Encoding.UTF8;

        public static byte[] EncryptString(string clearText, string password)
        {
            return EncryptString(clearText, password, DEFAULT_ENCODING);
        }

        public static byte[] EncryptString(string clearText, string password, Encoding encoding)
        {
            return EncryptString(clearText, password, DEFAULT_SALT, encoding);
        }

        public static byte[] EncryptString(string clearText, string password, string salt, Encoding encoding)
        {
            return Encrypt(encoding.GetBytes(clearText), password, salt, encoding);
        }

        public static byte[] Encrypt(byte[] clearData, string password)
        {
            return Encrypt(clearData, password, DEFAULT_ENCODING);
        }

        public static byte[] Encrypt(byte[] clearData, string password, Encoding encoding)
        {
            return Encrypt(clearData, password, DEFAULT_SALT, encoding);
        }

        public static byte[] Encrypt(byte[] clearData, string password, string salt, Encoding encoding)
        {
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, encoding.GetBytes(salt));

            Rijndael algorithm = Rijndael.Create();
            algorithm.Key = bytes.GetBytes(32);
            algorithm.IV = bytes.GetBytes(16);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(clearData, 0, clearData.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public static string DecryptString(byte[] cipherBytes, string password)
        {
            return DecryptString(cipherBytes, password, DEFAULT_ENCODING);
        }

        public static string DecryptString(byte[] cipherBytes, string password, Encoding encoding)
        {
            return DecryptString(cipherBytes, password, DEFAULT_SALT, encoding);
        }

        public static string DecryptString(byte[] cipherBytes, string password, string salt, Encoding encoding)
        {
            return encoding.GetString(Decrypt(cipherBytes, password, salt, encoding));
        }

        public static byte[] Decrypt(byte[] cipherBytes, string password)
        {
            return Decrypt(cipherBytes, password, DEFAULT_ENCODING);
        }

        public static byte[] Decrypt(byte[] cipherBytes, string password, Encoding encoding)
        {
            return Decrypt(cipherBytes, password, DEFAULT_SALT, encoding);
        }

        public static byte[] Decrypt(byte[] cipherBytes, string password, string salt, Encoding encoding)
        {
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, encoding.GetBytes(salt));

            Rijndael algorithm = Rijndael.Create();
            algorithm.Key = bytes.GetBytes(32);
            algorithm.IV = bytes.GetBytes(16);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
                }

                return memoryStream.ToArray();
            }
        }
    }
}