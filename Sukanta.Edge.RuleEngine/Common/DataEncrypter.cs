//*********************************************************************************************
//* File             :   DataEncrypter.cs
//* Author           :   Rout, Sukanta 
//* Date             :   10/1/2024
//* Description      :   Initial version
//* Version          :   1.0
//*-------------------------------------------------------------------------------------------
//* dd-MMM-yyyy	: Version 1.x, Changed By : xxx
//*
//*                 - 1)
//*                 - 2)
//*                 - 3)
//*                 - 4)
//*
//*********************************************************************************************

using System;
using System.IO;
using System.Security.Cryptography;

namespace Sukanta.Edge.RuleEngine.Common
{
    public static class DataEncrypter
    {
        private static byte[] KEY = new byte[] { 0xA4, 0xBA, 0x0C, 0xA0, 0xAE, 0xA8, 0x1F, 0x25, 0x8A, 0x4F, 0xFC, 0x70, 0xEB, 0xA7, 0x52, 0xEB, 0x7B, 0x4B, 0x91, 0x6F, 0xCD, 0xF7, 0x03, 0x06 };
        private static byte[] IV = new byte[] { 0x93, 0x46, 0xF2, 0x74, 0xB0, 0x0C, 0xEB, 0x85, 0x9F, 0x71, 0x05, 0x45, 0x5C, 0x8D, 0x0B, 0x07 };


        /// <summary>
        /// Encrypt using AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;

            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        encrypted = ms.ToArray();
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Decrypt data
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        private static string Decrypt(byte[] cipherText, byte[] Key, byte[] IV)
        {

            string plaintext = null;

            using (AesManaged aes = new AesManaged())
            {

                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);

                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            plaintext = reader.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;

        }

        /// <summary>
        /// Encrypt a plain text
        /// </summary>
        public static string Encrypt(string plainText)
        {
            byte[] data = Encrypt(plainText, KEY, IV);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Decrypt content
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                return Decrypt(data, KEY, IV);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Decryption failed for : '{cipherText}'. {ex.Message}");
            }
        }
    }
}