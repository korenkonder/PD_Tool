using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct ColorCorrection : System.IDisposable
    {
        private int i;
        private Stream _IO;
        private Header header;

        public CountPointer<CCT> CCTs;

        public void CCTReader(string file)
        {
            CCTs = default;
            _IO = File.OpenReader(file + ".cct", true);
            header = _IO.ReadHeader();
            if (header.Signature != 0x54524343 || header.InnerSignature != 0x3) return;
            _IO.P -= 0x4;

            CCTs = _IO.RCPE<CCT>();
            if (CCTs.C < 1) { _IO.C(); CCTs.C = -1; return; }

            if (CCTs.C > 0 && CCTs.O == 0) { _IO.C(); CCTs.C = -1; return; }
            /*{
                _IO.Format = Header.Format = Format.X;
                _IO.Offset = Header.Length;
                _IO.Position = CCTs.Offset;
                CCTs = _IO.ReadCountPointerX<CCT>();
            }*/

            _IO.P = CCTs.O;
            for (i = 0; i < CCTs.C; i++)
            {
                ref CCT cct = ref CCTs.E[i];
                _IO.RI32E();
                cct.Hue        = _IO.RF32E();
                cct.Saturation = _IO.RF32E();
                cct.Lightness  = _IO.RF32E();
                cct.Exposure   = _IO.RF32E();
                cct.Gamma.X    = _IO.RF32E();
                cct.Gamma.Y    = _IO.RF32E();
                cct.Gamma.Z    = _IO.RF32E();
                cct.Contrast   = _IO.RF32E();
            }

            _IO.C();
        }

        public void TXTWriter(string file)
        {
            if (CCTs.C < 1) return;

            _IO = File.OpenWriter();
            _IO.WPSSJIS("ID,Hue,Saturation,Lightness,Exposure,GammaR,GammaG,GammaB,Contrast\n");
            for (i = 0; i < CCTs.C; i++)
                _IO.W(i + "," + CCTs[i] + "\n");
            File.WriteAllBytes(file + "_cc.txt", _IO.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.D(); _IO = null; CCTs = default; header = default; disposed = true; } }

        public struct CCT
        {
            public float Hue;
            public float Saturation;
            public float Lightness;
            public float Exposure;
            public Vec3 Gamma;
            public float Contrast;

            public override string ToString() => Hue       .ToS(6) + "," +
                                                 Saturation.ToS(6) + "," +
                                                 Lightness .ToS(6) + "," +
                                                 Exposure  .ToS(6) + "," +
                                                 Gamma     .ToString(6) + "," +
                                                 Contrast  .ToS(6);
        }
    }
}
