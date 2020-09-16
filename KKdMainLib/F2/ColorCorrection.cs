using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct ColorCorrection : System.IDisposable
    {
        private int i;
        private Stream s;
        private Header header;

        public CountPointer<CCT> CCTs;

        public void CCTReader(string file)
        {
            CCTs = default;
            s = File.OpenReader(file + ".cct", true);
            header = s.ReadHeader();
            if (header.Signature != 0x54524343 || header.InnerSignature != 0x3) return;
            s.P -= 0x4;

            CCTs = s.RCPE<CCT>();
            if (CCTs.C < 1) { s.C(); CCTs.C = -1; return; }

            if (CCTs.C > 0 && CCTs.O == 0) { s.C(); CCTs.C = -1; return; }

            s.P = CCTs.O;
            for (i = 0; i < CCTs.C; i++)
            {
                ref CCT cct = ref CCTs.E[i];
                s.RI32E();
                cct.Hue        = s.RF32E();
                cct.Saturation = s.RF32E();
                cct.Lightness  = s.RF32E();
                cct.Exposure   = s.RF32E();
                cct.Gamma.X    = s.RF32E();
                cct.Gamma.Y    = s.RF32E();
                cct.Gamma.Z    = s.RF32E();
                cct.Contrast   = s.RF32E();
            }

            s.C();
        }

        public void TXTWriter(string file)
        {
            if (CCTs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("ID,Hue,Saturation,Lightness,Exposure,GammaR,GammaG,GammaB,Contrast\n");
            for (i = 0; i < CCTs.C; i++)
                s.W($"{i},{CCTs[i]}\n");
            File.WriteAllBytes($"{file}_cc.txt", s.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; CCTs = default; header = default; disposed = true; } }

        public struct CCT
        {
            public float Hue;
            public float Saturation;
            public float Lightness;
            public float Exposure;
            public Vec3 Gamma;
            public float Contrast;

            public override string ToString() =>
                $"{Hue.ToS(6)},{Saturation.ToS(6)},{Lightness.ToS(6)},{Exposure.ToS(6)}" +
                $",{Gamma.X.ToS(6)},{Gamma.Y.ToS(6)},{Gamma.Z.ToS(6)},{Contrast.ToS(6)}";
        }
    }
}
