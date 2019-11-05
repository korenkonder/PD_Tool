using KKdBaseLib.A3DA;

namespace KKdBaseLib.Interpolation
{
    public struct A3DAI //A3DA Interpolation
    {
        private A3DAKey key;

        private float f;
        private float last;
        private float deltaFrame;
        private float @if;
        private float rf;
        private float lastTime;

        private KFT3 firstKey;
        private KFT3  lastKey;

        public float InterpolationFramerate
        { get => @if; set { @if = value; deltaFrame = @if / rf; } }
        public float RequestedFramerate
        { get => rf; set { rf = value; deltaFrame = @if / rf; } }

        public float Frame => f;
        public float Value => last;
        public bool  IsNull => key.Keys == null ?  true : key.Length < 1;
        public bool NotNull => key.Keys == null ? false : key.Length > 0;

        public A3DAI(A3DAKey key, float a3daFramerate = 60, float requestedFramerate = 60)
        {
            lastTime = 0;
            this.key = key; f = -1; deltaFrame = last = rf = @if = 0;
            @if = a3daFramerate;
            firstKey = lastKey = default;
            RequestedFramerate = requestedFramerate;
            f = -deltaFrame;
            ResetFrameCount();

            if (key.Keys != null && key.Length > 0)
            {
                firstKey = key.Keys[0];
                 lastKey = key.Keys[key.Length - 1];
            }
        }

        public float SetTime(float time)
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            lastTime = time;
            f = time * @if;
            last = Interpolate(f);
            return last;
        }

        public float SetFrame(float frame)
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            lastTime = frame / rf;
            f = frame * deltaFrame;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame(float time)
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            lastTime += time;
            f = lastTime * @if;
            last = Interpolate(f);
            return last;
        }

        public float NextFrame()
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            f += deltaFrame;
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
                if (key.EPTypePost < EPType.EP_1 || key.EPTypePost > EPType.EP_3)
                    return firstKey.V;

                df = firstKey.F - frame;
                if (key.EPTypePre == EPType.EP_1)
                    return firstKey.V - df * firstKey.T1;
                else if (key.EPTypePre == EPType.EP_2 || key.EPTypePre == EPType.EP_3)
                {
                    frame = lastKey.F - df % key.FDBSaE;
                    f = (int)frame;
                }
            }
            else if (f >= lastKey.F)
            {
                if (key.EPTypePost < EPType.EP_1 || key.EPTypePost > EPType.EP_3)
                    return lastKey.V;

                df = frame - lastKey.F;
                if (key.EPTypePost == EPType.EP_1)
                    return lastKey.V + df * lastKey.T2;
                else if (key.EPTypePost == EPType.EP_2 || key.EPTypePost == EPType.EP_3)
                {
                    frame = firstKey.F + df % key.FDBSaE;
                    f = (int)frame;
                }
            }

            if ((f <  firstKey.F && key.EPTypePre  == EPType.EP_3) ||
                (f >= firstKey.F && key.EPTypePost == EPType.EP_3))
            {
                ep = df / key.FDBSaE;
                ep = (ep >= 0 && ep != (int)ep) ? (ep - (int)ep > 0 ? 1 : 0) : ep;
                ep = (f < firstKey.F ? -1 : 1) * (ep + 1) * key.VDBSaE;
            }

            if (f <= firstKey.F)
                return firstKey.V + ep;
            else if (f >= lastKey.F)
                return lastKey.V + ep;

            int data = 0;
            int length = key.Length;
            int tempLength;
            while (length > 0)
            {
                tempLength = length >> 1;
                if (f <= key.Keys[data + tempLength].F)
                    length = tempLength;
                else
                {
                    data += tempLength + 1;
                    length -= tempLength + 1;
                }
            }

            KFT3 c = key.Keys[data - 1];
            KFT3 n = key.Keys[data];
            float result;
            if (frame > c.F && frame < n.F)
            {
                if (key.Type == KeyType.Lerp)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    result = (1 - t) * c.V + t * n.V;
                }
                else if (key.Type == KeyType.Hermite)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    float t_2 = (1 - t) * (1 - t);
                    result = t_2 * c.V * (1 + 2 * t) + (t * n.V * (3 - 2 * t) +
                        (t_2 * c.T2 + t * (t - 1) * n.T1) * (n.F - c.F)) * t;
                }
                else
                    result = c.V;
            }
            else
                result = frame > c.F ? n.V : c.V;
            return result + ep;
        }

        public void ResetFrameCount() => f = -deltaFrame;

        public override string ToString() => $"Frame: {f}, Value: {last}";
    }
}
