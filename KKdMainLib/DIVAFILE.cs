using System;
using System.Security.Cryptography;
using KKdBaseLib;
using KKdMainLib.IO;
using MSIO = System.IO;

namespace KKdMainLib
{
    public static class DIVAFILE
    {
        private static readonly byte[] Key = "file access deny".ToASCII();

        public static void Decrypt(string file)
        {
            int streamLength, fileLength;
            byte[] encrypted, decrypted;
            using (Stream _IO = File.OpenReader(file))
            {
                if (_IO.RU64() != 0x454C494641564944u) return;

                streamLength = _IO.RI32();
                fileLength = _IO.RI32();
                encrypted = _IO.RBy(streamLength);
                decrypted = new byte[streamLength];
            }

            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(encrypted),
                    crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(decrypted, 0, streamLength);
            }

            using (Stream _IO = File.OpenWriter(file, fileLength))
                _IO.W(decrypted, fileLength < streamLength ? fileLength : streamLength);
        }

        public static void Encrypt(string file)
        {
            byte[] data;
            using (Stream _IO = File.OpenReader(file))
                data = _IO.ToArray();

            int fileLengthOrigin = data.Length;
            int fileLength = fileLengthOrigin.A(16);
            byte[] dataAlign = new byte[fileLength];
            Array.Copy(data, dataAlign, data.Length);
            data = null;
            byte[] encrypted = new byte[fileLength];
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(dataAlign),
                    crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(encrypted, 0, fileLength);
            }

            using (Stream _IO = File.OpenWriter(file, dataAlign.Length))
            {
                _IO.W(0x454C494641564944u);
                _IO.W(fileLength);
                _IO.W(fileLengthOrigin);
                _IO.W(encrypted);
            }
        }

        public static byte[] Decrypt(byte[] data)
        {
            int streamLength, fileLength;
            byte[] encrypted, decrypted;
            using (Stream _IO = File.OpenReader(data))
            {
                if (_IO.RU64() != 0x454C494641564944u) return data;

                streamLength = _IO.RI32();
                fileLength = _IO.RI32();
                encrypted = _IO.RBy(streamLength);
                decrypted = new byte[streamLength];
            }

            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(encrypted),
                    crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(decrypted, 0, streamLength);
            }

            data = new byte[fileLength];
            Array.Copy(decrypted, data, fileLength < streamLength ? fileLength : streamLength);
            return data;
        }

        public static byte[] Encrypt(byte[] data)
        {
            int fileLengthOrigin = data.Length;
            int fileLength = fileLengthOrigin.A(16);
            byte[] dataAlign = new byte[fileLength];
            Array.Copy(data, dataAlign, data.Length);
            data = null;
            byte[] encrypted = new byte[fileLength];
            using (AesManaged crypto = new AesManaged())
            {
                crypto.Key = Key; crypto.IV = new byte[16];
                crypto.Mode = CipherMode.ECB; crypto.Padding = PaddingMode.Zeros;
                using CryptoStream cryptoData = new CryptoStream(new MSIO.MemoryStream(dataAlign),
                    crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read);
                cryptoData.Read(encrypted, 0, fileLength);
            }

            using (Stream _IO = File.OpenWriter())
            {
                _IO.W(0x454C494641564944u);
                _IO.W(fileLength);
                _IO.W(fileLengthOrigin);
                _IO.W(encrypted);
                data = _IO.ToArray();
            }
            return data;
        }
    }
}
