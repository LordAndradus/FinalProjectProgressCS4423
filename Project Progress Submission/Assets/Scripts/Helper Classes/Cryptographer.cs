using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Cryptographer
{
    private static readonly int keySize = 32; //256 bits
    private static readonly int ivSize = 16; //128 bits

    public static string KeyPartTwo = "Mary Jane";

    public static string Encrypt(string data, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = keySize * 8;
            aes.BlockSize = ivSize * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = GenerateKey(key);
            aes.IV = GenerateIV(key);

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                cs.Write(dataBytes, 0, dataBytes.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public static string Decrypt(string cipherData, string key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = keySize * 8;
            aes.BlockSize = ivSize * 8;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.Key = GenerateKey(key);
            aes.IV = GenerateIV(key);

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(Convert.FromBase64String(cipherData)))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd(); 
            }
        }
    }

    private static byte[] GenerateKey(string key)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }

    private static byte[] GenerateIV(string key)
    {
        using (MD5 md5 = MD5.Create())
        {
            return md5.ComputeHash(Encoding.UTF8.GetBytes(key));
        }
    }
}