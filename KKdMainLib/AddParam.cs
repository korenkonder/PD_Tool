using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public unsafe struct AddParam : System.IDisposable
    {
        private long i;
        private Stream s;

        public HeaderData Header;

        public void AddParamReader(string file)
        {
            Header = new HeaderData();

            s = File.OpenReader(file + ".adp");
            Header.Count      = s.RI64();
            Header.DataLength = s.RI64();
            Header.DataOffset = s.RI64();

            if (Header.Count < 1 || Header.DataOffset > s.P || Header.DataLength > s.LI64
                - Header.DataOffset || Header.Count * 0x20 > s.LI64 - Header.DataOffset) { s.C(); return; }

            Header.Data = new HeaderData.Sub[Header.Count];
            byte[] data = s.RBy(Header.DataLength, Header.DataOffset);
            s.C();

            fixed (byte* ptr = data)
            {
                HeaderData.Sub* hPtr = (HeaderData.Sub*)ptr;
                for (i = 0; i < Header.Count; i++)
                    Header.Data[i] = hPtr[i];
            }

            data = null;
        }

        public void AddParamWriter(string file)
        {
            if (Header.Data == null || Header.Data.LongLength < 1) return;

            Header.Count = Header.Data.LongLength;
            Header.DataLength = Header.Count * 0x20;
            Header.DataOffset = 0x18L;

            byte[] data = new byte[Header.DataLength];
            fixed (byte* ptr = data)
            {
                HeaderData.Sub* hPtr = (HeaderData.Sub*)ptr;
                for (i = 0; i < Header.Count; i++)
                    hPtr[i] = Header.Data[i];
            }

            s = File.OpenWriter(file + ".adp", true);
            s.W(Header.Count);
            s.W(Header.DataLength);
            s.W(Header.DataOffset);
            s.W(data);
            s.C();
            data = null;
        }

        public void MsgPackReader(string file, bool json)
        {
            Header = new HeaderData();

            MsgPack addParam;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            if ((addParam = msgPack["AddParam", true]).IsNull) return;

            Header.Count = addParam.Array.LongLength;
            Header.Data = new HeaderData.Sub[Header.Count];
            for (i = 0; i < Header.Count; i++)
            {
                ref HeaderData.Sub sub = ref Header.Data[i];
                sub.Time  = addParam[i].RF32("Time");
                sub.Flags = addParam[i].RI32("Flags");
                sub.Frame = addParam[i].RI32("Frame");
                sub.ID    = addParam[i].RI32("ID");
                sub.Value = addParam[i].RF32("Value");

                sub.PVBranch = addParam[i].RnI32("PVBranch") ?? 0;
                float? f = addParam[i].RnF32("Value");
                if (f.HasValue)
                {
                    if (sub.ID == 0) { float v = 0; *(uint*)&v = addParam[i].RU32("Value"); sub.Value = v; }
                    else sub.Value = f.Value;
                }
                else { float v = 0; *(uint*)&v = 0xFFFFFFFF; sub.Value = v; }

            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (Header.Data == null || Header.Data.LongLength < 1) return;

            MsgPack addParam = new MsgPack(Header.Data.LongLength, "AddParam");
            fixed (HeaderData.Sub* ptr = Header.Data)
                for (i = 0; i < Header.Count; i++)
                {
                    ref HeaderData.Sub sub = ref Header.Data[i];
                    addParam[i] = MsgPack.New.Add("Time", sub.Time).Add("Flags", sub.Flags).Add("Frame", sub.Frame);

                    if (sub.PVBranch > 0)
                        addParam[i] = addParam[i].Add("PVBranch", sub.PVBranch);

                    addParam[i] = addParam[i].Add("ID", sub.ID);

                    if (sub.ID == 0)
                        addParam[i] = addParam[i].Add("Value", *(uint*)&ptr[i].Value);
                    else if (*(uint*)&ptr[i].Value != 0xFFFFFFFF)
                        addParam[i] = addParam[i].Add("Value", sub.Value);

                }
            addParam.WriteAfterAll(false, true, file, json);
        }

        public void Dispose()
        { if (s != null) s.D(); s = null; Header = default; }

        public struct HeaderData
        {
            public long Count;
            public long DataLength;
            public long DataOffset;

            public Sub[] Data;

            public struct Sub
            {
                public float Time;
                public   int Flags;
                public   int Frame;
                public   int Pad0C;
                public   int PVBranch;
                public   int ID;
                public float Value;
                public   int Pad1C;
            }
        }
    }
}