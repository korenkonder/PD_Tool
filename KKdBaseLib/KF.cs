namespace KKdBaseLib
{
    public struct KFT0 : IKF
    {
        public float F;

        public KFT0(float F = 0)
        { this.F = F; }

        public KFT0 ToT0() => this;
        public KFT1 ToT1() => this;
        public KFT2 ToT2() => this;
        public KFT3 ToT3() => this;

        public IKF Check() => this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            Extensions.ToString(F);

        public static implicit operator KFT1(KFT0 KF) =>
            new KFT1(KF.F);
        public static implicit operator KFT2(KFT0 KF) =>
            new KFT2(KF.F);
        public static implicit operator KFT3(KFT0 KF) =>
            new KFT3(KF.F);
    }
    
    public struct KFT1 : IKF
    {
        public float F;
        public float V;

        public KFT1(float F = 0, float V = 0)
        { this.F = F; this.V = V; }

        public KFT0 ToT0() => this;
        public KFT1 ToT1() => this;
        public KFT2 ToT2() => this;
        public KFT3 ToT3() => this;

        public IKF Check() =>
            V == 0 ? (KFT0)this : (IKF)this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," +
            Extensions.ToString(V) + (Brackets ? ")" : "");

        public static implicit operator KFT0(KFT1 KF) =>
            new KFT0(KF.F);
        public static implicit operator KFT2(KFT1 KF) =>
            new KFT2(KF.F, KF.V);
        public static implicit operator KFT3(KFT1 KF) =>
            new KFT3(KF.F, KF.V);
    }
    
    public struct KFT2 : IKF
    {
        public float F;
        public float V;
        public float T;

        public KFT2(float F = 0, float V = 0, float T = 0)
        { this.F = F; this.V = V; this.T = T; }

        public KFT0 ToT0() => this;
        public KFT1 ToT1() => this;
        public KFT2 ToT2() => this;
        public KFT3 ToT3() => this;

        public KFT3 ToT3(IKF Previous) =>
            Previous is KFT2 PreviousT2 ? 
            new KFT3(F, V, PreviousT2.T, T) :
            new KFT3(F, V,            T, T);

        public IKF Check() =>
            T == 0 ? (V == 0 ? (IKF)(KFT0)this : (KFT1)this) : this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.
            ToString(V) + "," + Extensions.ToString(T) + (Brackets ? ")" : "");

        public static implicit operator KFT0(KFT2 KF) =>
            new KFT0(KF.F);
        public static implicit operator KFT1(KFT2 KF) =>
            new KFT1(KF.F, KF.V);
        public static implicit operator KFT3(KFT2 KF) =>
            new KFT3(KF.F, KF.V, KF.T, KF.T);
    }
    
    public struct KFT3 : IKF
    {
        public float F;
        public float V;
        public float T1;
        public float T2;

        public KFT3(float F = 0, float V = 0, float T1 = 0, float T2 = 0)
        { this.F = F; this.V = V; this.T1 = T1; this.T2 = T2; }

        public KFT0 ToT0() => this;
        public KFT1 ToT1() => this;
        public KFT2 ToT2() => this;
        public KFT3 ToT3() => this;

        public IKF Check() =>
            T1 == 0 && T2 == 0 ? (V == 0 ? (IKF)(KFT0)this : (KFT1)this) : T1 == T2 ? (KFT2)this : (IKF)this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.ToString(V) + "," +
            Extensions.ToString(T1) + "," + Extensions.ToString(T2) + (Brackets ? ")" : "");

        public static implicit operator KFT0(KFT3 KF) =>
            new KFT0(KF.F);
        public static implicit operator KFT1(KFT3 KF) =>
            new KFT1(KF.F, KF.V);
        public static implicit operator KFT2(KFT3 KF) =>
            new KFT2(KF.F, KF.V, KF.T1);

        public IKF ToT2(IKF Previous, out IKF Current)
        {
            Current = Previous is KFT2 PreviousT2
                 ? new KFT2(PreviousT2.F, PreviousT2.V, T1)
                 : new KFT2(F, V, T1);
            return new KFT2(F, V, T2);
        }
    }
}
