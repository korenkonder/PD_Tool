using KKdMainLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public static class Extensions
    {
        public static double ReadWAVSample(this Stream IO, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   return IO. ReadInt16() / (double)0x00008000;
            else if (Bytes == 4 && Format == 0x01) return IO. ReadInt32() / (double)0x80000000;
            else if (Bytes == 4 && Format == 0x03) return IO.ReadSingle();
            else if (Bytes == 8 && Format == 0x03) return IO.ReadDouble();
            else                                   return 0;
        }

        public static void Write(this Stream IO, double Sample, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   IO.Write((Sample * 0x00008000).CFTS());
            else if (Bytes == 4 && Format == 0x01) IO.Write((Sample * 0x80000000).CFTI());
            else if (Bytes == 4 && Format == 0x03) IO.Write((float)Sample);
            else if (Bytes == 8 && Format == 0x03) IO.Write(       Sample);
        }

        public static WAV.Header ReadWAVHeader(this Stream IO)
        {
            WAV.Header Header = new WAV.Header();
            if (IO.ReadString(4) != "RIFF") return Header;
            IO.ReadUInt32();
            if (IO.ReadString(4) != "WAVE") return Header;
            if (IO.ReadString(4) != "fmt ") return Header;
            int Offset = IO.ReadInt32();
            Header.Format = IO.ReadUInt16();
            if (Header.Format == 0x01 || Header.Format == 0x03 || Header.Format == 0xFFFE)
            {
                Header.Channels = IO.ReadUInt16();
                Header.SampleRate = IO.ReadUInt32();
                IO.ReadInt32(); IO.ReadInt16();
                Header.Bytes = IO.ReadUInt16();
                if (Header.Bytes % 8 != 0) return Header;
                Header.Bytes >>= 3;
                if (Header.Bytes == 0) return Header;
                if (Header.Format == 0xFFFE)
                {
                    IO.ReadInt32();
                    Header.ChannelMask = IO.ReadUInt32();
                    Header.Format = IO.ReadUInt16();
                }
                if (Header.Bytes < 1 || (Header.Bytes > 4 && Header.Bytes  != 8)) return Header;
                if (Header.Bytes > 0 &&  Header.Bytes < 4 && Header.Format == 3 ) return Header;
                if (Header.Bytes == 8 && Header.Format == 1) return Header;
                IO.Seek(Offset + 0x14, 0);
                if (IO.ReadString(4) != "data") return Header;
                Header.Size = IO.ReadUInt32();
                Header.HeaderSize = IO.UIntPosition;
                Header.IsSupported = true;
                return Header;
            }
            return Header;
        }

        public static void Write(this Stream IO, WAV.Header Header, long Seek) => IO.Write(Header, Seek, 0);

        public static void Write(this Stream IO, WAV.Header Header, long Seek, SeekOrigin Origin)
        { IO.Seek(Seek, Origin); IO.Write(Header); }

        public static void Write(this Stream IO, WAV.Header Header)
        {
            IO.Write("RIFF");
            if (Header.Format != 0xFFFE) IO.Write(Header.Size + 0x24);
            else                         IO.Write(Header.Size + 0x3C);
            IO.Write("WAVE");
            IO.Write("fmt ");
            if (Header.Format != 0xFFFE) IO.Write(0x10);
            else                         IO.Write(0x28);
            
            IO.Write(Header.Format);
            IO.Write((short)Header.Channels);
            IO.Write(Header.SampleRate);
            IO.Write(Header.SampleRate * Header.Channels * Header.Bytes);
            IO.Write((short)(Header.Channels * Header.Bytes));
            IO.Write((short)(Header.Bytes << 3));
            if (Header.Format == 0xFFFE)
            {
                IO.Write((short)0x16);
                IO.Write((short)(Header.Bytes << 3));
                IO.Write(Header.ChannelMask);
                IO.Write(Header.Bytes == 2 ? 0x01 : 0x03);
                IO.Write(0x00100000);
                IO.Write(0xAA000080);
                IO.Write(0x719B3800);
            }
            IO.Write("data");
            IO.Write(Header.Size);
        }
    }
}
