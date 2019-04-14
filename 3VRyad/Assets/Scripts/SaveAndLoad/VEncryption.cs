using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class VEncryption
{
    #region Encryption

    private static string passPhrase = "jfkldsjfkl";        // can be any string
    private static string saltValue = "jfodsjfopsdj";        // can be any string
    private static string hashAlgorithm = "SHA1";             // can be "MD5"
    private static int passwordIterations = 12;                  // can be any number
    private static string initVector = "~32cc3a4e5e6g7h9"; // must be 16 bytes
    private static int keySize = 128;                // can be 192 or 128

    public static string Encrypt(string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(initVector);
        byte[] rgbSalt = Encoding.ASCII.GetBytes(saltValue);
        byte[] buffer = Encoding.UTF8.GetBytes(data);
        byte[] rgbKey = new PasswordDeriveBytes(passPhrase, rgbSalt, hashAlgorithm, passwordIterations).GetBytes(keySize / 8);
        RijndaelManaged managed = new RijndaelManaged();
        managed.Mode = CipherMode.CBC;
        ICryptoTransform transform = managed.CreateEncryptor(rgbKey, bytes);
        MemoryStream stream = new MemoryStream();
        CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
        stream2.Write(buffer, 0, buffer.Length);
        stream2.FlushFinalBlock();
        byte[] inArray = stream.ToArray();
        stream.Close();
        stream2.Close();
        return Convert.ToBase64String(inArray);
    }

    public static string Decrypt(string data)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(initVector);
        byte[] rgbSalt = Encoding.ASCII.GetBytes(saltValue);
        byte[] buffer = Convert.FromBase64String(data);
        byte[] rgbKey = new PasswordDeriveBytes(passPhrase, rgbSalt, hashAlgorithm, passwordIterations).GetBytes(keySize / 8);
        RijndaelManaged managed = new RijndaelManaged();
        managed.Mode = CipherMode.CBC;
        ICryptoTransform transform = managed.CreateDecryptor(rgbKey, bytes);
        MemoryStream stream = new MemoryStream(buffer);
        CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
        byte[] buffer5 = new byte[buffer.Length];
        int count = stream2.Read(buffer5, 0, buffer5.Length);
        stream.Close();
        stream2.Close();
        return Encoding.UTF8.GetString(buffer5, 0, count);
    }
    #endregion
}

