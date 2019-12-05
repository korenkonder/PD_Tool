namespace KKdBaseLib
{
    public struct KFT0 : IKF
    {
        public float F;

        public KFT0(float F = 0)
        { this.F = F; }

        public KFT0 ToT0() => this;
        public KFT1 ToT1() =>
            new KFT1(F);
        public KFT2 ToT2() =>
            new KFT2(F);
        public KFT3 ToT3() =>
            new KFT3(F);

        public IKF Check() => this;

        public override string ToString() => ToString(true, 7);
        public string ToString(int round = 7, bool brackets = true) =>
            ToString(brackets, round);
        public string ToString(bool brackets = true, int round = 7) =>
            Extensions.ToS(F, round);

        public static explicit operator KFT1(KFT0 KF) =>
            new KFT1(KF.F);
        public static explicit operator KFT2(KFT0 KF) =>
            new KFT2(KF.F);
        public static explicit operator KFT3(KFT0 KF) =>
            new KFT3(KF.F);

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        { if (obj is KFT0 b) return this == b; else return base.Equals(obj); }

        public static bool operator > (float a, KFT0 b) => a >  b.F;
        public static bool operator < (float a, KFT0 b) => a <  b.F;
        public static bool operator >=(float a, KFT0 b) => a >= b.F;
        public static bool operator <=(float a, KFT0 b) => a <= b.F;
        public static bool operator > (KFT0 a, float b) => a.F >  b;
        public static bool operator < (KFT0 a, float b) => a.F <  b;
        public static bool operator >=(KFT0 a, float b) => a.F >= b;
        public static bool operator <=(KFT0 a, float b) => a.F <= b;
        public static bool operator > (KFT0 a, KFT0 b) => a.F >  b.F;
        public static bool operator < (KFT0 a, KFT0 b) => a.F <  b.F;
        public static bool operator >=(KFT0 a, KFT0 b) => a.F >= b.F;
        public static bool operator <=(KFT0 a, KFT0 b) => a.F <= b.F;
        public static bool operator ==(KFT0 a, KFT0 b) => a.F == b.F;
        public static bool operator !=(KFT0 a, KFT0 b) => a.F != b.F;
    }

    public struct KFT1 : IKF
    {
        public float F;
        public float V;

        public KFT1(float F = 0, float V = 0)
        { this.F = F; this.V = V; }

        public KFT0 ToT0() =>
            new KFT0(F);
        public KFT1 ToT1() => this;
        public KFT2 ToT2() =>
            new KFT2(F, V);
        public KFT3 ToT3() =>
            new KFT3(F, V);

        public IKF Check() =>
            V == 0 ? (KFT0)this : (IKF)this;

        public override string ToString() => ToString(true, 7);
        public string ToString(int round = 7, bool brackets = true) =>
            ToString(brackets, round);
        public string ToString(bool brackets = true, int round = 7) =>
            (brackets ? "(" : "") + Extensions.ToS(F, round) + "," +
            Extensions.ToS(V, round) + (brackets ? ")" : "");

        public static explicit operator KFT0(KFT1 KF) =>
            new KFT0(KF.F);
        public static explicit operator KFT2(KFT1 KF) =>
            new KFT2(KF.F, KF.V);
        public static explicit operator KFT3(KFT1 KF) =>
            new KFT3(KF.F, KF.V);

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        { if (obj is KFT1 b) return this == b; else return base.Equals(obj); }

        public static bool operator > (float a, KFT1 b) => a >  b.F;
        public static bool operator < (float a, KFT1 b) => a <  b.F;
        public static bool operator >=(float a, KFT1 b) => a >= b.F;
        public static bool operator <=(float a, KFT1 b) => a <= b.F;
        public static bool operator > (KFT1 a, float b) => a.F >  b;
        public static bool operator < (KFT1 a, float b) => a.F <  b;
        public static bool operator >=(KFT1 a, float b) => a.F >= b;
        public static bool operator <=(KFT1 a, float b) => a.F <= b;
        public static bool operator > (KFT1 a, KFT1 b) => a.F >  b.F;
        public static bool operator < (KFT1 a, KFT1 b) => a.F <  b.F;
        public static bool operator >=(KFT1 a, KFT1 b) => a.F >= b.F;
        public static bool operator <=(KFT1 a, KFT1 b) => a.F <= b.F;
        public static bool operator ==(KFT1 a, KFT1 b) => a.F == b.F && a.V == b.V;
        public static bool operator !=(KFT1 a, KFT1 b) => a.F != b.F || a.V != b.V;
    }

    public struct KFT2 : IKF
    {
        public float F;
        public float V;
        public float T;

        public KFT2(float F = 0, float V = 0, float T = 0)
        { this.F = F; this.V = V; this.T = T; }

        public KFT0 ToT0() =>
            new KFT0(F);
        public KFT1 ToT1() =>
            new KFT1(F, V);
        public KFT2 ToT2() => this;
        public KFT3 ToT3() =>
            new KFT3(F, V, T, T);

        public KFT3 ToT3(IKF Previous) =>
            Previous is KFT2 PreviousT2 ?
            new KFT3(F, V, PreviousT2.T, T) :
            new KFT3(F, V,            T, T);

        public IKF Check() =>
            T == 0 ? (V == 0 ? (IKF)ToT0() : ToT1()) : this;

        public override string ToString() => ToString(true, 7);
        public string ToString(int round = 7, bool brackets = true) =>
            ToString(brackets, round);
        public string ToString(bool brackets = true, int round = 7) =>
            (brackets ? "(" : "") + Extensions.ToS(F, round) + "," + Extensions.
            ToS(V, round) + "," + Extensions.ToS(T, round) + (brackets ? ")" : "");

        public static explicit operator KFT0(KFT2 KF) =>
            new KFT0(KF.F);
        public static explicit operator KFT1(KFT2 KF) =>
            new KFT1(KF.F, KF.V);
        public static explicit operator KFT3(KFT2 KF) =>
            new KFT3(KF.F, KF.V, KF.T, KF.T);

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        { if (obj is KFT2 b) return this == b; else return base.Equals(obj); }

        public static bool operator > (float a, KFT2 b) => a >  b.F;
        public static bool operator < (float a, KFT2 b) => a <  b.F;
        public static bool operator >=(float a, KFT2 b) => a >= b.F;
        public static bool operator <=(float a, KFT2 b) => a <= b.F;
        public static bool operator > (KFT2 a, float b) => a.F >  b;
        public static bool operator < (KFT2 a, float b) => a.F <  b;
        public static bool operator >=(KFT2 a, float b) => a.F >= b;
        public static bool operator <=(KFT2 a, float b) => a.F <= b;
        public static bool operator > (KFT2 a, KFT2 b) => a.F >  b.F;
        public static bool operator < (KFT2 a, KFT2 b) => a.F <  b.F;
        public static bool operator >=(KFT2 a, KFT2 b) => a.F >= b.F;
        public static bool operator <=(KFT2 a, KFT2 b) => a.F <= b.F;
        public static bool operator ==(KFT2 a, KFT2 b) => a.F == b.F && a.V == b.V && a.T == b.T;
        public static bool operator !=(KFT2 a, KFT2 b) => a.F != b.F || a.V != b.V || a.T != b.T;
    }

    public struct KFT3 : IKF
    {
        public float F;
        public float V;
        public float T1;
        public float T2;

        public KFT3(float F = 0, float V = 0, float T1 = 0, float T2 = 0)
        { this.F = F; this.V = V; this.T1 = T1; this.T2 = T2; }

        public KFT0 ToT0() =>
            new KFT0(F);
        public KFT1 ToT1() =>
            new KFT1(F, V);
        public KFT2 ToT2() =>
            new KFT2(F, V, T1);
        public KFT3 ToT3() => this;

        public IKF Check() =>
            T1 == 0 && T2 == 0 ? (V == 0 ? (IKF)(KFT0)this : (KFT1)this) : T1 == T2 ? (KFT2)this : (IKF)this;

        public override string ToString() => ToString(true, 7);
        public string ToString(int round = 7, bool brackets = true) =>
            ToString(brackets, round);
        public string ToString(bool brackets = true, int round = 7) =>
            (brackets ? "(" : "") + Extensions.ToS(F, round) + "," + Extensions.ToS(V, round) + "," +
            Extensions.ToS(T1, round) + "," + Extensions.ToS(T2, round) + (brackets ? ")" : "");

        public static explicit operator KFT0(KFT3 KF) =>
            new KFT0(KF.F);
        public static explicit operator KFT1(KFT3 KF) =>
            new KFT1(KF.F, KF.V);
        public static explicit operator KFT2(KFT3 KF) =>
            new KFT2(KF.F, KF.V, KF.T1);

        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        { if (obj is KFT3 b) return this == b; else return base.Equals(obj); }

        public static bool operator > (float a, KFT3 b) => a >  b.F;
        public static bool operator < (float a, KFT3 b) => a <  b.F;
        public static bool operator >=(float a, KFT3 b) => a >= b.F;
        public static bool operator <=(float a, KFT3 b) => a <= b.F;
        public static bool operator > (KFT3 a, float b) => a.F >  b;
        public static bool operator < (KFT3 a, float b) => a.F <  b;
        public static bool operator >=(KFT3 a, float b) => a.F >= b;
        public static bool operator <=(KFT3 a, float b) => a.F <= b;
        public static bool operator > (KFT3 a, KFT3 b) => a.F >  b.F;
        public static bool operator < (KFT3 a, KFT3 b) => a.F <  b.F;
        public static bool operator >=(KFT3 a, KFT3 b) => a.F >= b.F;
        public static bool operator <=(KFT3 a, KFT3 b) => a.F <= b.F;
        public static bool operator ==(KFT3 a, KFT3 b) => a.F == b.F && a.V == b.V && a.T1 == b.T1 && a.T2 == b.T2;
        public static bool operator !=(KFT3 a, KFT3 b) => a.F != b.F || a.V != b.V || a.T1 != b.T1 || a.T2 != b.T2;

        public IKF ToT2(IKF Previous, out IKF Current)
        {
            Current = Previous is KFT2 PreviousT2
                 ? new KFT2(PreviousT2.F, PreviousT2.V, T1)
                 : new KFT2(F, V, T1);
            return new KFT2(F, V, T2);
        }
    }
}
