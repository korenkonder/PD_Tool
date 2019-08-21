namespace KKdBaseLib.F2
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;
        public bool EOFC;
        public ENRS[] ENRS;
        public KKdList<long> POF;

        public long DataOffset;

        public override string ToString() => Header.ToString() + (SubStructs != null ?
            "; SubStructs: " + SubStructs.Length : "") + (ENRS != null ? "; Has ENRS" : "") +
            (POF.NotNull ? "; Has POF" : "") + (EOFC ? "; Has EOFC" : "");
    }
}
