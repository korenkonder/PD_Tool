using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct ColorCorrection
    {
        public CountPointer<CCT> CCTs;
        private Stream IO;
        private Header Header;
        private int i;

        public void CCTReader(string file)
        {
            CCTs = default;
            IO = File.OpenReader(file + ".cct", true);
            Header = IO.ReadHeader();
            if (Header.Signature != 0x54524343 || Header.InnerSignature != 0x3) return;
            IO.P -= 0x4;

            CCTs = IO.RCPE<CCT>();
            if (CCTs.C < 1) { IO.C(); CCTs.C = -1; return; }

            if (CCTs.C > 0 && CCTs.O == 0) { IO.C(); CCTs.C = -1; return; }
            /*{
                IO.Format = Header.Format = Format.X;
                IO.Offset = Header.Length;
                IO.Position = CCTs.Offset;
                CCTs = IO.ReadCountPointerX<CCT>();
            }*/

            IO.P = CCTs.O;
            for (i = 0; i < CCTs.C; i++)
            {
                ref CCT CCT = ref CCTs.E[i];
                IO.RI32E();
                CCT.Hue        = IO.RF32E();
                CCT.Saturation = IO.RF32E();
                CCT.Lightness  = IO.RF32E();
                CCT.Exposure   = IO.RF32E();
                CCT.Gamma.X    = IO.RF32E();
                CCT.Gamma.Y    = IO.RF32E();
                CCT.Gamma.Z    = IO.RF32E();
                CCT.Contrast   = IO.RF32E();
            }

            IO.C();
        }

        public void TXTWriter(string file)
        {
            if (CCTs.C < 1) return;

            IO = File.OpenWriter();
            IO.WPSSJIS("ID,Hue,Saturation,Lightness,Exposure,GammaR,GammaG,GammaB,Contrast\n");
            for (i = 0; i < CCTs.C; i++)
                IO.W(i + "," + CCTs[i] + "\n");
            File.WriteAllBytes(file + "_cc.txt", IO.ToArray(true));
        }

        public struct CCT
        {
            public float Hue;
            public float Saturation;
            public float Lightness;
            public float Exposure;
            public Vector3 Gamma;
            public float Contrast;

            public override string ToString() => Hue       .ToString(6) + "," +
                                                 Saturation.ToString(6) + "," +
                                                 Lightness .ToString(6) + "," +
                                                 Exposure  .ToString(6) + "," +
                                                 Gamma     .ToString(6) + "," +
                                                 Contrast  .ToString(6);
        }
    }
}
