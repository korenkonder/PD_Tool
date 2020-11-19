using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Light : System.IDisposable
    {
        private int i, i0;
        private Stream s;
        private Header header;

        public CountPointer<CountPointer<LIT>> LITs;

        public void LITReader(string file)
        {
            LITs = default;
            s = File.OpenReader(file + ".lit", true);
            header = s.ReadHeader();
            if (header.Signature != 0x4354494C || header.InnerSignature != 0x2) return;
            s.IsBE = header.UseBigEndian;

            LITs = s.RCPE<CountPointer<LIT>>();
            if (LITs.C < 1) { s.C(); LITs.C = -1; return; }

            s.P = LITs.O;
            for (i = 0; i < LITs.C; i++)
            {
                LITs[i] = s.RCPX<LIT>();
                if ((LITs[i].C > 0 || LITs[i].O == 0) && !s.IsX) { s.C(); LITs.C = -1; return; }
            }

            for (i = 0; i < LITs.C; i++)
            {
                s.P = LITs[i].O;
                for (i0 = 0; i0 < LITs[i].C; i0++)
                {
                    ref LIT lit = ref LITs.E[i].E[i0];
                    lit.Id    = (Id   )s.RI32E();
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
            s.C();
        }

        public void TXTWriter(string file)
        {
            i = 0;
            if (LITs.C < 1) return;

            s = File.OpenWriter();
            s.WPSSJIS("Type,AmbientR,AmbientG,AmbientB,DiffuseR,DiffuseG,DiffuseB,SpecularR,SpecularG," +
                "SpecularB,SpecularA,PosX,PosY,PosZ,ToneCurveBegin,ToneCurveEnd,ToneCurveBlendRate," +
                (file.EndsWith("_chara") ? "コメント" : "ID") + "\n");
            for (i0 = 0; i0 < LITs[i].C; i0++)
                s.W($"{LITs[i][i0]},{i}\n");
            File.WriteAllBytes($"{file}_light.txt", s.ToArray(true));
        }

        public struct LIT
        {
            public Id Id;
            public Flags Flags;
            public Type Type;
            public Vec4 Ambient;
            public Vec4 Diffuse;
            public Vec4 Specular;
            public Vec3 Position;
            public Vec3 ToneCurve;

            public override string ToString() => Flags == 0 ? ",,,,,,,,,,,,,,,," : $"{Type}," +
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

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (s != null) s.D(); s = null; LITs = default; header = default; disposed = true; } }

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
