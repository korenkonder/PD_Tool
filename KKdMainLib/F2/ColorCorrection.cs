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
            IO.Position -= 0x4;

            CCTs = IO.ReadCountPointerEndian<CCT>();
            if (CCTs.Count < 1) { IO.Close(); CCTs.Count = -1; return; }

            if (CCTs.Count > 0 && CCTs.Offset == 0) { IO.Close(); CCTs.Count = -1; return; }
            /*{
                IO.Format = Header.Format = Format.X;
                IO.Offset = Header.Length;
                IO.Position = CCTs.Offset;
                CCTs = IO.ReadCountPointerX<CCT>();
            }*/

            IO.Position = CCTs.Offset;
            for (i = 0; i < CCTs.Count; i++)
            {
                ref CCT CCT = ref CCTs.Entries[i];
                IO.ReadInt32Endian();
                CCT.Hue        = IO.ReadSingleEndian();
                CCT.Saturation = IO.ReadSingleEndian();
                CCT.Lightness  = IO.ReadSingleEndian();
                CCT.Exposure   = IO.ReadSingleEndian();
                CCT.Gamma.X    = IO.ReadSingleEndian();
                CCT.Gamma.Y    = IO.ReadSingleEndian();
                CCT.Gamma.Z    = IO.ReadSingleEndian();
                CCT.Contrast   = IO.ReadSingleEndian();
            }

            IO.Close();
        }

        public void TXTWriter(string file)
        {
            if (CCTs.Count < 1) return;

            IO = File.OpenWriter();
            IO.WriteShiftJIS("ID,Hue,Saturation,Lightness,Exposure,GammaR,GammaG,GammaB,Contrast\n");
            for (i = 0; i < CCTs.Count; i++)
                IO.Write(i + "," + CCTs[i] + "\n");
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
