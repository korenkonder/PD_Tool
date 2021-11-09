using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Light : System.IDisposable
    {
        private int i, i0;
        private Stream s;

        public bool IsX;
        public CountPointer<LightStruct> LITs;

        public void LITReader(string file)
        {
            IsX = false;
            LITs = default;
            byte[] litData = File.ReadAllBytes(file + ".lit");
            Struct _LITC = litData.RSt(); litData = null;
            if (_LITC.Header.Signature != 0x4354494C) return;

            s = File.OpenReader(_LITC.Data);
            s.IsBE = _LITC.Header.UseBigEndian;

            s.RI32();
            LITs = s.RCPE<LightStruct>();
            if (LITs.C < 1) { s.C(); LITs.C = -1; return; }

            s.PI64 = LITs.O - _LITC.Header.Length;
            if (s.RI32() != 0)
            {
                s.PI64 = LITs.O - _LITC.Header.Length;
                for (i = 0; i < LITs.C; i++)
                    LITs.E[i] = new LightStruct
                    { C0 = s.RI32E(), O0 = s.RI32E() };

                for (i = 0; i < LITs.C; i++)
                {
                    s.PI64 = LITs.E[i].O0 - _LITC.Header.Length;
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        lit.Id = s.RI32E();
                        lit.Flags = (Flags)s.RI32E();
                        lit.Type  = (Type )s.RI32E();
                        lit.Ambient  .X = s.RF32E();
                        lit.Ambient  .Y = s.RF32E();
                        lit.Ambient  .Z = s.RF32E();
                        lit.Ambient  .W = s.RF32E();
                        lit.Diffuse  .X = s.RF32E();
                        lit.Diffuse  .Y = s.RF32E();
                        lit.Diffuse  .Z = s.RF32E();
                        lit.Diffuse  .W = s.RF32E();
                        lit.Specular .X = s.RF32E();
                        lit.Specular .Y = s.RF32E();
                        lit.Specular .Z = s.RF32E();
                        lit.Specular .W = s.RF32E();
                        lit.Position .X = s.RF32E();
                        lit.Position .Y = s.RF32E();
                        lit.Position .Z = s.RF32E();
                        lit.ToneCurve.X = s.RF32E();
                        lit.ToneCurve.Y = s.RF32E();
                        lit.ToneCurve.Z = s.RF32E();
                    }
                }
            }
            else
            {
                IsX = true;
                s.PI64 = LITs.O;
                for (i = 0; i < LITs.C; i++)
                    LITs.E[i] = new LightStruct
                    { C0 = s.RI32E(), C1 = s.RI32E(), O0 = (int)s.RI64E(), O1 = (int)s.RI64E() };

                for (i = 0; i < LITs.C; i++)
                {
                    s.P = LITs.E[i].O0;
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        lit.Id = s.RI32E();
                        lit.Flags = (Flags)s.RI32E();
                        lit.Type  = (Type )s.RI32E();
                        s.RI32();
                        long off = s.RI64();
                        lit.HasStr = s.RI64() != 0;
                        lit.Unk10       = s.RI32();
                        lit.Ambient  .X = s.RF32E();
                        lit.Ambient  .Y = s.RF32E();
                        lit.Ambient  .Z = s.RF32E();
                        lit.Ambient  .W = s.RF32E();
                        lit.Diffuse  .X = s.RF32E();
                        lit.Diffuse  .Y = s.RF32E();
                        lit.Diffuse  .Z = s.RF32E();
                        lit.Diffuse  .W = s.RF32E();
                        lit.Specular .X = s.RF32E();
                        lit.Specular .Y = s.RF32E();
                        lit.Specular .Z = s.RF32E();
                        lit.Specular .W = s.RF32E();
                        lit.Position .X = s.RF32E();
                        lit.Position .Y = s.RF32E();
                        lit.Position .Z = s.RF32E();
                        lit.Unk01       = s.RF32();
                        lit.Unk02       = s.RF32();
                        lit.Unk03       = s.RF32();
                        lit.Unk04       = s.RF32();
                        lit.Unk05.X     = s.RF32();
                        lit.Unk05.Y     = s.RF32();
                        lit.Unk05.Z     = s.RF32();
                        lit.ToneCurve.X = s.RF32E();
                        lit.ToneCurve.Y = s.RF32E();
                        lit.ToneCurve.Z = s.RF32E();
                        lit.Unk         = s.RF32();
                        lit.Unk06       = s.RF32();
                        lit.Unk07       = s.RF32();
                        lit.Unk08       = s.RF32();

                        lit.Str = off > 0 ? s.RaO(off).ToSJIS() : "";
                    }

                    s.P = LITs.E[i].O1;
                    for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                    {
                        ref LITX lit = ref LITs.E[i].E1[i0];
                        lit.C = s.RI32E();
                                s.RI32E();
                        lit.O = (int)s.RI64E();
                    }

                    for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                    {
                        ref LITX lit = ref LITs.E[i].E1[i0];
                        ref KeyValuePair<int, int>[] data = ref lit.E;
                        data = new KeyValuePair<int, int>[lit.C];

                        s.P = lit.O;
                        for (int i1 = 0; i1 < lit.C; i1++)
                            data[i1] = new KeyValuePair<int, int>(s.RI32E(), s.RI32E());
                    }
                }
            }
            s.C();
        }

        public void LITWriter(string file)
        {
            if (LITs.E == null || LITs.C < 1) return;

            if (file.EndsWith("_light"))
                file = file.Substring(0, file.Length - 6);

            s = File.OpenWriter();
            s.IsBE = false;
            s.Format = Format.F2;

            ENRS e = default;
            POF p = default;
            p.Offsets = KKdList<long>.New;
            if (!IsX)
            {
                s.W(0x02);
                s.W(LITs.C);
                s.W(0x50);
                s.A(0x10);

                int offset0 = 0x50 + (LITs.C * 0x08).A(0x10);
                for (i = 0; i < LITs.C; i++)
                {
                    s.W(LITs.E[i].C0 > 0 ? LITs.E[i].C0 : 0);
                    s.W(LITs.E[i].C0 > 0 ? offset0 : 0);
                    offset0 += (0x54 * (LITs.E[i].C0 > 0 ? LITs.E[i].C0 : 0)).A(0x10);
                }
                s.A(0x10);

                for (i = 0; i < LITs.C; i++)
                {
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        s.W(     lit.Id   );
                        s.W((int)lit.Flags);
                        s.W((int)lit.Type );
                        s.W(lit.Ambient  .X);
                        s.W(lit.Ambient  .Y);
                        s.W(lit.Ambient  .Z);
                        s.W(lit.Ambient  .W);
                        s.W(lit.Diffuse  .X);
                        s.W(lit.Diffuse  .Y);
                        s.W(lit.Diffuse  .Z);
                        s.W(lit.Diffuse  .W);
                        s.W(lit.Specular .X);
                        s.W(lit.Specular .Y);
                        s.W(lit.Specular .Z);
                        s.W(lit.Specular .W);
                        s.W(lit.Position .X);
                        s.W(lit.Position .Y);
                        s.W(lit.Position .Z);
                        s.W(lit.ToneCurve.X);
                        s.W(lit.ToneCurve.Y);
                        s.W(lit.ToneCurve.Z);
                    }
                    s.A(0x10);
                }

                int count = 0;
                for (i = 0; i < LITs.C; i++)
                    if (LITs.E[i].C0 > 0)
                        count += LITs.E[i].C0;

                e.Array = new ENRS.ENRSEntry[3];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x01, 0x10, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x03, ENRS.ENRSEntry.Type.DWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x10, 0x01, 0x08, LITs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[2] = new ENRS.ENRSEntry((LITs.C * 0x08).A(0x10), 0x01, 0x54, count);
                e.Array[2].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x15, ENRS.ENRSEntry.Type.DWORD);

                p.Offsets.Add(0x48);
                for (i = 0; i < LITs.C; i++)
                    p.Offsets.Add(0x54 + 0x08 * i);
            }
            else
            {
                long offset0 = 0x50 + (LITs.C * 0x18).A(0x10);
                long offset1 = offset0;
                long offset2 = offset0 + 0x10;
                for (i = 0; i < LITs.C; i++)
                    offset1 += LITs.E[i].C0 * 0x98;
                offset1 = offset1.A(0x10);

                long offset3 = offset1;
                long offset4 = offset1 + 0x08;
                for (i = 0; i < LITs.C; i++)
                    offset3 += LITs.E[i].C1 * 0x10;
                offset3 = offset3.A(0x10);
                long offset5 = offset3;
                long offset6 = offset3;
                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                        offset6 += LITs.E[i].E1[i0].C * 0x08;

                byte[][][] strdata = new byte[LITs.C][][];
                for (i = 0; i < LITs.C; i++)
                {
                    strdata[i] = new byte[LITs.E[i].C0][];
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        strdata[i][i0] = lit.HasStr && lit.Str != null && lit.Str != "" ? lit.Str.ToSJIS() : null;
                    }
                }

                s.W(0x02);
                s.W(LITs.C);
                s.W(0x50L);
                s.A(0x10);
                s.P = 0x50;

                for (i = 0; i < LITs.C; i++)
                {
                    s.W(LITs.E[i].C0 > 0 ? LITs.E[i].C0 : 0);
                    s.W(LITs.E[i].C1 > 0 ? LITs.E[i].C1 : 0);
                    s.W(LITs.E[i].C0 > 0 ? offset0 : 0);
                    s.W(LITs.E[i].C1 > 0 ? offset1 : 0);
                    offset0 += 0x98 * (LITs.E[i].C0 > 0 ? LITs.E[i].C0 : 0);
                    offset1 += 0x10 * (LITs.E[i].C1 > 0 ? LITs.E[i].C1 : 0);
                }
                s.A(0x10);

                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        s.W(     lit.Id   );
                        s.W((int)lit.Flags);
                        s.W((int)lit.Type );
                        s.W((int)0);
                        if (lit.HasStr && strdata[i][i0] != null && strdata[i][i0].Length > 0)
                        { s.W(offset6); s.W((long)strdata[i][i0].HashMurmurHash(strdata[i][i0].Length));
                          offset6 += strdata[i][i0].Length + 1; }
                        else
                        { s.W(0x00L); s.W((long)(lit.HasStr ? 0x0CAD3078 : 0)); }
                        s.W(lit.Unk10      );
                        s.W(lit.Ambient  .X);
                        s.W(lit.Ambient  .Y);
                        s.W(lit.Ambient  .Z);
                        s.W(lit.Ambient  .W);
                        s.W(lit.Diffuse  .X);
                        s.W(lit.Diffuse  .Y);
                        s.W(lit.Diffuse  .Z);
                        s.W(lit.Diffuse  .W);
                        s.W(lit.Specular .X);
                        s.W(lit.Specular .Y);
                        s.W(lit.Specular .Z);
                        s.W(lit.Specular .W);
                        s.W(lit.Position .X);
                        s.W(lit.Position .Y);
                        s.W(lit.Position .Z);
                        s.W(lit.Unk01      );
                        s.W(lit.Unk02      );
                        s.W(lit.Unk03      );
                        s.W(lit.Unk04      );
                        s.W(lit.Unk05.X    );
                        s.W(lit.Unk05.Y    );
                        s.W(lit.Unk05.Z    );
                        s.W(lit.ToneCurve.X);
                        s.W(lit.ToneCurve.Y);
                        s.W(lit.ToneCurve.Z);
                        s.W(lit.Unk        );
                        s.W(lit.Unk06      );
                        s.W(lit.Unk07      );
                        s.W(lit.Unk08      );
                    }
                s.A(0x10);

                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                    {
                        ref LITX lit = ref LITs.E[i].E1[i0];
                        s.W(lit.C);
                        s.W((int)0);
                        s.W(offset5);
                        offset5 += lit.C * 0x08;
                    }
                s.A(0x10);

                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                    {
                        ref LITX lit = ref LITs.E[i].E1[i0];
                        for (int i1 = 0; i1 < lit.C; i1++)
                        {
                            s.W(lit.E[i1].Key);
                            s.W(lit.E[i1].Value);
                        }
                    }

                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        if (lit.HasStr && strdata[i][i0] != null && strdata[i][i0].Length > 0)
                        {
                            s.W(strdata[i][i0]);
                            s.W((byte)0);
                        }
                    }
                s.A(0x10);

                bool sub = false;
                for (i = 0; i < LITs.C; i++)
                    if (LITs.E[i].C1 > 0) { sub = true; break; }

                int count = 0;
                for (i = 0; i < LITs.C; i++)
                    if (LITs.E[i].C0 > 0)
                        count += LITs.E[i].C0;

                e.Array = new ENRS.ENRSEntry[sub ? 5 : 3];
                e.Array[0] = new ENRS.ENRSEntry(0x00, 0x02, 0x50, 0x01);
                e.Array[0].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[0].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x01, ENRS.ENRSEntry.Type.QWORD);
                e.Array[1] = new ENRS.ENRSEntry(0x50, 0x02, 0x18, LITs.C);
                e.Array[1].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                e.Array[1].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.QWORD);
                e.Array[2] = new ENRS.ENRSEntry((LITs.C * 0x18).A(0x10), 0x03, 0x98, count);
                e.Array[2].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x03, ENRS.ENRSEntry.Type.DWORD);
                e.Array[2].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x18, 0x0F, ENRS.ENRSEntry.Type.DWORD);
                e.Array[2].Sub[2] = new ENRS.ENRSEntry.SubENRSEntry(0x20, 0x03, ENRS.ENRSEntry.Type.DWORD);
                if (sub)
                {
                    int sub_count = 0;
                    for (i = 0; i < LITs.C; i++)
                        if (LITs.E[i].C1 > 0)
                            sub_count += LITs.E[i].C1;

                    e.Array[3] = new ENRS.ENRSEntry((count * 0x98).A(0x10), 0x02, 0x10, sub_count);
                    e.Array[3].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x01, ENRS.ENRSEntry.Type.DWORD);
                    e.Array[3].Sub[1] = new ENRS.ENRSEntry.SubENRSEntry(0x04, 0x01, ENRS.ENRSEntry.Type.QWORD);
                    e.Array[4] = new ENRS.ENRSEntry(sub_count * 0x10, 0x01, 0x08, LITs.E[0].E1[0].C);
                    e.Array[4].Sub[0] = new ENRS.ENRSEntry.SubENRSEntry(0x00, 0x02, ENRS.ENRSEntry.Type.DWORD);
                }

                p.Offsets.Add(0x08);
                for (i = 0; i < LITs.C; i++)
                {
                    if (LITs.E[i].C0 > 0)
                        p.Offsets.Add(0x58 + 0x18 * i);
                    if (LITs.E[i].C1 > 0)
                        p.Offsets.Add(0x60 + 0x18 * i);
                }

                for (i = 0; i < LITs.C; i++)
                    for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                    {
                        ref LIT lit = ref LITs.E[i].E0[i0];
                        if (lit.HasStr && strdata[i][i0] != null && strdata[i][i0].Length > 0)
                            p.Offsets.Add(offset2);
                        offset2 += 0x98;
                    }

                if (sub)
                    for (i = 0; i < LITs.C; i++)
                        for (i0 = 0; i0 < LITs.E[i].C1; i0++)
                        {
                            p.Offsets.Add(offset4);
                            offset4 += 0x10;
                        }
            }

            s.A(0x10, true);
            byte[] data = s.ToArray(true);

            Struct _LITC = default;
            _LITC.Header.Signature = 0x4354494C;
            _LITC.Header.DataSize = (uint)data.Length;
            _LITC.Header.Length = 0x40;
            _LITC.Header.Depth = 0;
            _LITC.Header.SectionSize = (uint)data.Length;
            _LITC.Header.Version = 0;
            _LITC.Header.InnerSignature = 0x02;

            _LITC.Header.UseBigEndian = false;
            _LITC.Header.UseSectionSize = true;
            _LITC.Header.Format = Format.F2;
            _LITC.Data = data;
            _LITC.ENRS = e;
            _LITC.POF  = p;
            File.WriteAllBytes(file + ".lit", _LITC.W(IsX, true));
        }

        public void MsgPackReader(string file, bool json)
        {
            IsX = false;
            LITs = default;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack light;
            if ((light = msgPack["Light", true]).NotNull)
            {
                LITs.E = new LightStruct[light.Array.Length];
                for (i = 0; i < LITs.C; i++)
                {
                    ref LightStruct lit = ref LITs.E[i];
                    lit = default;
                    MsgPack l = light[i];
                    MsgPack l0;
                    if ((l0 = l["MainData", true]).IsNull)
                        continue;

                    lit.C0 = l0.Array.Length;
                    for (i0 = 0; i0 < lit.C0; i0++)
                    {
                        ref LIT lit0 = ref lit.E0[i0];
                        MsgPack l1 = l0[i0];
                        MsgPack temp;
                        if ((temp = l1["Id"]).Object != null)
                            lit0.Id = temp.RI32();

                        if ((temp = l1["Type"]).Object != null)
                        {
                            lit0.Flags |= Flags.Type;
                            lit0.Type = (Type)temp.RI32();
                        }

                        if ((temp = l1["Str"]).Object != null)
                        {
                            lit0.HasStr = true;
                            lit0.Str = temp.RS();
                        }

                        if ((temp = l1["Ambient"]).Object != null)
                        {
                            lit0.Flags |= Flags.Ambient;
                            lit0.Ambient.X = temp.RF32("R");
                            lit0.Ambient.Y = temp.RF32("G");
                            lit0.Ambient.Z = temp.RF32("B");
                            lit0.Ambient.W = temp.RF32("A");
                        }

                        if ((temp = l1["Diffuse"]).Object != null)
                        {
                            lit0.Flags |= Flags.Diffuse;
                            lit0.Diffuse.X = temp.RF32("R");
                            lit0.Diffuse.Y = temp.RF32("G");
                            lit0.Diffuse.Z = temp.RF32("B");
                            lit0.Diffuse.W = temp.RF32("A");
                        }

                        if ((temp = l1["Specular"]).Object != null)
                        {
                            lit0.Flags |= Flags.Specular;
                            lit0.Specular.X = temp.RF32("R");
                            lit0.Specular.Y = temp.RF32("G");
                            lit0.Specular.Z = temp.RF32("B");
                            lit0.Specular.W = temp.RF32("A");
                        }

                        if ((temp = l1["Position"]).Object != null)
                        {
                            lit0.Flags |= Flags.Position;
                            lit0.Position.X = temp.RF32("X");
                            lit0.Position.Y = temp.RF32("Y");
                            lit0.Position.Z = temp.RF32("Z");
                        }

                        if ((temp = l1["ToneCurve"]).Object != null)
                        {
                            lit0.Flags |= Flags.ToneCurve;
                            lit0.ToneCurve.X = temp.RF32("Begin"    );
                            lit0.ToneCurve.Y = temp.RF32("End"      );
                            lit0.ToneCurve.Z = temp.RF32("BlendRate");
                        }
                    }
                }
            }
            else if ((light = msgPack["LightX", true]).NotNull)
            {
                IsX = true;
                LITs.E = new LightStruct[light.Array.Length];
                for (i = 0; i < LITs.C; i++)
                {
                    ref LightStruct lit = ref LITs.E[i];
                    lit = default;
                    MsgPack l = light[i];
                    MsgPack l0;
                    if ((l0 = l["MainData", true]).IsNull)
                        continue;

                    lit.C0 = l0.Array.Length;
                    for (i0 = 0; i0 < lit.C0; i0++)
                    {
                        ref LIT lit0 = ref lit.E0[i0];
                        lit0 = default;
                        MsgPack l1;
                        if ((l1 = l0[i0]).IsNull)
                            continue;

                        MsgPack temp;
                        if ((temp = l1["Id"]).Object != null)
                            lit0.Id = temp.RI32();

                        if ((temp = l1["Flags"]).Object != null)
                            lit0.Flags = (Flags)temp.RI32();

                        if ((temp = l1["Type"]).Object != null)
                            lit0.Type = (Type)temp.RI32();

                        if ((temp = l1["Ambient"]).Object != null)
                        {
                            lit0.Ambient.X = temp.RF32("R");
                            lit0.Ambient.Y = temp.RF32("G");
                            lit0.Ambient.Z = temp.RF32("B");
                            lit0.Ambient.W = temp.RF32("A");
                        }

                        if ((temp = l1["Diffuse"]).Object != null)
                        {
                            lit0.Diffuse.X = temp.RF32("R");
                            lit0.Diffuse.Y = temp.RF32("G");
                            lit0.Diffuse.Z = temp.RF32("B");
                            lit0.Diffuse.W = temp.RF32("A");
                        }

                        if ((temp = l1["Specular"]).Object != null)
                        {
                            lit0.Specular.X = temp.RF32("R");
                            lit0.Specular.Y = temp.RF32("G");
                            lit0.Specular.Z = temp.RF32("B");
                            lit0.Specular.W = temp.RF32("A");
                        }

                        if ((temp = l1["Position"]).Object != null)
                        {
                            lit0.Position.X = temp.RF32("X");
                            lit0.Position.Y = temp.RF32("Y");
                            lit0.Position.Z = temp.RF32("Z");
                        }

                        if ((temp = l1["ToneCurve"]).Object != null)
                            lit0.ToneCurve.X = temp.RF32("Begin"    );
                            lit0.ToneCurve.Y = temp.RF32("End"      );
                            lit0.ToneCurve.Z = temp.RF32("BlendRate");

                        if ((temp = l1["Str"]).Object != null)
                        {
                            lit0.HasStr = true;
                            lit0.Str = temp.RS();
                        }

                        if ((temp = l1["Unk01"]).Object != null)
                            lit0.Unk01 = temp.RF32();

                        if ((temp = l1["Unk02"]).Object != null)
                            lit0.Unk02 = temp.RF32();

                        if ((temp = l1["Unk03"]).Object != null)
                            lit0.Unk03 = temp.RF32();

                        if ((temp = l1["Unk04"]).Object != null)
                            lit0.Unk04 = temp.RF32();

                        if ((temp = l1["Unk05"]).Object != null)
                            lit0.Unk05.X = temp.RF32("X");
                            lit0.Unk05.Y = temp.RF32("Y");
                            lit0.Unk05.Z = temp.RF32("Z");

                        if ((temp = l1["Unk06"]).Object != null)
                            lit0.Unk06 = temp.RF32();

                        if ((temp = l1["Unk07"]).Object != null)
                            lit0.Unk07 = temp.RF32();

                        if ((temp = l1["Unk08"]).Object != null)
                            lit0.Unk08 = temp.RF32();

                        if ((temp = l1["Unk10"]).Object != null)
                            lit0.Unk10 = temp.RI32();

                        if ((temp = l1["Unk"]).Object != null)
                            lit0.Unk = temp.RF32();
                    }

                    if ((l0 = l["SubData", true]).IsNull)
                        continue;

                    lit.C1 = l0.Array.Length;
                    for (i0 = 0; i0 < lit.C1; i0++)
                    {
                        ref LITX lit1 = ref lit.E1[i0];
                        lit1 = default;
                        MsgPack l1;
                        if ((l1 = l0[i0]).IsNull && l1.Array != null)
                            continue;

                        lit1.C = l1.Array.Length;
                        lit1.E = new KeyValuePair<int, int>[lit1.C];
                        for (int i1 = 0; i1 < lit1.C; i1++)
                        {
                            lit1.E[i1] = default;
                            MsgPack l2 = l1[i1];
                            MsgPack temp;
                            if ((temp = l2["Key"]).Object != null)
                                lit1.E[i1].Key = temp.RI32();

                            if ((temp = l2["Value"]).Object != null)
                                lit1.E[i1].Value = temp.RI32();
                        }
                    }
                }
            }
            light.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (LITs.E == null || LITs.C < 1) return;

            MsgPack light;
            if (!IsX)
            {
                light = new MsgPack(LITs.C, "Light");
                for (i = 0; i < LITs.C; i++)
                {
                    ref LightStruct lit = ref LITs.E[i];
                    MsgPack l = MsgPack.New;
                    MsgPack l0 = new MsgPack(lit.C0, "MainData");
                    for (i0 = 0; i0 < lit.C0; i0++)
                    {
                        ref LIT lit0 = ref lit.E0[i0];
                        MsgPack l1 = MsgPack.New;
                        l1.Add("Id", lit0.Id);
                        if ((lit0.Flags & Flags.Type) != 0)
                            l1.Add("Type", (int)lit0.Type);
                        if ((lit0.Flags & Flags.Ambient) != 0)
                            l1.Add(new MsgPack("Ambient")
                                .Add("R", lit0.Ambient.X)
                                .Add("G", lit0.Ambient.Y)
                                .Add("B", lit0.Ambient.Z)
                                .Add("A", lit0.Ambient.W));
                        if ((lit0.Flags & Flags.Diffuse) != 0)
                            l1.Add(new MsgPack("Diffuse")
                                .Add("R", lit0.Diffuse.X)
                                .Add("G", lit0.Diffuse.Y)
                                .Add("B", lit0.Diffuse.Z)
                                .Add("A", lit0.Diffuse.W));
                        if ((lit0.Flags & Flags.Specular) != 0)
                            l1.Add(new MsgPack("Specular")
                                .Add("R", lit0.Specular.X)
                                .Add("G", lit0.Specular.Y)
                                .Add("B", lit0.Specular.Z)
                                .Add("A", lit0.Specular.W));
                        if ((lit0.Flags & Flags.Position) != 0)
                            l1.Add(new MsgPack("Position")
                                .Add("X", lit0.Position.X)
                                .Add("Y", lit0.Position.Y)
                                .Add("Z", lit0.Position.Z));
                        if ((lit0.Flags & Flags.ToneCurve) != 0)
                            l1.Add(new MsgPack("ToneCurve")
                                .Add("Begin"    , lit0.ToneCurve.X)
                                .Add("End"      , lit0.ToneCurve.Y)
                                .Add("BlendRate", lit0.ToneCurve.Z));
                        l0[i0] = l1;
                    }
                    l.Add(l0);
                    light[i] = l;
                }
            }
            else
            {
                light = new MsgPack(LITs.C, "LightX");
                for (i = 0; i < LITs.C; i++)
                {
                    ref LightStruct lit = ref LITs.E[i];
                    MsgPack l = MsgPack.New;
                    MsgPack l0 = new MsgPack(lit.C0, "MainData");
                    for (i0 = 0; i0 < lit.C0; i0++)
                    {
                        ref LIT lit0 = ref lit.E0[i0];
                        MsgPack l1 = MsgPack.New;
                        l1.Add("Id", lit0.Id);
                        l1.Add("Flags", (int)lit0.Flags);
                        l1.Add("Type", (int)lit0.Type);
                        l1.Add(new MsgPack("Ambient")
                            .Add("R", lit0.Ambient.X)
                            .Add("G", lit0.Ambient.Y)
                            .Add("B", lit0.Ambient.Z)
                            .Add("A", lit0.Ambient.W));
                        l1.Add(new MsgPack("Diffuse")
                            .Add("R", lit0.Diffuse.X)
                            .Add("G", lit0.Diffuse.Y)
                            .Add("B", lit0.Diffuse.Z)
                            .Add("A", lit0.Diffuse.W));
                        l1.Add(new MsgPack("Specular")
                            .Add("R", lit0.Specular.X)
                            .Add("G", lit0.Specular.Y)
                            .Add("B", lit0.Specular.Z)
                            .Add("A", lit0.Specular.W));
                        l1.Add(new MsgPack("Position")
                            .Add("X", lit0.Position.X)
                            .Add("Y", lit0.Position.Y)
                            .Add("Z", lit0.Position.Z));
                        l1.Add(new MsgPack("ToneCurve")
                            .Add("Begin"    , lit0.ToneCurve.X)
                            .Add("End"      , lit0.ToneCurve.Y)
                            .Add("BlendRate", lit0.ToneCurve.Z));

                        if (lit0.HasStr)
                            l1.Add("Str", lit0.Str ?? "");
                        l1.Add("Unk01", lit0.Unk01);
                        l1.Add("Unk02", lit0.Unk02);
                        l1.Add("Unk03", lit0.Unk03);
                        l1.Add("Unk04", lit0.Unk04);
                        l1.Add(new MsgPack("Unk05")
                            .Add("X", lit0.Unk05.X)
                            .Add("Y", lit0.Unk05.Y)
                            .Add("Z", lit0.Unk05.Z));
                        l1.Add("Unk06", lit0.Unk06);
                        l1.Add("Unk07", lit0.Unk07);
                        l1.Add("Unk08", lit0.Unk08);
                        l1.Add("Unk10", lit0.Unk10);
                        l1.Add("Unk", lit0.Unk);
                        l0[i0] = l1;
                    }
                    l.Add(l0);

                    if (lit.C1 > 0)
                    {
                        l0 = new MsgPack(lit.C1, "SubData");
                        for (i0 = 0; i0 < lit.C1; i0++)
                        {
                            ref LITX lit1 = ref lit.E1[i0];
                            MsgPack l1 = new MsgPack(lit1.C);
                            for (int i1 = 0; i1 < lit1.C; i1++)
                                l1[i1] = MsgPack.New
                                    .Add("Key"  , lit1.E[i1].Key  )
                                    .Add("Value", lit1.E[i1].Value);
                            l0[i0] = l1;
                        }
                        l.Add(l0);
                    }
                    light[i] = l;
                }
            }
            light.Write(false, true, file + "_light", json);
        }

        public void TXTWriter(string file)
        {
            i = 0;
            if (LITs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("Type,AmbientR,AmbientG,AmbientB,DiffuseR,DiffuseG,DiffuseB,SpecularR,SpecularG," +
                "SpecularB,SpecularA,PosX,PosY,PosZ,ToneCurveBegin,ToneCurveEnd,ToneCurveBlendRate," +
                (file.EndsWith("_chara") ? "コメント" : "ID") + "\n");
            for (i0 = 0; i0 < LITs.E[i].C0; i0++)
                s.W($"{LITs.E[i].E0[i0]},{i}\n");
            File.WriteAllBytes($"{file}_light.txt", s.ToArray(true));
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; LITs = default; IsX = false; disposed = true; } }

        public struct LightStruct
        {
            public int C0 { get => E0 != null ? E0.Length : 0;
                            set => E0 = value > -1 ? new LIT[value] : null; }
            public int O0;
            public LIT[] E0;

            public int C1 { get => E1 != null ? E1.Length : 0;
                            set => E1 = value > -1 ? new LITX[value] : default; }
            public int O1;
            public LITX[] E1;

            public LIT this[int index]
            {   get =>    E0 != null && index > -1 && index < E0.LongLength ? E0[index] : default;
                set { if (E0 != null && index > -1 && index < E0.LongLength)  E0[index] =   value; } }

            public LITX this[int index, bool x]
            {   get =>    E1 != null && index > -1 && index < E1.LongLength ? E1[index] : default;
                set { if (E1 != null && index > -1 && index < E1.LongLength)  E1[index] =   value; } }

            public override string ToString() => C0 < 1 ? "No Entries" :
                C0 == 1 ? E0[0].ToString() : "Count: " + C0;
        }

        public struct LIT
        {
            public int Id;
            public Flags Flags;
            public Type  Type;
            public string Str;
            public   bool HasStr;
            public   Vec4 Ambient;
            public   Vec4 Diffuse;
            public   Vec4 Specular;
            public   Vec3 Position;
            public  float Unk01;
            public  float Unk02;
            public  float Unk03;
            public  float Unk04;
            public   Vec3 Unk05;
            public   Vec3 ToneCurve;
            public  float Unk;
            public  float Unk06;
            public  float Unk07;
            public  float Unk08;
            public    int Unk10;

            public override string ToString() => Flags == 0 ? ",,,,,,,,,,,,,,,," : $"{(Type)Type}," +
                ((Flags & Flags.Ambient  ) != 0
                ? $"{Ambient  .X.ToS(6)},{Ambient  .Y.ToS(6)},{Ambient  .Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.Diffuse  ) != 0
                ? $"{Diffuse  .X.ToS(6)},{Diffuse  .Y.ToS(6)},{Diffuse  .Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.Specular ) != 0
                ? $"{Specular .X.ToS(6)},{Specular .Y.ToS(6)},{Specular .Z.ToS(6)},{Specular .W.ToS(6)}," : ",,,,") +
                ((Flags & Flags.Position ) != 0
                ? $"{Position .X.ToS(6)},{Position .Y.ToS(6)},{Position .Z.ToS(6)}," : ",,,") +
                ((Flags & Flags.ToneCurve) != 0
                ? $"{ToneCurve.X.ToS(6)},{ToneCurve.Y.ToS(6)},{ToneCurve.Z.ToS(6)}" : ",,");
        }

        public enum Id : int
        {
            CHARA       = 0x00,
            STAGE       = 0x01,
            SUN         = 0x02,
            REFLECT     = 0x03,
            SHADOW      = 0x04,
            CHARA_COLOR = 0x05,
            CHARA_F     = 0x06,
            PROJECTION  = 0x07,
        }

        public enum Type : int
        {
            OFF      = 0,
            PARALLEL = 1,
            POINT    = 2,
            SPOT     = 3,
        }

        public enum Flags : int
        {
            Type       = 0b00000000000000001,
            Ambient    = 0b00000000000000010,
            Diffuse    = 0b00000000000000100,
            Specular   = 0b00000000000001000,
            Position   = 0b00000000000010000,
            ToneCurve  = 0b00000001000000000,
        }
    }

    public struct LITX
    {
        public int C;
        public int O;
        public KeyValuePair<int, int>[] E;
    }
}
