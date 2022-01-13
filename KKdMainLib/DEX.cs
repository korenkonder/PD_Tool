using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct DEX : System.IDisposable
    {
        private int i, i0, i1;
        private Header header;
        private Stream s;

        public EXP[] Dex;

        public void DEXReader(string filepath, string ext)
        {
            Dex = null;
            header = new Header();
            s = File.OpenReader(filepath + ext);

            header.Format = Format.F;
            uint signature = s.RU32();
            if (signature == 0x43505845) { header = s.ReadHeader(true); signature = s.RU32(); }
            if (signature != 0x64) return;

            s.O = s.P - 0x4;
            Dex = new EXP[s.RI32()];
            int DEXOffset = s.RI32();
            int DEXNameOffset = s.RI32();
            if (DEXNameOffset == 0x00) { s.Format = header.Format = Format.X; DEXNameOffset = (int)s.RIX(); }

            s.P = DEXOffset;
            for (i = 0; i < Dex.Length; i++)
            {
                Dex[i].FaceOffset = s.RIX();
                Dex[i].FaceCLOffset = s.RIX();
            }
            s.P = DEXNameOffset;
            for (i = 0; i < Dex.Length; i++)
                Dex[i].NameOffset = s.RIX();

            for (i = 0; i < Dex.Length; i++)
            {
                EXPData element = new EXPData();
                Dex[i].Face = KKdList<EXPData>.New;
                s.PI64 = Dex[i].FaceOffset;
                while (true)
                {
                    element.Frame = s.RF32();
                    element.Type  = s.RU16();
                    element.ID    = s.RU16();
                    element.Value = s.RF32();
                    element.Trans = s.RF32();
                    if (element.Type == 0xFFFF) break;
                    Dex[i].Face.Add(element);
                }
                Dex[i].Face.Capacity = Dex[i].Face.Count;

                Dex[i].FaceCL = KKdList<EXPData>.New;
                s.PI64 = Dex[i].FaceCLOffset;
                while(true)
                {
                    element.Frame = s.RF32();
                    element.Type  = s.RU16();
                    element.ID    = s.RU16();
                    element.Value = s.RF32();
                    element.Trans = s.RF32();
                    if (element.Type == 0xFFFF) break;
                    Dex[i].FaceCL.Add(element);
                }
                Dex[i].FaceCL.Capacity = Dex[i].FaceCL.Count;

                Dex[i].Name = s.RSaO(Dex[i].NameOffset);
            }

            s.C();
        }

        public void DEXWriter(string filepath, Format format)
        {
            if (Dex == null || Dex.Length < 1) return;

            header = new Header();
            s = File.OpenWriter(filepath + (format > Format.F && format < Format.FT ? ".dex" : ".bin"), true);
            header.Format = s.Format = format;

            s.Format = format;
            s.O = format > Format.F ? 0x20 : 0;
            s.P = 0;
            s.W(0x64);
            s.W(Dex.Length);

            s.WX(header.IsX ? 0x28 : 0x20);
            s.WX(0x00);

            int Position0 = s.P;
            s.W(0x00L);
            s.W(0x00L);

            for (i = 0; i < Dex.Length * 3; i++) s.WX(0x00);
            s.A(0x20);

            for (i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].FaceOffset = s.P;
                for (i1 = 0; i1 < Dex[i0].Face.Count; i1++)
                {
                    s.W(Dex[i0].Face[i1].Frame);
                    s.W(Dex[i0].Face[i1].Type );
                    s.W(Dex[i0].Face[i1].ID   );
                    s.W(Dex[i0].Face[i1].Value);
                    s.W(Dex[i0].Face[i1].Trans);
                }
                s.W(999999.0f);
                s.W(0xFFFF);
                s.W(0x0L);
                s.A(0x20);

                Dex[i0].FaceCLOffset = s.P;
                for (i1 = 0; i1 < Dex[i0].FaceCL.Count; i1++)
                {
                    s.W(Dex[i0].FaceCL[i1].Frame);
                    s.W(Dex[i0].FaceCL[i1].Type );
                    s.W(Dex[i0].FaceCL[i1].ID   );
                    s.W(Dex[i0].FaceCL[i1].Value);
                    s.W(Dex[i0].FaceCL[i1].Trans);
                }
                s.W(999999.0f);
                s.W(0xFFFF);
                s.W(0x0L);
                s.A(0x20);
            }
            for (i = 0; i < Dex.Length; i++)
            {
                Dex[i].NameOffset = s.P;
                s.W(Dex[i].Name + "\0");
            }
            s.A(0x10, true);

            s.P = header.IsX ? 0x28 : 0x20;
            for (i = 0; i < Dex.Length; i++)
            {
                s.WX(Dex[i].FaceOffset);
                s.WX(Dex[i].FaceCLOffset);
            }
            int namesPosition = s.P;
            for (i = 0; i < Dex.Length; i++)
                s.WX(Dex[i].NameOffset);

            s.P = Position0 - (header.IsX ? 8 : 4);
            s.W(namesPosition);

            if (format > Format.F)
            {
                uint offset = s.LU32;
                s.O = 0;
                s.P = s.L;
                s.WEOFC(0);
                s.P = 0;
                header.DataSize = offset;
                header.SectionSize = offset;
                header.Signature = 0x43505845;
                header.UseSectionSize = true;
                s.W(header);
            }
            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            Dex = null;
            header = new Header();

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack dex;
            if ((dex = msgPack["Dex", true]).NotNull)
            {
                Dex = new EXP[dex.Array.Length];
                for (i0 = 0; i0 < Dex.Length; i0++)
                {
                    Dex[i0] = new EXP { Name = dex[i0].RS("Name") };

                    MsgPack temp;
                    if ((temp = dex[i0]["Face", true]).NotNull)
                    {
                        Dex[i0].Face = KKdList<EXPData>.New;
                        Dex[i0].Face.Capacity = temp.Array.Length;
                        for (i1 = 0; i1 < Dex[i0].Face.Capacity; i1++)
                            Dex[i0].Face.Add(EXPData.Read(temp[i1]));
                    }
                    if ((temp = dex[i0]["FaceCL", true]).NotNull)
                    {
                        Dex[i0].FaceCL = KKdList<EXPData>.New;
                        Dex[i0].FaceCL.Capacity = temp.Array.Length;
                        for (i1 = 0; i1 < Dex[i0].FaceCL.Capacity; i1++)
                            Dex[i0].FaceCL.Add(EXPData.Read(temp[i1]));
                    }
                    temp.Dispose();
                }
            }
            dex.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (Dex == null || Dex.Length < 1) return;

            MsgPack dex = new MsgPack(Dex.Length, "Dex");
            for (i0 = 0; i0 < Dex.Length; i0++)
            {
                MsgPack exp = MsgPack.New.Add("Name", Dex[i0].Name);
                MsgPack main = new MsgPack(Dex[i0].Face.Count, "Face");
                for (i1 = 0; i1 < Dex[i0].Face.Count; i1++)
                    main[i1] = Dex[i0].Face[i1].Write();
                exp.Add(main);

                MsgPack eyes = new MsgPack(Dex[i0].FaceCL.Count, "FaceCL");
                for (i1 = 0; i1 < Dex[i0].FaceCL.Count; i1++)
                    eyes[i1] = Dex[i0].FaceCL[i1].Write();
                exp.Add(eyes);
                dex[i0] = exp;
            }

            dex.Write(false, true, file, json);
        }

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; Dex = null; header = default; disposed = true; } }

        public struct EXP
        {
            public long FaceOffset;
            public long FaceCLOffset;
            public long NameOffset;
            public string Name;
            public KKdList<EXPData> Face;
            public KKdList<EXPData> FaceCL;

            public override string ToString() => Name;
        }

        public struct EXPData
        {
            public  float Frame;
            public ushort Type;
            public ushort ID;
            public  float Value;
            public  float Trans;

            public static EXPData Read(MsgPack msg) =>
                new EXPData() { Frame = msg.RF32("Frame"), Type  = msg.RU16("Type" ),
                                ID    = msg.RU16("ID"   ), Value = msg.RF32("Value"),
                                Trans = msg.RF32("Trans"), };

            public MsgPack Write() =>
                MsgPack.New.Add("Frame", Frame).Add("Type" , Type )
                           .Add("ID"   , ID   ).Add("Value", Value)
                           .Add("Trans", Trans);
        }
    }
}
