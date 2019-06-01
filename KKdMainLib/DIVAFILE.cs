using System;
using System.Security.Cryptography;
using KKdMainLib.IO;
using MSIO = System.IO;

namespace KKdMainLib
{
    public static class DIVAFILE
    {
        private static readonly byte[] Key = "file access deny".ToASCII();

        public static void Decrypt(this string file)
        {
            Console.Title = "DIVAFILE Decrypt - File: " + Path.GetFileName(file);
            Stream reader = File.OpenReader(file);
            if (reader.ReadInt64() != 0x454C494641564944)
            { reader.Close(); return; }
            int StreamLenght = reader.ReadInt32();
            int FileLenght = reader.ReadInt32();
            byte[] decrypted = new byte[StreamLenght];
            reader.Seek(0, 0);
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using (CryptoStream cryptoData = new CryptoStream(reader.BaseStream,
                    crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                    cryptoData.Read(decrypted, 0, StreamLenght);
            }
            Stream writer = File.OpenWriter(file, FileLenght);
            for (int i = 0x10; i < StreamLenght && i < FileLenght + 0x10; i++)
                writer.Write(decrypted[i]);
            writer.Close();
        }

        public static void Encrypt(this string file)
        {
            Console.Title = "DIVAFILE Encrypt - File: " + Path.GetFileName(file);
            Stream reader = File.OpenReader(file);
            int FileLenghtOrigin = reader.Length;
            int FileLenght = FileLenghtOrigin.Align(16);
            reader.Close();
            byte[] In = File.OpenReader(file).ToArray(true);
            byte[] Inalign = new byte[FileLenght];
            for (int i = 0; i < In.Length; i++) Inalign[i] = In[i];
            In = null;
            byte[] encrypted = new byte[FileLenght];
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using (CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(Inalign),
                    crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                    cryptoData.Read(encrypted, 0, FileLenght);
            }
            Stream writer = File.OpenWriter(file, Inalign.Length);
            writer.Write(0x454C494641564944);
            writer.Write(FileLenght);
            writer.Write(FileLenghtOrigin);
            writer.Write(encrypted);
            writer.Close();
        }
    }
}
