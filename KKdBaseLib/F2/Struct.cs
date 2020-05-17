namespace KKdBaseLib.F2
{
    public struct Struct
    {
        public Header Header;
        public byte[] Data;
        public Struct[] SubStructs;

        public ENRS ENRS;
        public POF POF;

        public int Length  => length(false);
        public int LengthX => length( true);

        public int Depth => Header.Depth;

        public bool HasPOF        => POF .NotNull;
        public bool HasENRS       => ENRS.NotNull;
        public bool HasSubStructs => SubStructs != null;

        public long DataOffset;

        public override string ToString() => $"{Header.ToString()}" +
            $"{(HasSubStructs ? $"; SubStructs: {SubStructs.Length}" : "")}" +
            $"{(HasENRS ? "; Has ENRS" : "")}{(HasPOF ? "; Has POF" : "")}";

        private int length(bool shiftX = false)
        {
            int length = Data != null ? Data.Length : 0;
            if (HasPOF ) length += 0x20 + (shiftX ? POF.LengthX : POF.Length);
            if (HasENRS) length += 0x20 + ENRS.Length;

            if (HasSubStructs)
            {
                for (int i = 0; i < SubStructs.Length; i++)
                    length += (shiftX ? SubStructs[i].LengthX : SubStructs[i].Length) + SubStructs[i].Header.Length;
                length += 0x20;
            }
            return length;
        }

        public void Update(bool ShiftX = false)
        {
            Header.SectionSize = Data != null ? Data.Length : 0;
            Header.DataSize = length(ShiftX);
        }
    }
}
