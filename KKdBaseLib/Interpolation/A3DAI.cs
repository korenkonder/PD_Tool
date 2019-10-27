using Type = KKdBaseLib.A3DAKey.KeyType;
using EPType = KKdBaseLib.A3DAKey.EPType;

namespace KKdBaseLib.Interpolation
{
    public struct A3DAI //A3DA Interpolation
    {
        private A3DAKey key;

        private float F;
        private float Last;
        private float DeltaFrame;
        private float IF;
        private float RF;
        private float LastTime;

        private KFT3 firstKey;
        private KFT3  lastKey;

        public float InterpolationFramerate
        { get => IF; set { IF = value; DeltaFrame = IF / RF; } }
        public float RequestedFramerate
        { get => RF; set { RF = value; DeltaFrame = IF / RF; } }

        public float Frame => F;
        public float Value => Last;
        public bool  IsNull => key.Keys == null ?  true : key.Length < 1;
        public bool NotNull => key.Keys == null ? false : key.Length > 0;

        public A3DAI(A3DAKey key, float A3DAFramerate = 60, float RequestedFramerate = 60)
        {
            LastTime = 0;
            this.key = key; F = -1; DeltaFrame = Last = RF = IF = 0;
            IF = A3DAFramerate;
            firstKey = lastKey = default;
            this.RequestedFramerate = RequestedFramerate;
            F = -DeltaFrame;
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

            LastTime = time;
            F = time * IF;
            Last = Interpolate(F);
            return Last;
        }

        public float SetFrame(float frame)
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            LastTime = frame / RF;
            F = frame * DeltaFrame;
            Last = Interpolate(F);
            return Last;
        }

        public float NextFrame(float time)
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            LastTime += time;
            F = LastTime * IF;
            Last = Interpolate(F);
            return Last;
        }

        public float NextFrame()
        {
            if ((int)key.Type <  1 || (int)key.Type > 4 || key.Length < 1) return 0;
            if ((int)key.Type == 1) return key.Keys[0].V;

            F += DeltaFrame;
            LastTime = F / IF;
            Last = Interpolate(F);
            return Last;
        }

        private float Interpolate(float frame)
        {
            float DF = 0;
            float EP;
            float f = (int)frame;
            if (f < firstKey.F)
            {
                DF = firstKey.F - frame;
                if (key.EPTypePre == EPType.EP_2 || key.EPTypePre == EPType.EP_3)
                {
                    frame = firstKey.F - DF % key.FDBSaE;
                    f = (int)frame;
                }
                else if (key.EPTypePre == EPType.EP_1)
                    return firstKey.V - DF * firstKey.T1;
                else
                    return firstKey.V;
            }
            else if (f >= lastKey.F)
            {
                DF = frame - lastKey.F;
                if (key.EPTypePost == EPType.EP_2 || key.EPTypePost == EPType.EP_3)
                {
                    frame = firstKey.F + DF % key.FDBSaE;
                    f = (int)frame;
                }
                else if (key.EPTypePost == EPType.EP_1)
                    return lastKey.V + DF * lastKey.T2;
                else
                    return lastKey.V;
            }

            if ((f <  firstKey.F && key.EPTypePre  == EPType.EP_3) ||
                (f >= firstKey.F && key.EPTypePost == EPType.EP_3))
            {
                EP = DF / key.FDBSaE;
                EP = (EP >= 0 && (int)EP != EP) ? ((int)EP - EP < 0 ? 1 : 0) : EP;

                if (f < firstKey.F)
                    EP = -(EP + 1) * key.VDBSaE;
                else
                    EP =  (EP + 1) * key.VDBSaE;
            }
            else
                EP = 0;

            int data = 0;
            int length = key.Length;
            while (length > 0)
                if (f <= key.Keys[data + (length >> 1)].F)
                    length >>= 1;
                else
                {
                    int delta = (length >> 1) + 1;
                    data += delta;
                    length -= delta;
                }

            if (data == 0)
                return firstKey.V + EP;
            else if (data >= key.Length)
                return  lastKey.V + EP;

            KFT3 c = key.Keys[data - 1];
            KFT3 n = key.Keys[data];
            float result = c.F == n.F ? c.V : n.V;
            if (key.Type == Type.Lerp)
            {
                if (frame < n.F)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    result = (1 - t) * c.V + t * n.V;
                }
            }
            else if (key.Type == Type.Hermite)
            {
                if (frame < n.F)
                {
                    float t = (frame - c.F) / (n.F - c.F);
                    float t_2 = (1 - t) * (1 - t);
                    result = t_2 * c.V * (1 + 2 * t) + (t * n.V * (3 - 2 * t) +
                        (t_2 * c.T2 + t * (t - 1) * n.T1) * (n.F - c.F)) * t;
                }
            }
            else if (key.Type == Type.Hold)
            {
                if (frame < n.F)
                    result = c.V;
            }
            else
                return EP;
            return result + EP;
        }

        public void ResetFrameCount() => F = -DeltaFrame;

        public override string ToString() => $"Frame: {F}, Value: {Value}";
    }
}
