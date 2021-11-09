using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Bloom : System.IDisposable
    {
        private int i;
        private Stream s;

        public bool IsX;
        public CountPointer<BLT> BLTs;

        public void BLTReader(string file)
        {
            IsX = false;
            BLTs = default;
            byte[] bltData = File.ReadAllBytes(file + ".blt");
            Struct _BLMT = bltData.RSt(); bltData = null;
            if (_BLMT.Header.Signature != 0x544D4C42) return;

            s = File.OpenReader(_BLMT.Data);
            s.IsBE = _BLMT.Header.UseBigEndian;

            BLTs = s.RCPE<BLT>();
            if (BLTs.C < 1) { s.C(); BLTs.C = -1; return; }

            if (!(BLTs.C > 0 && BLTs.O == 0))
            {
                s.PI64 = BLTs.O - _BLMT.Header.Length;
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    blt.Flags = (Flags)s.RI32E();
                    blt.Color     .X = s.RF32E();
                    blt.Color     .Y = s.RF32E();
                    blt.Color     .Z = s.RF32E();
                    blt.Brightpass.X = s.RF32E();
                    blt.Brightpass.Y = s.RF32E();
                    blt.Brightpass.Z = s.RF32E();
                    blt.Range        = s.RF32E();
                }
            }
            else
            {
                IsX = true;
                BLTs.O = (int)s.RI64E();
                s.PI64 = BLTs.O;
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    blt.Flags = (Flags)s.RI32E();
                    blt.Color     .X = s.RF32E();
                    blt.Color     .Y = s.RF32E();
                    blt.Color     .Z = s.RF32E();
                    blt.Brightpass.X = s.RF32E();
                    blt.Brightpass.Y = s.RF32E();
                    blt.Brightpass.Z = s.RF32E();
                    blt.Range        = s.RF32E();
                    blt.Unk1         = s.RF32 ();
                    blt.Unk2         = s.RI32 ();
                    blt.Unk3         = s.RF32 ();
                }
            }
            s.C();
        }

        public void BLTWriter(string file)
        {
            if (BLTs.E == null || BLTs.C < 1) return;

            if (file.EndsWith("_bloom"))
                file = file.Substring(0, file.Length - 6);

            s = File.OpenWriter();
            s.IsBE = false;
            s.Format = Format.F2;

            ENRS e = default;
            POF p = default;
            p.Offsets = KKdList<long>.New;
            if (!IsX)
            {
                s.W(BLTs.C);
                s.W(0x50);
                s.A(0x10);

                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    s.W((int)blt.Flags);
                    s.W(blt.Color     .X);
                    s.W(blt.Color     .Y);
                    s.W(blt.Color     .Z);
                    s.W(blt.Brightpass.X);
                    s.W(blt.Brightpass.Y);
                    s.W(blt.Brightpass.Z);
                    s.W(blt.Range       );
                }

                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x01, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x20, BLTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x08, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x44);
            }
            else
            {
                s.W(BLTs.C);
                s.A(0x08);
                s.W(0x10L);
                s.A(0x10);

                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    s.W((int)blt.Flags);
                    s.W(blt.Color     .X);
                    s.W(blt.Color     .Y);
                    s.W(blt.Color     .Z);
                    s.W(blt.Brightpass.X);
                    s.W(blt.Brightpass.Y);
                    s.W(blt.Brightpass.Z);
                    s.W(blt.Range       );
                    s.W(blt.Unk1        );
                    s.W(blt.Unk2        );
                    s.W(blt.Unk3        );
                }

                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x02, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x01, ENRS.ENRSEntry.Type.DWORD);
                e.Array[0].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x04, 0x01, ENRS.ENRSEntry.Type.QWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x2C, BLTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x08, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x08);
            }

            s.A(0x10, true);
            byte[] data = s.ToArray(true);

            Struct _BLMT = default;
            _BLMT.Header.Signature = 0x544D4C42;
            _BLMT.Header.DataSize = (uint)data.Length;
            _BLMT.Header.Length = 0x40;
            _BLMT.Header.Depth = 0;
            _BLMT.Header.SectionSize = (uint)data.Length;
            _BLMT.Header.Version = 0;
            _BLMT.Header.InnerSignature = 0x03;

            _BLMT.Header.UseBigEndian = false;
            _BLMT.Header.UseSectionSize = true;
            _BLMT.Header.Format = Format.F2;
            _BLMT.Data = data;
            _BLMT.ENRS = e;
            _BLMT.POF  = p;
            File.WriteAllBytes(file + ".blt", _BLMT.W(IsX, true));
        }

        public void MsgPackReader(string file, bool json)
        {
            IsX = false;
            BLTs = default;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack bloom;
            if ((bloom = msgPack["Bloom", true]).NotNull)
            {
                BLTs.E = new BLT[bloom.Array.Length];
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    blt = default;
                    MsgPack b = bloom[i];
                    MsgPack temp;
                    if ((temp = b["Color"]).NotNull)
                    {
                        blt.Flags |= Flags.Color;
                        blt.Color.X = temp.RF32("X");
                        blt.Color.Y = temp.RF32("Y");
                        blt.Color.Z = temp.RF32("Z");
                    }

                    if ((temp = b["Brightpass"]).NotNull)
                    {
                        blt.Flags |= Flags.Brightpass;
                        blt.Brightpass.X = temp.RF32("X");
                        blt.Brightpass.Y = temp.RF32("Y");
                        blt.Brightpass.Z = temp.RF32("Z");
                    }

                    if ((temp = b["Range"]).Object != null)
                    {
                        blt.Flags |= Flags.Range;
                        blt.Range = temp.RF32();
                    }
                }
            }
            else if ((bloom = msgPack["BloomX", true]).NotNull)
            {
                IsX = true;
                s.IsX = true;
                BLTs.E = new BLT[bloom.Array.Length];
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    blt = default;
                    MsgPack b = bloom[i];
                    MsgPack temp;
                    if ((temp = b["Color"]).NotNull)
                    {
                        blt.Flags |= Flags.Color;
                        blt.Color.X = temp.RF32("X");
                        blt.Color.Y = temp.RF32("Y");
                        blt.Color.Z = temp.RF32("Z");
                    }

                    if ((temp = b["Brightpass"]).NotNull)
                    {
                        blt.Flags |= Flags.Brightpass;
                        blt.Brightpass.X = temp.RF32("X");
                        blt.Brightpass.Y = temp.RF32("Y");
                        blt.Brightpass.Z = temp.RF32("Z");
                    }

                    if ((temp = b["Range"]).Object != null)
                    {
                        blt.Flags |= Flags.Range;
                        blt.Range = temp.RF32();
                    }

                    if ((temp = b["Unk1"]).Object != null)
                    {
                        blt.Flags |= Flags.Unk1;
                        blt.Unk1 = temp.RF32();
                    }

                    if ((temp = b["Unk2"]).Object != null)
                    {
                        blt.Flags |= Flags.Unk2;
                        blt.Unk2 = temp.RI32();
                    }

                    if ((temp = b["Unk3"]).Object != null)
                    {
                        blt.Flags |= Flags.Unk3;
                        blt.Unk3 = temp.RF32();
                    }
                }
            }
            bloom.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (BLTs.E == null || BLTs.C < 1) return;

            MsgPack bloom;
            if (!IsX)
            {
                bloom = new MsgPack(BLTs.C, "Bloom");
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    MsgPack b = MsgPack.New;
                    if ((blt.Flags & Flags.Color) != 0)
                        b.Add(new MsgPack("Color")
                            .Add("X", blt.Color.X)
                            .Add("Y", blt.Color.Y)
                            .Add("Z", blt.Color.Z));
                    if ((blt.Flags & Flags.Brightpass) != 0)
                        b.Add(new MsgPack("Brightpass")
                            .Add("X", blt.Brightpass.X)
                            .Add("Y", blt.Brightpass.Y)
                            .Add("Z", blt.Brightpass.Z));
                    if ((blt.Flags & Flags.Range) != 0)
                        b.Add("Range", blt.Range);
                    bloom[i] = b;
                }
            }
            else
            {
                bloom = new MsgPack(BLTs.C, "BloomX");
                for (i = 0; i < BLTs.C; i++)
                {
                    ref BLT blt = ref BLTs.E[i];
                    MsgPack b = MsgPack.New;
                    if ((blt.Flags & Flags.Color) != 0)
                        b.Add(new MsgPack("Color")
                            .Add("X", blt.Color.X)
                            .Add("Y", blt.Color.Y)
                            .Add("Z", blt.Color.Z));
                    if ((blt.Flags & Flags.Brightpass) != 0)
                        b.Add(new MsgPack("Brightpass")
                            .Add("X", blt.Brightpass.X)
                            .Add("Y", blt.Brightpass.Y)
                            .Add("Z", blt.Brightpass.Z));
                    if ((blt.Flags & Flags.Range) != 0)
                        b.Add("Range", blt.Range);
                    if ((blt.Flags & Flags.Unk1) != 0)
                        b.Add("Unk1", blt.Unk1);
                    if ((blt.Flags & Flags.Unk2) != 0)
                        b.Add("Unk2", blt.Unk2);
                    if ((blt.Flags & Flags.Unk3) != 0)
                        b.Add("Unk3", blt.Unk3);
                    bloom[i] = b;
                }
            }
            bloom.Write(false, true, file + "_bloom", json);
        }

        public void TXTWriter(string file)
        {
            if (BLTs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("ID,ColorR,ColorG,ColorB,BrightpassR,BrightpassG,BrightpassB,Range\n");
            for (i = 0; i < BLTs.C; i++)
                s.W($"{i},{ BLTs[i]}\n");
            File.WriteAllBytes($"{file}_bloom.txt", s.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; BLTs = default; IsX = false; disposed = true; } }

        public struct BLT
        {
            public Flags Flags;
            public  Vec3 Color;
            public  Vec3 Brightpass;
            public float Range;
            public float Unk1;
            public   int Unk2;
            public float Unk3;

            public override string ToString() =>
                ((Flags & Flags.Color) != 0
                ? $"{Color.X.ToS(6)},{Color.Y.ToS(6)},{Color.Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.Brightpass) != 0
                ? $"{Brightpass.X.ToS(6)},{Brightpass.Y.ToS(6)},{Brightpass.Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.Range) != 0
                ? $"{Range.ToS(6)}" : "");
        }

        public enum Flags : int
        {
            Color      = 0b000001,
            Brightpass = 0b000010,
            Range      = 0b000100,
            Unk1       = 0b001000,
            Unk2       = 0b010000,
            Unk3       = 0b100000,
        }
    }
}
