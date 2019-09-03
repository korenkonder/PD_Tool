namespace KKdBaseLib
{
    public interface IKF<TKey, TVal>
    {
        TKey F { get; set; }

        KFT0<TKey, TVal> ToT0();
        KFT1<TKey, TVal> ToT1();
        KFT2<TKey, TVal> ToT2();
        KFT3<TKey, TVal> ToT3();

        IKF<TKey, TVal> Check();
        string ToString();
        string ToString(bool Brackets);
    }
    
    public struct KFT0<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }

        public KFT0(TKey F = default)
        { this.F = F; }

        public KFT0<TKey, TVal> ToT0() => this;
        public KFT1<TKey, TVal> ToT1() => this;
        public KFT2<TKey, TVal> ToT2() => this;
        public KFT3<TKey, TVal> ToT3() => this;

        public IKF<TKey, TVal> Check() => this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            Extensions.ToString(F);

        public static implicit operator KFT1<TKey, TVal>(KFT0<TKey, TVal> KF) =>
            new KFT1<TKey, TVal>(KF.F);
        public static implicit operator KFT2<TKey, TVal>(KFT0<TKey, TVal> KF) =>
            new KFT2<TKey, TVal>(KF.F);
        public static implicit operator KFT3<TKey, TVal>(KFT0<TKey, TVal> KF) =>
            new KFT3<TKey, TVal>(KF.F);
    }
    
    public struct KFT1<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;

        public KFT1(TKey F = default, TVal V = default)
        { this.F = F; this.V = V; }

        public KFT0<TKey, TVal> ToT0() => this;
        public KFT1<TKey, TVal> ToT1() => this;
        public KFT2<TKey, TVal> ToT2() => this;
        public KFT3<TKey, TVal> ToT3() => this;

        public IKF<TKey, TVal> Check() =>
            V.Equals(default(TVal)) ? (KFT0<TKey, TVal>)this : (IKF<TKey, TVal>)this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," +
            Extensions.ToString(V) + (Brackets ? ")" : "");

        public static implicit operator KFT0<TKey, TVal>(KFT1<TKey, TVal> KF) =>
            new KFT0<TKey, TVal>(KF.F);
        public static implicit operator KFT2<TKey, TVal>(KFT1<TKey, TVal> KF) =>
            new KFT2<TKey, TVal>(KF.F, KF.V);
        public static implicit operator KFT3<TKey, TVal>(KFT1<TKey, TVal> KF) =>
            new KFT3<TKey, TVal>(KF.F, KF.V);
    }
    
    public struct KFT2<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;
        public TVal T;

        public KFT2(TKey F = default, TVal V = default, TVal T = default)
        { this.F = F; this.V = V; this.T = T; }

        public KFT0<TKey, TVal> ToT0() => this;
        public KFT1<TKey, TVal> ToT1() => this;
        public KFT2<TKey, TVal> ToT2() => this;
        public KFT3<TKey, TVal> ToT3() => this;

        public KFT3<TKey, TVal> ToT3(IKF<TKey, TVal> Previous) =>
            Previous is KFT2<TKey, TVal> PreviousT2 ? 
            new KFT3<TKey, TVal>(F, V, PreviousT2.T, T) :
            new KFT3<TKey, TVal>(F, V,            T, T);

        public IKF<TKey, TVal> Check() =>
            T.Equals(default(TVal)) ? (V.Equals(default(TVal)) ?
            (KFT0<TKey, TVal>)this : (IKF<TKey, TVal>)this) : this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.
            ToString(V) + "," + Extensions.ToString(T) + (Brackets ? ")" : "");

        public static implicit operator KFT0<TKey, TVal>(KFT2<TKey, TVal> KF) =>
            new KFT0<TKey, TVal>(KF.F);
        public static implicit operator KFT1<TKey, TVal>(KFT2<TKey, TVal> KF) =>
            new KFT1<TKey, TVal>(KF.F, KF.V);
        public static implicit operator KFT3<TKey, TVal>(KFT2<TKey, TVal> KF) =>
            new KFT3<TKey, TVal>(KF.F, KF.V, KF.T, KF.T);
    }
    
    public struct KFT3<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;
        public TVal T1;
        public TVal T2;

        public KFT3(TKey F = default, TVal V = default, TVal T1 = default, TVal T2 = default)
        { this.F = F; this.V = V; this.T1 = T1; this.T2 = T2; }

        public KFT0<TKey, TVal> ToT0() => this;
        public KFT1<TKey, TVal> ToT1() => this;
        public KFT2<TKey, TVal> ToT2() => this;
        public KFT3<TKey, TVal> ToT3() => this;

        public IKF<TKey, TVal> Check() =>
            T1.Equals(default(TVal)) && T2.Equals(default(TVal)) ?
                (V.Equals(default(TVal)) ? (KFT0<TKey, TVal>)this : (IKF<TKey, TVal>)this) :
                T1.Equals(T2) ? (KFT2<TKey, TVal>)this : (IKF<TKey, TVal>)this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.ToString(V) + "," +
            Extensions.ToString(T1) + "," + Extensions.ToString(T2) + (Brackets ? ")" : "");

        public static implicit operator KFT0<TKey, TVal>(KFT3<TKey, TVal> KF) =>
            new KFT0<TKey, TVal>(KF.F);
        public static implicit operator KFT1<TKey, TVal>(KFT3<TKey, TVal> KF) =>
            new KFT1<TKey, TVal>(KF.F, KF.V);
        public static implicit operator KFT2<TKey, TVal>(KFT3<TKey, TVal> KF) =>
            new KFT2<TKey, TVal>(KF.F, KF.V, KF.T1);

        public IKF<TKey, TVal> ToT2(IKF<TKey, TVal> Previous, out IKF<TKey, TVal> Current)
        {
            Current = Previous is KFT2<TKey, TVal> PreviousT2
                 ? new KFT2<TKey, TVal>(PreviousT2.F, PreviousT2.V, T1)
                 : new KFT2<TKey, TVal>(F, V, T1);
            return new KFT2<TKey, TVal>(F, V, T2);
        }
    }
}
