using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public static class Extensions
    {
        public static double RS(this Stream IO, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   return IO. RI16() / (double)0x00008000;
            else if (Bytes == 4 && Format == 0x01) return IO. RI32() / (double)0x80000000;
            else if (Bytes == 4 && Format == 0x03) return IO.RF32();
            else if (Bytes == 8 && Format == 0x03) return IO.RF64();
            else                                   return 0;
        }

        public static void W(this Stream IO, double Sample, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   IO.W((Sample * 0x00008000).CFTS());
            else if (Bytes == 4 && Format == 0x01) IO.W((Sample * 0x80000000).CFTI());
            else if (Bytes == 4 && Format == 0x03) IO.W((float)Sample);
            else if (Bytes == 8 && Format == 0x03) IO.W(       Sample);
        }

        public static WAV.Header ReadWAVHeader(this Stream IO)
        {
            WAV.Header Header = new WAV.Header();
            if (IO.RS(4) != "RIFF") return Header;
            IO.RU32();
            if (IO.RS(4) != "WAVE") return Header;
            if (IO.RS(4) != "fmt ") return Header;
            int Offset = IO.RI32();
            Header.Format = IO.RU16();
            if (Header.Format == 0x01 || Header.Format == 0x03 || Header.Format == 0xFFFE)
            {
                Header.Channels = IO.RU16();
                Header.SampleRate = IO.RU32();
                IO.RI32(); IO.RI16();
                Header.Bytes = IO.RU16();
                if (Header.Bytes % 8 != 0) return Header;
                Header.Bytes >>= 3;
                if (Header.Bytes == 0) return Header;
                if (Header.Format == 0xFFFE)
                {
                    IO.RI32();
                    Header.ChannelMask = IO.RU32();
                    Header.Format = IO.RU16();
                }
                if (Header.Bytes < 1 || (Header.Bytes > 4 && Header.Bytes  != 8)) return Header;
                if (Header.Bytes > 0 &&  Header.Bytes < 4 && Header.Format == 3 ) return Header;
                if (Header.Bytes == 8 && Header.Format == 1) return Header;
                IO.S(Offset + 0x14, 0);
                if (IO.RS(4) != "data") return Header;
                Header.Size = IO.RU32();
                Header.HeaderSize = IO.U32P;
                Header.IsSupported = true;
                return Header;
            }
            return Header;
        }

        public static void W(this Stream IO, WAV.Header Header, long Seek) => IO.W(Header, Seek, 0);

        public static void W(this Stream IO, WAV.Header Header, long Seek, SeekOrigin Origin)
        { IO.S(Seek, Origin); IO.W(Header); }

        public static void W(this Stream IO, WAV.Header Header)
        {
            IO.W("RIFF");
            if (Header.Format != 0xFFFE) IO.W(Header.Size + 0x24);
            else                         IO.W(Header.Size + 0x3C);
            IO.W("WAVE");
            IO.W("fmt ");
            if (Header.Format != 0xFFFE) IO.W(0x10);
            else                         IO.W(0x28);
            
            IO.W(Header.Format);
            IO.W((short)Header.Channels);
            IO.W(Header.SampleRate);
            IO.W(Header.SampleRate * Header.Channels * Header.Bytes);
            IO.W((short)(Header.Channels * Header.Bytes));
            IO.W((short)(Header.Bytes << 3));
            if (Header.Format == 0xFFFE)
            {
                IO.W((short)0x16);
                IO.W((short)(Header.Bytes << 3));
                IO.W(Header.ChannelMask);
                IO.W(Header.Bytes == 2 ? 0x01 : 0x03);
                IO.W(0x00100000);
                IO.W(0xAA000080);
                IO.W(0x719B3800);
            }
            IO.W("data");
            IO.W(Header.Size);
        }
    }
}
