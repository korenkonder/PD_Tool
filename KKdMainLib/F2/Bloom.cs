using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib.F2
{
    public struct Bloom
    {
        public CountPointer<BLT> BLTs;
        private Stream IO;
        private Header Header;
        private int i;

        public void BLTReader(string file)
        {
            BLTs = default;
            IO = File.OpenReader(file + ".blt", true);
            Header = IO.ReadHeader();
            if (Header.Signature != 0x544D4C42 || Header.InnerSignature != 0x3) return;
            IO.Position -= 0x4;

            BLTs = IO.ReadCountPointerEndian<BLT>();
            if (BLTs.Count < 1) { IO.Close(); BLTs.Count = -1; return; }

            if (BLTs.Count > 0 && BLTs.Offset == 0) { IO.Close(); BLTs.Count = -1; return; }
            /*{
                IO.Format = Header.Format = Format.X;
                IO.Offset = Header.Length;
                IO.Position = BLTs.Offset;
                BLTs = IO.ReadCountPointerX<BLT>();
            }*/

            IO.Position = BLTs.Offset;
            for (i = 0; i < BLTs.Count; i++)
            {
                ref BLT BLT = ref BLTs.Entries[i];
                IO.ReadInt32Endian();
                BLT.Color     .X = IO.ReadSingleEndian();
                BLT.Color     .Y = IO.ReadSingleEndian();
                BLT.Color     .Z = IO.ReadSingleEndian();
                BLT.Brightpass.X = IO.ReadSingleEndian();
                BLT.Brightpass.Y = IO.ReadSingleEndian();
                BLT.Brightpass.Z = IO.ReadSingleEndian();
                BLT.Range        = IO.ReadSingleEndian();
            }
            IO.Close();
        }

        public void TXTWriter(string file)
        {
            if (BLTs.Count < 1) return;

            IO = File.OpenWriter();
            IO.WriteShiftJIS("ID,ColorR,ColorG,ColorB,BrightpassR,BrightpassG,BrightpassB,Range\n");
            for (i = 0; i < BLTs.Count; i++)
                IO.Write(i + "," + BLTs[i] + "\n");
            File.WriteAllBytes(file + "_bloom.txt", IO.ToArray(true));
        }

        public struct BLT
        {
            public Vector3 Color;
            public Vector3 Brightpass;
            public float Range;

            public override string ToString() => Color     .ToString(6) + "," +
                                                 Brightpass.ToString(6) + "," +
                                                 Range     .ToString(6);
        }
    }
}
