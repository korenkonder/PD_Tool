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
        public FARC() { Files = new FARCFile[0]; Signature = Farc.FArC; FT = false; }

        public FARCFile[] Files = new FARCFile[0];
        public Farc Signature = Farc.FArC;

        private bool FT = false;

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

        public void UnPack(string file, bool SaveToDisk = true)
        {
            Files = null;
            Signature = Farc.FArC;
            FT = false;
            Console.Title = "FARC Extractor - Archive: " + Path.GetFileName(file);
            if (!File.Exists(file))
            {
                Console.WriteLine("File {0} doesn't exist.", Path.GetFileName(file));
                Console.Clear();
                return;
            }

            Stream reader = File.OpenReader(file);
            string directory = Path.GetFullPath(file).Replace(Path.GetExtension(file), "");
            Signature = (Farc)reader.ReadInt32Endian(true);
            if (Signature != Farc.FArc && Signature != Farc.FArC && Signature != Farc.FARC)
            {
                Console.WriteLine("Unknown signature"); reader.Close();
                Console.Clear();
                return;
            }

            MSIO.Directory.CreateDirectory(directory);
            int HeaderLength = reader.ReadInt32Endian(true);
            if (Signature != Farc.FARC)
            {
                reader.ReadUInt32();
                HeaderReader(HeaderLength, ref Files, ref reader);
                reader.Close();

                for (int i = 0; i < Files.Length; i++)
                {
                    if (Signature == Farc.FArC)
                        using (MSIO.MemoryStream memorystream = new MSIO.MemoryStream(
                            File.ReadAllBytes(file, Files[i].SizeComp, Files[i].Offset)))
                        {
                            GZipStream gZipStream = new GZipStream(memorystream, CompressionMode.Decompress);
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        }
                    else
                        Files[i].Data = File.ReadAllBytes(file, Files[i].SizeUnc, Files[i].Offset);

                    if (SaveToDisk)
                    {
                        File.WriteAllBytes(Path.Combine(directory, Files[i].Name), Files[i].Data);
                        Files[i].Data = null;
                    }
                }
                Console.Clear();
                return;
            }

            int Mode = reader.ReadInt32Endian(true);
            reader.ReadUInt32();
            bool GZip = (Mode & 2) == 2;
            bool ECB  = (Mode & 4) == 4;

            int FARCType = reader.ReadInt32Endian(true);
            FT = FARCType == 0x10;
            bool CBC = !FT && FARCType != 0x40;
            if (ECB && CBC)
            {
                byte[] Header = new byte[HeaderLength - 0x08];
                FT = true;
                reader.Close();
                MSIO.FileStream stream = new MSIO.FileStream(file, MSIO.FileMode.Open,
                   MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite);
                stream.Seek(0x10, 0);

                using (CryptoStream cryptoStream = new CryptoStream(stream,
                    GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                    cryptoStream.Read(Header, 0x00, HeaderLength - 0x08);
                Header = SkipData(Header, 0x10);
                Stream CBCreader = new Stream(new MSIO.MemoryStream(Header));
                CBCreader.BaseStream.Seek(0, 0);

                FARCType = CBCreader.ReadInt32Endian(true);
                FT = FARCType == 0x10;
                if (CBCreader.ReadInt32Endian(true) == 1)
                    Files = new FARCFile[CBCreader.ReadInt32Endian(true)];
                CBCreader.ReadUInt32();
                HeaderReader(HeaderLength, ref Files, ref CBCreader);
                CBCreader.Close();
            }
            else
            {
                if (reader.ReadInt32Endian(true) == 1)
                    Files = new FARCFile[reader.ReadInt32Endian(true)];
                reader.ReadUInt32();
                HeaderReader(HeaderLength, ref Files, ref reader);
                reader.Close();
            }

            for (int i = 0; i < Files.Length; i++)
            {
                int FileSize = ECB || Files[i].ECB ? Files[i].SizeComp.Align(0x10) : Files[i].SizeComp;
                MSIO.FileStream stream = new MSIO.FileStream(file, MSIO.FileMode.Open,
                    MSIO.FileAccess.ReadWrite, MSIO.FileShare.ReadWrite);
                stream.Seek(Files[i].Offset, 0);
                Files[i].Data = new byte[FileSize];

                bool Encrypted = false;
                if (ECB)
                {
                    if ((FT && Files[i].ECB) || CBC)
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Files[i].Data, 0, FileSize);
                        Files[i].Data = SkipData(Files[i].Data, 0x10);
                    }
                    else
                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(false, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Files[i].Data, 0, FileSize);
                    Encrypted = true;
                }

                bool Compressed = false;
                bool LocalGZip = (FT && Files[i].GZip) || GZip && Files[i].SizeUnc != 0;
                if (LocalGZip)
                {
                    GZipStream gZipStream;
                    if (Encrypted)
                    {
                        gZipStream = new GZipStream(new MSIO.MemoryStream(
                            Files[i].Data), CompressionMode.Decompress);
                        stream.Close();
                    }
                    else gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                    Files[i].Data = new byte[Files[i].SizeUnc];
                    gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);

                    Compressed = true;
                }

                if (!Encrypted && !Compressed)
                {
                    Files[i].Data = new byte[Files[i].SizeUnc];
                    stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                    stream.Close();
                }
                
                if (SaveToDisk)
                {
                    File.WriteAllBytes(Path.Combine(directory, Files[i].Name), Files[i].Data);
                    Files[i].Data = null;
                }

            }
            Console.Clear();
        }

        byte[] SkipData(byte[] Data, int Skip)
        {
            byte[] SkipData = new byte[Data.Length - Skip];
            for (int i = 0; i < Data.Length - Skip; i++) SkipData[i] = Data[i + Skip];
            return SkipData;
        }

        void HeaderReader(int HeaderLenght, ref FARCFile[] Files, ref Stream reader)
        {
            if (Files == null)
            {
                int Count = 0;
                long Position = reader.BaseStream.Position;
                while (reader.BaseStream.Position < HeaderLenght)
                {
                    reader.NullTerminated();
                    reader.ReadInt32();
                    if (Signature != Farc.FArc      ) reader.ReadInt32();
                    reader.ReadInt32();
                    if (Signature == Farc.FARC && FT) reader.ReadInt32();
                    Count++;
                }
                reader.Seek(Position, 0);
                Files = new FARCFile[Count];
            }

            int LocalMode = 0;
            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Name = reader.NullTerminatedUTF8();
                Files[i].Offset = reader.ReadInt32Endian(true);
                if (Signature != Farc.FArc) Files[i].SizeComp = reader.ReadInt32Endian(true);
                Files[i].SizeUnc = reader.ReadInt32Endian(true);
                if (Signature == Farc.FARC && FT)
                {
                    LocalMode = reader.ReadInt32Endian(true);
                    Files[i].GZip = (LocalMode & 2) == 2;
                    Files[i].ECB  = (LocalMode & 4) == 4;
                }
            }
        }

        public void Pack(string file)
        {
            Files = null;
            FT = false;
            string[] files = Directory.GetFiles(file);
            Files = new FARCFile[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Files[i] = new FARCFile { Name = files[i] };
                string ext = Path.GetExtension(files[i]).ToLower();
                if (ext == ".a3da" || ext == ".diva" || ext == ".vag")
                    Signature = Farc.FArc;
            }
            files = null;

            Stream writer = File.OpenWriter(file + ".farc", true);
            writer.WriteEndian((int)Signature, true);

            Stream HeaderWriter = File.OpenWriter();
            for (int i = 0; i < 3; i++)      HeaderWriter.WriteByte(0x00);
                 if (Signature == Farc.FArc) HeaderWriter.WriteByte(0x20);
            else if (Signature == Farc.FArC) HeaderWriter.WriteByte(0x10);
            else if (Signature == Farc.FARC)
            {
                                            HeaderWriter.WriteByte(0x06);
                for (int i = 0; i < 7; i++) HeaderWriter.WriteByte(0x00);
                                            HeaderWriter.WriteByte(0x40);
                for (int i = 0; i < 8; i++) HeaderWriter.WriteByte(0x00);
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

            if (Signature == Farc.FARC) writer.Seek(0x1C, 0);
            else                        writer.Seek(0x0C, 0);
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

        void CompressStuff(int i, ref FARCFile[] Files, ref Stream writer)
        {
            Files[i].Offset = writer.Position;
            Files[i].Data = File.ReadAllBytes(Files[i].Name);
            Files[i].SizeUnc = Files[i].Data.Length;

            if (Signature != Farc.FArc)
            {
                if (Signature != Farc.FArc)
                {
                    MSIO.MemoryStream stream = new MSIO.MemoryStream();
                    using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        gZipStream.Write(Files[i].Data, 0, Files[i].Data.Length);
                    Files[i].Data = stream.ToArray();
                    stream.Dispose();
                    Files[i].SizeComp = Files[i].Data.Length;
                }
                else if (Signature == Farc.FARC)
                {
                    int AlignData = Files[i].Data.Length.Align(0x40);
                    byte[] Data = new byte[AlignData];
                    for (int i1 = 0; i1 < Files[i].Data.Length; i1++)
                        Data[i1] = Files[i].Data[i1];
                    for (int i1 = Files[i].Data.Length; i1 < AlignData; i1++)
                        Data[i1] = 0x78;

                    Files[i].Data = Encrypt(Data, false);
                }
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

        byte[] Encrypt(byte[] Data, bool isFT)
        {
            MSIO.MemoryStream stream = new MSIO.MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(stream,
                GetAes(isFT, null).CreateEncryptor(),CryptoStreamMode.Write))
                cryptoStream.Write(Data, 0, Data.Length);
            return stream.ToArray();
        }

        public enum Farc
        {
            FArc = 0x46417263,
            FArC = 0x46417243,
            FARC = 0x46415243,
        }

        public struct FARCFile
        {
            public int Offset;
            public int SizeComp;
            public int SizeUnc;
            public bool GZip;
            public bool ECB;
            public byte[] Data;
            public string Name;
        }
    }
}
