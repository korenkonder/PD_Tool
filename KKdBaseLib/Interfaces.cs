namespace KKdBaseLib
{
    public interface IKF
    {
        KFT0 ToT0();
        KFT1 ToT1();
        KFT2 ToT2();
        KFT3 ToT3();

        IKF Check();
        string ToString();
        string ToString(bool Brackets);
    }

    public interface INull
    {
        bool  IsNull { get; }
        bool NotNull { get; }
    }
}
