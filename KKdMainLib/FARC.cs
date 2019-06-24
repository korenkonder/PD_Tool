//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System;
using System.IO.Compression;
using System.Security.Cryptography;
using KKdMainLib.IO;
using MSIO = System.IO;

namespace KKdMainLib
{
    public class FARC
    {
        public FARC() => NewFARC();
        public FARC(string File, bool IsDirectory = false)
        { if (IsDirectory) DirectoryPath = File; else FilePath = File; NewFARC(); }

        private void NewFARC() { Files = null; Signature = Farc.FArC; CBC = FT = false; }

        public FARCFile[] Files = null;
        public Type FARCType;
        public Farc Signature = Farc.FArC;
        public string FilePath, DirectoryPath;

        private bool CBC, FT;

        private readonly byte[] Key = Text.ToASCII("project_diva.bin");

        private readonly byte[] KeyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        AesManaged GetAes(bool isFT, byte[] iv)
        {
            AesManaged AesManaged = new AesManaged { KeySize = 128, Key = isFT ? KeyFT : Key,
                BlockSize = 128, Mode = isFT ? CipherMode.CBC : CipherMode.ECB,
                Padding = PaddingMode.Zeros, IV = iv ?? new byte[16] };
            return AesManaged;
        }
        
        public void UnPack(bool SaveToDisk = true)
        {
            HeaderReader().FileReader();
            if (SaveToDisk) this.SaveToDisk();
        }

        private FARC HeaderReader()
        {
            NewFARC();
            Console.Title = "FARC Extractor - Archive: " + Path.GetFileName(FilePath);
            if (!File.Exists(FilePath))
            { Console.WriteLine("File {0} doesn't exist.", Path.GetFileName(FilePath)); return this; }

            Stream reader = File.OpenReader(FilePath);
            DirectoryPath = Path.GetFullPath(FilePath).Replace(Path.GetExtension(FilePath), "");
            Signature = (Farc)reader.ReadInt32Endian(true);
            if (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)
            { Console.WriteLine("Unknown signature"); reader.Close(); return this; }

            int HeaderLength = reader.ReadInt32Endian(true);
            if (Signature == Farc.FARC)
            {
                FARCType = (Type)reader.ReadInt32Endian(true);
                reader.ReadInt32();

                int FARCMode = reader.ReadInt32Endian(true);
                FT  = FARCMode == 0x10;
                CBC = FARCMode != 0x10 && FARCMode != 0x40;

                if (CBC && FARCType.HasFlag(Type.ECB))
                {
                    reader.Close();
                    byte[] Header = new byte[HeaderLength - 0x08];
                    MSIO.FileStream stream = new MSIO.FileStream(FilePath, MSIO.FileMode.Open,
                       MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite) { Position = 0x10 };

                    using (CryptoStream cryptoStream = new CryptoStream(stream,
                        GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                        cryptoStream.Read(Header, 0x00, HeaderLength - 0x08);
                    Header = SkipData(Header, 0x10);
                    reader = File.OpenReader(Header);

                    FARCMode = reader.ReadInt32Endian(true);
                    FT = FARCMode == 0x10;
                }
            }

            if (Signature == Farc.FARC)
                if (reader.ReadInt32Endian(true) == 1)
                    Files = new FARCFile[reader.ReadInt32Endian(true)];
            reader.ReadInt32();

            if (Files == null)
            {
                int Count = 0;
                long Position = reader.LongPosition;
                while (reader.LongPosition < HeaderLength)
                {
                    reader.NullTerminated();
                    reader.ReadInt32();
                    if (Signature != Farc.FArc      ) reader.ReadInt32();
                    reader.ReadInt32();
                    if (Signature == Farc.FARC && FT) reader.ReadInt32();
                    Count++;
                }
                reader.LongPosition = Position;
                Files = new FARCFile[Count];
            }

            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Name = reader.NullTerminatedUTF8();
                Files[i].Offset = reader.ReadInt32Endian(true);
                if (Signature != Farc.FArc) Files[i].SizeComp = reader.ReadInt32Endian(true);
                Files[i].SizeUnc = reader.ReadInt32Endian(true);
                if (Signature == Farc.FARC && FT)
                    Files[i].Type = (Type)reader.ReadInt32Endian(true);
            }

            reader.Close();
            return this;
        }

        private FARC FileReader()
        {
            if (Files == null) return this;
            if (Files.Length < 1) return this;
            if (Signature != Farc.FARC)
            {
                for (int i = 0; i < Files.Length; i++)
                    if (Signature == Farc.FArC)
                        using (MSIO.MemoryStream memorystream = new MSIO.MemoryStream(
                            File.ReadAllBytes(FilePath, Files[i].SizeComp, Files[i].Offset)))
                        {
                            GZipStream gZipStream = new GZipStream(memorystream, CompressionMode.Decompress);
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        }
                    else Files[i].Data = File.ReadAllBytes(FilePath, Files[i].SizeUnc, Files[i].Offset);
                return this;
            }

            for (int i = 0; i < Files.Length; i++)
            {
                int FileSize = FARCType.HasFlag(Type.ECB) || Files[i].Type.HasFlag(Type.ECB) ?
                    Files[i].SizeComp.Align(0x10) : Files[i].SizeComp;
                MSIO.FileStream stream = new MSIO.FileStream(FilePath, MSIO.FileMode.Open,
                    MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite);
                stream.Seek(Files[i].Offset, 0);
                Files[i].Data = new byte[FileSize];

                bool Encrypted = false;
                if (FARCType.HasFlag(Type.ECB))
                {
                    if ((FT && Files[i].Type.HasFlag(Type.ECB)) || CBC)
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Files[i].Data, 0, FileSize);
                        Files[i].Data = SkipData(Files[i].Data, 0x10);
                    }
                    else if (Files[i].Type.HasFlag(Type.ECB))
                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(false, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Files[i].Data, 0, FileSize);
                    else stream.Read(Files[i].Data, 0, FileSize);
                    Encrypted = true;
                }

                bool Compressed = false;
                if (((FT && Files[i].Type.HasFlag(Type.GZip)) ||
                    FARCType.HasFlag(Type.GZip)) && Files[i].SizeUnc > 0)
                {
                    GZipStream gZipStream;
                    if (Encrypted) { gZipStream = new GZipStream(new MSIO.MemoryStream(Files[i].Data),
                        CompressionMode.Decompress); stream.Close(); }
                    else gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                    byte[] Temp = new byte[Files[i].SizeUnc];
                    gZipStream.Read(Temp, 0, Files[i].SizeUnc);
                    Files[i].Data = Temp;

                    Compressed = true;
                }

                if (!Encrypted && !Compressed)
                {
                    Files[i].Data = new byte[Files[i].SizeUnc];
                    stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                    stream.Close();
                }
            }
            return this;
        }

