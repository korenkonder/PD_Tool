//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System.IO.Compression;
using System.Security.Cryptography;
using KKdBaseLib;
using KKdMainLib.IO;
using MSIO = System.IO;

namespace KKdMainLib
{
    public class FARC : System.IDisposable
    {
        public FARC() => NewFARC();
        public FARC(string file, bool isDirectory = false)
        { if (isDirectory) DirectoryPath = file; else FilePath = file; NewFARC(); }

        private void NewFARC() { Files = KKdList<FARCFile>.New; Signature = Farc.FArC; FT = false; }

        public KKdList<FARCFile> Files = KKdList<FARCFile>.New;
        public Flags FARCFlags;
        public Farc Signature = Farc.FArC;
        public bool FT = false;
        public string FilePath, DirectoryPath;
        public bool HasFiles => !Files.IsNull || Files.Count > 0;
        public int CompressionLevel = 12;

        private readonly byte[] key = "project_diva.bin".ToASCII();

        private readonly byte[] keyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        private AesManaged GetAes(byte[] iv = null) =>
            new AesManaged () { KeySize = 128, Key = iv != null ? keyFT : key,
                BlockSize = 128, Mode = iv != null ? CipherMode.CBC : CipherMode.ECB,
                Padding = iv != null ? PaddingMode.PKCS7 : PaddingMode.Zeros, IV = iv ?? new byte[16] };

        public void Unpack(bool saveToDisk = true)
        { if (HeaderReader()) FileReader(saveToDisk);  }

        public bool HeaderReader()
        {
            NewFARC();
            if (!File.Exists(FilePath)) return false;

            Stream reader = File.OpenReader(FilePath);
            DirectoryPath = Path.RemoveExtension(Path.GetFullPath(FilePath));
            Signature = (Farc)reader.RI32E(true);
            if (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)
            { reader.Dispose(); return false; }

            FT = false;

            int headerLength = reader.RI32E(true);
            if (Signature == Farc.FARC)
            {
                FARCFlags = (Flags)reader.RI32E(true);
                reader.RI32();
                int alignment = reader.RI32E(true);

                FT = (FARCFlags & Flags.AES) != 0 && (alignment & (alignment - 1)) != 0;

                if (FT)
                {
                    headerLength -= 0x08;
                    reader.S(-0x04, SeekOrigin.Current);
                }
                else
                    headerLength -= 0x0C;
            }

            Files.Capacity = 0;

            if (FT)
            {
                headerLength -= 0x10;

                byte[] iv = reader.RBy(0x10);
                byte[] header = reader.RBy(headerLength);
                reader.Dispose();

                using (MSIO.MemoryStream ms = new MSIO.MemoryStream())
                {
                    using (AesManaged aes = GetAes(iv))
                    using (CryptoStream cryptoStream = new CryptoStream(ms,
                        aes.CreateDecryptor(), CryptoStreamMode.Write))
                        cryptoStream.Write(header, 0x00, headerLength);
                    header = ms.ToArray();
                    headerLength = header.Length;
                }

                reader = File.OpenReader(header);
                int alignment = reader.RI32E(true);
            }
            else
            {
                byte[] header = reader.RBy(headerLength);
                reader.Dispose();
                reader = File.OpenReader(header);
            }

            bool hasPerFileFlags = false;
            if (Signature == Farc.FARC) {
                if (reader.RI32E(true) == 1) {
                    Files.Capacity = reader.RI32E(true);
                    hasPerFileFlags = true;
                }
            }
            reader.RI32E(true);

            if (Files.Capacity == 0)
            {
                int Count = 0;
                long Position = reader.PI64;
                int Size = 0;
                if (hasPerFileFlags)
                    Size = sizeof(int) * 4;
                else if (Signature != Farc.FArc)
                    Size = sizeof(int) * 3;
                else
                    Size = sizeof(int) * 2;

                while (reader.PI64 < headerLength)
                {
                    reader.NT();
                    reader.RBy(Size);
                    Count++;
                }
                reader.PI64 = Position;
                Files.Capacity = Count;
            }

            if (hasPerFileFlags)
                for (int i = 0; i < Files.Capacity; i++)
                {
                    FARCFile file = default;
                    file.Name = reader.NTUTF8();
                    file.Offset = reader.RI32E(true);
                    file.SizeComp = reader.RI32E(true);
                    file.Size = reader.RI32E(true);
                    Flags Flags = (Flags)reader.RI32E(true);

                    file.Compressed = file.Size != 0 || (FARCFlags & Flags.GZip) != 0;
                    file.Encrypted = ((FARCFlags | Flags) & Flags.AES) != 0;
                    Files.Add(file);
                }
            else if (Signature != Farc.FArc)
                for (int i = 0; i < Files.Capacity; i++)
                {
                    FARCFile file = default;
                    file.Name = reader.NTUTF8();
                    file.Offset = reader.RI32E(true);
                    file.SizeComp = reader.RI32E(true);
                    file.Size = reader.RI32E(true);

                    file.Compressed = file.Size != 0;
                    file.Encrypted = (FARCFlags & Flags.AES) != 0;
                    Files.Add(file);
                }
            else
                for (int i = 0; i < Files.Capacity; i++)
                {
                    FARCFile file = default;
                    file.Name = reader.NTUTF8();
                    file.Offset = reader.RI32E(true);
                    file.Size = reader.RI32E(true);

                    file.SizeComp = 0;
                    file.Compressed = false;
                    file.Encrypted = false;
                    Files.Add(file);
                }

            reader.Dispose();
            return true;
        }
        
