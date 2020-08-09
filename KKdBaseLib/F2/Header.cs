namespace KKdBaseLib.F2
{
    public struct Header
    {
        public uint Signature;        // 0x00
        public uint DataSize;         // 0x04
        public uint Length;           // 0x08
        public uint Flags;            // 0x0C
        public uint Depth;            // 0x10
        public uint SectionSize;      // 0x14
        public uint Version;          // 0x18
        public uint   InnerSignature; // 0x30
        public uint SectionSignature; // 0x40

        public Format Format;
        public bool UseBigEndian;
        public bool UseSectionSize;

        public bool IsX  => Format == Format.X || Format == Format.XHD;

        public override string ToString() => Signature.ToS(false);
    }
}
