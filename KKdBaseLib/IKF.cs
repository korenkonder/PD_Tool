namespace KKdBaseLib
{
    public interface IKF<TKey, TVal>
    {
        TKey F { get; set; }

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
        public KFT1<TKey, TVal> ToT1() => new KFT1<TKey, TVal>(F);
        public KFT2<TKey, TVal> ToT2() => new KFT2<TKey, TVal>(F);
        public KFT3<TKey, TVal> ToT3() => new KFT3<TKey, TVal>(F);

        public IKF<TKey, TVal> Check() => this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            Extensions.ToString(F);
    }
    
    public struct KFT1<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;

        public KFT1(TKey F = default, TVal V = default)
        { this.F = F; this.V = V; }

        public KFT0<TKey, TVal> ToT0() => new KFT0<TKey, TVal>(F);
        public KFT1<TKey, TVal> ToT1() => this;
        public KFT2<TKey, TVal> ToT2() => new KFT2<TKey, TVal>(F, V);
        public KFT3<TKey, TVal> ToT3() => new KFT3<TKey, TVal>(F, V);

        public IKF<TKey, TVal> Check() =>
            V.Equals(default(TVal)) ? (IKF<TKey, TVal>)ToT0() : this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets = true) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," +
            Extensions.ToString(V) + (Brackets ? ")" : "");
    }
    
    public struct KFT2<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;
        public TVal T;

        public KFT2(TKey F = default, TVal V = default, TVal T = default)
        { this.F = F; this.V = V; this.T = T; }

        public KFT0<TKey, TVal> ToT0() => new KFT0<TKey, TVal>(F);
        public KFT1<TKey, TVal> ToT1() => new KFT1<TKey, TVal>(F, V);
        public KFT2<TKey, TVal> ToT2() => this;
        public KFT3<TKey, TVal> ToT3() => new KFT3<TKey, TVal>(F, V, T, T);

        public KFT3<TKey, TVal> ToT3(IKF<TKey, TVal> Previous) =>
            Previous is KFT2<TKey, TVal> PreviousT2 ? 
            new KFT3<TKey, TVal>(F, V, PreviousT2.T, T) :
            new KFT3<TKey, TVal>(F, V,            T, T);

        public IKF<TKey, TVal> Check() =>
            T.Equals(default(TVal)) ? (V.Equals(default(TVal)) ?
            (IKF<TKey, TVal>)ToT0() : ToT1()) : this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.
            ToString(V) + "," + Extensions.ToString(T) + (Brackets ? ")" : "");
    }
    
    public struct KFT3<TKey, TVal> : IKF<TKey, TVal>
    {
        public TKey F { get; set; }
        public TVal V;
        public TVal T1;
        public TVal T2;

        public KFT3(TKey F = default, TVal V = default, TVal T1 = default, TVal T2 = default)
        { this.F = F; this.V = V; this.T1 = T1; this.T2 = T2; }

        public KFT0<TKey, TVal> ToT0() => new KFT0<TKey, TVal>(F);
        public KFT1<TKey, TVal> ToT1() => new KFT1<TKey, TVal>(F, V);
        public KFT2<TKey, TVal> ToT2() => new KFT2<TKey, TVal>(F, V, T1);
        public KFT3<TKey, TVal> ToT3() => this;

        public IKF<TKey, TVal> Check() =>
            T1.Equals(default(TVal)) && T2.Equals(default(TVal)) ?
                (V.Equals(default(TVal)) ? ToT0() : (IKF<TKey, TVal>)ToT1()) :
                T1.Equals(T2) ? (IKF<TKey, TVal>)ToT2() : this;

        public override string ToString() => ToString(true);
        public string ToString(bool Brackets) =>
            (Brackets ? "(" : "") + Extensions.ToString(F) + "," + Extensions.ToString(V) + "," +
            Extensions.ToString(T1) + "," + Extensions.ToString(T2) + (Brackets ? ")" : "");

        public IKF<TKey, TVal> ToT2(IKF<TKey, TVal> Previous, out IKF<TKey, TVal> Current)
        {
            Current = Previous is KFT2<TKey, TVal> PreviousT2
                 ? new KFT2<TKey, TVal>(PreviousT2.F, PreviousT2.V, T1)
                 : new KFT2<TKey, TVal>(F, V, T1);
            return new KFT2<TKey, TVal>(F, V, T2);
        }
    }
}
