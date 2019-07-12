using KKdBaseLib;

namespace KKdMainLib.F2nd
{
    public struct Header
    {
        public int Signature;
        public int DataSize;
        public int Length;
        public Main.Format Format;
        public int ID;
        public int SectionSize;
        public int Count;
        public int InnerSignature;
        public int SectionSignature;
        public bool IsBE => Format == Main.Format.F2BE;
        public bool IsX => Format == Main.Format.X || Format == Main.Format.XHD;


        public override string ToString() => Signature.ToString(false);
    }
}
