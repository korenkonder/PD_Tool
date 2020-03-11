namespace KKdBaseLib.Interpolation
{
    public struct PDI : IInterpolation //Project DIVA Interpolation
    {
        private KFT2[] array;

        private float f;
        private float last;
        private float df;
        private float @if;
        private float rf;
        private float lastTime;

        private KFT2 firstKey;
        private KFT2  lastKey;

        public float InterpolationFramerate { get => @if; set { @if = value; df = @if / rf; } }
        public float     RequestedFramerate { get =>  rf; set {  rf = value; df = @if / rf; } }

        public float Frame => f;
        public float Value => last;
        public bool  IsNull => array == null ?  true : array.Length < 1;
        public bool NotNull => array == null ? false : array.Length > 0;

        public PDI(KFT2[] array, float interpolationFramerate = 60, float requestedFramerate = 60)
        {
            lastTime = 0;
            this.array = array; f = -1; df = last = rf = @if = 0;
            @if = interpolationFramerate;
            firstKey = lastKey = default;
            RequestedFramerate = requestedFramerate;
            f = -df;
            ResetFrameCount();

            if (array != null && array.Length > 0)
            {
                firstKey = array[0];
                 lastKey = array[array.Length - 1];
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
            f = frame * df;
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

            f += df;
            lastTime = f / @if;
            last = Interpolate(f);
            return last;
        }

        private float Interpolate(float frame)
        {
            float f = (int)frame;

                 if (f <= firstKey.F) return firstKey.V;
            else if (f >=  lastKey.F) return  lastKey.V;

            int key = 0;
            int length = array.Length;
            int temp;
            while (length > 0)
                if (f > array[key + (temp = length >> 1)].F)
                {
                       key += temp + 1;
                    length -= temp + 1;
                }
                else length = temp;

            ref KFT2 c = ref array[key - 1];
            ref KFT2 n = ref array[key];
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

        public void ResetFrameCount() => f = -df;

        public override string ToString() => $"Frame: {f}, Value: {last}";
    }
}
