using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Bloom : System.IDisposable
    {
        private int i;
        private Stream _IO;
        private Header header;

        public CountPointer<BLT> BLTs;

        public void BLTReader(string file)
        {
            BLTs = default;
            _IO = File.OpenReader(file + ".blt", true);
            header = _IO.ReadHeader();
            if (header.Signature != 0x544D4C42 || header.InnerSignature != 0x3) return;
            _IO.P -= 0x4;

            BLTs = _IO.RCPE<BLT>();
            if (BLTs.C < 1) { _IO.C(); BLTs.C = -1; return; }

            if (BLTs.C > 0 && BLTs.O == 0) { _IO.C(); BLTs.C = -1; return; }
            /*{
                _IO.Format = Header.Format = Format.X;
                _IO.Offset = Header.Length;
                _IO.Position = BLTs.Offset;
                BLTs = _IO.ReadCountPointerX<BLT>();
            }*/

            _IO.P = BLTs.O;
            for (i = 0; i < BLTs.C; i++)
            {
                ref BLT blt = ref BLTs.E[i];
                _IO.RI32E();
                blt.Color     .X = _IO.RF32E();
                blt.Color     .Y = _IO.RF32E();
                blt.Color     .Z = _IO.RF32E();
                blt.Brightpass.X = _IO.RF32E();
                blt.Brightpass.Y = _IO.RF32E();
                blt.Brightpass.Z = _IO.RF32E();
                blt.Range        = _IO.RF32E();
            }
            _IO.C();
        }

        public void TXTWriter(string file)
        {
            if (BLTs.C < 1) return;

            _IO = File.OpenWriter();
            _IO.WPSSJIS("ID,ColorR,ColorG,ColorB,BrightpassR,BrightpassG,BrightpassB,Range\n");
            for (i = 0; i < BLTs.C; i++)
                _IO.W(i + "," + BLTs[i] + "\n");
            File.WriteAllBytes(file + "_bloom.txt", _IO.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); BLTs = default; header = default; disposed = true; } }

        public struct BLT
        {
            public Vector3 Color;
            public Vector3 Brightpass;
            public float Range;

            public override string ToString() => Color     .ToString(6) + "," +
                                                 Brightpass.ToString(6) + "," +
                                                 Range     .ToS(6);
        }
    }
}
