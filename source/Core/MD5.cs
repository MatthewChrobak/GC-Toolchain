using System;
using System.IO;

namespace Core
{
    public static class MD5
    {
        public static string HashFile(string filePath) {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
