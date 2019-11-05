using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public static class Extensions
    {
        public static double RS(this Stream _IO, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   return _IO. RI16() / (double)0x00008000;
            else if (Bytes == 4 && Format == 0x01) return _IO. RI32() / (double)0x80000000;
            else if (Bytes == 4 && Format == 0x03) return _IO.RF32();
            else if (Bytes == 8 && Format == 0x03) return _IO.RF64();
            else                                   return 0;
        }

        public static void W(this Stream _IO, double Sample, ushort Bytes, ushort Format)
        {
                 if (Bytes == 2)                   _IO.W((Sample * 0x00008000).CFTS());
            else if (Bytes == 4 && Format == 0x01) _IO.W((Sample * 0x80000000).CFTI());
            else if (Bytes == 4 && Format == 0x03) _IO.W((float)Sample);
            else if (Bytes == 8 && Format == 0x03) _IO.W(       Sample);
        }

        public static WAV.Header ReadWAVHeader(this Stream _IO)
        {
            WAV.Header Header = new WAV.Header();
            if (_IO.RS(4) != "RIFF") return Header;
            _IO.RU32();
            if (_IO.RS(4) != "WAVE") return Header;
            if (_IO.RS(4) != "fmt ") return Header;
            int Offset = _IO.RI32();
            Header.Format = _IO.RU16();
            if (Header.Format == 0x01 || Header.Format == 0x03 || Header.Format == 0xFFFE)
            {
                Header.Channels = _IO.RU16();
                Header.SampleRate = _IO.RU32();
                _IO.RI32(); _IO.RI16();
                Header.Bytes = _IO.RU16();
                if (Header.Bytes % 8 != 0) return Header;
                Header.Bytes >>= 3;
                if (Header.Bytes == 0) return Header;
                if (Header.Format == 0xFFFE)
                {
                    _IO.RI32();
                    Header.ChannelMask = _IO.RU32();
                    Header.Format = _IO.RU16();
                }
                if (Header.Bytes < 1 || (Header.Bytes > 4 && Header.Bytes  != 8)) return Header;
                if (Header.Bytes > 0 &&  Header.Bytes < 4 && Header.Format == 3 ) return Header;
                if (Header.Bytes == 8 && Header.Format == 1) return Header;
                _IO.S(Offset + 0x14, 0);
                if (_IO.RS(4) != "data") return Header;
                Header.Size = _IO.RU32();
                Header.HeaderSize = _IO.U32P;
                Header.IsSupported = true;
                return Header;
            }
            return Header;
        }

        public static void W(this Stream _IO, WAV.Header Header, long Seek) => _IO.W(Header, Seek, 0);

        public static void W(this Stream _IO, WAV.Header Header, long Seek, SeekOrigin Origin)
        { _IO.S(Seek, Origin); _IO.W(Header); }

        public static void W(this Stream _IO, WAV.Header Header)
        {
            _IO.W("RIFF");
            if (Header.Format != 0xFFFE) _IO.W(Header.Size + 0x24);
            else                         _IO.W(Header.Size + 0x3C);
            _IO.W("WAVE");
            _IO.W("fmt ");
            if (Header.Format != 0xFFFE) _IO.W(0x10);
            else                         _IO.W(0x28);
            
            _IO.W(Header.Format);
            _IO.W((short)Header.Channels);
            _IO.W(Header.SampleRate);
            _IO.W(Header.SampleRate * Header.Channels * Header.Bytes);
            _IO.W((short)(Header.Channels * Header.Bytes));
            _IO.W((short)(Header.Bytes << 3));
            if (Header.Format == 0xFFFE)
            {
                _IO.W((short)0x16);
                _IO.W((short)(Header.Bytes << 3));
                _IO.W(Header.ChannelMask);
                _IO.W(Header.Bytes == 2 ? 0x01 : 0x03);
                _IO.W(0x00100000);
                _IO.W(0xAA000080);
                _IO.W(0x719B3800);
            }
            _IO.W("data");
            _IO.W(Header.Size);
        }
    }
}
