using KKdBaseLib.A3DA;

namespace KKdBaseLib
{
    public struct A3DAKey
    {
        public KeyType Type;
        public int Unk04;
        public float MaxFrames;
        public EPType EPTypePre;
        public EPType EPTypePost;
        public float FDBSaE; //Frame Difference Between Start and End
        public float VDBSaE; //Value Difference Between Start and End
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