        private void SaveToDisk()
        {
            if (DirectoryPath == null || Files ==    null) return;
            if (DirectoryPath ==   "" || Files.Length < 1) return;
            MSIO.Directory.CreateDirectory(DirectoryPath);
            for (int i = 0; i < Files.Length; i++)
            {
                File.WriteAllBytes(Path.Combine(DirectoryPath, Files[i].Name), Files[i].Data);
                Files[i].Data = null;
            }
        }

        private byte[] SkipData(byte[] Data, int Skip)
        {
            byte[] SkipData = new byte[Data.Length - Skip];
            for (int i = 0; i < Data.Length - Skip; i++) SkipData[i] = Data[i + Skip];
            return SkipData;
        }

        public void Pack()
        {
            NewFARC();
            string[] files = Directory.GetFiles(DirectoryPath);
            Files = new FARCFile[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Files[i] = new FARCFile { Name = files[i] };
                string ext = Path.GetExtension(files[i]).ToLower();
                if (ext == ".a3da" || ext == ".diva" || ext == ".vag") Signature = Farc.FArc;
            }
            files = null;

            Stream writer = File.OpenWriter(DirectoryPath + ".farc", true);
            writer.WriteEndian((int)Signature, true);

            Stream HeaderWriter = File.OpenWriter();
                 if (Signature == Farc.FArc) HeaderWriter.WriteEndian(0x20, true);
            else if (Signature == Farc.FArC) HeaderWriter.WriteEndian(0x10, true);
            else if (Signature == Farc.FARC)
            {
                HeaderWriter.WriteEndian((int)FARCType, true);
                HeaderWriter.Write      (0x00);
                HeaderWriter.WriteEndian(0x40         , true);
                HeaderWriter.Write      (0x00);
            }
            int HeaderPartLength = Signature == Farc.FArc ? 0x09 : 0x0D;
            for (int i = 0; i < Files.Length; i++)
                HeaderWriter.Length += Path.GetFileName(Files[i].Name).Length + HeaderPartLength;
            writer.WriteEndian(HeaderWriter.Length, true);
            writer.Write(HeaderWriter.ToArray(true));
            HeaderWriter = null;

            int Align = writer.Position.Align(0x10) - writer.Position;
            for (int i1 = 0; i1 < Align; i1++)
                if (Signature == Farc.FArc) writer.WriteByte(0x00);
                else                        writer.WriteByte(0x78);

            for (int i = 0; i < Files.Length; i++)
                CompressStuff(i, ref Files, ref writer);

            if (Signature == Farc.FARC) writer.Position = 0x1C;
            else                        writer.Position = 0x0C;
            for (int i = 0; i < Files.Length; i++)
            {
                writer.Write(Path.GetFileName(Files[i].Name) + "\0");
                writer.WriteEndian(Files[i].Offset, true);
                if (Signature != Farc.FArc)
                    writer.WriteEndian(Files[i].SizeComp, true);
                writer.WriteEndian(Files[i].SizeUnc, true);
            }
            writer.Close();
        }

        private void CompressStuff(int i, ref FARCFile[] Files, ref Stream writer)
        {
            Files[i].Offset = writer.Position;
            Files[i].Data = File.ReadAllBytes(Files[i].Name);
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
                int AlignData = Files[i].Data.Length.Align(0x40);
                byte[] Data = new byte[AlignData];
                for (int i1 = 0; i1 < AlignData           ; i1++) Data[i1] = 0x78;
                for (int i1 = 0; i1 < Files[i].Data.Length; i1++) Data[i1] = Files[i].Data[i1];

                Files[i].Data = Encrypt(Data, false);
            }

            writer.Write(Files[i].Data);
            Files[i].Data = null;

            if (Signature != Farc.FARC)
            {
                int Align = writer.Position.Align(0x20) - writer.Position;
                for (int i1 = 0; i1 < Align; i1++)
                    if (Signature == Farc.FArc) writer.WriteByte(0x00);
                    else                        writer.WriteByte(0x78);
            }
        }

        private byte[] Encrypt(byte[] Data, bool isFT)
        {
            MSIO.MemoryStream stream = new MSIO.MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(stream,
                GetAes(isFT, null).CreateEncryptor(), CryptoStreamMode.Write))
                cryptoStream.Write(Data, 0, Data.Length);
            return stream.ToArray();
        }

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
