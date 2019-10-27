using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Bloom
    {
        public CountPointer<BLT> BLTs;
        private Stream IO;
        private Header Header;
        private int i;

        public void BLTReader(string file)
        {
            BLTs = default;
            IO = File.OpenReader(file + ".blt", true);
            Header = IO.ReadHeader();
            if (Header.Signature != 0x544D4C42 || Header.InnerSignature != 0x3) return;
            IO.P -= 0x4;

            BLTs = IO.RCPE<BLT>();
            if (BLTs.C < 1) { IO.C(); BLTs.C = -1; return; }

            if (BLTs.C > 0 && BLTs.O == 0) { IO.C(); BLTs.C = -1; return; }
            /*{
                IO.Format = Header.Format = Format.X;
                IO.Offset = Header.Length;
                IO.Position = BLTs.Offset;
                BLTs = IO.ReadCountPointerX<BLT>();
            }*/

            IO.P = BLTs.O;
            for (i = 0; i < BLTs.C; i++)
            {
                ref BLT BLT = ref BLTs.E[i];
                IO.RI32E();
                BLT.Color     .X = IO.RF32E();
                BLT.Color     .Y = IO.RF32E();
                BLT.Color     .Z = IO.RF32E();
                BLT.Brightpass.X = IO.RF32E();
                BLT.Brightpass.Y = IO.RF32E();
                BLT.Brightpass.Z = IO.RF32E();
                BLT.Range        = IO.RF32E();
            }
            IO.C();
        }

        public void TXTWriter(string file)
        {
            if (BLTs.C < 1) return;

            IO = File.OpenWriter();
            IO.WPSSJIS("ID,ColorR,ColorG,ColorB,BrightpassR,BrightpassG,BrightpassB,Range\n");
            for (i = 0; i < BLTs.C; i++)
                IO.W(i + "," + BLTs[i] + "\n");
            File.WriteAllBytes(file + "_bloom.txt", IO.ToArray(true));
        }

        public struct BLT
        {
            public Vector3 Color;
            public Vector3 Brightpass;
            public float Range;

            public override string ToString() => Color     .ToString(6) + "," +
                                                 Brightpass.ToString(6) + "," +
                                                 Range     .ToString(6);
        }
    }
}
