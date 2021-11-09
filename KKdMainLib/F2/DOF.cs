using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct DOF : System.IDisposable
    {
        private int i;
        private Stream s;

        public bool IsX;
        public CountPointer<DFT> DFTs;

        public void DFTReader(string file)
        {
            IsX = false;
            DFTs = default;
            byte[] dftData = File.ReadAllBytes(file + ".dft");
            Struct _DOFT = dftData.RSt(); dftData = null;
            if (_DOFT.Header.Signature != 0x54464F44) return;

            s = File.OpenReader(_DOFT.Data);
            s.IsBE = _DOFT.Header.UseBigEndian;

            DFTs = s.RCPE<DFT>();
            if (DFTs.C < 1) { s.C(); DFTs.C = -1; return; }

            if (!(DFTs.C > 0 && DFTs.O == 0))
            {
                s.PI64 = DFTs.O - _DOFT.Header.Length;
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    dft.Flags = (Flags)s.RI32E();
                    dft.Focus        = s.RF32E();
                    dft.FocusRange   = s.RF32E();
                    dft.FuzzingRange = s.RF32E();
                    dft.Ratio        = s.RF32E();
                    dft.Quality      = s.RF32E();
                }
            }
            else
            {
                IsX = true;
                s.IsX = true;
                DFTs.O = (int)s.RI64E();
                s.PI64 = DFTs.O;
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    dft.Flags = (Flags)s.RI32E();
                    dft.Focus        = s.RF32E();
                    dft.FocusRange   = s.RF32E();
                    dft.FuzzingRange = s.RF32E();
                    dft.Ratio        = s.RF32E();
                    dft.Quality      = s.RF32E();
                    dft.Unk          = s.RI32 ();
                }
            }
            s.C();
        }

        public void DFTWriter(string file)
        {
            if (DFTs.E == null || DFTs.C < 1) return;

            if (file.EndsWith("_dof"))
                file = file.Substring(0, file.Length - 4);

            s = File.OpenWriter();
            s.IsBE = false;
            s.Format = Format.F2;

            ENRS e = default;
            POF p = default;
            p.Offsets = KKdList<long>.New;
            if (!IsX)
            {
                s.W(DFTs.C);
                s.W(0x50);
                s.A(0x10);

                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    s.W((int)dft.Flags);
                    s.W(dft.Focus       );
                    s.W(dft.FocusRange  );
                    s.W(dft.FuzzingRange);
                    s.W(dft.Ratio       );
                    s.W(dft.Quality     );
                }

                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x01, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x18, DFTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x06, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x44);
            }
            else
            {
                s.W(DFTs.C);
                s.A(0x08);
                s.W(0x10L);
                s.A(0x10);

                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    s.W((int)dft.Flags);
                    s.W(dft.Focus       );
                    s.W(dft.FocusRange  );
                    s.W(dft.FuzzingRange);
                    s.W(dft.Ratio       );
                    s.W(dft.Quality     );
                    s.W(dft.Unk         );
                }

                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x02, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x01, ENRS.ENRSEntry.Type.DWORD);
                e.Array[0].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x04, 0x01, ENRS.ENRSEntry.Type.QWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x1C, DFTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x06, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x08);
            }

            s.A(0x10, true);
            byte[] data = s.ToArray(true);

            Struct _DOFT = default;
            _DOFT.Header.Signature = 0x54464F44;
            _DOFT.Header.DataSize = (uint)data.Length;
            _DOFT.Header.Length = 0x40;
            _DOFT.Header.Depth = 0;
            _DOFT.Header.SectionSize = (uint)data.Length;
            _DOFT.Header.Version = 0;
            _DOFT.Header.InnerSignature = 0x03;

            _DOFT.Header.UseBigEndian = false;
            _DOFT.Header.UseSectionSize = true;
            _DOFT.Header.Format = Format.F2;
            _DOFT.Data = data;
            _DOFT.ENRS = e;
            _DOFT.POF  = p;
            File.WriteAllBytes(file + ".dft", _DOFT.W(IsX, true));
        }

        public void MsgPackReader(string file, bool json)
        {
            IsX = false;
            DFTs = default;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack dof;
            if ((dof = msgPack["DOF", true]).NotNull)
            {
                DFTs.E = new DFT[dof.Array.Length];
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    dft = default;
                    MsgPack d = dof[i];
                    MsgPack temp;
                    if ((temp = d["Focus"]).Object != null)
                    {
                        dft.Flags |= Flags.Focus;
                        dft.Focus = temp.RF32();
                    }

                    if ((temp = d["FocusRange"]).Object != null)
                    {
                        dft.Flags |= Flags.FocusRange;
                        dft.FocusRange = temp.RF32();
                    }

                    if ((temp = d["FuzzingRange"]).Object != null)
                    {
                        dft.Flags |= Flags.FuzzingRange;
                        dft.FuzzingRange = temp.RF32();
                    }

                    if ((temp = d["Ratio"]).Object != null)
                    {
                        dft.Flags |= Flags.Ratio;
                        dft.Ratio = temp.RF32();
                    }

                    if ((temp = d["Quality"]).Object != null)
                    {
                        dft.Flags |= Flags.Quality;
                        dft.Quality = temp.RF32();
                    }
                }
            }
            else if ((dof = msgPack["DOFX", true]).NotNull)
            {
                IsX = true;
                DFTs.E = new DFT[dof.Array.Length];
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    dft = default;
                    MsgPack d = dof[i];
                    MsgPack temp;
                    if ((temp = d["Focus"]).Object != null)
                    {
                        dft.Flags |= Flags.Focus;
                        dft.Focus = temp.RF32();
                    }

                    if ((temp = d["FocusRange"]).Object != null)
                    {
                        dft.Flags |= Flags.FocusRange;
                        dft.FocusRange = temp.RF32();
                    }

                    if ((temp = d["FuzzingRange"]).Object != null)
                    {
                        dft.Flags |= Flags.FuzzingRange;
                        dft.FuzzingRange = temp.RF32();
                    }

                    if ((temp = d["Ratio"]).Object != null)
                    {
                        dft.Flags |= Flags.Ratio;
                        dft.Ratio = temp.RF32();
                    }

                    if ((temp = d["Quality"]).Object != null)
                    {
                        dft.Flags |= Flags.Quality;
                        dft.Quality = temp.RF32();
                    }

                    if ((temp = d["Unk"]).Object != null)
                    {
                        dft.Flags |= Flags.Unk;
                        dft.Unk = temp.RI32();
                    }
                }
            }
            dof.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (DFTs.E == null || DFTs.C < 1) return;

            MsgPack dof;
            if (!IsX)
            {
                dof = new MsgPack(DFTs.C, "DOF");
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    MsgPack d = MsgPack.New;
                    if ((dft.Flags & Flags.Focus) != 0)
                        d.Add("Focus", dft.Focus);
                    if ((dft.Flags & Flags.FocusRange) != 0)
                        d.Add("FocusRange", dft.FocusRange);
                    if ((dft.Flags & Flags.FuzzingRange) != 0)
                        d.Add("FuzzingRange", dft.FuzzingRange);
                    if ((dft.Flags & Flags.Ratio) != 0)
                        d.Add("Ratio", dft.Ratio);
                    if ((dft.Flags & Flags.Quality) != 0)
                        d.Add("Quality", dft.Quality);
                    dof[i] = d;
                }
            }
            else
            {
                dof = new MsgPack(DFTs.C, "DOFX");
                for (i = 0; i < DFTs.C; i++)
                {
                    ref DFT dft = ref DFTs.E[i];
                    MsgPack d = MsgPack.New;
                    if ((dft.Flags & Flags.Focus) != 0)
                        d.Add("Focus", dft.Focus);
                    if ((dft.Flags & Flags.FocusRange) != 0)
                        d.Add("FocusRange", dft.FocusRange);
                    if ((dft.Flags & Flags.FuzzingRange) != 0)
                        d.Add("FuzzingRange", dft.FuzzingRange);
                    if ((dft.Flags & Flags.Ratio) != 0)
                        d.Add("Ratio", dft.Ratio);
                    if ((dft.Flags & Flags.Quality) != 0)
                        d.Add("Quality", dft.Quality);
                    if ((dft.Flags & Flags.Unk) != 0)
                        d.Add("Unk", dft.Unk);
                    dof[i] = d;
                }
            }
            dof.Write(false, true, file + "_dof", json);
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
        { if (!disposed) { if (s != null) s.D(); s = null; DFTs = default; IsX = false; disposed = true; } }

        public struct DFT
        {
            public Flags Flags;
            public float Focus;
            public float FocusRange;
            public float FuzzingRange;
            public float Ratio;
            public float Quality;
            public   int Unk;

            public override string ToString() =>
                ((Flags & Flags.Focus) != 0
                ? $"{Focus.ToS(6)}," : ",") +
                ((Flags & Flags.FocusRange) != 0
                ? $"{FocusRange.ToS(6)}," : ",") +
                ((Flags & Flags.FuzzingRange) != 0
                ? $"{FuzzingRange.ToS(6)}," : ",") +
                ((Flags & Flags.Ratio) != 0
                ? $"{Ratio.ToS(6)}," : ",") +
                ((Flags & Flags.Quality) != 0
                ? $"{Quality.ToS(6)}" : "");
        }

        public enum Flags : int
        {
            Focus        = 0b000001,
            FocusRange   = 0b000010,
            FuzzingRange = 0b000100,
            Ratio        = 0b001000,
            Quality      = 0b010000,
            Unk          = 0b100000,
        }
    }
}
