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
        public int Unk1C;
        public int Unk20;
        public int Unk24;
        public int Unk28;
        public int Unk2C;
        public int Unk30;
        public int Unk34;
        public long Length;
        public long DataOffset; //Used only in-game and points to KFT3 Array

        public KFT3[] Keys;

        public A3DAKey(Key k)
        {
            Type = 0; Value = MaxFrames = FrameDelta = ValueDelta = 0; EPTypePre = EPTypePost = 0;
            Unk1C = Unk20 = Unk24 = Unk28 = Unk2C = Unk30 = Unk34 = 0;
            Length = DataOffset = 0; Keys = null;

            EPTypePost = k.EPTypePost;
            EPTypePre = k.EPTypePre;
            MaxFrames = k.Max ?? 0;
            if (k.Type > KeyType.Value && k.Length > 1)
            {
                Type = k.Type;
                Length = k.Length;
                Keys = k.Keys;
                FrameDelta = k.Keys[k.Length - 1].F - k.Keys[0].F;
                ValueDelta = k.Keys[k.Length - 1].V - k.Keys[0].V;
            }
            else
            {
                Type = k.Type;
                Value = k.Type > 0 ? k.Value : 0.0f;
            }
        }
    }
}
