using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Light
    {
        public CountPointer<CountPointer<LIT>> LITs;
        private Stream IO;
        private Header Header;
        private int i, i0;

        public void LITReader(string file)
        {
            LITs = default;
            IO = File.OpenReader(file + ".lit", true);
            Header = IO.ReadHeader();
            if (Header.Signature != 0x4354494C || Header.InnerSignature != 0x2 ||
                Header.SectionSignature != 0x2) return;

            LITs = IO.RCPE<CountPointer<LIT>>();
            if (LITs.C < 1) { IO.C(); LITs.C = -1; return; }

            IO.P = LITs.O;
            for (i = 0; i < LITs.C; i++)
            {
                LITs[i] = IO.RCPX<LIT>();
                if ((LITs[i].C > 0 || LITs[i].O == 0) && !IO.IsX) { IO.C(); LITs.C = -1; return; }
                /*{
                    IO.Format = Header.Format = Format.X;
                    IO.Offset = Header.Length;
                    IO.Position = LITs.Offset;
                    LITs[i] = IO.ReadCountPointerX<LIT>();
                }
                if (IO.IsX) IO.ReadInt64();*/
            }

            for (i = 0; i < LITs.C; i++)
            {
                IO.P = LITs[i].O;
                for (i0 = 0; i0 < LITs[i].C; i0++)
                {
                    ref LIT LIT = ref LITs.E[i].E[i0];
                    LIT.Id    = (Id   )IO.RI32E();
                    LIT.Flags = (Flags)IO.RI32E();
                    LIT.Type  = (Type )IO.RI32E();
                    if (IO.IsX) { IO.RI64(); IO.RI64(); IO.RI64(); }
                    LIT.Ambient  .X = IO.RF32E();
                    LIT.Ambient  .Y = IO.RF32E();
                    LIT.Ambient  .Z = IO.RF32E();
                    LIT.Ambient  .W = IO.RF32E();
                    LIT.Diffuse  .X = IO.RF32E();
                    LIT.Diffuse  .Y = IO.RF32E();
                    LIT.Diffuse  .Z = IO.RF32E();
                    LIT.Diffuse  .W = IO.RF32E();
                    LIT.Specular .X = IO.RF32E();
                    LIT.Specular .Y = IO.RF32E();
                    LIT.Specular .Z = IO.RF32E();
                    LIT.Specular .W = IO.RF32E();
                    LIT.Position .X = IO.RF32E();
                    LIT.Position .Y = IO.RF32E();
                    LIT.Position .Z = IO.RF32E();
                    LIT.ToneCurve.X = IO.RF32E();
                    LIT.ToneCurve.Y = IO.RF32E();
                    LIT.ToneCurve.Z = IO.RF32E();
                    if (IO.IsX) { IO.RI64(); IO.RI64(); IO.RI64();
                                  IO.RI64(); IO.RI64(); IO.RI32(); }
                }
            }
            IO.C();
        }

        public void TXTWriter(string file)
        {
            i = 0;
            if (LITs.C < 1) return;

            IO = File.OpenWriter();
            IO.WPSSJIS("Type,AmbientR,AmbientG,AmbientB,DiffuseR,DiffuseG,DiffuseB,SpecularR,SpecularG," +
                "SpecularB,SpecularA,PosX,PosY,PosZ,ToneCurveBegin,ToneCurveEnd,ToneCurveBlendRate," +
                (file.EndsWith("_chara") ? "コメント" : "ID") + "\n");
            for (i0 = 0; i0 < LITs[i].C; i0++)
                IO.W(LITs[i][i0] + "," + i + "\n");
            File.WriteAllBytes(file + "_light.txt", IO.ToArray(true));
        }

        public struct LIT
        {
            public Id Id;
            public Flags Flags;
            public Type Type;
            public Vector4 Ambient;
            public Vector4 Diffuse;
            public Vector4 Specular;
            public Vector3 Position;
            public Vector3 ToneCurve;

            public override string ToString() => Flags == 0 ? ",,,,,,,,,,,,,,,," : Type + "," +  
                ((Flags & Flags.Ambient  ) == 0 ? ",,,"  : Ambient  .ToString(6) + ",") +
                ((Flags & Flags.Diffuse  ) == 0 ? ",,,"  : Diffuse  .ToString(6) + ",") +
                ((Flags & Flags.Specular ) == 0 ? ",,,," : Specular .ToString(6) + ",") +
                ((Flags & Flags.Position ) == 0 ? ",,,"  : Position .ToString(6) + ",") +
                ((Flags & Flags.ToneCurve) == 0 ? ",,,"  : ToneCurve.ToString(6));
        }

        public enum Id : int
        {
            CHARA       = 0,
            STAGE       = 1,
            SUN         = 2,
            REFLECT     = 3,
            SHADOW      = 4,
            CHARA_COLOR = 5,
            CHARA_F     = 6,
            PROJECTION  = 7,
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
            Type      = 0b0000000001,
            Ambient   = 0b0000000010,
            Diffuse   = 0b0000000100,
            Specular  = 0b0000001000,
            Position  = 0b0000010000,
            ToneCurve = 0b1000000000,
        }
    }
}
