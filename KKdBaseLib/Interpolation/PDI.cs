namespace KKdBaseLib.Interpolation
{
    public class PDI : IInterpolation<float> //Project DIVA Interpolation
    {
        private KFT2[] array;
        private int length;

        private float f;
        private float t;
        private float v;
        private float df;
        private float @if;
        private float rf;

        private KFT2 firstKey;
        private KFT2  lastKey;

        public float RequestedFramerate { get =>  rf; set {  rf = value; df = @if / rf; } }

        public float Frame => f;
        public float Time  => t;
        public float Value => v;
        public bool  IsNull => array == null || array.Length < 1;
        public bool NotNull => array != null && array.Length > 0;

        public PDI(KFT2[] array, float interpolationFramerate = 60, float requestedFramerate = 60)
        {
            length = 0;
            this.array = array; f = -1; df = @if = rf = t = v = 0;
            @if = interpolationFramerate;
            firstKey = lastKey = default;
            RequestedFramerate = requestedFramerate;
            f = -df;
            ResetFrameCount();

            if (array != null && array.Length > 0)
            {
                firstKey = array[0];
                 lastKey = array[array.Length - 1];
                  length = array.Length;
            }
        }

        public float SetTime(float time)
        {
            t = time;
            f = time * @if;

            if (array == null || length < 1) v = 0.0f;
            else                             v = Interpolate(f);

            return v;
        }

        public float SetFrame(float frame)
        {
            t = frame / rf;
            f = frame * df;

            if (array == null || length < 1) v = 0.0f;
            else                             v = Interpolate(f);

            return v;
        }

        public float NextFrame(float time)
        {
            t += time;
            f = t * @if;

            if (array == null || length < 1) v = 0.0f;
            else                             v = Interpolate(f);

            return v;
        }

        public float NextFrame()
        {
            f += df;
            t = f / @if;

            if (array == null || length < 1) v = 0.0f;
            else                             v = Interpolate(f);

            return v;
        }

        private float Interpolate(float frame)
        {
            float f = (int)frame;

                 if (f <= firstKey.F) return firstKey.V;
            else if (f >=  lastKey.F) return  lastKey.V;

            KFT2 c, n;
            unsafe
            {
                fixed (KFT2* ptr = array)
                {
                    long key = 0;
                    long length = this.length;
                    long temp;
                    while (length > 0)
                        if (f > array[key + (temp = length >> 1)].F)
                        {
                               key += temp + 1;
                            length -= temp + 1;
                        }
                        else length = temp;

                    c = ptr[key - 1];
                    n = ptr[key];
                }
            }

            float result;
            if (frame > c.F && frame < n.F)
            {
                float t = (this.f - c.F) / (n.F - c.F);
                float t_1 = t - 1;
                result = (t_1 * 2 - 1) * (c.V - n.V) * t * t +
                    (t_1 * c.T + t * n.T) * t_1 * (this.f - c.F) + c.V;
            }
            else result = frame > c.F ? n.V : c.V;
            return result;
        }

        public void ResetFrameCount() { f = -df; t = f / rf; }

        public override string ToString() => $"F: {f}; T: {t}; V: {v}";
    }
}
