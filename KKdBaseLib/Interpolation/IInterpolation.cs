namespace KKdBaseLib.Interpolation
{
    public interface IInterpolation : INull
    {
        float     RequestedFramerate { get; set; }
        float InterpolationFramerate { get; set; }

        float Frame { get; }
        float Value { get; }
        
        float  SetTime (float  time);
        float  SetFrame(float frame);
        float NextFrame(float  time);
        float NextFrame();
        void ResetFrameCount();
    }
}
