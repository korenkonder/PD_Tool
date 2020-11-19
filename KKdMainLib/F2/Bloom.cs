using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Bloom : System.IDisposable
    {
        private int i;
        private Stream s;
        private Header header;

        public CountPointer<BLT> BLTs;

        public void BLTReader(string file)
        {
            BLTs = default;
            s = File.OpenReader(file + ".blt", true);
            header = s.ReadHeader();
            if (header.Signature != 0x544D4C42 || header.InnerSignature != 0x3) return;
            s.P -= 0x4;
            s.IsBE = header.UseBigEndian;

            BLTs = s.RCPE<BLT>();
            if (BLTs.C < 1) { s.C(); BLTs.C = -1; return; }

            if (BLTs.C > 0 && BLTs.O == 0) { s.C(); BLTs.C = -1; return; }

            s.P = BLTs.O;
            for (i = 0; i < BLTs.C; i++)
            {
                ref BLT blt = ref BLTs.E[i];
                s.RI32E();
                blt.Color     .X = s.RF32E();
                blt.Color     .Y = s.RF32E();
                blt.Color     .Z = s.RF32E();
                blt.Brightpass.X = s.RF32E();
                blt.Brightpass.Y = s.RF32E();
                blt.Brightpass.Z = s.RF32E();
                blt.Range        = s.RF32E();
            }
            s.C();
        }

        public void TXTWriter(string file)
        {
            if (BLTs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("ID,ColorR,ColorG,ColorB,BrightpassR,BrightpassG,BrightpassB,Range\n");
            for (i = 0; i < BLTs.C; i++)
                s.W($"{i},{ BLTs[i]}\n");
            File.WriteAllBytes($"{file}_bloom.txt", s.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; BLTs = default; header = default; disposed = true; } }

        public struct BLT
        {
            public Vec3 Color;
            public Vec3 Brightpass;
            public float Range;

            public override string ToString() =>
                $"{Color.X.ToS(6)},{Color.Y.ToS(6)},{Color.Z.ToS(6)},{Brightpass.X.ToS(6)}" +
                $",{Brightpass.Y.ToS(6)},{Brightpass.Z.ToS(6)},{Range.ToS(6)}";
        }
    }
}
