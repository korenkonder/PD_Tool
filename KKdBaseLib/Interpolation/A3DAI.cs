using KKdBaseLib.Auth3D;

namespace KKdBaseLib.Interpolation
{
    public struct A3DAI //A3DA Interpolation
    {
        private A3DAKey a3daKey;

        private float f;
        private float last;
        private float df;
        private float @if;
        private float rf;
        private float lastTime;

        private KFT3 firstKey;
        private KFT3  lastKey;

        public float InterpolationFramerate { get => @if; set { @if = value; df = @if / rf; } }
        public float     RequestedFramerate { get =>  rf; set {  rf = value; df = @if / rf; } }

        public float Frame => f;
        public float Value => last;
        public bool  IsNull => a3daKey.Keys == null ?  true : a3daKey.Length < 1;
        public bool NotNull => a3daKey.Keys == null ? false : a3daKey.Length > 0;

        public A3DAI(Key key, float a3daFramerate = 60, float requestedFramerate = 60)
        {
            lastTime = 0;
            a3daKey = (A3DAKey)key; f = -1; df = last = rf = @if = 0;
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
            lastTime = 0;
            a3daKey = key; f = -1; df = last = rf = @if = 0;
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
                 if ((int)a3daKey.Type < 0 || (int)a3daKey.Type > 4) return 0.0f;
            else if ((int)a3daKey.Type < 2) return a3daKey.Value;
            else if (a3daKey.Length < 1) return 0.0f;

            lastTime = time;
            f = time * @if;
            last = Interpolate(f);
            return last;
        }

        public float SetFrame(float frame)
        {
                 if ((int)a3daKey.Type < 0 || (int)a3daKey.Type > 4) return 0.0f;
            else if ((int)a3daKey.Type < 2) return a3daKey.Value;
            else if (a3daKey.Length < 1) return 0.0f;

            lastTime = frame / rf;
            f = frame * df;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame(float time)
        {
                 if ((int)a3daKey.Type < 0 || (int)a3daKey.Type > 4) return 0.0f;
            else if ((int)a3daKey.Type < 2) return a3daKey.Value;
            else if (a3daKey.Length < 1) return 0.0f;

            lastTime += time;
            f = lastTime * @if;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame()
        {
                 if ((int)a3daKey.Type < 0 || (int)a3daKey.Type > 4) return 0.0f;
            else if ((int)a3daKey.Type < 2) return a3daKey.Value;
            else if (a3daKey.Length < 1) return 0.0f;

            f += df;
            lastTime = f / @if;
            last = Interpolate(f);
            return last;
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

            ref KFT3 c = ref a3daKey.Keys[key - 1];
            ref KFT3 n = ref a3daKey.Keys[key];
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

        public void ResetFrameCount() => f = -df;

        public override string ToString() => $"Frame: {f}, Value: {last}";
    }
}