        public bool Exists(string file)
        {
            if (!HasFiles) return false;
            for (int i = 0; i < Files.Count; i++)
                if (Files[i].Name.ToLower() == file.ToLower()) return true;
            return false;
        }

        private void FileReader(bool saveToDisk)
        {
            if (saveToDisk)
                MSIO.Directory.CreateDirectory(DirectoryPath);

            Stream reader = File.OpenReader(FilePath);
            for (int i = 0; i < Files.Count; i++)
                FileReader(i, saveToDisk, ref reader);
            reader.Dispose();
        }

        public byte[] FileReader(string file)
        {
            if (!HasFiles) return null;
            for (int i = 0; i < Files.Count; i++)
                if (Files[i].Name.ToLower() == file.ToLower())
                    return FileReader(i);
            return null;
        }

        public byte[] FileReader(int i)
        {
            Stream reader = File.OpenReader(FilePath);
            byte[] data = FileReader(i, false, ref reader);
            reader.Dispose();
            return data;
        }

        public byte[] FileReader(int i, bool saveToDisk, ref Stream reader)
        {
            if (!HasFiles) return null;
            if (i >= Files.Count) return null;

            FARCFile file = Files[i];
            file.Data = null;

            reader.S(file.Offset, SeekOrigin.Begin);

            if (Signature == Farc.FArc)
            {
                file.DataComp = null;
                file.Data = reader.RBy(file.Size);
            }
            else if (Signature == Farc.FArC)
            {
                file.DataComp = reader.RBy(file.SizeComp);
                file.Data = file.DataComp.InflateGZip(file.Size);
            }
            else if (file.Encrypted || file.Compressed)
            {
                int tempSize = file.Encrypted ? file.SizeComp.A(0x10) : file.SizeComp;

                byte[] temp;
                if (file.Encrypted)
                    if (FT)
                    {
                        file.SizeComp -= 0x10;
                        tempSize -= 0x10;

                        byte[] iv = reader.RBy(0x10);
                        using (MSIO.MemoryStream ms = new MSIO.MemoryStream())
                        {
                            using (AesManaged aes = GetAes(iv))
                            using (CryptoStream cryptoStream = new CryptoStream(ms,
                                aes.CreateDecryptor(), CryptoStreamMode.Write))
                                cryptoStream.Write(reader.RBy(tempSize), 0x00, tempSize);
                            temp = ms.ToArray();
                            file.SizeComp = temp.Length;
                        }
                    }
                    else
                    {
                        using (MSIO.MemoryStream ms = new MSIO.MemoryStream())
                        {
                            using (AesManaged aes = GetAes())
                            using (CryptoStream cryptoStream = new CryptoStream(ms,
                                aes.CreateDecryptor(), CryptoStreamMode.Write))
                                cryptoStream.Write(reader.RBy(tempSize), 0x00, tempSize);
                            temp = ms.ToArray();
                        }
                    }
                else
                    temp = reader.RBy(tempSize);

                if (!file.Compressed)
                {
                    file.DataComp = null;
                    file.Data = temp;
                }
                else
                {
                    file.DataComp = temp;
                    file.Data = file.DataComp.InflateGZip(file.Size);
                }
            }
            else
            {
                file.DataComp = null;
                file.Data = reader.RBy(file.Size);
            }

            if (saveToDisk)
            {
                File.WriteAllBytes(Path.Combine(DirectoryPath, file.Name), file.Data);
                file.DataComp = null;
                file.Data = null;
            }

            Files[i] = file;
            return Files[i].Data;
        }

