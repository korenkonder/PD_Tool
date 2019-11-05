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
        public FARC(string File, bool IsDirectory = false)
        { if (IsDirectory) DirectoryPath = File; else FilePath = File; NewFARC(); }

        private void NewFARC() { Files = null; Signature = Farc.FArC; cbc = ft = false; }

        public FARCFile[] Files = null;
        public Type FARCType;
        public Farc Signature = Farc.FArC;
        public string FilePath, DirectoryPath;
        public bool HasFiles => Files == null ? false : Files.Length > 0;

        private bool cbc, ft;

        private readonly byte[] key = Text.ToASCII("project_diva.bin");

        private readonly byte[] keyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        private AesManaged GetAes(bool isFT, byte[] iv) =>
            new AesManaged { KeySize = 128, Key = isFT ? keyFT : key,
                BlockSize = 128, Mode = isFT ? CipherMode.CBC : CipherMode.ECB,
                Padding = PaddingMode.Zeros, IV = iv ?? new byte[16] };
        
        public void UnPack(bool saveToDisk = true)
        { if (HeaderReader()) { FileReader(); if (saveToDisk) this.SaveToDisk(); } }

        public bool HeaderReader()
        {
            NewFARC();
            if (!File.Exists(FilePath)) return false;

            Stream reader = File.OpenReader(FilePath);
            DirectoryPath = Path.GetFullPath(FilePath).Replace(Path.GetExtension(FilePath), "");
            Signature = (Farc)reader.RI32E(true);
            if (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)
            { reader.Dispose(); return false; }

            int headerLength = reader.RI32E(true);
            if (Signature == Farc.FARC)
            {
                FARCType = (Type)reader.RI32E(true);
                reader.RI32();

                int farcMode = reader.RI32E(true);
                ft  = farcMode == 0x10;
                cbc = farcMode != 0x10 && farcMode != 0x40;

                if (cbc && FARCType.HasFlag(Type.ECB))
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
                    ft = farcMode == 0x10;
                }
            }

            if (Signature == Farc.FARC)
                if (reader.RI32E(true) == 1)
                    Files = new FARCFile[reader.RI32E(true)];
            reader.RI32();

            if (Files == null)
            {
                int Count = 0;
                long Position = reader.I64P;
                while (reader.I64P < headerLength)
                {
                    reader.NT();
                    reader.RI32();
                    if (Signature != Farc.FArc      ) reader.RI32();
                    reader.RI32();
                    if (Signature == Farc.FARC && ft) reader.RI32();
                    Count++;
                }
                reader.I64P = Position;
                Files = new FARCFile[Count];
            }

            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Name = reader.NTUTF8();
                Files[i].Offset = reader.RI32E(true);
                if (Signature != Farc.FArc) Files[i].SizeComp = reader.RI32E(true);
                Files[i].SizeUnc = reader.RI32E(true);
                if (Signature == Farc.FARC && ft)
                    Files[i].Type = (Type)reader.RI32E(true);
            }

            reader.Dispose();
            return true;
        }

        private void FileReader()
        { for (int i = 0; i < Files.Length; i++) FileReader(i); }

        public byte[] FileReader(string file)
        {
            if (!HasFiles) return null;
            for (int i = 0; i < Files.Length; i++)
                if (Files[i].Name.ToLower() == file.ToLower()) return FileReader(i);
            return null;
        }

        public byte[] FileReader(int i)
        {
            if (!HasFiles) return null;
            if (i >= Files.Length) return null;
            if (Signature != Farc.FARC)
            {
                if (Signature == Farc.FArC)
                    using (MSIO.MemoryStream memorystream = new MSIO.MemoryStream(
                        File.ReadAllBytes(FilePath, Files[i].SizeComp, Files[i].Offset)))
                    using (GZipStream gZipStream = new GZipStream(memorystream, CompressionMode.Decompress))
                    {
                        Files[i].Data = new byte[Files[i].SizeUnc];
                        gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                    }
                else Files[i].Data = File.ReadAllBytes(FilePath, Files[i].SizeUnc, Files[i].Offset);
                return Files[i].Data;
            }

            int FileSize = FARCType.HasFlag(Type.ECB) || Files[i].Type.HasFlag(Type.ECB) ?
                Files[i].SizeComp.A(0x10) : Files[i].SizeComp;
            MSIO.FileStream stream = new MSIO.FileStream(FilePath, MSIO.FileMode.Open,
                MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite);
            stream.Seek(Files[i].Offset, 0);
            Files[i].Data = new byte[FileSize];

            bool encrypted = false;
            if (FARCType.HasFlag(Type.ECB))
            {
                if ((ft && Files[i].Type.HasFlag(Type.ECB)) || cbc)
                {
                    using (AesManaged aes = GetAes(true, null))
                    using (CryptoStream cryptoStream = new CryptoStream(stream,
                        aes.CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(Files[i].Data, 0, FileSize);
                    Files[i].Data = SkipData(Files[i].Data, 0x10);
                }
                else
                    using (AesManaged aes = GetAes(false, null))
                    using (CryptoStream cryptoStream = new CryptoStream(stream,
                        aes.CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(Files[i].Data, 0, FileSize);
                encrypted = true;
            }

            bool compressed = false;
            if (((ft && Files[i].Type.HasFlag(Type.GZip)) ||
                FARCType.HasFlag(Type.GZip)) && Files[i].SizeUnc > 0)
            {
                GZipStream gZipStream = new GZipStream(encrypted ? new MSIO.MemoryStream(Files[i].Data) :
                    (MSIO.Stream)stream, CompressionMode.Decompress);
                byte[] Temp = new byte[Files[i].SizeUnc];
                gZipStream.Read(Temp, 0, Files[i].SizeUnc);
                Files[i].Data = Temp;
                gZipStream.Dispose();
                compressed = true;
            }

            if (!encrypted && !compressed)
            {
                Files[i].Data = new byte[Files[i].SizeUnc];
                stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
            }
            stream.Dispose();
            return Files[i].Data;
        }

        private void SaveToDisk()
        {
            if (DirectoryPath == null || Files ==    null) return;
            if (DirectoryPath ==   "" || Files.Length < 1) return;
            MSIO.Directory.CreateDirectory(DirectoryPath);
            for (int i = 0; i < Files.Length; i++)
            {
                if (Files[i].Data != null)
                    File.WriteAllBytes(Path.Combine(DirectoryPath, Files[i].Name), Files[i].Data);
                Files[i].Data = null;
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
            Files = new FARCFile[files.Length];
            for (int i = 0; i < files.Length; i++)
                Files[i] = new FARCFile { Name = Path.GetFileName(files[i]), Data = File.ReadAllBytes(files[i]) };
            files = null;
            Signature = signature;
            Save();
        }

        public void Save()
        {
            for (int i = 0; i < Files.Length; i++)
            {
                string ext = Path.GetExtension(Files[i].Name).ToLower();
                if (ext == ".a3da" || ext == ".diva" || ext == ".vag")
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
                    headerWriter.W      (0x00);
                    headerWriter.WE(0x40, true);
                    headerWriter.W      (0x00);
                }
                int HeaderPartLength = Signature == Farc.FArc ? 0x09 : 0x0D;
                for (int i = 0; i < Files.Length; i++)
                    headerWriter.L += Path.GetFileName(Files[i].Name).Length + HeaderPartLength;
                writer.WE(headerWriter.L, true);
                writer.W(headerWriter.ToArray(true));
            }

            int align = writer.P.A(0x10) - writer.P;
            for (int i1 = 0; i1 < align; i1++)
                writer.W((byte)(Signature == Farc.FArc ? 0x00 : 0x78));

            for (int i = 0; i < Files.Length; i++)
                CompressStuff(i, ref writer);

            writer.P = Signature == Farc.FARC ? 0x1C : 0x0C;
            for (int i = 0; i < Files.Length; i++)
            {
                writer.W(Path.GetFileName(Files[i].Name) + "\0");
                writer.WE(Files[i].Offset, true);
                if (Signature != Farc.FArc)
                    writer.WE(Files[i].SizeComp, true);
                writer.WE(Files[i].SizeUnc, true);
            }
            writer.Dispose();
        }

        private void CompressStuff(int i, ref Stream writer)
        {
            Files[i].Offset = writer.P;
            Files[i].SizeUnc = Files[i].Data.Length;
            Files[i].Type = Type.None;

            if (Signature == Farc.FArC || (Signature == Farc.FARC && FARCType.HasFlag(Type.GZip)))
            {
                Files[i].Type |= Type.GZip;
                MSIO.MemoryStream stream = new MSIO.MemoryStream();
                using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    gZipStream.Write(Files[i].Data, 0, Files[i].Data.Length);
                Files[i].Data = stream.ToArray();
                stream.Dispose();
                Files[i].SizeComp = Files[i].Data.Length;
            }

            if (Signature == Farc.FARC && FARCType.HasFlag(Type.ECB))
            {
                int AlignData = Files[i].Data.Length.A(0x40);
                byte[] Data = new byte[AlignData];
                for (int i1 = 0; i1 < AlignData           ; i1++) Data[i1] = 0x78;
                for (int i1 = 0; i1 < Files[i].Data.Length; i1++) Data[i1] = Files[i].Data[i1];

                Files[i].Data = Encrypt(Data, false);
            }

            writer.W(Files[i].Data);
            Files[i].Data = null;

            if (Signature != Farc.FARC)
            {
                int Align = writer.P.A(0x20) - writer.P;
                for (int i1 = 0; i1 < Align; i1++)
                    writer.W((byte)(Signature == Farc.FArc ? 0x00 : 0x78));
            }
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
            ECB  = 0b100,
        }
    }
}
