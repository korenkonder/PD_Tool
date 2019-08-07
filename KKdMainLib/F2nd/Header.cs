using KKdBaseLib;

namespace KKdMainLib.F2nd
{
    public struct Header
    {
        public int Signature;
        public int DataSize;
        public int Length;
        public Format Format;
        public int ID;
        public int SectionSize;
        public int SubID;
        public int InnerSignature;
        public int SectionSignature;

        public bool IsBE => Format == Format.F2BE;
        public bool IsX => Format == Format.X || Format == Format.XHD;

        public override string ToString() => Signature.ToString(false);
    }
}
