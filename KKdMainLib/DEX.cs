using System.Collections.Generic;
using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public class DEX
    {
        public DEX()
        { Dex = null; Header = new Header(); }

        private int Offset = 0;
        private Header Header;
        private Stream IO;

        public EXP[] Dex;

        public int DEXReader(string filepath, string ext)
        {
            Header = new Header();
            IO = File.OpenReader(filepath + ext);

            Header.Format = Format.F;
            Header.SectionSignature = IO.RI32();
            if (Header.SectionSignature == 0x43505845)
                Header = IO.ReadHeader(true, true);
            if (Header.SectionSignature != 0x64) return 0;

            IO.O = IO.P - 0x4;
            Dex = new EXP[IO.RI32()];
            int DEXOffset = IO.RI32();
            int DEXNameOffset = IO.RI32();
            if (DEXNameOffset == 0x00) { Header.Format = Format.X; DEXNameOffset = (int)IO.RI64(); }

            IO.S(DEXOffset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
                Dex[i0] = new EXP { Main = KKdList<EXPElement>.New, Eyes = KKdList<EXPElement>.New };

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.RI32();
                if (Header.IsX) IO.RI32();
                Dex[i0].EyesOffset = IO.RI32();
                if (Header.IsX) IO.RI32();
            }
            IO.S(DEXNameOffset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.RI32();
                if (Header.IsX) IO.RI32();
            }

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                EXPElement element = new EXPElement();
                IO.S(Dex[i0].MainOffset + Offset, 0);
                while (true)
                {
                    element.Frame = IO.RF32();
                    element.Both  = IO.RU16();
                    element.ID    = IO.RU16();
                    element.Value = IO.RF32();
                    element.Trans = IO.RF32();
                    Dex[i0].Main.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                IO.S(Dex[i0].EyesOffset, 0);
                while(true)
                {
                    element.Frame = IO.RF32();
                    element.Both  = IO.RU16();
                    element.ID    = IO.RU16();
                    element.Value = IO.RF32();
                    element.Trans = IO.RF32();
                    Dex[i0].Eyes.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                }

                Dex[i0].Name = IO.RSaO(Dex[i0].NameOffset);
            }

            IO.C();
            return 1;
        }

        public void DEXWriter(string filepath, Format Format)
        {
            Header = new Header();
            IO = File.OpenWriter(filepath + (Format > Format.F ? ".dex" : ".bin"), true);
            Header.Format = IO.Format = Format;

            IO.O = Format > Format.F ? 0x20 : 0;
            IO.W(0x64);
            IO.W(Dex.Length);

            IO.WX(Header.IsX ? 0x28 : 0x20);
            IO.WX(0x00);

            int Position0 = IO.P;
            IO.W(0x00L);
            IO.W(0x00L);

            for (int i = 0; i < Dex.Length * 3; i++) IO.WX(0x00);

            IO.A(0x20, true);

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.P;
                for (int i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    IO.W(Dex[i0].Main[i1].Frame);
                    IO.W(Dex[i0].Main[i1].Both );
                    IO.W(Dex[i0].Main[i1].ID   );
                    IO.W(Dex[i0].Main[i1].Value);
                    IO.W(Dex[i0].Main[i1].Trans);
                }
                IO.A(0x20, true);

                Dex[i0].EyesOffset = IO.P;
                for (int i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                {
                    IO.W(Dex[i0].Eyes[i1].Frame);
                    IO.W(Dex[i0].Eyes[i1].Both );
                    IO.W(Dex[i0].Eyes[i1].ID   );
                    IO.W(Dex[i0].Eyes[i1].Value);
                    IO.W(Dex[i0].Eyes[i1].Trans);
                }
                IO.A(0x20, true);
            }
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.P;
                IO.W(Dex[i0].Name + "\0");
            }
            IO.A(0x10, true);

            IO.P = Header.IsX ? 0x28 : 0x20;
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                IO.WX(Dex[i0].MainOffset);
                IO.WX(Dex[i0].EyesOffset);
            }
            int Position1 = IO.P;
            for (int i0 = 0; i0 < Dex.Length; i0++)
                IO.WX(Dex[i0].NameOffset);

            IO.P = Position0 - (Header.IsX ? 8 : 4);
            IO.W(Position1);

            if (Format > Format.F)
            {
                Offset = IO.L;
                IO.O = 0;
                IO.P = IO.L;
                IO.WEOFC(0);
                IO.P = 0;
                Header.DataSize = Offset;
                Header.SectionSize = Offset;
                Header.Signature = 0x43505845;
                IO.W(Header, true);
            }
            IO.C();
        }

        public int MsgPackReader(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            this.Dex = new EXP[0];
            Header = new Header();

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            MsgPack Dex;
            if ((Dex = MsgPack["Dex", true]).NotNull)
            {
                this.Dex = new EXP[Dex.Array.Length];
                for (i0 = 0; i0 < this.Dex.Length; i0++)
                {
                    this.Dex[i0] = new EXP { Name = Dex[i0].RS("Name") };

                    MsgPack Temp;
                    if ((Temp = MsgPack["Main", true]).NotNull)
                    {
                        this.Dex[i0].Main = KKdList<EXPElement>.New;
                        for (i1 = 0; i1 < this.Dex[i0].Main.Count; i1++)
                            this.Dex[i0].Main.Add(EXPElement.Read(Temp[i1]));
                    }
                    if ((Temp = MsgPack["Eyes", true]).NotNull)
                    {
                        this.Dex[i0].Eyes = KKdList<EXPElement>.New;
                        for (i1 = 0; i1 < this.Dex[i0].Main.Count; i1++)
                            this.Dex[i0].Eyes.Add(EXPElement.Read(Temp[i1]));
                    }
                    Temp.Dispose();
                }
            }
            Dex.Dispose();
            MsgPack.Dispose();
            return 1;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            MsgPack Dex = new MsgPack(this.Dex.Length, "Dex");
            for (i0 = 0; i0 < this.Dex.Length; i0++)
            {
                MsgPack EXP = MsgPack.New.Add("Name", this.Dex[i0].Name);
                MsgPack Main = new MsgPack(this.Dex[i0].Main.Count, "Main");
                for (i1 = 0; i1 < this.Dex[i0].Main.Count; i1++)
                    Main[i1] = this.Dex[i0].Main[i1].Write();
                EXP.Add(Main);

                MsgPack Eyes = new MsgPack(this.Dex[i0].Eyes.Count, "Eyes");
                for (i1 = 0; i1 < this.Dex[i0].Eyes.Count; i1++)
                    Eyes[i1] = this.Dex[i0].Eyes[i1].Write();
                EXP.Add(Eyes);
                Dex[i0] = EXP;
            }

            Dex.Write(true, file, JSON);
        }

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
