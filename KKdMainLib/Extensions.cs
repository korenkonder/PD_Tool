using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool seek)
        {
            if (seek)
                if (stream.PI64 > 4) stream.PI64 -= 4;
                else                 stream.PI64  = 0;
            return stream.ReadHeader();
        }

        public static Header ReadHeader(this Stream stream)
        {
            Header header = new Header { Format = Format.F2 };
            header.Signature   = stream.RU32();
            header.DataSize    = stream.RU32();
            header.Length      = stream.RU32();
            header.Flags       = stream.RU32();
            header.Depth       = stream.RU32();
            header.SectionSize = stream.RU32();
            header.Version     = stream.RU32();
            stream.RI32();
            header.UseBigEndian   = (header.Flags & 0x08000000) != 0;
            header.UseSectionSize = (header.Flags & 0x10000000) != 0;
            if (header.Length == 0x40)
            {
                stream.RI64();
                stream.RI64();
                header.InnerSignature = stream.RU32();
                stream.RI32();
                stream.RI64();
            }
            stream.Format = header.Format;
            return header;
        }

        public static void W(this Stream stream, Header header, bool extended = false)
        {
            header.Length = (header.Format < Format.X && extended) ? 0x40u : 0x20u;
            header.Flags = (header.UseSectionSize ? 0x10000000 : 0u) |
                           (header.UseBigEndian   ? 0x08000000 : 0u);

            stream.W(header.Signature);
            stream.W(header.DataSize);
            stream.W(header.Length);
            stream.W(header.Flags);
            stream.W(header.Depth);
            stream.W(header.SectionSize);
            stream.W(header.Version);
            stream.W(0x00);
            if (header.Length == 0x40)
            {
                stream.W(0x00L);
                stream.W(0x00L);
                stream.W(header.InnerSignature);
                stream.W(0x00);
                stream.W(0x00L);
            }
        }

        public static void WEOFC(this Stream stream, uint depth = 0) =>
            stream.W(new Header { Depth = depth, Length = 0x20, Signature = 0x43464F45, UseSectionSize = true });
    }

    public static class POFExtensions
    {
        public static void W(this Stream stream, POF pof, bool shiftX = false, uint depth = 0)
        {
            byte[] data = pof.Write(shiftX);
            Header header = new Header { Depth = depth, Format = Format.F2, Length = 0x20,
                Signature = shiftX ? 0x31464F50u : 0x30464F50u, UseSectionSize = true };
            header.DataSize = header.SectionSize = (uint)data.Length.A(0x10);
            stream.W(header);
            stream.W(data);
            for (int c = data.Length.A(0x10) - data.Length; c > 0; c--)
                stream.W((byte)0);
        }
    }

    public static class ENRSExtensions
    {
        public static void W(this Stream stream, ENRS enrs, uint depth = 0)
        {
            byte[] data = enrs.Write();
            Header header = new Header { Depth = depth, Format = Format.F2,
                Length = 0x20, Signature = 0x53524E45, UseSectionSize = true };
            header.DataSize = header.SectionSize = (uint)data.Length;
            stream.W(header);
            stream.W(data);
        }
    }

    public static class StructExtensions
    {
        public static Struct RSt(this byte[] data)
        {
            if (data == null || data.Length < 1) return default;
            data = DIVAFILE.Decrypt(data);
            Struct @struct;
            using (Stream stream = File.OpenReader(data))
                @struct = stream.RSt(stream.ReadHeader(false));
            return @struct;
        }

        private static Struct RSt(this Stream stream, Header header)
        {
            uint l = header.UseSectionSize ? header.SectionSize : header.DataSize;
            Struct @struct = new Struct { Header = header, DataOffset =
                stream.P, Data = stream.RBy(l) };
            uint depth = header.Depth;

            uint lastSig = 0, sig;
            long length = stream.L - stream.P;
            long position = 0;
            KKdList<Struct> subStructs = KKdList<Struct>.New;
            while (length > position)
            {
                header = stream.ReadHeader(false);
                sig = header.Signature;
                l = header.UseSectionSize ? header.SectionSize : header.DataSize;
                position += header.Length + l;
                if (sig == 0x43464F45 && header.Depth == depth + 1) break;
                else if (sig == 0x53524E45 || (sig & 0xF0FFFFFF) == 0x30464F50)
                {
                    byte[] Data = stream.RBy(l);
                    if (sig == 0x53524E45) @struct.ENRS.Read(Data);
                    else                   @struct.POF .Read(Data, sig == 0x31464F50);
                }
                else if (header.Depth == 0 && sig == 0x43505854)
                    subStructs.Add(stream.RSt(header));
                else if (header.Depth > depth) subStructs.Add(stream.RSt(header));
                else { stream.PI64 -= header.Length; break; }
                lastSig = sig;
            }
            subStructs.Capacity = subStructs.Count;

            if (subStructs.Capacity > 0) @struct.SubStructs = subStructs.ToArray();
            return @struct;
        }

        public static byte[] W(this Struct @struct, bool shiftX = false, bool useDepth = true)
        {
            byte[] Data;
            using (Stream stream = File.OpenWriter())
            { stream.W(@struct, 0, shiftX, useDepth); stream.WEOFC(); Data = stream.ToArray(); }
            return Data;
        }

        public static void W(this Stream stream, Struct @struct, uint depth = 0,
            bool shiftX = false, bool useDepth = true)
        {
            @struct.Update(shiftX);

            @struct.Header.Depth = depth;
            stream.W(@struct.Header, @struct.Header.Length == 0x40);
            if (@struct.Data != null) stream.W(@struct.Data);
            if (@struct.HasPOF ) stream.W(@struct.POF , shiftX, useDepth ? depth + 1 : 0u);
            if (@struct.HasENRS) stream.W(@struct.ENRS,         useDepth ? depth + 1 : 0u);
            if (@struct.HasSubStructs)
                for (int i = 0; i < @struct.SubStructs.Length; i++)
                    stream.W(@struct.SubStructs[i], depth + 1, shiftX, useDepth);
            if (@struct.HasPOF || @struct.HasENRS || @struct.HasSubStructs)
                stream.WEOFC(depth + 1);
        }
    }

    public static class MPExt
    {
        public static MsgPack ReadMP(this byte[] array, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(array))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(array))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMPAllAtOnce(this string file, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(file + ".json", true))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(file + ".mp"  , true))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMP(this string file, bool json = false)
        {
            MsgPack MsgPack;
            if (json) using (JSON _IO = new JSON(File.OpenReader(file + ".json"))) MsgPack = _IO.Read(    );
            else      using (  MP _IO = new   MP(File.OpenReader(file + ".mp"  ))) MsgPack = _IO.Read(true);
            return MsgPack;
        }

        public static void Write(this MsgPack mp, bool ignoreNull, bool temp, string file, bool json = false)
        { if (temp) MsgPack.New.Add(mp).Write(file, ignoreNull, json).Dispose();
          else                      mp .Write(file, ignoreNull, json); }

        public static MsgPack Write(this MsgPack mp, string file, bool ignoreNull, bool json = false)
        {
            if (json)
                using (JSON _IO = new JSON(File.OpenWriter(file + ".json", true)))
                    _IO.W(mp, ignoreNull, "\n", "  ");
            else
                using (  MP _IO = new   MP(File.OpenWriter(file + ".mp"  , true)))
                    _IO.W(mp, ignoreNull);
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool ignoreNull, bool temp, string file, bool json = false)
        { if (temp) MsgPack.New.Add(mp).WriteAfterAll(file, ignoreNull, json).Dispose();
          else                      mp .WriteAfterAll(file, ignoreNull, json); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool ignoreNull, bool json = false)
        {
            byte[] data = null;
            if (json)
                using (JSON _IO = new JSON(File.OpenWriter()))
                { _IO.W(mp, ignoreNull, true); data = _IO.ToArray(); }
            else
                using (  MP _IO = new   MP(File.OpenWriter()))
                { _IO.W(mp, ignoreNull      ); data = _IO.ToArray(); }
            File.WriteAllBytes(file + (json ? ".json" : ".mp"), data);
            return mp;
        }

        public static void ToJSON   (this string file) =>
            file.ReadMP(    ).Write(file, false, true).Dispose();

        public static void ToMsgPack(this string file) =>
            file.ReadMP(true).Write(file, false      ).Dispose();
    }

    public static class IKFExt
    {
        public static IKF Round(this IKF kf, int d)
        {
                 if (kf is KFT0 kft0) { kft0.F  = kft0.F .Round(d);                             return kft0; }
            else if (kf is KFT1 kft1) { kft1.F  = kft1.F .Round(d); kft1.V  = kft1.V .Round(d); return kft1; }
            else if (kf is KFT2 kft2) { kft2.F  = kft2.F .Round(d); kft2.V  = kft2.V .Round(d);
                                        kft2.T  = kft2.T .Round(d);                             return kft2; }
            else if (kf is KFT3 kft3) { kft3.F  = kft3.F .Round(d); kft3.V  = kft3.V .Round(d);
                                        kft3.T1 = kft3.T1.Round(d); kft3.T2 = kft3.T2.Round(d); return kft3; }
            return   kf;
        }
    }

    public static class HashExt
    {
        public static uint HashFNV1a64(this byte[] array) =>
            array.HashMurmurHash(array.Length);

        // FNV 1a 64-bit Modified
        // 0x1403B04D0 in SBZV_7.10
        public static ulong HashFNV1a64m(this byte[] array, int length)
        {
            int i = 0;
            ulong hash = 0xCBF29CE484222325;
            while (i < length)
            { hash = 0x100000001B3 * (hash ^ (ulong)array[i]); i++; }
            return (hash >> 32) ^ hash; // Actual Modification
        }

        public static uint HashMurmurHash(this byte[] array, uint salt = 0,
            bool alreadyUpper = false, bool bigEndian = false) =>
            array.HashMurmurHash(array.Length, salt, alreadyUpper, bigEndian);

        // MurmurHash
        // 0x814D7A9C in PCSB00554
        // 0x8134C304 in PCSB01007
        // 0x0069CEA4 in NPEB02013
        public static uint HashMurmurHash(this byte[] array, int length, uint salt = 0,
            bool alreadyUpper = false, bool bigEndian = false)
        {
            uint a, b, hash;
            int i;

            const uint m = 0x7FD652AD;
            const int r = 16;

            hash = salt + 0xDEADBEEF;
            if (alreadyUpper)
            {
                if (bigEndian)
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 3] | ((uint)array[i + 2] << 8)
                            | ((uint)array[i + 1] << 16) | ((uint)array[i + 0] << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }
                else
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 0] | ((uint)array[i + 1] << 8)
                            | ((uint)array[i + 2] << 16) | ((uint)array[i + 3] << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }

                if (length > 0)
                {
                    if (length > 1)
                    {
                        if (length > 2)
                            hash += (uint)array[i + 2] << 16;
                        hash += (uint)array[i + 1] << 8;
                    }
                    hash += array[i];
                    hash *= m;
                    hash ^= hash >> r;
                }
            }
            else
            {
                if (bigEndian)
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 3] | ((uint)array[i + 2] << 8)
                            | ((uint)array[i + 1] << 16) | ((uint)array[i + 0] << 24);
                        a =  b        & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFFFF00) | a        ;
                        a = (b >>  8) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFF00FF) | (a <<  8);
                        a = (b >> 16) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFF00FFFF) | (a << 16);
                        a = (b >> 24) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0x00FFFFFF) | (a << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }
                else
                    for (i = 0; length > 3; length -= 4, i += 4)
                    {
                        b = (uint)array[i + 0] | ((uint)array[i + 1] << 8)
                            | ((uint)array[i + 2] << 16) | ((uint)array[i + 3] << 24);
                        a =  b        & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFFFF00) | a        ;
                        a = (b >>  8) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFFFF00FF) | (a <<  8);
                        a = (b >> 16) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0xFF00FFFF) | (a << 16);
                        a = (b >> 24) & 0xFF; if (a > 0x60 && a < 0x7B) a -= 0x20; b = (b & 0x00FFFFFF) | (a << 24);
                        hash += b;
                        hash *= m;
                        hash ^= hash >> r;
                    }

                if (length > 0)
                {
                    if (length > 1)
                    {
                        if (length > 2)
                        {
                            b = array[i + 2]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b << 16;
                        }
                        b = array[i + 1]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b << 8;
                    }
                    b = array[i]; if (b > 0x60 && b < 0x7B) b -= 0x20; hash += b;
                    hash *= m;
                    hash ^= hash >> r;
                }
            }

            hash *= m;
            hash ^= hash >> 10;
            hash *= m;
            hash ^= hash >> 17;
            return hash;
        }

        private static readonly ushort[] CRC16_CCITT_Table = {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
            0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
            0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
            0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
            0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
            0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
            0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
            0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
            0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
            0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
            0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
            0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
            0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
            0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
            0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
            0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
            0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
            0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
            0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
            0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
            0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
            0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
            0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
            0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
            0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
            0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
            0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0,
        };

        // CRC16-CCITT
        // 0x140011A90 in SBZV_7.10
        public static ushort HashCRC16_CCITT(this byte[] data, int offset = 0, ushort seed = 0xFFFF)
        {
            int length = data.Length;
            ushort hash = seed;
            for (int i = offset; i < length; i++)
                hash = (ushort)(CRC16_CCITT_Table[(hash >> 8) ^ data[i]] ^ (hash << 8));
            return hash;
        }
    }
}
