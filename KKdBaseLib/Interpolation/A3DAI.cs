using KKdBaseLib.Auth3D;

namespace KKdBaseLib.Interpolation
{
    public class A3DAI : IInterpolation<float>, System.IDisposable // A3DA Interpolation
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

            if (key.Keys != null && key.Keys.Length > 0)
            {
                firstKey = key.Keys[0];
                 lastKey = key.Keys[key.Keys.Length - 1];
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
            float ep = 0;

            if (frame < firstKey.F)
            {
                if (a3daKey.EPTypePost < EPType.Linear || a3daKey.EPTypePost > EPType.CycleOffset)
                    return firstKey.V;

                float df = firstKey.F - frame;
                if (a3daKey.EPTypePre == EPType.Linear)
                    return firstKey.V - df * firstKey.T1;

                frame = lastKey.F - df % a3daKey.FrameDelta;
                if (a3daKey.EPTypePre == EPType.CycleOffset)
                    ep = -(float)((int)(df / a3daKey.FrameDelta) + 1) * a3daKey.ValueDelta;
            }
            else if (frame >= lastKey.F)
            {
                if (a3daKey.EPTypePost < EPType.Linear || a3daKey.EPTypePost > EPType.CycleOffset)
                    return lastKey.V;

                float df = frame - lastKey.F;
                if (a3daKey.EPTypePost == EPType.Linear)
                    return lastKey.V + df * lastKey.T2;

                frame = firstKey.F + df % a3daKey.FrameDelta;
                if (a3daKey.EPTypePost == EPType.CycleOffset)
                    ep = (float)((int)(df / a3daKey.FrameDelta) + 1) * a3daKey.ValueDelta;
            }

            KFT3 c, n;
            long key = 0;
            unsafe
            {
                fixed (KFT3* ptr = a3daKey.Keys)
                {
                    long length = a3daKey.Length;
                    long temp;
                    while (length > 0)
                        if (frame >= ptr[key + (temp = length >> 1)].F)
                        {
                               key += temp + 1;
                            length -= temp + 1;
                        }
                        else length = temp;

                    c = ptr[key - 1];
                    n = ptr[key];
                }
            }

            float v;
            if (frame <= c.F || frame >= n.F)
                v = frame > c.F ? n.V : c.V;
            else if (a3daKey.Type == KeyType.Linear)
            {
                float t = (frame - c.F) / (n.F - c.F);
                v = (1.0f - t) * c.V + t * n.V;
            }
            else if (a3daKey.Type == KeyType.Hermite)
            {
                float df  = f - c.F;
                float t   = df / (n.F - c.F);
                float t_1 = t - 1.0f;
                return c.V + t * t * (3.0f - 2.0f * t) * (n.V - c.V) + (t_1 * c.T2 + t * n.T1) * df * t_1;
            }
            else v = c.V;
            return v + ep;
        }

        public void ResetFrameCount() { f = -df; t = f / rf; }

        public override string ToString() => $"F: {f}; T: {t}; V: {v}";

        public void Dispose()
        {
            a3daKey.Keys = null;
            a3daKey = default;
            f = t = v = df = @if = rf = default;
            firstKey = lastKey = default;
        }
    }
}
