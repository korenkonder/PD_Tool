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
            header.SectionSignature = s.RU32();
            if (header.SectionSignature == 0x43505845)
                header = s.ReadHeader(true, true);
            if (header.SectionSignature != 0x64) return;

            s.O = s.P - 0x4;
            Dex = new EXP[s.RI32()];
            int DEXOffset = s.RI32();
            int DEXNameOffset = s.RI32();
            if (DEXNameOffset == 0x00) { s.Format = header.Format = Format.X; DEXNameOffset = (int)s.RIX(); }

            s.P = DEXOffset;
            for (i = 0; i < Dex.Length; i++)
            {
                Dex[i].MainOffset = (int)s.RIX();
                Dex[i].EyesOffset = (int)s.RIX();
            }
            s.P = DEXNameOffset;
            for (i = 0; i < Dex.Length; i++)
                Dex[i].NameOffset = (int)s.RIX();

            for (i = 0; i < Dex.Length; i++)
            {
                EXPElement element = new EXPElement();
                Dex[i].Main = KKdList<EXPElement>.New;
                s.P = Dex[i].MainOffset;
                while (true)
                {
                    element.Frame = s.RF32();
                    element.Both  = s.RU16();
                    element.ID    = s.RU16();
                    element.Value = s.RF32();
                    element.Trans = s.RF32();
                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                    Dex[i].Main.Add(element);
                }
                Dex[i].Main.Capacity = Dex[i].Main.Count;

                Dex[i].Eyes = KKdList<EXPElement>.New;
                s.P = Dex[i].EyesOffset;
                while(true)
                {
                    element.Frame = s.RF32();
                    element.Both  = s.RU16();
                    element.ID    = s.RU16();
                    element.Value = s.RF32();
                    element.Trans = s.RF32();
                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                    Dex[i].Eyes.Add(element);
                }
                Dex[i].Eyes.Capacity = Dex[i].Eyes.Count;

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
                Dex[i0].MainOffset = s.P;
                for (i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    s.W(Dex[i0].Main[i1].Frame);
                    s.W(Dex[i0].Main[i1].Both );
                    s.W(Dex[i0].Main[i1].ID   );
                    s.W(Dex[i0].Main[i1].Value);
                    s.W(Dex[i0].Main[i1].Trans);
                }
                s.W(999999f);
                s.W(0xFFFF);
                s.W(0x0L);
                s.A(0x20);

                Dex[i0].EyesOffset = s.P;
                for (i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                {
                    s.W(Dex[i0].Eyes[i1].Frame);
                    s.W(Dex[i0].Eyes[i1].Both );
                    s.W(Dex[i0].Eyes[i1].ID   );
                    s.W(Dex[i0].Eyes[i1].Value);
                    s.W(Dex[i0].Eyes[i1].Trans);
                }
                s.W(999999f);
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
                s.WX(Dex[i].MainOffset);
                s.WX(Dex[i].EyesOffset);
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
                    if ((temp = dex[i0]["Main", true]).NotNull)
                    {
                        Dex[i0].Main = KKdList<EXPElement>.New;
                        Dex[i0].Main.Capacity = temp.Array.Length;
                        for (i1 = 0; i1 < Dex[i0].Main.Capacity; i1++)
                            Dex[i0].Main.Add(EXPElement.Read(temp[i1]));
                    }
                    if ((temp = dex[i0]["Eyes", true]).NotNull)
                    {
                        Dex[i0].Eyes = KKdList<EXPElement>.New;
                        Dex[i0].Eyes.Capacity = temp.Array.Length;
                        for (i1 = 0; i1 < Dex[i0].Eyes.Capacity; i1++)
                            Dex[i0].Eyes.Add(EXPElement.Read(temp[i1]));
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
                MsgPack main = new MsgPack(Dex[i0].Main.Count, "Main");
                for (i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                    main[i1] = Dex[i0].Main[i1].Write();
                exp.Add(main);

                MsgPack eyes = new MsgPack(Dex[i0].Eyes.Count, "Eyes");
                for (i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                    eyes[i1] = Dex[i0].Eyes[i1].Write();
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
            public int MainOffset;
            public int EyesOffset;
            public int NameOffset;
            public string Name;
            public KKdList<EXPElement> Main;
            public KKdList<EXPElement> Eyes;

            public override string ToString() => Name;
        }

        public struct EXPElement
        {
            public  float Frame;
            public ushort Both;
            public ushort ID;
            public  float Value;
            public  float Trans;

            public static EXPElement Read(MsgPack msg) =>
                new EXPElement() { Frame = msg.RF32("F"), Both  = msg.RU16("B"),
                                   ID    = msg.RU16("I"), Value = msg.RF32("V"),
                                   Trans = msg.RF32("T"), };

            public MsgPack Write() =>
                MsgPack.New.Add("F", Frame).Add("B", Both )
                           .Add("I",    ID).Add("V", Value)
                           .Add("T", Trans);
        }
    }
}
