namespace KKdBaseLib.F2
{
    public struct Header
    {
        public int Signature;
        public int DataSize;
        public int Length;
        public int Flags;
        public int ID;
        public int SectionSize;
        public int Mode;
        public int InnerSignature;
        public int SectionSignature;

        public Format Format;
        public bool NotUseDataSizeAsSectionSize;

        public bool IsBE => Format == Format.F2BE;
        public bool IsX  => Format == Format.X || Format == Format.XHD;

        public override string ToString() => Signature.ToString(false);
    }
}
