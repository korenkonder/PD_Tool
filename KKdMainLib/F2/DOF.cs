using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct DOF : System.IDisposable
    {
        private int i;
        private Stream _IO;
        private Header header;

        public CountPointer<DFT> DFTs;

        public void DFTReader(string file)
        {
            DFTs = default;
            _IO = File.OpenReader(file + ".dft", true);
            header = _IO.ReadHeader();
            if (header.Signature != 0x54464F44 || header.InnerSignature != 0x3) return;
            _IO.P -= 0x4;

            DFTs = _IO.RCPE<DFT>();
            if (DFTs.C < 1) { _IO.C(); DFTs.C = -1; return; }

            if (DFTs.C > 0 && DFTs.O == 0) { _IO.C(); DFTs.C = -1; return; }
            /*{
                _IO.Format = Header.Format = Format.X;
                _IO.Offset = Header.Length;
                _IO.Position = DFTs.Offset;
                DFTs = _IO.ReadCountPointerX<DFT>();
            }*/

            _IO.P = DFTs.O;
            for (i = 0; i < DFTs.C; i++)
            {
                ref DFT DFT = ref DFTs.E[i];
                _IO.RI32E();
                DFT.  Focus      = _IO.RF32E();
                DFT.  FocusRange = _IO.RF32E();
                DFT.FuzzingRange = _IO.RF32E();
                DFT.Ratio        = _IO.RF32E();
                DFT.Quality      = _IO.RF32E();
                if (_IO.IsX) _IO.RI32();
            }

            _IO.C();
        }

        public void TXTWriter(string file)
        {
            if (DFTs.C < 1) return;

            _IO = File.OpenWriter();
            _IO.WPSSJIS("ID,Focus,FocusRange,FuzzingRange,Ratio,Quality\n");
            for (i = 0; i < DFTs.C; i++)
                _IO.W(i + "," + DFTs[i] + "\n");
            File.WriteAllBytes(file + "_dof.txt", _IO.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); DFTs = default; header = default; disposed = true; } }

        public struct DFT
        {
            public float Focus;
            public float FocusRange;
            public float FuzzingRange;
            public float Ratio;
            public float Quality;

            public override string ToString() =>   Focus     .ToS(6) + "," +
                                                   FocusRange.ToS(6) + "," +
                                                 FuzzingRange.ToS(6) + "," +
                                                 Ratio       .ToS(6) + "," +
                                                 Quality     .ToS(6);
        }
    }
}
