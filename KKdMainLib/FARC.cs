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

        private void NewFARC() { Files = KKdList<FARCFile>.New; Signature = Farc.FArC; Format = Format.DT; }

        public KKdList<FARCFile> Files = KKdList<FARCFile>.New;
        public Type FARCType;
        public Farc Signature = Farc.FArC;
        public Format Format = Format.DT;
        public string FilePath, DirectoryPath;
        public bool HasFiles => !Files.IsNull || Files.Count > 0;
        public int CompressionLevel = 12;

        private readonly byte[] key = "project_diva.bin".ToASCII();

        private readonly byte[] keyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        private AesManaged GetAes(bool isFT, byte[] iv) =>
            new AesManaged () { KeySize = 128, Key = isFT ? keyFT : key,
                BlockSize = 128, Mode = isFT ? CipherMode.CBC : CipherMode.ECB,
                Padding = PaddingMode.Zeros, IV = iv ?? new byte[16] };

        public void Unpack(bool saveToDisk = true)
        { if (HeaderReader()) { FileReader(); if (saveToDisk) SaveToDisk(); } }

        public bool HeaderReader()
        {
            NewFARC();
            if (!File.Exists(FilePath)) return false;

            Stream reader = File.OpenReader(FilePath);
            DirectoryPath = Path.RemoveExtension(Path.GetFullPath(FilePath));
            Signature = (Farc)reader.RI32E(true);
            if (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)
            { reader.Dispose(); return false; }

            int headerLength = reader.RI32E(true);
            if (Signature == Farc.FARC)
            {
                FARCType = (Type)reader.RI32E(true);
                reader.RI32();
                int farcMode = reader.RI32E(true);

                Format = (FARCType & Type.Enc) != 0 && (farcMode & (farcMode - 1)) != 0 ? Format.FT : Format.DT;

                if (Format == Format.FT && (FARCType & Type.Enc) != 0)
                {
                    reader.Dispose();
                    byte[] header = new byte[headerLength - 0x08];
                    MSIO.FileStream stream = new MSIO.FileStream(FilePath, MSIO.FileMode.Open,
                       MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite) { Position = 0x10 };

                    using (AesManaged aes = GetAes(true, null))
                    using (CryptoStream cryptoStream = new CryptoStream(stream,
                        aes.CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(header, 0x00, headerLength - 0x08);
                    header = SkipData(header, 0x10);
                    reader = File.OpenReader(header);

                    farcMode = reader.RI32E(true);
                }
            }

            if (Signature == Farc.FARC)
                if (reader.RI32E(true) == 1)
                    Files.Capacity = reader.RI32E(true);
            reader.RI32();

            if (Files.Capacity == 0)
            {
                int Count = 0;
                long Position = reader.PI64;
                while (reader.PI64 < headerLength)
                {
                    reader.NT();
                    reader.RI32();
                    if (Signature != Farc.FArc) reader.RI32();
                    reader.RI32();
                    if (Signature == Farc.FARC && Format == Format.FT) reader.RI32();
                    Count++;
                }
                reader.PI64 = Position;
                Files.Capacity = Count;
            }

            for (int i = 0; i < Files.Capacity; i++)
            {
                FARCFile file = default;
                file.Name = reader.NTUTF8();
                file.Offset = reader.RI32E(true);
                if (Signature != Farc.FArc) file.SizeComp = reader.RI32E(true);
                file.SizeUnc = reader.RI32E(true);
                if (Signature == Farc.FARC && Format == Format.FT)
                    file.Type = (Type)reader.RI32E(true);
                Files.Add(file);
            }

            reader.Dispose();
            return true;
        }

        private void FileReader()
        { for (int i = 0; i < Files.Count; i++) FileReader(i); }

        public byte[] FileReader(string file)
        {
            if (!HasFiles) return null;
            for (int i = 0; i < Files.Count; i++)
                if (Files[i].Name.ToLower() == file.ToLower()) return FileReader(i);
            return null;
        }

        public bool Exists(string file)
        {
            if (!HasFiles) return false;
            for (int i = 0; i < Files.Count; i++)
                if (Files[i].Name.ToLower() == file.ToLower()) return true;
            return false;
        }

        public byte[] FileReader(int i)
        {
            if (!HasFiles) return null;
            if (i >= Files.Count) return null;

            FARCFile file = Files[i];
            if (Signature != Farc.FARC)
            {
                file.Data = Signature == Farc.FArC && file.SizeUnc > 0
                    ? File.ReadAllBytes(FilePath, file.SizeComp, file.Offset).InflateGZip(file.SizeUnc)
                    : File.ReadAllBytes(FilePath, file.SizeUnc, file.Offset);
                Files[i] = file;
                return file.Data;
            }

            int FileSize = ((FARCType | file.Type) & Type.Enc) != 0 ? file.SizeComp.A(0x10)
                : (((FARCType | file.Type) & Type.GZip) != 0 ? file.SizeComp : file.SizeUnc);
            using (Stream stream = File.OpenReader(FilePath))
            {
                stream.S(file.Offset, 0);
                file.Data = stream.RBy(FileSize);
            }

            if ((FARCType & Type.Enc) != 0)
            {
                if (Format == Format.FT && (file.Type & Type.Enc) != 0)
                {
                    using (AesManaged aes = GetAes(true, null))
                    using (CryptoStream cryptoStream = new CryptoStream(new MSIO.MemoryStream(file.Data),
                        aes.CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(file.Data, 0, FileSize);
                    file.Data = SkipData(file.Data, 0x10);
                }
                else
                    using (AesManaged aes = GetAes(false, null))
                    using (CryptoStream cryptoStream = new CryptoStream(new MSIO.MemoryStream(file.Data),
                        aes.CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(file.Data, 0, FileSize);
            }

            if (((file.Type | FARCType) & Type.GZip) != 0 && file.SizeUnc > 0)
                file.Data = file.Data.InflateGZip(file.SizeUnc);

            Files[i] = file;
            return file.Data;
        }

        private void SaveToDisk()
        {
            if (DirectoryPath == null || Files.IsNull   ) return;
            if (DirectoryPath ==   "" || Files.Count < 1) return;
            MSIO.Directory.CreateDirectory(DirectoryPath);
            for (int i = 0; i < Files.Count; i++)
            {
                FARCFile file = Files[i];
                if (file.Data != null)
                    File.WriteAllBytes(Path.Combine(DirectoryPath, file.Name), file.Data);
            }
        }

        private byte[] SkipData(byte[] data, int skip)
        {
            byte[] skipData = new byte[data.Length - skip];
            System.Array.Copy(data, skip, skipData, 0, data.Length - skip);
            return skipData;
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
                     if (Signature == Farc.FArc) headerWriter.WE(0x20, true);
                else if (Signature == Farc.FArC) headerWriter.WE(0x10, true);
                else if (Signature == Farc.FARC)
                {
                    headerWriter.WE((int)FARCType, true);
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
                writer.WE(file.SizeUnc, true);
            }
            writer.Dispose();
        }

        private void CompressStuff(int i, ref Stream writer)
        {
            FARCFile file = Files[i];
            file.Offset = writer.P;
            file.SizeUnc = file.Data.Length;
            file.Type = Type.None;

            byte[] data = file.Data;
            if (Signature == Farc.FArC || (Signature == Farc.FARC && (FARCType & Type.GZip) != 0))
            {
                file.Type |= Type.GZip;
                data = file.Data.DeflateGZip(CompressionLevel);
            }
            file.SizeComp = data.Length;

            if (Signature == Farc.FARC && (FARCType & Type.Enc) != 0)
            {
                int alignLength = data.Length.A(0x40);
                byte[] tempData = new byte[alignLength];
                System.Array.Copy(data, tempData, data.Length);
                for (int i1 = file.Data.Length; i1 < alignLength; i1++) tempData[i1] = 0x78;
                data = Encrypt(tempData, false);
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

        private byte[] Encrypt(byte[] data, bool isFT)
        {
            MSIO.MemoryStream stream = new MSIO.MemoryStream();
            using (AesManaged aes = GetAes(isFT, null))
            using (CryptoStream cryptoStream = new CryptoStream(stream,
                aes.CreateEncryptor(), CryptoStreamMode.Write))
                cryptoStream.Write(data, 0, data.Length);
            return stream.ToArray();
        }

        public void Dispose() => NewFARC();

        public struct FARCFile
        {
            public int Offset;
            public int SizeComp;
            public int SizeUnc;
            public Type Type;
            public byte[] Data;
            public string Name;

            public override string ToString() => Name;
        }

        public enum Farc : int
        {
            FArc = 0x46417263,
            FArC = 0x46417243,
            FARC = 0x46415243,
        }

        public enum Type : int
        {
            None = 0b000,
            GZip = 0b010,
            Enc  = 0b100,
        }
    }
}
