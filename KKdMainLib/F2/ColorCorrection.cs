using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct ColorCorrection : System.IDisposable
    {
        private int i;
        private Stream s;

        public bool IsX;
        public CountPointer<CCT> CCTs;

        public void CCTReader(string file)
        {
            IsX = false;
            CCTs = default;
            byte[] ccData = File.ReadAllBytes(file + ".cct");
            Struct _CCRT = ccData.RSt(); ccData = null;
            if (_CCRT.Header.Signature != 0x54524343) return;

            s = File.OpenReader(_CCRT.Data);
            s.IsBE = _CCRT.Header.UseBigEndian;

            CCTs = s.RCPE<CCT>();
            if (CCTs.C < 1) { s.C(); CCTs.C = -1; return; }

            if (!(CCTs.C > 0 && CCTs.O == 0))
            {
                s.PI64 = CCTs.O - _CCRT.Header.Length;
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    cct.Flags = (Flags)s.RI32E();
                    cct.Hue        = s.RF32E();
                    cct.Saturation = s.RF32E();
                    cct.Lightness  = s.RF32E();
                    cct.Exposure   = s.RF32E();
                    cct.Gamma.X    = s.RF32E();
                    cct.Gamma.Y    = s.RF32E();
                    cct.Gamma.Z    = s.RF32E();
                    cct.Contrast   = s.RF32E();
                }
            }
            else
            {
                IsX = true;
                s.IsX = true;
                CCTs.O = (int)s.RI64E();
                s.PI64 = CCTs.O;
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    cct.Flags = (Flags)s.RI32E();
                    cct.Hue        = s.RF32E();
                    cct.Saturation = s.RF32E();
                    cct.Lightness  = s.RF32E();
                    cct.Exposure   = s.RF32E();
                    cct.Gamma.X    = s.RF32E();
                    cct.Gamma.Y    = s.RF32E();
                    cct.Gamma.Z    = s.RF32E();
                    cct.Contrast   = s.RF32E();
                }
            }
            s.C();
        }

        public void CCTWriter(string file)
        {
            if (CCTs.E == null || CCTs.C < 1) return;

            if (file.EndsWith("_cc"))
                file = file.Substring(0, file.Length - 3);

            s = File.OpenWriter();
            s.IsBE = false;
            s.Format = Format.F2;

            ENRS e = default;
            POF p = default;
            p.Offsets = KKdList<long>.New;
            if (!IsX)
            {
                s.W(CCTs.C);
                s.W(0x50);
                s.A(0x10);

                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    s.W((int)cct.Flags);
                    s.W(cct.Hue       );
                    s.W(cct.Saturation);
                    s.W(cct.Lightness );
                    s.W(cct.Exposure  );
                    s.W(cct.Gamma.X   );
                    s.W(cct.Gamma.Y   );
                    s.W(cct.Gamma.Z   );
                    s.W(cct.Contrast  );
                }
                
                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x01, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x24, CCTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x08, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x44);
            }
            else
            {
                s.W(CCTs.C);
                s.A(0x08);
                s.W(0x10L);
                s.A(0x10);

                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    s.W((int)cct.Flags);
                    s.W(cct.Hue       );
                    s.W(cct.Saturation);
                    s.W(cct.Lightness );
                    s.W(cct.Exposure  );
                    s.W(cct.Gamma.X   );
                    s.W(cct.Gamma.Y   );
                    s.W(cct.Gamma.Z   );
                    s.W(cct.Contrast  );
                }

                e.Array = new ENRS.ENRSEntry[2];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x02, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x01, ENRS.ENRSEntry.Type.DWORD);
                e.Array[0].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x04, 0x01, ENRS.ENRSEntry.Type.QWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x24, CCTs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x08, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x08);
            }

            s.A(0x10, true);
            byte[] data = s.ToArray(true);

            Struct _CCRT = default;
            _CCRT.Header.Signature = 0x54524343;
            _CCRT.Header.DataSize = (uint)data.Length;
            _CCRT.Header.Length = 0x40;
            _CCRT.Header.Depth = 0;
            _CCRT.Header.SectionSize = (uint)data.Length;
            _CCRT.Header.Version = 0;
            _CCRT.Header.InnerSignature = 0x03;

            _CCRT.Header.UseBigEndian = false;
            _CCRT.Header.UseSectionSize = true;
            _CCRT.Header.Format = Format.F2;
            _CCRT.Data = data;
            _CCRT.ENRS = e;
            _CCRT.POF  = p;
            File.WriteAllBytes(file + ".cct", _CCRT.W(IsX, true));
        }

        public void MsgPackReader(string file, bool json)
        {
            IsX = false;
            CCTs = default;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack cc;
            if ((cc = msgPack["ColorCorrection", true]).NotNull)
            {
                CCTs.E = new CCT[cc.Array.Length];
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    cct = default;
                    MsgPack c = cc[i];
                    MsgPack temp;
                    if ((temp = c["Hue"]).Object != null)
                    {
                        cct.Flags |= Flags.Hue;
                        cct.Hue = temp.RF32();
                    }

                    if ((temp = c["Saturation"]).Object != null)
                    {
                        cct.Flags |= Flags.Saturation;
                        cct.Saturation = temp.RF32();
                    }

                    if ((temp = c["Lightness"]).Object != null)
                    {
                        cct.Flags |= Flags.Lightness;
                        cct.Lightness = temp.RF32();
                    }

                    if ((temp = c["Exposure"]).Object != null)
                    {
                        cct.Flags |= Flags.Exposure;
                        cct.Exposure = temp.RF32();
                    }

                    if ((temp = c["Gamma"]).NotNull)
                    {
                        cct.Flags |= Flags.Gamma;
                        cct.Gamma.X = temp.RF32("X");
                        cct.Gamma.Y = temp.RF32("Y");
                        cct.Gamma.Z = temp.RF32("Z");
                    }

                    if ((temp = c["Contrast"]).Object != null)
                    {
                        cct.Flags |= Flags.Contrast;
                        cct.Contrast = temp.RF32();
                    }
                }
            }
            else if ((cc = msgPack["ColorCorrectionX", true]).NotNull)
            {
                IsX = true;
                CCTs.E = new CCT[cc.Array.Length];
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    cct = default;
                    MsgPack c = cc[i];
                    MsgPack temp;
                    if ((temp = c["Hue"]).Object != null)
                    {
                        cct.Flags |= Flags.Hue;
                        cct.Hue = temp.RF32();
                    }

                    if ((temp = c["Saturation"]).Object != null)
                    {
                        cct.Flags |= Flags.Saturation;
                        cct.Saturation = temp.RF32();
                    }

                    if ((temp = c["Lightness"]).Object != null)
                    {
                        cct.Flags |= Flags.Lightness;
                        cct.Lightness = temp.RF32();
                    }

                    if ((temp = c["Exposure"]).Object != null)
                    {
                        cct.Flags |= Flags.Exposure;
                        cct.Exposure = temp.RF32();
                    }

                    if ((temp = c["Gamma"]).NotNull)
                    {
                        cct.Flags |= Flags.Gamma;
                        cct.Gamma.X = temp.RF32("X");
                        cct.Gamma.Y = temp.RF32("Y");
                        cct.Gamma.Z = temp.RF32("Z");
                    }

                    if ((temp = c["Contrast"]).Object != null)
                    {
                        cct.Flags |= Flags.Contrast;
                        cct.Contrast = temp.RF32();
                    }
                }
            }
            cc.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (CCTs.E == null || CCTs.C < 1) return;

            MsgPack cc;
            if (!IsX)
            {
                cc = new MsgPack(CCTs.C, "ColorCorrection");
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    MsgPack c = MsgPack.New;
                    if ((cct.Flags & Flags.Hue) != 0)
                        c.Add("Hue", cct.Hue);
                    if ((cct.Flags & Flags.Saturation) != 0)
                        c.Add("Saturation", cct.Saturation);
                    if ((cct.Flags & Flags.Lightness) != 0)
                        c.Add("Lightness", cct.Lightness);
                    if ((cct.Flags & Flags.Exposure) != 0)
                        c.Add("Exposure", cct.Exposure);
                    if ((cct.Flags & Flags.Gamma) != 0)
                        c.Add(new MsgPack("Gamma")
                            .Add("X", cct.Gamma.X)
                            .Add("Y", cct.Gamma.Y)
                            .Add("Z", cct.Gamma.Z));
                    if ((cct.Flags & Flags.Contrast) != 0)
                        c.Add("Contrast", cct.Contrast);
                    cc[i] = c;
                }
            }
            else
            {
                cc = new MsgPack(CCTs.C, "ColorCorrectionX");
                for (i = 0; i < CCTs.C; i++)
                {
                    ref CCT cct = ref CCTs.E[i];
                    MsgPack c = MsgPack.New;
                    if ((cct.Flags & Flags.Hue) != 0)
                        c.Add("Hue", cct.Hue);
                    if ((cct.Flags & Flags.Saturation) != 0)
                        c.Add("Saturation", cct.Saturation);
                    if ((cct.Flags & Flags.Lightness) != 0)
                        c.Add("Lightness", cct.Lightness);
                    if ((cct.Flags & Flags.Exposure) != 0)
                        c.Add("Exposure", cct.Exposure);
                    if ((cct.Flags & Flags.Gamma) != 0)
                        c.Add(new MsgPack("Gamma")
                            .Add("X", cct.Gamma.X)
                            .Add("Y", cct.Gamma.Y)
                            .Add("Z", cct.Gamma.Z));
                    if ((cct.Flags & Flags.Contrast) != 0)
                        c.Add("Contrast", cct.Contrast);
                    cc[i] = c;
                }
            }
            cc.Write(false, true, file + "_cc", json);
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
        { if (!disposed) { if (s != null) s.D(); s = null; CCTs = default; IsX = false; disposed = true; } }

        public struct CCT
        {
            public Flags Flags;
            public float Hue;
            public float Saturation;
            public float Lightness;
            public float Exposure;
            public  Vec3 Gamma;
            public float Contrast;

            public override string ToString() =>
                ((Flags & Flags.Hue) != 0
                ? $"{Hue.ToS(6)}," : ",") +
                ((Flags & Flags.Saturation) != 0
                ? $"{Saturation.ToS(6)}," : ",") +
                ((Flags & Flags.Lightness) != 0
                ? $"{Lightness.ToS(6)}," : ",") +
                ((Flags & Flags.Exposure) != 0
                ? $"{Exposure.ToS(6)}," : ",") +
                ((Flags & Flags.Gamma) != 0
                ? $"{Gamma.X.ToS(6)},{Gamma.Y.ToS(6)},{Gamma.Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.Contrast) != 0
                ? $"{Contrast.ToS(6)}" : "");
        }

        public enum Flags : int
        {
            Hue         = 0b000001,
            Saturation  = 0b000010,
            Lightness   = 0b000100,
            Exposure    = 0b001000,
            Gamma       = 0b010000,
            Contrast    = 0b100000,
        }
    }
}
