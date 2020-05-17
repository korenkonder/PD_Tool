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
        string ToString(int round, bool brackets);
        string ToString(bool brackets, int round);
    }

    public interface INull
    {
        bool  IsNull { get; }
        bool NotNull { get; }
    }
}
