namespace KKdBaseLib.Interpolation
{
    public struct PDI : IInterpolation //Project DIVA Interpolation
    {
        private KFT2[] Array;

        private float F;
        private float Last;
        private float DeltaFrame;
        private float IF;
        private float RF;
        private float LastTime;

        private KFT2 firstKey;
        private KFT2 lastKey;

        public float InterpolationFramerate
        { get => IF; set { IF = value; DeltaFrame = IF / RF; } }
        public float RequestedFramerate
        { get => RF; set { RF = value; DeltaFrame = IF / RF; } }

        public float Frame => F;
        public float Value => Last;
        public bool  IsNull => Array == null ?  true : Array.Length < 1;
        public bool NotNull => Array == null ? false : Array.Length > 0;
        
        public PDI(KFT2[] Array, bool Loop, float InterpolationFramerate = 60, float RequestedFramerate = 60)
        {
            LastTime = 0;
            this.Array = Array; F = -1; DeltaFrame = Last = RF = IF = 0;
            IF = InterpolationFramerate;
            firstKey = lastKey = default;
            this.RequestedFramerate = RequestedFramerate;
            F = -DeltaFrame;
            ResetFrameCount();

            if (Array != null && Array.Length > 0)
            {
                firstKey = Array[0];
                 lastKey = Array[Array.Length - 1];
            }
        }

        public float SetTime(float time)
        {
            if (Array == null || Array.Length < 1) return 0;

            LastTime = time;
            F = time * IF;
            Last = Interpolate(F);
            return Last;
        }

        public float SetFrame(float frame)
        {
            if (Array == null || Array.Length < 1) return 0;

            LastTime = frame / RF;
            F = frame * DeltaFrame;
            Last = Interpolate(F);
            return Last;
        }

        public float NextFrame(float time)
        {
            if (Array == null || Array.Length < 1) return 0;

            LastTime += time;
            F = LastTime * IF;
            Last = Interpolate(F);
            return Last;
        }

        public float NextFrame()
        {
            if (Array == null || Array.Length < 1) return 0;

            F += DeltaFrame;
            LastTime = F / IF;
            Last = Interpolate(F);
            return Last;
        }

        private float Interpolate(float frame)
        {
            float f = (int)frame;

            int data = 0;
            int length = Array.Length;
            while (length > 0)
                if (f <= Array[data + (length >> 1)].F)
                    length >>= 1;
                else
                {
                    int delta = (length >> 1) + 1;
                    data += delta;
                    length -= delta;
                }

            if (data == 0)
                return firstKey.V;
            else if (data >= Array.Length)
                return  lastKey.V;

            KFT2 c = Array[data - 1];
            KFT2 n = Array[data];
            float result = c.F == n.F ? c.V : n.V;
            if (frame < n.F)
            {

                float t = (F - c.F) / (n.F - c.F);
                float t_1 = t - 1;
                result = (t_1 * 2 - 1) * (c.V - n.V) * t * t +
                    (t_1 * c.T + t * n.T) * t_1 * (F - c.F) + c.V;
            }
            return result;
        }

        public void ResetFrameCount() => F = -DeltaFrame;
    }
}
