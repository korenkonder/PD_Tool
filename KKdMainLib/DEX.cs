using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct DEX : System.IDisposable
    {
        private int i, i0, i1;
        private Header header;
        private Stream _IO;

        public EXP[] Dex;

        public void DEXReader(string filepath, string ext)
        {
            Dex = null;
            header = new Header();
            _IO = File.OpenReader(filepath + ext);

            header.Format = Format.F;
            header.SectionSignature = _IO.RI32();
            if (header.SectionSignature == 0x43505845)
                header = _IO.ReadHeader(true, true);
            if (header.SectionSignature != 0x64) return;

            _IO.O = _IO.P - 0x4;
            Dex = new EXP[_IO.RI32()];
            int DEXOffset = _IO.RI32();
            int DEXNameOffset = _IO.RI32();
            if (DEXNameOffset == 0x00) { _IO.Format = header.Format = Format.X; DEXNameOffset = (int)_IO.RIX(); }

            _IO.P = DEXOffset;
            for (i = 0; i < Dex.Length; i++)
            {
                Dex[i].MainOffset = (int)_IO.RIX();
                Dex[i].EyesOffset = (int)_IO.RIX();
            }
            _IO.P = DEXNameOffset;
            for (i = 0; i < Dex.Length; i++)
                Dex[i].NameOffset = (int)_IO.RIX();

            for (i = 0; i < Dex.Length; i++)
            {
                EXPElement element = new EXPElement();
                Dex[i].Main = KKdList<EXPElement>.New;
                _IO.P = Dex[i].MainOffset;
                while (true)
                {
                    element.Frame = _IO.RF32();
                    element.Both  = _IO.RU16();
                    element.ID    = _IO.RU16();
                    element.Value = _IO.RF32();
                    element.Trans = _IO.RF32();
                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                    Dex[i].Main.Add(element);
                }

                Dex[i].Eyes = KKdList<EXPElement>.New;
                _IO.P = Dex[i].EyesOffset;
                while(true)
                {
                    element.Frame = _IO.RF32();
                    element.Both  = _IO.RU16();
                    element.ID    = _IO.RU16();
                    element.Value = _IO.RF32();
                    element.Trans = _IO.RF32();
                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                    Dex[i].Eyes.Add(element);
                }

                Dex[i].Name = _IO.RSaO(Dex[i].NameOffset);
            }

            _IO.C();
        }

        public void DEXWriter(string filepath, Format format)
        {
            if (Dex == null || Dex.Length < 1) return;

            header = new Header();
            _IO = File.OpenWriter(filepath + (format > Format.F && format < Format.FT ? ".dex" : ".bin"), true);
            header.Format = _IO.Format = format;

            _IO.Format = format;
            _IO.O = format > Format.F ? 0x20 : 0;
            _IO.P = 0;
            _IO.W(0x64);
            _IO.W(Dex.Length);

            _IO.WX(header.IsX ? 0x28 : 0x20);
            _IO.WX(0x00);

            int Position0 = _IO.P;
            _IO.W(0x00L);
            _IO.W(0x00L);

            for (i = 0; i < Dex.Length * 3; i++) _IO.WX(0x00);
            _IO.A(0x20);

            for (i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = _IO.P;
                for (i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    _IO.W(Dex[i0].Main[i1].Frame);
                    _IO.W(Dex[i0].Main[i1].Both );
                    _IO.W(Dex[i0].Main[i1].ID   );
                    _IO.W(Dex[i0].Main[i1].Value);
                    _IO.W(Dex[i0].Main[i1].Trans);
                }
                _IO.W(999999f);
                _IO.W(0xFFFF);
                _IO.W(0x0L);
                _IO.A(0x20);

                Dex[i0].EyesOffset = _IO.P;
                for (i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                {
                    _IO.W(Dex[i0].Eyes[i1].Frame);
                    _IO.W(Dex[i0].Eyes[i1].Both );
                    _IO.W(Dex[i0].Eyes[i1].ID   );
                    _IO.W(Dex[i0].Eyes[i1].Value);
                    _IO.W(Dex[i0].Eyes[i1].Trans);
                }
                _IO.W(999999f);
                _IO.W(0xFFFF);
                _IO.W(0x0L);
                _IO.A(0x20);
            }
            for (i = 0; i < Dex.Length; i++)
            {
                Dex[i].NameOffset = _IO.P;
                _IO.W(Dex[i].Name + "\0");
            }
            _IO.A(0x10, true);

            _IO.P = header.IsX ? 0x28 : 0x20;
            for (i = 0; i < Dex.Length; i++)
            {
                _IO.WX(Dex[i].MainOffset);
                _IO.WX(Dex[i].EyesOffset);
            }
            int namesPosition = _IO.P;
            for (i = 0; i < Dex.Length; i++)
                _IO.WX(Dex[i].NameOffset);

            _IO.P = Position0 - (header.IsX ? 8 : 4);
            _IO.W(namesPosition);

            if (format > Format.F)
            {
                int offset = _IO.L;
                _IO.O = 0;
                _IO.P = _IO.L;
                _IO.WEOFC(0);
                _IO.P = 0;
                header.DataSize = offset;
                header.SectionSize = offset;
                header.Signature = 0x43505845;
                header.UseSectionSize = true;
                _IO.W(header);
            }
            _IO.C();
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
                        for (i1 = 0; i1 < this.Dex[i0].Eyes.Capacity; i1++)
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
        { if (!disposed) { if (_IO != null) _IO.D(); _IO = null; Dex = null; header = default; disposed = true; } }

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
