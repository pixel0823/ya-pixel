using System;
using System.Security.Cryptography;
using System.Text;

public static class HashUtil 
{
    public static string GetSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
                builder.AppendFormat("{0:x2}", b);
            return builder.ToString();
        }
    }
}
