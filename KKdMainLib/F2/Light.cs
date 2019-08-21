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

            LITs = IO.ReadCountPointerEndian<CountPointer<LIT>>();
            if (LITs.Count < 1) { IO.Close(); LITs.Count = -1; return; }

            IO.Position = LITs.Offset;
            for (i = 0; i < LITs.Count; i++)
            {
                LITs[i] = IO.ReadCountPointerX<LIT>();
                if ((LITs[i].Count > 0 || LITs[i].Offset == 0) && !IO.IsX) { IO.Close(); LITs.Count = -1; return; }
            }

            for (i = 0; i < LITs.Count; i++)
            {
                IO.Position = LITs[i].Offset;
                for (i0 = 0; i0 < LITs[i].Count; i0++)
                {
                    ref LIT LIT = ref LITs.Entries[i].Entries[i0];
                    LIT.Id    = (Id   )IO.ReadInt32Endian();
                    LIT.Flags = (Flags)IO.ReadInt32Endian();
                    LIT.Type  = (Type )IO.ReadInt32Endian();
                    if (IO.IsX) { IO.ReadInt64(); IO.ReadInt64(); IO.ReadInt64(); }
                    LIT.Ambient  .X = IO.ReadSingleEndian();
                    LIT.Ambient  .Y = IO.ReadSingleEndian();
                    LIT.Ambient  .Z = IO.ReadSingleEndian();
                    LIT.Ambient  .W = IO.ReadSingleEndian();
                    LIT.Diffuse  .X = IO.ReadSingleEndian();
                    LIT.Diffuse  .Y = IO.ReadSingleEndian();
                    LIT.Diffuse  .Z = IO.ReadSingleEndian();
                    LIT.Diffuse  .W = IO.ReadSingleEndian();
                    LIT.Specular .X = IO.ReadSingleEndian();
                    LIT.Specular .Y = IO.ReadSingleEndian();
                    LIT.Specular .Z = IO.ReadSingleEndian();
                    LIT.Specular .W = IO.ReadSingleEndian();
                    LIT.Position .X = IO.ReadSingleEndian();
                    LIT.Position .Y = IO.ReadSingleEndian();
                    LIT.Position .Z = IO.ReadSingleEndian();
                    LIT.ToneCurve.X = IO.ReadSingleEndian();
                    LIT.ToneCurve.Y = IO.ReadSingleEndian();
                    LIT.ToneCurve.Z = IO.ReadSingleEndian();
                    if (IO.IsX) { IO.ReadInt64(); IO.ReadInt64(); IO.ReadInt64();
                                  IO.ReadInt64(); IO.ReadInt64(); IO.ReadInt32(); }
                }
            }
            IO.Close();
        }

        public void TXTWriter(string file)
        {
            i = 0;
            if (LITs.Count < 1) return;

            IO = File.OpenWriter();
            IO.WriteShiftJIS("Type,AmbientR,AmbientG,AmbientB,DiffuseR,DiffuseG,DiffuseB,SpecularR,SpecularG," +
                "SpecularB,SpecularA,PosX,PosY,PosZ,ToneCurveBegin,ToneCurveEnd,ToneCurveBlendRate," +
                (file.EndsWith("_chara") ? "コメント" : "ID") + "\n");
            for (i0 = 0; i0 < LITs[i].Count; i0++)
                IO.Write(LITs[i][i0] + "," + i + "\n");
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
