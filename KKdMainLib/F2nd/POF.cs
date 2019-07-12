using System.Collections.Generic;

namespace KKdMainLib.F2nd
{
    public struct POF
    {
        public byte Type;
        public int Length;
        public int Offset;
        public int LastOffset;
        public List<long> Offsets;
        public Header Header;
    }
}
