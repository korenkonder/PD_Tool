using KKdBaseLib.Auth3D;

namespace KKdBaseLib.Interpolation
{
    public class A3DAI : IInterpolation<float> //A3DA Interpolation
    {
        private A3DAKey a3daKey;

        private float f;
        private float t;
        private float v;
        private float df;
        private float @if;
        private float rf;

        private KFT3 firstKey;
        private KFT3  lastKey;

        public float RequestedFramerate { get => rf; set { rf = value; df = @if / rf; } }

        public float Frame => f;
        public float Time  => t;
        public float Value => v;
        public bool  IsNull => a3daKey.Keys == null || a3daKey.Length < 1;
        public bool NotNull => a3daKey.Keys != null && a3daKey.Length > 0;

        public A3DAI(Key key, float a3daFramerate = 60, float requestedFramerate = 60)
        {
            a3daKey = new A3DAKey(key); f = -1; df = @if = rf = t = v = 0;
            @if = a3daFramerate;
            firstKey = lastKey = default;
            RequestedFramerate = requestedFramerate;
            f = -df;
            ResetFrameCount();

            if (key.Keys != null && key.Length > 0)
            {
                firstKey = key.Keys[0];
                 lastKey = key.Keys[key.Length - 1];
            }
        }

        public A3DAI(A3DAKey key, float a3daFramerate = 60, float requestedFramerate = 60)
        {
            t = 0;
            a3daKey = key; f = -1; df = v = rf = @if = 0;
            @if = a3daFramerate;
            firstKey = lastKey = default;
            RequestedFramerate = requestedFramerate;
            f = -df;
            ResetFrameCount();

            if (key.Keys != null && key.Length > 0)
            {
                firstKey = key.Keys[0];
                 lastKey = key.Keys[key.Length - 1];
            }
        }

        public float SetTime(float time)
        {
            t = time;
            f = time * @if;

            int type = (int)a3daKey.Type;
                 if (type < 0 || type > 4) v = 0.0f;
            else if (type < 2)             v = a3daKey.Value;
            else if (a3daKey.Length   < 1) v = 0.0f;
            else                           v = Interpolate(f);
            return v;
        }

        public float SetFrame(float frame)
        {
            t = frame / rf;
            f = frame * df;

            int type = (int)a3daKey.Type;
                 if (type < 0 || type > 4) v = 0.0f;
            else if (type < 2)             v = a3daKey.Value;
            else if (a3daKey.Length   < 1) v = 0.0f;
            else                           v = Interpolate(f);
            return v;
        }

        public float NextFrame(float time)
        {
            t += time;
            f = t * @if;

            int type = (int)a3daKey.Type;
                 if (type < 0 || type > 4) v = 0.0f;
            else if (type < 2)             v = a3daKey.Value;
            else if (a3daKey.Length   < 1) v = 0.0f;
            else                           v = Interpolate(f);
            return v;
        }

        public float NextFrame()
        {
            f += df;
            t = f / @if;

            int type = (int)a3daKey.Type;
                 if (type < 0 || type > 4) v = 0.0f;
            else if (type < 2)             v = a3daKey.Value;
            else if (a3daKey.Length   < 1) v = 0.0f;
            else                           v = Interpolate(f);
            return v;
        }

        private float Interpolate(float frame)
        {
            float df = 0;
            float ep = 0;
            float f = (int)frame;

            if (f < firstKey.F)
            {
                if (a3daKey.EPTypePost < EPType.EP_1 || a3daKey.EPTypePost > EPType.EP_3) return firstKey.V;

                df = firstKey.F - frame;
                if (a3daKey.EPTypePre == EPType.EP_1) return firstKey.V - df * firstKey.T1;

                frame = lastKey.F - df % a3daKey.FrameDelta;
                f = (int)frame;
                if (a3daKey.EPTypePre == EPType.EP_3)
                {
                    ep = df / a3daKey.FrameDelta;
                    ep = (ep > 0 && ep != (int)ep) ? (ep - (int)ep > 0 ? 1 : 0) : ep;
                    ep = -(ep + 1) * a3daKey.ValueDelta;
                }
            }
            else if (f >= lastKey.F)
            {
                if (a3daKey.EPTypePost < EPType.EP_1 || a3daKey.EPTypePost > EPType.EP_3) return lastKey.V;

                df = frame - lastKey.F;
                if (a3daKey.EPTypePost == EPType.EP_1) return lastKey.V + df * lastKey.T2;

                frame = firstKey.F + df % a3daKey.FrameDelta;
                f = (int)frame;
                if (a3daKey.EPTypePost == EPType.EP_3)
                {
                    ep = df / a3daKey.FrameDelta;
                    ep = (ep > 0 && ep != (int)ep) ? (ep - (int)ep > 0 ? 1 : 0) : ep;
                    ep = (ep + 1) * a3daKey.ValueDelta;
                }
            }

                 if (f <= firstKey.F) return firstKey.V + ep;
            else if (f >=  lastKey.F) return  lastKey.V + ep;

            KFT3 c, n;
            unsafe
            {
                fixed (KFT3* ptr = a3daKey.Keys)
                {
                    long key = 0;
                    long length = a3daKey.Length;
                    long temp;
                    while (length > 0)
                        if (f > a3daKey.Keys[key + (temp = length >> 1)].F)
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
                if (a3daKey.Type == KeyType.Lerp)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    result = (1 - t) * c.V + t * n.V;
                }
                else if (a3daKey.Type == KeyType.Hermite)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    float t_2 = (1 - t) * (1 - t);
                    result = t_2 * c.V * (1 + 2 * t) + (t * n.V * (3 - 2 * t) +
                        (t_2 * c.T2 + t * (t - 1) * n.T1) * (n.F - c.F)) * t;
                }
                else result = c.V;
            }
            else result = frame > c.F ? n.V : c.V;
            return result + ep;
        }

        public void ResetFrameCount() { f = -df; t = f / rf; }

        public override string ToString() => $"F: {f}; T: {t}; V: {v}";
    }
}