        public void Pack(Farc signature = Farc.FArC)
        {
            NewFARC();
            string[] files = Directory.GetFiles(DirectoryPath);
            Files.Capacity = files.Length;
            for (int i = 0; i < files.Length; i++)
                Files.Add(new FARCFile { Name = Path.GetFileName(files[i]), Data = File.ReadAllBytes(files[i]) });
            files = null;
            Signature = signature;
            Save();
        }

        public void Save()
        {
            if (!HasFiles || (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)) return;

            for (int i = 0; i < Files.Count; i++)
            {
                string ext = Path.GetExtension(Files[i].Name).ToLower();
                if (ext == ".a3da" || ext == ".diva" || ext == ".farc" || ext == ".vag")
                { Signature = Farc.FArc; break; }
            }

            Stream writer = File.OpenWriter(DirectoryPath + ".farc", true);
            writer.WE((int)Signature, true);

            using (Stream headerWriter = File.OpenWriter())
            {
                     if (Signature == Farc.FArc) headerWriter.WE(0x01, true);
                else if (Signature == Farc.FArC) headerWriter.WE(0x01, true);
                else if (Signature == Farc.FARC)
                {
                    headerWriter.WE((int)FARCFlags, true);
                    headerWriter.W (0x00);
                    headerWriter.WE(0x40, true);
                    headerWriter.W (0x00);
                    headerWriter.W (0x00);
                }
                if (Signature != Farc.FArc)
                    for (int i = 0; i < Files.Count; i++)
                    { headerWriter.W(Files[i].Name + "\0"); headerWriter.W(0x00L); headerWriter.W(0x00); }
                else
                    for (int i = 0; i < Files.Count; i++)
                    { headerWriter.W(Files[i].Name + "\0"); headerWriter.W(0x00L); }
                writer.WE(headerWriter.L, true);
                writer.W (headerWriter.ToArray(true));
            }

            int align = writer.P.A(0x10) - writer.P;
            for (int i1 = 0; i1 < align; i1++)
                writer.W((byte)(Signature == Farc.FArc ? 0x00 : 0x78));

            writer.F();
            for (int i = 0; i < Files.Count; i++)
                CompressStuff(i, ref writer);

            writer.P = Signature == Farc.FARC ? 0x1C : 0x0C;
            for (int i = 0; i < Files.Count; i++)
            {
                FARCFile file = Files[i];
                writer.W (file.Name + "\0");
                writer.WE(file.Offset, true);
                if (Signature != Farc.FArc) writer.WE(file.SizeComp, true);
                writer.WE(file.Size, true);
            }
            writer.Dispose();
        }

        private void CompressStuff(int i, ref Stream writer)
        {
            FARCFile file = Files[i];
            file.Offset = writer.P;
            file.Size = file.Data.Length;

            byte[] data = file.Data;
            if (Signature == Farc.FArC || (Signature == Farc.FARC && (FARCFlags & Flags.GZip) != 0))
                data = file.Data.DeflateGZip(CompressionLevel);
            file.SizeComp = data.Length;

            if (Signature == Farc.FARC && (FARCFlags & Flags.AES) != 0)
            {
                int alignLength = data.Length.A(0x40);
                byte[] tempData = new byte[alignLength];
                System.Array.Copy(data, tempData, data.Length);
                for (int i1 = file.Data.Length; i1 < alignLength; i1++) tempData[i1] = 0x78;
                data = Encrypt(tempData);
            }
            writer.W(data);

            if (Signature != Farc.FARC)
            {
                int Align = writer.P.A(0x10) - writer.P;
                for (int i1 = 0; i1 < Align; i1++)
                    writer.W((byte)(Signature == Farc.FArc ? 0x00 : 0x78));
            }
            Files[i] = file;
        }

        private byte[] Encrypt(byte[] data)
        {
            MSIO.MemoryStream ms = new MSIO.MemoryStream();
            using (AesManaged aes = GetAes())
            using (CryptoStream cryptoStream = new CryptoStream(ms,
                aes.CreateDecryptor(), CryptoStreamMode.Write))
                cryptoStream.Write(data, 0x00, data.Length);
            return ms.ToArray();
        }

        public void Dispose() => NewFARC();

        public struct FARCFile
        {
            public string Name;
            public int Offset;
            public int Size;
            public int SizeComp;
            public byte[] Data;
            public byte[] DataComp;
            public bool Compressed;
            public bool Encrypted;

            public override string ToString() => Name;
        }

        public enum Farc : int
        {
            FArc = 0x46417263,
            FArC = 0x46417243,
            FARC = 0x46415243,
        }

        public enum Flags : int
        {
            None = 0b000,
            GZip = 0b010,
            AES  = 0b100,
        }
    }
}
