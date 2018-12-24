using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public class Hashing
    {
        public static string GetMD5Hash(byte[] retrievedBuffer)
        {
            // Validate MD5 Value
            var md5Check = System.Security.Cryptography.MD5.Create();
            md5Check.TransformBlock(retrievedBuffer, 0, retrievedBuffer.Length, null, 0);
            md5Check.TransformFinalBlock(new byte[0], 0, 0);

            // Get Hash Value
            byte[] hashBytes = md5Check.Hash;
            string hashVal = Convert.ToBase64String(hashBytes);
            return hashVal;
        }

        public static string GetSHA256Hash(string StringToHash)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            var stringBytes = UTF8Encoding.UTF8.GetBytes(StringToHash);
            sha256.TransformBlock(stringBytes, 0, stringBytes.Length, null, 0); 
            sha256.TransformFinalBlock(new byte[0], 0, 0);

            byte[] hashBytes = sha256.Hash;
            string hashVal = Convert.ToBase64String(hashBytes);
            return hashVal;
        }
    }
}
