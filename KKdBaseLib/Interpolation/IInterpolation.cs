namespace KKdBaseLib.Interpolation
{
    public interface IInterpolation<T> : INull
    {
        float RequestedFramerate { get; set; }

        float Frame { get; }
        float  Time { get; }
            T Value { get; }

        T  SetTime (float  time);
        T  SetFrame(float frame);
        T NextFrame(float  time);
        T NextFrame();
        void ResetFrameCount();
    }
}
