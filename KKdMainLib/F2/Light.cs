using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Light : System.IDisposable
    {
        private int i, i0;
        private Stream _IO;
        private Header header;

        public CountPointer<CountPointer<LIT>> LITs;

        public void LITReader(string file)
        {
            LITs = default;
            _IO = File.OpenReader(file + ".lit", true);
            header = _IO.ReadHeader();
            if (header.Signature != 0x4354494C || header.InnerSignature != 0x2 ||
                header.SectionSignature != 0x2) return;

            LITs = _IO.RCPE<CountPointer<LIT>>();
            if (LITs.C < 1) { _IO.C(); LITs.C = -1; return; }

            _IO.P = LITs.O;
            for (i = 0; i < LITs.C; i++)
            {
                LITs[i] = _IO.RCPX<LIT>();
                if ((LITs[i].C > 0 || LITs[i].O == 0) && !_IO.IsX) { _IO.C(); LITs.C = -1; return; }
                /*{
                    _IO.Format = Header.Format = Format.X;
                    _IO.Offset = Header.Length;
                    _IO.Position = LITs.Offset;
                    LITs[i] = _IO.ReadCountPointerX<LIT>();
                }
                if (_IO.IsX) IO.ReadInt64();*/
            }

            for (i = 0; i < LITs.C; i++)
            {
                _IO.P = LITs[i].O;
                for (i0 = 0; i0 < LITs[i].C; i0++)
                {
                    ref LIT lit = ref LITs.E[i].E[i0];
                    lit.Id    = (Id   )_IO.RI32E();
                    lit.Flags = (Flags)_IO.RI32E();
                    lit.Type  = (Type )_IO.RI32E();
                    if (_IO.IsX) { _IO.RI64(); _IO.RI64(); _IO.RI64(); }
                    lit.Ambient  .X = _IO.RF32E();
                    lit.Ambient  .Y = _IO.RF32E();
                    lit.Ambient  .Z = _IO.RF32E();
                    lit.Ambient  .W = _IO.RF32E();
                    lit.Diffuse  .X = _IO.RF32E();
                    lit.Diffuse  .Y = _IO.RF32E();
                    lit.Diffuse  .Z = _IO.RF32E();
                    lit.Diffuse  .W = _IO.RF32E();
                    lit.Specular .X = _IO.RF32E();
                    lit.Specular .Y = _IO.RF32E();
                    lit.Specular .Z = _IO.RF32E();
                    lit.Specular .W = _IO.RF32E();
                    lit.Position .X = _IO.RF32E();
                    lit.Position .Y = _IO.RF32E();
                    lit.Position .Z = _IO.RF32E();
                    lit.ToneCurve.X = _IO.RF32E();
                    lit.ToneCurve.Y = _IO.RF32E();
                    lit.ToneCurve.Z = _IO.RF32E();
                    if (_IO.IsX) { _IO.RI64(); _IO.RI64(); _IO.RI64();
                                  _IO.RI64(); _IO.RI64(); _IO.RI32(); }
                }
            }
            _IO.C();
        }

        public void TXTWriter(string file)
        {
            i = 0;
            if (LITs.C < 1) return;

            _IO = File.OpenWriter();
            _IO.WPSSJIS("Type,AmbientR,AmbientG,AmbientB,DiffuseR,DiffuseG,DiffuseB,SpecularR,SpecularG," +
                "SpecularB,SpecularA,PosX,PosY,PosZ,ToneCurveBegin,ToneCurveEnd,ToneCurveBlendRate," +
                (file.EndsWith("_chara") ? "コメント" : "ID") + "\n");
            for (i0 = 0; i0 < LITs[i].C; i0++)
                _IO.W(LITs[i][i0] + "," + i + "\n");
            File.WriteAllBytes(file + "_light.txt", _IO.ToArray(true));
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

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); LITs = default; header = default; disposed = true; } }

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
