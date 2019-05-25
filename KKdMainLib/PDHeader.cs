using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct PDHead
    {
        public int ID;
        public int Lenght;
        public int DataSize;
        public int Signature;
        public int SectionSize;
        public int RealSignature;
        public Main.Format Format;
        public bool IsBE => Format == Main.Format.F2BE;
        public bool IsX => Format == Main.Format.X || Format == Main.Format.XHD;
    }

    public static class PDHeadExtensions
    {
        public static PDHead ReadHeader(this Stream stream, bool Seek)
        {
            if (Seek)
                if (stream.Position >= 4) stream.Seek(-4, SeekOrigin.Current);
                else                      stream.Seek( 0, 0);
            return stream.ReadHeader();
        }

        public static PDHead ReadHeader(this Stream stream)
        {
            long Position = stream.LongPosition;
            PDHead Header = new PDHead
            { Format = Main.Format.F2LE, Signature = stream.ReadInt32(),
                DataSize = stream.ReadInt32(), Lenght = stream.ReadInt32() };
            if (stream.ReadUInt32() == 0x18000000)
            { Header.Format = Main.Format.F2BE; }
            Header.ID = stream.ReadInt32();
            Header.SectionSize = stream.ReadInt32();
            stream.IsBE = Header.Format == Main.Format.F2BE;
            stream.Format = Header.Format;
            stream.LongPosition = Position + Header.Lenght;
            Header.Signature = stream.ReadInt32Endian();
            return Header;
        }

        public static void Write(this Stream stream, PDHead Header)
        {
            stream.Write(Header.Signature);
            stream.Write(Header.DataSize);
            stream.Write(Header.Lenght);
            if (Header.Format == Main.Format.F2BE) stream.Write(0x18000000);
            else                                   stream.Write(0x10000000);
            stream.Write(Header.ID);
            stream.Write(Header.SectionSize);
            stream.Write(0x00);
            stream.Write(0x00);
        }

        public static void WriteEOFC(this Stream stream, int ID)
        { PDHead Header = new PDHead { Format = Main.Format.F2LE, ID = ID,
            Lenght = 0x20, Signature = 0x43464F45, }; stream.Write(Header); }

    }
}
