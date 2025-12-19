using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSD_Assignment___Banking_Application
{
    public class EncryptionService
    {
        private static EncryptionService instance = new EncryptionService();
        private readonly byte[] _key;
        private readonly byte[] salt = { 232, 17, 89, 142, 87, 66, 96, 192 };
        private EncryptionService() 
        {
            Console.WriteLine("Enter Password: ");
            string password = Console.ReadLine();
            
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);//Create PBKDF2 Object - Do Not Change The Value 10,000; The Purpose Of This Value Will Be Explained In A Later Lecture.
            this._key = pbkdf2.GetBytes(16);//Generate Encryption Key - Same Length As Data To Be Encrypted (In The Case Of A Stream Cipher).

            Console.Clear();
        }

        public static EncryptionService getInstance()
        {
            return instance;
        }

        public byte[] Encrypt(byte[] plaintext_data, Aes aes)
        {
           
            byte[] ciphertext_data;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            MemoryStream msEncrypt = new MemoryStream();

            CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(plaintext_data, 0, plaintext_data.Length);
            csEncrypt.Dispose();

            ciphertext_data = msEncrypt.ToArray();
            msEncrypt.Dispose();

            return ciphertext_data;
        }

        public byte[] Decrypt(byte[] ciphertext_data, Aes aes)
        {
            byte[] plaintext_data;
            ICryptoTransform decryptor = aes.CreateDecryptor();

            using (MemoryStream msDecrypt = new MemoryStream(ciphertext_data))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream output = new MemoryStream())
                    {
                        csDecrypt.CopyTo(output);
                        plaintext_data = output.ToArray();
                    }
                }
            } 

            return plaintext_data;
        }

        public string EncryptString(string plaintext) 
        {
            if (string.IsNullOrEmpty(plaintext))
            {
                return plaintext;
            }

            byte[] plaintext_data = Encoding.UTF8.GetBytes(plaintext);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.Key = _key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();

                byte[] iv = aes.IV;

                byte[] ciphertext_data = Encrypt(plaintext_data, aes);
                
                return Convert.ToBase64String(iv) + ":" + Convert.ToBase64String(ciphertext_data);

            }
        }
        public string DecryptString(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext))
            {
                return ciphertext;
            }
            try
            {
                string[] parts = ciphertext.Split(':');
                if (parts.Length != 2) 
                    throw new FormatException("Invalid ciphertext format.");

                byte[] iv = Convert.FromBase64String(parts[0]);
                byte[] ciphertext_data = Convert.FromBase64String(parts[1]);

                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.Key = _key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    byte[] plaintext_data = Decrypt(ciphertext_data, aes);
                    return Encoding.UTF8.GetString(plaintext_data);
                }
            }
            catch (Exception)
            {
                return "[Decryption Failed]";
            }
            
            
        }

    }
}
