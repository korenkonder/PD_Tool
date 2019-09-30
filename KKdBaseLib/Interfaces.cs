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

    public interface INull
    {
        bool  IsNull { get; }
        bool NotNull { get; }
    }
}
