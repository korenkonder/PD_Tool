//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdMainLib.IO;
using KKdMainLib.Types;
using KKdMainLib.MessagePack;

namespace KKdMainLib
{
    public class Mot
    {
        private int i, i0, i1;
        private Stream IO;

        public Mot()
        { i = i0 = i1 = 0; IO = null; }

        public unsafe void MOTReader(string file, bool JSON)
        {
            IO = File.OpenReader(file + ".bin");

            i = 0;
            while (true)
            {
                if (IO.ReadInt32() == 0) break;
                IO.ReadInt32();
                IO.ReadInt32();
                IO.ReadInt32();
                i++;
            }
            if (i == 0) return;

            int MOTCount = i;
            MsgPack m = new MsgPack(MOTCount, "Mot");
            MotHeader[] MMOT = new MotHeader[MOTCount];
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MMOT[i];
                IO.Position = 0x10 * i;
                Mot.    KeySet.Offset = IO.ReadInt32();
                Mot.KeySetTypesOffset = IO.ReadInt32();
                Mot.     KeySetOffset = IO.ReadInt32();
                Mot.  BoneInfo.Offset = IO.ReadInt32();

                i0 = 1;
                IO.Position = Mot.BoneInfo.Offset;
                IO.ReadUInt16();
                while (true) { if (IO.ReadUInt16() == 0) break; i0++; }

                Mot.BoneInfo.Value = new BoneInfo[i0];
                IO.Position = Mot.BoneInfo.Offset;
                for (i0 = 0; i0 < Mot.BoneInfo.Value.Length; i0++)
                    Mot.BoneInfo.Value[i0].Id = IO.ReadUInt16();

                IO.Position = Mot.KeySet.Offset;
                int info = IO.ReadUInt16();
                Mot.HighBits = info >> 14;
                Mot.FrameCount = IO.ReadUInt16();

                Mot.KeySet.Value = new KeySet[info & 0x3FFF];
                IO.Position = Mot.KeySetTypesOffset;
                for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
                {
                    if (i0 % 8 == 0) i1 = IO.ReadUInt16();

                    Mot.KeySet.Value[i0] = new KeySet
                    { Type = (KeySetType)((i1 >> (i0 % 8 * 2)) & 3) };
                }

                IO.Position = Mot.KeySetOffset;
                for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
                    Mot.KeySet.Value[i0].Read(IO);

                
                m[i] = MsgPackWriter(ref MMOT[i]);
                m[i].Write(true, file + "." + i.ToString("d2"), JSON);
            }
            IO.Close();
            //for (i = 0; i < MOTCount; i++)
            //    m[i] = MsgPackWriter(ref MMOT[i]);

            m.Write(true, file, JSON);
            MMOT = null;
            m = new MsgPack();
            System.GC.Collect();
        }

        public MsgPack MsgPackWriter(ref MotHeader Mot)
        {
            MsgPack MOT = MsgPack.NewReserve(3).Add("FrameCount", Mot.FrameCount).Add("HighBits", Mot.HighBits);

            MsgPack KeySets = new MsgPack(Mot.KeySet.Value.Length, "KeySets");
            for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
            {
                ref KeySet KeySet = ref Mot.KeySet.Value[i0];
                if (KeySet.Type != KeySetType.None)
                {
                    MsgPack keySet = MsgPack.NewReserve(2).Add("T", KeySet.Type.ToString());

                    if (KeySet.Type == KeySetType.Interpolated)
                    {
                        MsgPack Keys = new MsgPack(KeySet.Keys.Length, "K");
                        for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                                 if (KeySet.Keys[i1] is KeyFrameT0<ushort, float> KeyT0)
                                Keys[i1] = MsgPack.NewReserve(1).Add("F", KeyT0.Frame);
                            else if (KeySet.Keys[i1] is KeyFrameT1<ushort, float> KeyT1)
                                Keys[i1] = MsgPack.NewReserve(2).Add("F", KeyT1.Frame).Add("V", KeyT1.Value);
                            else if (KeySet.Keys[i1] is KeyFrameT2<ushort, float> KeyT2)
                                Keys[i1] = MsgPack.NewReserve(3).Add("F", KeyT2.Frame).Add("V",
                                    KeyT2.Value).Add("I", KeyT2.Interpolation);
                        keySet.Add(Keys);
                    }
                    else if(KeySet.Type == KeySetType.Linear)
                    {
                        MsgPack Keys = new MsgPack(KeySet.Keys.Length, "K");
                        for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                                 if (KeySet.Keys[i1] is KeyFrameT0<ushort, float> KeyT0)
                                Keys[i1] = MsgPack.NewReserve(1).Add("F", KeyT0.Frame);
                            else if (KeySet.Keys[i1] is KeyFrameT1<ushort, float> KeyT1)
                                Keys[i1] = MsgPack.NewReserve(2).Add("F", KeyT1.Frame).Add("V", KeyT1.Value);
                        keySet.Add(Keys);
                    }
                    else if (KeySet.Type == KeySetType.Static)
                        if (KeySet.Keys[0] is KeyFrameT1<ushort, float> Key) keySet.Add("K", Key.Value);

                    KeySets[i0] = keySet;
                }
            }
            MOT.Add(KeySets);

            MsgPack BoneInfo = new MsgPack(Mot.BoneInfo.Value.Length, "BoneInfo");
            for (i0 = 0; i0 < Mot.BoneInfo.Value.Length; i0++)
                BoneInfo[i0] = (MsgPack)Mot.BoneInfo.Value[i0].Id;
            MOT.Add(BoneInfo);

            return MOT;
        }

        public struct MotHeader
        {
            public int      KeySetOffset;
            public int KeySetTypesOffset;
            public Pointer< KeySet []>  KeySet ;
            public Pointer<BoneInfo[]> BoneInfo;

            public int HighBits;
            public int FrameCount;
        }

        public struct KeySet
        {
            public IKeyFrame<ushort, float>[] Keys;
            public KeySetType Type;

            public void Read(Stream IO)
            {
                     if (Type == KeySetType.None  ) return;
                else if (Type == KeySetType.Static) { Keys = new IKeyFrame<ushort, float>[]
                { new KeyFrameT1<ushort, float> { Value = IO.ReadSingle() } }; return; }
                     
                Keys = new IKeyFrame<ushort, float>[IO.ReadUInt16()];
                if (Type == KeySetType.Interpolated)
                {
                    for (int i = 0; i < Keys.Length; i++)
                        Keys[i] = new KeyFrameT2<ushort, float>{ Frame = IO.ReadUInt16() };
                    IO.Align(0x4);
                    for (int i = 0; i < Keys.Length; i++)
                        if (Keys[i] is KeyFrameT2<ushort, float> Key)
                        {
                            Key.Value         = IO.ReadSingle();
                            Key.Interpolation = IO.ReadSingle();
                            Keys[i] = Key.Check();
                        }
                }
                else
                {
                    for (int i = 0; i < Keys.Length; i++)
                        Keys[i] = new KeyFrameT1<ushort, float> { Frame = IO.ReadUInt16() };
                    IO.Align(0x4);
                    for (int i = 0; i < Keys.Length; i++)
                        if (Keys[i] is KeyFrameT1<ushort, float> Key)
                        { Key.Value = IO.ReadSingle(); Keys[i] = Key; }
                }
            }
        }

        public enum KeySetType : byte
        {
            None         = 0b00,
            Static       = 0b01,
            Linear       = 0b10,
            Interpolated = 0b11,
        }

        public struct BoneInfo
        {
            public string Name;
            public int Id;
        }
    }
}
