using KKdBaseLib.Auth3D;

namespace KKdBaseLib
{
    public struct A3DAKey
    {
        public KeyType Type;
        public float Value;
        public float MaxFrames;
        public EPType EPTypePre;
        public EPType EPTypePost;
        public float FrameDelta;
        public float ValueDelta;
        public int Padding;
        public long FirstKey;
        public long SecondKey;
        public long AfterLastKey;
        public long Length;
        public long DataOffset; //Used only in-game and points to KFT3 Array

        public KFT3[] Keys;

        public A3DAKey(Key k)
        {
            Type = 0; Value = MaxFrames = FrameDelta = ValueDelta = 0; EPTypePre = EPTypePost = 0;
            FirstKey = SecondKey = AfterLastKey = Padding = 0;
            Length = DataOffset = 0; Keys = null;

            MaxFrames = k.Max ?? 0;
            if (k.Type > KeyType.Static && k.Length > 1)
            {
                Type = k.Type;
                Length = k.Length;
                Keys = k.Keys;
                EPTypePost = k.EPTypePost;
                EPTypePre = k.EPTypePre;
                FrameDelta = k.Keys[k.Length - 1].F - k.Keys[0].F;
                ValueDelta = k.Keys[k.Length - 1].V - k.Keys[0].V;
            }
            else
            {
                Type = k.Type;
                Value = k.Type > 0 ? k.Value : 0.0f;
                EPTypePost = 0;
                EPTypePre = 0;
                FrameDelta = 0;
                ValueDelta = Value;
            }
        }
    }
}
