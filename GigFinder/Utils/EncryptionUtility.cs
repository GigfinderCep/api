using GigFinder.Models;
using System;
using System.Security.Cryptography;

namespace GigFinder.Utils
{
    public class EncryptionUtility
    {
        public static string GenerateAesKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 256; // 256 bits (32 bytes)
                aesAlg.GenerateKey();
                return Convert.ToBase64String(aesAlg.Key);
            }
        }
    }
}