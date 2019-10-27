using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public static class HeaderExtensions
    {
        public static Header ReadHeader(this Stream stream, bool Seek, bool ReadSectionSignature = true)
        {
            if (Seek)
                if (stream.I64P > 4) stream.I64P -= 4;
                else                 stream.I64P  = 0;
            return stream.ReadHeader(ReadSectionSignature);
        }

        public static Header ReadHeader(this Stream stream, bool ReadSectionSignature = true)
        {
            Header Header = new Header { Format = Format.F2LE, Signature = stream.RI32(),
                DataSize = stream.RI32(), Length = stream.RI32(), Flags = stream.RI32(),
                Depth = stream.RI32(), SectionSize = stream.RI32(),
                Mode = stream.RI32() };
            stream.RI32();
            if ((Header.Flags & 0x08000000) == 0x08000000) Header.Format = Format.F2BE;
            Header.NotUseDataSizeAsSectionSize = (Header.Flags & 0x10000000) != 0x10000000;
            if (Header.Length == 0x40)
            {
                stream.RI64();
                stream.RI64();
                Header.InnerSignature = stream.RI32();
                stream.RI32();
                stream.RI64();
            }
            stream.Format = Header.Format;
            if (ReadSectionSignature) Header.SectionSignature = stream.RI32E();
            return Header;
        }

        public static void W(this Stream stream, Header Header, bool Extended = false)
        {
            Header.Length = (Header.Format < Format.X && Extended) ? 0x40 : 0x20;
            Header.Flags = (!Header.NotUseDataSizeAsSectionSize ? 0x10000000 : 0) |
                            (Header.Format == Format.F2BE ? 0x08000000 : 0);

            stream.W(Header.Signature);
            stream.W(Header.DataSize);
            stream.W(Header.Length);
            stream.W(Header.Flags);
            stream.W(Header.Depth);
            stream.W(Header.SectionSize);
            stream.W(Header.Mode);
            stream.W(0x00);
            if (Header.Length == 0x40)
            {
                stream.W(Header.Format < Format.MGF ? (int)((Header.SectionSignature ^
                    (Header.DataSize * (long)Header.Signature)) - Header.Depth + Header.SectionSize) : 0);
                stream.W(0x00);
                stream.W(0x00L);
                stream.W(Header.InnerSignature);
                stream.W(0x00);
                stream.W(0x00L);
            }
        }

        public static void WEOFC(this Stream stream, int ID = 0) =>
            stream.W(new Header { Depth = ID, Length = 0x20, Signature = 0x43464F45 });
    }

    public static class POFExtensions
    {
        public static void W(this Stream stream, POF POF, bool ShiftX = false)
        {
            byte[] data = POF.Write(POF, ShiftX);
            Header Header = new Header { Depth = POF.Depth, Format = Format.F2LE,
                Length = 0x20, Signature = ShiftX ? 0x31464F50 : 0x30464F50 };
            Header.DataSize = Header.SectionSize = data.Length;
            stream.W(Header);
            stream.W(data);
        }
    }

    public static class ENRSExtensions
    {
        public static void W(this Stream stream, ENRSList ENRS)
        {
            byte[] data = ENRSList.Write(ENRS);
            Header Header = new Header { Depth = ENRS.Depth,
                Format = Format.F2LE, Length = 0x20, Signature = 0x53524E45 };
            Header.DataSize = Header.SectionSize = data.Length;
            stream.W(Header);
            stream.W(data);
        }
    }

    public static class StructExtensions
    {
        public static Struct RSt(this byte[] Data)
        {
            if (Data == null || Data.Length < 1) return default;
            Struct Struct;
            using (Stream stream = File.OpenReader(Data))
                Struct = stream.RSt(stream.ReadHeader(false));
            return Struct;
        }

        public static Struct RSt(this Stream stream, Header Header)
        {
            Struct Struct = new Struct { Header = Header, DataOffset =
                stream.P, Data = stream.RBy(Header.SectionSize) };
            int Depth = Header.Depth;

            int LastSig = 0, Sig;
            long Length = stream.L - stream.P;
            long Position = 0;
            KKdList<Struct> SubStructs = KKdList<Struct>.New;
            while (Length > Position)
            {
                Header = stream.ReadHeader(false);
                Sig = Header.Signature;
                Position += Header.Length + Header.SectionSize;
                if (Sig == 0x43464F45 && Header.Depth == Depth + 1) break;
                else if (Header.Depth == 0 &&  (Sig == 0x53524E45 ||
                    (Sig & 0xF0FFFFFF) == 0x30464F50 || Sig == 0x43505854))
                {
                    byte[] Data = stream.RBy(Header.SectionSize);
                    if (Sig == 0x53524E45) Struct.ENRS = ENRSList.Read(Data,                    Header.Depth);
                    else                   Struct.POF  = POF     .Read(Data, Sig == 0x31464F50, Header.Depth);
                }
                else if (Header.Depth <= Depth) { stream.I64P -= Header.Length; break; }
                else SubStructs.Add(stream.RSt(Header));
                LastSig = Sig;
            }

            if (SubStructs.Capacity > 0) Struct.SubStructs = SubStructs.ToArray();
            return Struct;
        }

        public static byte[] W(this Struct Struct, bool ShiftX = false)
        {
            byte[] Data;
            using (Stream stream = File.OpenWriter()) { Struct.Update(ShiftX);
                stream.W(Struct, ShiftX); stream.WEOFC(); Data = stream.ToArray(); }
            return Data;
        }

        public static void W(this Stream stream, Struct Struct, bool ShiftX = false)
        {
            stream.W(Struct.Header);
            stream.W(Struct.Data  );
            if (Struct.HasPOF ) stream.W(Struct.POF , ShiftX);
            if (Struct.HasENRS) stream.W(Struct.ENRS);
            if (Struct.HasSubStructs)
            {
                for (int i = 0; i < Struct.SubStructs.Length; i++)
                    stream.W(Struct.SubStructs[i], ShiftX);
                stream.WEOFC(Struct.Depth + 1);
            }
        }
    }
    
    public static class MPExt
    {
        public static MsgPack ReadMP(this byte[] array, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON) using (JSON IO = new JSON(File.OpenReader(array))) MsgPack = IO.Read(    );
            else      using (  MP IO = new   MP(File.OpenReader(array))) MsgPack = IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMPAllAtOnce(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON) using (JSON IO = new JSON(File.OpenReader(file + ".json", true))) MsgPack = IO.Read(    );
            else      using (  MP IO = new   MP(File.OpenReader(file + ".mp"  , true))) MsgPack = IO.Read(true);
            return MsgPack;
        }

        public static MsgPack ReadMP(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON) using (JSON IO = new JSON(File.OpenReader(file + ".json"))) MsgPack = IO.Read(    );
            else      using (  MP IO = new   MP(File.OpenReader(file + ".mp"  ))) MsgPack = IO.Read(true);
            return MsgPack;
        }
        
        public static void Write(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) MsgPack.New.Add(mp).Write(file, JSON).Dispose();
          else                      mp .Write(file, JSON); }

        public static MsgPack Write(this MsgPack mp, string file, bool JSON = false)
        {
            if (JSON) using (JSON IO = new JSON(File.OpenWriter(file + ".json", true))) IO.W(mp, "\n", "  ");
            else      using (  MP IO = new   MP(File.OpenWriter(file + ".json", true))) IO.Write(mp);
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) MsgPack.New.Add(mp).WriteAfterAll(file, JSON).Dispose();
          else                      mp .WriteAfterAll(file, JSON); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool JSON = false)
        {
            byte[] data = null;
            if (JSON) using (JSON IO = new JSON(File.OpenWriter())) { IO.W(mp, true); data = IO.ToArray(); }
            else      using (  MP IO = new   MP(File.OpenWriter())) { IO.Write(mp      ); data = IO.ToArray(); }
            File.WriteAllBytes(file + (JSON ? ".json" : ".mp"), data);
            return mp;
        }

        public static void ToJSON   (this string file) =>
            file.ReadMP(    ).Write(file, true).Dispose();

        public static void ToMsgPack(this string file) =>
            file.ReadMP(true).Write(file      ).Dispose();
    }

    public static class IKFExt
    {
        public static IKF Round(this IKF KF, int d)
        {
                 if (KF is KFT0 KFT0) { KFT0.F  = KFT0.F .Round(d);                             return KFT0; }
            else if (KF is KFT1 KFT1) { KFT1.F  = KFT1.F .Round(d); KFT1.V  = KFT1.V .Round(d); return KFT1; }
            else if (KF is KFT2 KFT2) { KFT2.F  = KFT2.F .Round(d); KFT2.V  = KFT2.V .Round(d);
                                        KFT2.T  = KFT2.T .Round(d);                             return KFT2; }
            else if (KF is KFT3 KFT3) { KFT3.F  = KFT3.F .Round(d); KFT3.V  = KFT3.V .Round(d);
                                        KFT3.T1 = KFT3.T1.Round(d); KFT3.T2 = KFT3.T2.Round(d); return KFT3; }
            return   KF;
        }
    }
}
