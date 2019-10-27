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
            int StreamLength, FileLength;
            byte[] encrypted, decrypted;
            using (Stream IO = File.OpenReader(file))
            {
                if (IO.RI64() != 0x454C494641564944) return;

                StreamLength = IO.RI32();
                FileLength = IO.RI32();
                encrypted = IO.RBy(StreamLength);
                decrypted = new byte[StreamLength];
            }

            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(encrypted),
                    crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(decrypted, 0, StreamLength);
            }

            using (Stream IO = File.OpenWriter(file, FileLength))
                IO.W(decrypted, FileLength < StreamLength ? FileLength : StreamLength);
        }

        public static void Encrypt(this string file)
        {
            byte[] In;
            using (Stream IO = File.OpenReader(file))
                In = IO.ToArray();

            int FileLengthOrigin = In.Length;
            int FileLength = FileLengthOrigin.Align(16);
            byte[] Inalign = new byte[FileLength];
            for (int i = 0; i < In.Length; i++) Inalign[i] = In[i];
            In = null;
            byte[] encrypted = new byte[FileLength];
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(Inalign),
                    crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(encrypted, 0, FileLength);
            }

            using (Stream IO = File.OpenWriter(file, Inalign.Length))
            {
                IO.W(0x454C494641564944);
                IO.W(FileLength);
                IO.W(FileLengthOrigin);
                IO.W(encrypted);
            }
        }
    }
}
