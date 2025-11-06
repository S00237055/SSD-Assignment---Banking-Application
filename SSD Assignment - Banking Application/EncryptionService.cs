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
        private EncryptionService() 
        {
            Console.WriteLine("Enter Encryption Password: ");
            string password = Console.ReadLine();

            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            byte[] salt = new byte[8];
            rng.GetBytes(salt);//Generare 64-Bit/8-Byte Random Value
            Console.WriteLine("SALT Value: [{0}]", string.Join(", ", salt));//Output

            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA512);//Create PBKDF2 Object - Do Not Change The Value 10,000; The Purpose Of This Value Will Be Explained In A Later Lecture.
            byte[] key = pbkdf2.GetBytes(key_length_required);//Generate Encryption Key - Same Length As Data To Be Encrypted (In The Case Of A Stream Cipher).

        }

    }
}
