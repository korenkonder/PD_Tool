using System;
using System.Security.Cryptography;
using KKdBaseLib;
using KKdMainLib.IO;
using MSIO = System.IO;

namespace KKdMainLib
{
    public static class DIVAFILE
    {
        private static readonly byte[] Key = new byte[]
        { 0x66, 0x69, 0x6C, 0x65, 0x20, 0x61, 0x63, 0x63,
          0x65, 0x73, 0x73, 0x20, 0x64, 0x65, 0x6E, 0x79 };

        public static void Decrypt(this string file)
        {
            Console.Title = "DIVAFILE Decrypt - File: " + Path.GetFileName(file);
            Stream IO = File.OpenReader(file);
            if (IO.ReadInt64() != 0x454C494641564944)
            { IO.Close(); return; }
            int StreamLength = IO.ReadInt32();
            int FileLength = IO.ReadInt32();
            byte[] encrypted = IO.ReadBytes(StreamLength);
            byte[] decrypted = new byte[StreamLength];
            IO.Close();

            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using (CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(encrypted),
                    crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                    cryptoData.Read(decrypted, 0, StreamLength);
            }
            IO = File.OpenWriter(file, FileLength);
            IO.Write(decrypted, FileLength < StreamLength ? FileLength : StreamLength);
            IO.Close();
        }

        public static void Encrypt(this string file)
        {
            Console.Title = "DIVAFILE Encrypt - File: " + Path.GetFileName(file);
            Stream IO = File.OpenReader(file);
            int FileLengthOrigin = IO.Length;
            int FileLength = FileLengthOrigin.Align(16);
            IO.Close();
            byte[] In = File.OpenReader(file).ToArray(true);
            byte[] Inalign = new byte[FileLength];
            for (int i = 0; i < In.Length; i++) Inalign[i] = In[i];
            In = null;
            byte[] encrypted = new byte[FileLength];
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using (CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(Inalign),
                    crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                    cryptoData.Read(encrypted, 0, FileLength);
            }
            IO = File.OpenWriter(file, Inalign.Length);
            IO.Write(0x454C494641564944);
            IO.Write(FileLength);
            IO.Write(FileLengthOrigin);
            IO.Write(encrypted);
            IO.Close();
        }
    }
}
