using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib
{
    public class DEX
    {
        public DEX()
        { Dex = null; Header = new PDHead(); }

        private int Offset = 0;
        private PDHead Header;
        
        public Stream IO;
        public EXP[] Dex;

        public int DEXReader(string filepath, string ext)
        {
            Header = new PDHead();
            IO = File.OpenReader(filepath + ext);

            Header.Format = Main.Format.F;
            Header.Signature = IO.ReadInt32();
            if (Header.Signature == 0x43505845)
                Header = IO.ReadHeader(true);
            if (Header.Signature != 0x64)
                return 0;

            Offset = IO.Position - 0x4;
            Dex = new EXP[IO.ReadInt32()];
            int DEXOffset = IO.ReadInt32();
            if (IO.ReadInt32() == 0x00) Header.Format = Main.Format.X;
            int DEXNameOffset = IO.ReadInt32();
            if (Header.IsX) IO.ReadInt32();

            IO.Seek(DEXOffset + Offset, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
                Dex[i0] = new EXP { Main = new List<EXPElement>(), Eyes = new List<EXPElement>() };

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
                Dex[i0].EyesOffset = IO.ReadInt32();
                if (Header.IsX) IO.ReadInt32();
            }
            IO.Seek(DEXNameOffset + Offset, 0);
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

                IO.Seek(Dex[i0].EyesOffset + Offset, 0);
                while(true)
                {
                    element.Frame = IO.ReadSingle();
                    element.Both  = IO.ReadUInt16();
                    element.ID    = IO.ReadUInt16();
                    element.Value = IO.ReadSingle();
                    element.Trans = IO.ReadSingle();
                    Dex[i0].Eyes.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                IO.Seek(Dex[i0].NameOffset + Offset, 0);
                Dex[i0].Name = IO.NullTerminatedUTF8();
            }

            IO.Close();
            return 1;
        }

        public void DEXWriter(string filepath, Main.Format Format)
        {
            Header = new PDHead() { Format = Format };
            IO = File.OpenWriter(filepath + (Header.Format > Main.Format.F ? ".Dex" : ".bin"), true);
            IO.Format = Header.Format;

            if (IO.Format > Main.Format.F)
            {
                Header.Lenght = 0x20;
                Header.DataSize = 0x00;
                Header.Signature = 0x43505845;
                Header.SectionSize = 0x00;
                IO.Write(Header);
            }

            IO.Write(0x64);
            IO.Write(Dex.Length);
            
            if (Header.IsX) IO.Write((long)0x28);
            else            IO.Write(      0x20);
            if (Header.IsX) IO.Write((long)0x00);
            else            IO.Write(      0x00);

            int Position0 = IO.Position;
            IO.Write((long)0x00);
            IO.Write((long)0x00);

            for (int i = 0; i < Dex.Length * 3; i++)
                if (Header.IsX) IO.Write((long)0x00);
                else            IO.Write(      0x00);

            IO.Align(0x20, true);

            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                Dex[i0].MainOffset = IO.Position - Header.Lenght;
                for (int i1 = 0; i1 < Dex[i0].Main.Count; i1++)
                {
                    IO.Write(Dex[i0].Main[i1].Frame);
                    IO.Write(Dex[i0].Main[i1].Both );
                    IO.Write(Dex[i0].Main[i1].ID   );
                    IO.Write(Dex[i0].Main[i1].Value);
                    IO.Write(Dex[i0].Main[i1].Trans);
                }
                IO.Align(0x20, true);

                Dex[i0].EyesOffset = IO.Position - Header.Lenght;
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
                Dex[i0].NameOffset = IO.Position - Header.Lenght;
                IO.Write(Dex[i0].Name + "\0");
            }
            IO.Align(0x10, true);

            if (Header.IsX) IO.Seek(Header.Lenght + 0x28, 0);
            else                                   IO.Seek(Header.Lenght + 0x20, 0);
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                IO.Write(Dex[i0].MainOffset);
                if (Header.IsX) IO.Write(0x00);
                IO.Write(Dex[i0].EyesOffset);
                if (Header.IsX) IO.Write(0x00);
            }
            int Position1 = IO.Position - Header.Lenght;
            for (int i0 = 0; i0 < Dex.Length; i0++)
            {
                IO.Write(Dex[i0].NameOffset);
                if (Header.IsX) IO.Write(0x00);
            }

            if (Header.IsX) IO.Seek(Position0 - 8, 0);
            else                                   IO.Seek(Position0 - 4, 0);
            IO.Write(Position1);

            if (IO.Format > Main.Format.F)
            {
                Offset = IO.Length - Header.Lenght;
                IO.Seek(IO.Length, 0);
                IO.WriteEOFC(0);
                IO.Seek(0, 0);
                Header.DataSize = Offset;
                Header.SectionSize = Offset;
                IO.Write(Header);
            }
            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            this.Dex = new EXP[0];
            Header = new PDHead();

            MsgPack MsgPack = file.ReadMP(JSON);

            if (MsgPack.Element("Dex", out MsgPack Dex, typeof(object[])))
            {
                MsgPack Temp = new MsgPack();

                this.Dex = new EXP[((object[])Dex.Object).Length];
                MsgPack EXP = new MsgPack();
                for (i0 = 0; i0 < this.Dex.Length; i0++)
                    if (Dex[i0].GetType() == typeof(MsgPack))
                    {
                        this.Dex[i0] = new EXP();
                        EXP = (MsgPack)Dex[i0];
                        this.Dex[i0].Name = EXP.ReadString("Name");
                        if (EXP.Element("Main", out Temp, typeof(object[])))
                        {
                            this.Dex[i0].Main = new List<EXPElement>
                            { Capacity = ((object[])Temp.Object).Length };
                            for (i1 = 0; i1 < this.Dex[i0].Main.Capacity; i1++)
                                if (Temp[i1].GetType() == typeof(MsgPack))
                                    this.Dex[i0].Main.Add(ReadEXP((MsgPack)Temp[i1]));
                        }
                        if (EXP.Element("Eyes", out Temp, typeof(object[])))
                        {
                            this.Dex[i0].Eyes = new List<EXPElement>
                            { Capacity = ((object[])Temp.Object).Length };
                            for (i1 = 0; i1 < this.Dex[i0].Eyes.Capacity; i1++)
                                if (Temp[i1].GetType() == typeof(MsgPack))
                                    this.Dex[i0].Eyes.Add(ReadEXP((MsgPack)Temp[i1]));
                        }
                    }
            }
            MsgPack = null;
        }

        private EXPElement ReadEXP(MsgPack mp) =>
            new EXPElement() { Frame = mp.ReadSingle("F"), Both  = mp.ReadUInt16("B"),
                               ID    = mp.ReadUInt16("I"), Value = mp.ReadSingle("V"),
                               Trans = mp.ReadSingle("T") };

        public void MsgPackWriter(string file, bool JSON)
        {
            int i0 = 0;
            int i1 = 0;
            MsgPack Dex = new MsgPack("Dex", this.Dex.Length);
            for (i0 = 0; i0 < this.Dex.Length; i0++)
            {
                MsgPack EXP = new MsgPack().Add("Name", this.Dex[i0].Name);
                MsgPack Main = new MsgPack("Main", this.Dex[i0].Main.Count);
                for (i1 = 0; i1 < this.Dex[i0].Main.Count; i1++)
                    Main[i1] = WriteEXP(this.Dex[i0].Main[i1]);
                EXP.Add(Main);

                MsgPack Eyes = new MsgPack("Eyes", this.Dex[i0].Eyes.Count);
                for (i1 = 0; i1 < this.Dex[i0].Eyes.Count; i1++)
                    Eyes[i1] = WriteEXP(this.Dex[i0].Eyes[i1]);
                EXP.Add(Eyes);
                Dex[i0] = EXP;
            }

            Dex.Write(true, file, JSON);
        }

        private MsgPack WriteEXP(EXPElement element) =>
            new MsgPack().Add("F", element.Frame).Add("B", element.Both ).Add("I", element.ID   )
                         .Add("V", element.Value).Add("T", element.Trans);

        public struct EXP
        {
            public int MainOffset;
            public int EyesOffset;
            public int NameOffset;
            public string Name;
            public List<EXPElement> Main;
            public List<EXPElement> Eyes;
        }

        public struct EXPElement
        {
            public  float Frame;
            public ushort Both;
            public ushort ID;
            public  float Value;
            public  float Trans;
        }
    }
}
