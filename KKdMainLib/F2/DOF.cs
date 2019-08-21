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
            IO.Position -= 0x4;

            DFTs = IO.ReadCountPointerEndian<DFT>();
            if (DFTs.Count < 1) { IO.Close(); DFTs.Count = -1; return; }

            if (DFTs.Count > 0 && DFTs.Offset == 0) { IO.Close(); DFTs.Count = -1; return; }

            IO.Position = DFTs.Offset;
            for (i = 0; i < DFTs.Count; i++)
            {
                ref DFT DFT = ref DFTs.Entries[i];
                IO.ReadInt32Endian();
                DFT.  Focus      = IO.ReadSingleEndian();
                DFT.  FocusRange = IO.ReadSingleEndian();
                DFT.FuzzingRange = IO.ReadSingleEndian();
                DFT.Ratio        = IO.ReadSingleEndian();
                DFT.Quality      = IO.ReadSingleEndian();
                if (IO.IsX) IO.ReadInt32();
            }

            IO.Close();
        }

        public void TXTWriter(string file)
        {
            if (DFTs.Count < 1) return;

            IO = File.OpenWriter();
            IO.WriteShiftJIS("ID,Focus,FocusRange,FuzzingRange,Ratio,Quality\n");
            for (i = 0; i < DFTs.Count; i++)
                IO.Write(i + "," + DFTs[i] + "\n");
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
