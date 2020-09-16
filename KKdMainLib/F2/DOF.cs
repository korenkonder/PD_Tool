using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct DOF : System.IDisposable
    {
        private int i;
        private Stream s;
        private Header header;

        public CountPointer<DFT> DFTs;

        public void DFTReader(string file)
        {
            DFTs = default;
            s = File.OpenReader(file + ".dft", true);
            header = s.ReadHeader();
            if (header.Signature != 0x54464F44 || header.InnerSignature != 0x3) return;
            s.P -= 0x4;

            DFTs = s.RCPE<DFT>();
            if (DFTs.C < 1) { s.C(); DFTs.C = -1; return; }

            if (DFTs.C > 0 && DFTs.O == 0) { s.C(); DFTs.C = -1; return; }

            s.P = DFTs.O;
            for (i = 0; i < DFTs.C; i++)
            {
                ref DFT DFT = ref DFTs.E[i];
                s.RI32E();
                DFT.  Focus      = s.RF32E();
                DFT.  FocusRange = s.RF32E();
                DFT.FuzzingRange = s.RF32E();
                DFT.Ratio        = s.RF32E();
                DFT.Quality      = s.RF32E();
            }

            s.C();
        }

        public void TXTWriter(string file)
        {
            if (DFTs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("ID,Focus,FocusRange,FuzzingRange,Ratio,Quality\n");
            for (i = 0; i < DFTs.C; i++)
                s.W($"{i},{DFTs[i]}\n");
            File.WriteAllBytes($"{file}_dof.txt", s.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; DFTs = default; header = default; disposed = true; } }

        public struct DFT
        {
            public float Focus;
            public float FocusRange;
            public float FuzzingRange;
            public float Ratio;
            public float Quality;

            public override string ToString() =>
                $"{Focus.ToS(6)},{FocusRange.ToS(6)},{FuzzingRange.ToS(6)},{Ratio.ToS(6)},{Quality.ToS(6)}";
        }
    }
}
