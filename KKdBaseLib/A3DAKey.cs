using KKdBaseLib.Auth3D;

namespace KKdBaseLib
{
    public struct A3DAKey
    {
        public KeyType Type;
        public int Unk04;
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
        public int Length;
        public int Unk3C;
        public int DataOffset; //Used only in-game and points to KFT3 Array
        public int Unk44;

        public KFT3[] Keys;
    }
}
