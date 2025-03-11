using GigFinder.Models;
using System;
using System.IO;
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

        // Method to encrypt a string using AES
        public static string encriptkey (string plainText)
        {
            try
            {
                string base64Key = "PeWBxdZ5X2Y9z+gc1EdJUJtj4SYL3m3y3zIHAjLZ4aU=";

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Convert.FromBase64String(base64Key); // AES key in byte array
                    aesAlg.GenerateIV(); // Generate a random IV

                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText); // Write the string to be encrypted
                            }

                            byte[] encryptedData = ms.ToArray();
                            byte[] result = new byte[aesAlg.IV.Length + encryptedData.Length];

                            // Concatenate IV and encrypted data
                            Array.Copy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
                            Array.Copy(encryptedData, 0, result, aesAlg.IV.Length, encryptedData.Length);

                            return Convert.ToBase64String(result); // Return the encrypted data with IV as a Base64 string
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Encryption failed.", ex);
            }
        }

        // Method to decrypt a string using AES
        public static string decriptKey(string cipherTextBase64)
        {
            try
            {
                string base64Key = "PeWBxdZ5X2Y9z+gc1EdJUJtj4SYL3m3y3zIHAjLZ4aU=";

                byte[] cipherText = Convert.FromBase64String(cipherTextBase64);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Convert.FromBase64String(base64Key);

                    // Extract the IV from the beginning of the cipherText
                    byte[] iv = new byte[aesAlg.BlockSize / 8];
                    Array.Copy(cipherText, 0, iv, 0, iv.Length);

                    // Extract the encrypted data
                    byte[] encryptedData = new byte[cipherText.Length - iv.Length];
                    Array.Copy(cipherText, iv.Length, encryptedData, 0, encryptedData.Length);

                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv))
                    {
                        using (MemoryStream ms = new MemoryStream(encryptedData))
                        {
                            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd(); // Return the decrypted string
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Decryption failed.", ex);
            }
        }
    }
}