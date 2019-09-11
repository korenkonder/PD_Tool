namespace KKdBaseLib.F2
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;

        public bool EOFC;
        public ENRSList ENRS;
        public POF POF;

        public int ID => Header.ID;

        public bool HasPOF        => POF .NotNull;
        public bool HasENRS       => ENRS.NotNull;
        public bool HasSubStructs => SubStructs != null;

        public long DataOffset;

        public override string ToString() => $"{Header.ToString()}" +
            $"{(HasSubStructs ? $"; SubStructs: {SubStructs.Length}" : "")}" +
            $"{(HasENRS ? "; Has ENRS" : "")}{(HasPOF ? "; Has POF" : "")}{(EOFC ? "; Has EOFC" : "")}";
    }
}
