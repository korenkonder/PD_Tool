namespace KKdBaseLib.Interpolation
{
    public struct PDI : IInterpolation //Project DIVA Interpolation
    {
        private KFT2[] array;

        private float f;
        private float last;
        private float deltaFrame;
        private float @if;
        private float rf;
        private float lastTime;

        private KFT2 firstKey;
        private KFT2 lastKey;

        public float InterpolationFramerate
        { get => @if; set { @if = value; deltaFrame = @if / rf; } }
        public float RequestedFramerate
        { get => rf; set { rf = value; deltaFrame = @if / rf; } }

        public float Frame => f;
        public float Value => last;
        public bool  IsNull => array == null ?  true : array.Length < 1;
        public bool NotNull => array == null ? false : array.Length > 0;
        
        public PDI(KFT2[] Array, float InterpolationFramerate = 60, float RequestedFramerate = 60)
        {
            lastTime = 0;
            this.array = Array; f = -1; deltaFrame = last = rf = @if = 0;
            @if = InterpolationFramerate;
            firstKey = lastKey = default;
            this.RequestedFramerate = RequestedFramerate;
            f = -deltaFrame;
            ResetFrameCount();

            if (Array != null && Array.Length > 0)
            {
                firstKey = Array[0];
                 lastKey = Array[Array.Length - 1];
            }
        }

        public float SetTime(float time)
        {
            if (array == null || array.Length < 1) return 0;

            lastTime = time;
            f = time * @if;
            last = Interpolate(f);
            return last;
        }

        public float SetFrame(float frame)
        {
            if (array == null || array.Length < 1) return 0;

            lastTime = frame / rf;
            f = frame * deltaFrame;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame(float time)
        {
            if (array == null || array.Length < 1) return 0;

            lastTime += time;
            f = lastTime * @if;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame()
        {
            if (array == null || array.Length < 1) return 0;

            f += deltaFrame;
            lastTime = f / @if;
            last = Interpolate(f);
            return last;
        }

        private float Interpolate(float frame)
        {
            float f = (int)frame;

            int data = 0;
            int length = array.Length;
            while (length > 0)
                if (f <= array[data + (length >> 1)].F)
                    length >>= 1;
                else
                {
                    int delta = (length >> 1) + 1;
                    data += delta;
                    length -= delta;
                }

            if (data == 0)
                return firstKey.V;
            else if (data >= array.Length)
                return  lastKey.V;

            KFT2 c = array[data - 1];
            KFT2 n = array[data];
            float result = c.F == n.F ? c.V : n.V;
            if (frame < n.F)
            {

                float t = (this.f - c.F) / (n.F - c.F);
                float t_1 = t - 1;
                result = (t_1 * 2 - 1) * (c.V - n.V) * t * t +
                    (t_1 * c.T + t * n.T) * t_1 * (this.f - c.F) + c.V;
            }
            return result;
        }

        public void ResetFrameCount() => f = -deltaFrame;

        public override string ToString() => $"Frame: {f}, Value: {last}";
    }
}
