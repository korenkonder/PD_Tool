using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct DOF
    {
        public CountPointer<DFT> DFTs;
        private Stream IO;
        private Header Header;
        private int i;

        public void DFTReader(string file)
        {
            DFTs = default;
            IO = File.OpenReader(file + ".dft", true);
            Header = IO.ReadHeader();
            if (Header.Signature != 0x54464F44 || Header.InnerSignature != 0x3) return;
            IO.P -= 0x4;

            DFTs = IO.RCPE<DFT>();
            if (DFTs.C < 1) { IO.C(); DFTs.C = -1; return; }

            if (DFTs.C > 0 && DFTs.O == 0) { IO.C(); DFTs.C = -1; return; }
            /*{
                IO.Format = Header.Format = Format.X;
                IO.Offset = Header.Length;
                IO.Position = DFTs.Offset;
                DFTs = IO.ReadCountPointerX<DFT>();
            }*/

            IO.P = DFTs.O;
            for (i = 0; i < DFTs.C; i++)
            {
                ref DFT DFT = ref DFTs.E[i];
                IO.RI32E();
                DFT.  Focus      = IO.RF32E();
                DFT.  FocusRange = IO.RF32E();
                DFT.FuzzingRange = IO.RF32E();
                DFT.Ratio        = IO.RF32E();
                DFT.Quality      = IO.RF32E();
                if (IO.IsX) IO.RI32();
            }

            IO.C();
        }

        public void TXTWriter(string file)
        {
            if (DFTs.C < 1) return;

            IO = File.OpenWriter();
            IO.WPSSJIS("ID,Focus,FocusRange,FuzzingRange,Ratio,Quality\n");
            for (i = 0; i < DFTs.C; i++)
                IO.W(i + "," + DFTs[i] + "\n");
            File.WriteAllBytes(file + "_dof.txt", IO.ToArray(true));
        }

        public struct DFT
        {
            public float Focus;
            public float FocusRange;
            public float FuzzingRange;
            public float Ratio;
            public float Quality;

            public override string ToString() =>   Focus     .ToString(6) + "," +
                                                   FocusRange.ToString(6) + "," +
                                                 FuzzingRange.ToString(6) + "," +
                                                 Ratio       .ToString(6) + "," +
                                                 Quality     .ToString(6);
        }
    }
}
