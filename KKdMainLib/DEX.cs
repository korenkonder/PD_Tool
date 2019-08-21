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
            Header.SectionSignature = IO.ReadInt32();
            if (Header.SectionSignature == 0x43505845)
                Header = IO.ReadHeader(true, true);
            if (Header.SectionSignature != 0x64) return 0;

            IO.Offset = IO.Position - 0x4;
            Dex = new EXP[IO.ReadInt32()];
            int DEXOffset = IO.ReadInt32();
            int DEXNameOffset = IO.ReadInt32();
            if (DEXNameOffset == 0x00) { Header.Format = Format.X; DEXNameOffset = (int)IO.ReadInt64(); }

            IO.Seek(DEXOffset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
                Dex[i0] = new EXP { Main = new List<EXPElement>(), Eyes = new List<EXPElement>() };

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
                Dex[i0].EyesOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
            }
            IO.Seek(DEXNameOffset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
            }

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                EXPElement element = new EXPElement();
                IO.Seek(Dex[i0].MainOffset + Offset, 0);
                while (true)
                {
                    element.Frame = IO.ReadSingle();
                    element.Both  = IO.ReadUInt16();
                    element.ID    = IO.ReadUInt16();
                    element.Value = IO.ReadSingle();
                    element.Trans = IO.ReadSingle();
                    Dex[i0].Main.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                IO.Seek(Dex[i0].EyesOffset, 0);
                while(true)
                {
                    element.Frame = IO.ReadSingle();
                    element.Both  = IO.ReadUInt16();
                    element.ID    = IO.ReadUInt16();
                    element.Value = IO.ReadSingle();
                    element.Trans = IO.ReadSingle();
                    Dex[i0].Eyes.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF) break;
                }

                Dex[i0].Name = IO.ReadStringAtOffset(Dex[i0].NameOffset);
            }

            IO.Close();
            return 1;
        }

        public void DEXWriter(string filepath, Format Format)
        {
            Header = new Header();
            IO = File.OpenWriter(filepath + (Format > Format.F ? ".dex" : ".bin"), true);
            Header.Format = IO.Format = Format;

            IO.Offset = Format > Format.F ? 0x20 : 0;
            IO.Write(0x64);
            IO.Write(Dex.Length);

            IO.WriteX(Header.IsX ? 0x28 : 0x20);
            IO.WriteX(0x00);

            int Position0 = IO.Position;
            IO.Write(0x00L);
            IO.Write(0x00L);

            for (int i = 0; i < Dex.Length * 3; i++) IO.WriteX(0x00);

            IO.Align(0x20, true);

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.Position;
                for (int i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    IO.Write(Dex[i0].Main[i1].Frame);
                    IO.Write(Dex[i0].Main[i1].Both );
                    IO.Write(Dex[i0].Main[i1].ID   );
                    IO.Write(Dex[i0].Main[i1].Value);
                    IO.Write(Dex[i0].Main[i1].Trans);
                }
                IO.Align(0x20, true);

                Dex[i0].EyesOffset = IO.Position;
                for (int i1 = 0; i1 < Dex[i0].Eyes.Count; i1++)
                {
                    IO.Write(Dex[i0].Eyes[i1].Frame);
                    IO.Write(Dex[i0].Eyes[i1].Both );
                    IO.Write(Dex[i0].Eyes[i1].ID   );
                    IO.Write(Dex[i0].Eyes[i1].Value);
                    IO.Write(Dex[i0].Eyes[i1].Trans);
                }
                IO.Align(0x20, true);
            }
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].NameOffset = IO.Position;
                IO.Write(Dex[i0].Name + "\0");
            }
            IO.Align(0x10, true);

            IO.Position = Header.IsX ? 0x28 : 0x20;
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                IO.WriteX(Dex[i0].MainOffset);
                IO.WriteX(Dex[i0].EyesOffset);
            }
            int Position1 = IO.Position;
            for (int i0 = 0; i0 < Dex.Length; i0++)
                IO.WriteX(Dex[i0].NameOffset);

            IO.Position = Position0 - (Header.IsX ? 8 : 4);
            IO.Write(Position1);

            if (Format > Format.F)
            {
                Offset = IO.Length;
                IO.Offset = 0;
                IO.Position = IO.Length;
                IO.WriteEOFC(0);
                IO.Position = 0;
                Header.DataSize = Offset;
                Header.SectionSize = Offset;
                Header.Signature = 0x43505845;
                IO.Write(Header, true);
            }
            IO.Close();
        }

        public int MsgPackReader(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            this.Dex = new EXP[0];
            Header = new Header();

            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);
            if (!MsgPack.ElementArray("Dex", out MsgPack Dex)) return 0;

            this.Dex = new EXP[Dex.Array.Length];
            for (i0 = 0; i0 < this.Dex.Length; i0++)
            {
                this.Dex[i0] = new EXP { Name = Dex[i0].ReadString("Name") };

                if (Dex[i0].ElementArray("Main", out MsgPack Main))
                {
                    this.Dex[i0].Main = new List<EXPElement>();
                    for (i1 = 0; i1 < Main.Array.Length; i1++)
                        this.Dex[i0].Main.Add(EXPElement.Read(Main[i1]));
                }
                if (Dex[i0].ElementArray("Eyes", out MsgPack Eyes))
                {
                    this.Dex[i0].Eyes = new List<EXPElement>();
                    for (i1 = 0; i1 < Eyes.Array.Length; i1++)
                        this.Dex[i0].Eyes.Add(EXPElement.Read(Eyes[i1]));
                }
            }
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
            public List<EXPElement> Main;
            public List<EXPElement> Eyes;

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
                new EXPElement() { Frame = msg.ReadSingle("F"), Both  = msg.ReadUInt16("B"),
                                   ID    = msg.ReadUInt16("I"), Value = msg.ReadSingle("V"),
                                   Trans = msg.ReadSingle("T"), };

            public MsgPack Write() =>
                MsgPack.New.Add("F", Frame).Add("B", Both )
                           .Add("I",    ID).Add("V", Value)
                           .Add("T", Trans);
        }
    }
}
