namespace KKdBaseLib.F2
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;

        public ENRS ENRS;
        public POF POF;

        public uint Length  => length(false);
        public uint LengthX => length( true);

        public uint Depth => Header.Depth;

        public bool HasPOF        => POF .NotNull;
        public bool HasENRS       => ENRS.NotNull;
        public bool HasSubStructs => SubStructs != null;

        public long DataOffset;

        public void Update(bool ShiftX = false)
        {
            Header.SectionSize = Data != null ? (uint)Data.Length : 0;
            Header.DataSize = length(ShiftX);
        }

        private uint length(bool shiftX = false)
        {
            uint length = Data != null ? (uint)Data.Length : 0;
            if (HasPOF ) length += 0x20 + (uint)(shiftX ? POF.LengthX : POF.Length);
            if (HasENRS) length += 0x20 + (uint)ENRS.Length;
            if (HasSubStructs)
                for (int i = 0; i < SubStructs.Length; i++)
                    length += (uint)(shiftX ? SubStructs[i].LengthX : SubStructs[i].Length)
                        + SubStructs[i].Header.Length;
            if (HasPOF || HasENRS || HasSubStructs)
                length += 0x20;
            return length;
        }

        public override string ToString() => $"{Header}" +
            $"{(HasSubStructs ? $"; SubStructs: {SubStructs.Length}" : "")}" +
            $"{(HasENRS ? "; Has ENRS" : "")}{(HasPOF ? "; Has POF" : "")}";
    }
}
