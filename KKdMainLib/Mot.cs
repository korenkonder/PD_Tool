//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct Mot
    {
        private int i, i0, i1;
        private MotHeader[] MOT;
        private Stream IO;

        public void MOTReader(string file)
        {
            IO = File.OpenReader(file + ".bin");

            i = 0;
            while (true)
                if    (IO.ReadInt64() == 0) break;
                else { IO.ReadInt64(); i++; }
            if (i == 0) return;

            int MOTCount = i;
            MsgPack m = new MsgPack(MOTCount, "Mot");
            MOT = new MotHeader[MOTCount];
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                Mot.KeySet    .Offset = IO.ReadInt32();
                Mot.KeySetTypesOffset = IO.ReadInt32();
                Mot.     KeySetOffset = IO.ReadInt32();
                Mot.BoneInfo  .Offset = IO.ReadInt32();
            }

            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];

                i0 = 1;
                IO.Position = Mot.BoneInfo.Offset;
                IO.ReadUInt16();
                while (IO.ReadUInt16() != 0) i0++;

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

                    Mot.KeySet.Value[i0] = new KeySet { Type = (KeySetType)((i1 >> (i0 % 8 * 2)) & 0b11) };
                }

                IO.Position = Mot.KeySetOffset;
                for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
                {
                    ref KeySet Key = ref Mot.KeySet.Value[i0];
                    if (Key.Type == KeySetType.Static)
                    {   Key.Keys = new KFT2<ushort, float>[1];
                        Key.Keys[0].V = IO.ReadSingle(); }
                    else if (Key.Type == KeySetType.Linear)
                    {
                        Key.Keys = new KFT2<ushort, float>[IO.ReadUInt16()];
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            Key.Keys[i1].F = IO.ReadUInt16();
                        IO.Align(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            Key.Keys[i1].V = IO.ReadSingle();
                    }
                    else if (Key.Type == KeySetType.Interpolated)
                    {
                        Key.Keys = new KFT2<ushort, float>[IO.ReadUInt16()];
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                          Key.Keys[i].F = IO.ReadUInt16();
                        IO.Align(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                        { Key.Keys[i1].V = IO.ReadSingle(); Key.Keys[i1].T = IO.ReadSingle(); }
                    }
                }
            }
            IO.Close();
        }

        public void MOTWriter(string file)
        {
            if (MOT == null) return;
            IO = File.OpenWriter(file + ".bin");

            int MOTCount = MOT.Length;
            IO.Position = (MOTCount + 1) << 4;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                Mot.KeySet.Offset = IO.Position;
                IO.Write((ushort)((Mot.HighBits << 14) | (Mot.KeySet.Value.Length & 0x3FFF)));
                IO.Write((ushort)Mot.FrameCount);

                Mot.KeySetTypesOffset = IO.Position;
                for (i0 = 0, i1 = 0; i0 < Mot.KeySet.Value.Length; i0++)
                {
                    i1 |= ((byte)Mot.KeySet.Value[i0].Type << (i0 % 8 * 2)) & (0b11 << (i0 % 8 * 2));

                    if (i0 % 8 == 7) { IO.Write((ushort)i1); i1 = 0; }
                }
                IO.Write((ushort)i1);

                IO.Align(0x4);
                Mot.KeySetOffset = IO.Position;
                for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
                {
                    ref KeySet Key = ref Mot.KeySet.Value[i0];
                    if (Key.Type == KeySetType.Static)
                        IO.Write(Key.Keys[0].V); 
                    else if (Key.Type == KeySetType.Linear)
                    {
                        IO.Write((ushort)Key.Keys.Length);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            IO.Write(Key.Keys[i1].F);
                        IO.Align(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                            IO.Write(Key.Keys[i1].V);
                    }
                    else if (Key.Type == KeySetType.Interpolated)
                    {
                        IO.Write((ushort)Key.Keys.Length);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                          IO.Write(Key.Keys[i1].F);
                        IO.Align(0x4);
                        for (i1 = 0; i1 < Key.Keys.Length; i1++)
                        { IO.Write(Key.Keys[i1].V); IO.Write(Key.Keys[i1].T); }
                    }
                }
                IO.Align(0x4);

                Mot.BoneInfo.Offset = IO.Position;
                for (i0 = 0; i0 < Mot.BoneInfo.Value.Length; i0++)
                    IO.Write((ushort)Mot.BoneInfo.Value[i0].Id);
                IO.Write((ushort)0);
            }
            IO.Align(0x4, true);

            IO.Position = 0;
            for (i = 0; i < MOTCount; i++)
            {
                ref MotHeader Mot = ref MOT[i];
                IO.Write(Mot.KeySet    .Offset);
                IO.Write(Mot.KeySetTypesOffset);
                IO.Write(Mot.     KeySetOffset);
                IO.Write(Mot.BoneInfo  .Offset);
            }

            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MOT = null;

            MsgPack MsgPack = file.ReadMP(JSON);
            if (!MsgPack.ElementArray("MOT", out MsgPack MOTS)) { MsgPack.Dispose(); return; }

            if (MOTS.Array != null)
                if (MOTS.Array.Length > 0)
                {
                    MOT = new MotHeader[MOTS.Array.Length];
                    for (int i = 0; i < MOT.Length; i++)
                        MsgPackReader(MOTS.Array[i], ref MOT[i]);
                }

            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            int MOTCount = MOT.Length;
            MsgPack MOTS = new MsgPack(MOTCount, "MOT");
            for (i = 0; i < MOTCount; i++)
                MOTS[i] = MsgPackWriter(ref MOT[i]);
            MOTS.Write(true, file, JSON);
        }

        public void MsgPackReader(MsgPack MOT, ref MotHeader Mot)
        {
            MsgPack Temp = MsgPack.New;
            Mot.HighBits   = MOT.ReadInt32("HighBits"  );
            Mot.FrameCount = MOT.ReadInt32("FrameCount");

            if (!MOT.ElementArray("KeySets", out Temp)) return;

            Mot.KeySet.Value = new KeySet[Temp.Array.Length];
            for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
            {
                ref KeySet KeySet = ref Mot.KeySet.Value[i0];

                if (Temp.Array[i0].Array == null || Temp.Array[i0].Array.Length != 2) continue;
                KeySet.Type = (KeySetType)Temp.Array[i0].Array[0].ReadInt32();

                MsgPack keySet = Temp.Array[i0].Array[1];

                if (keySet.Array == null) { KeySet.Type = 0; continue; }
                else if (KeySet.Type == KeySetType.None) continue;
                else if (KeySet.Type == KeySetType.Static)
                {
                    KeySet.Keys = new KFT2<ushort, float>[1];
                    KeySet.Keys[0].F = keySet.Array[0][0].ReadUInt16();
                    KeySet.Keys[0].V = keySet.Array[0][1].ReadSingle();
                }
                else if (KeySet.Type == KeySetType.Linear)
                {
                    KeySet.Keys = new KFT2<ushort, float>[keySet.Array.Length];
                    for (i1 = 0; i1 < keySet.Array.Length; i1++)
                    {
                        KeySet.Keys[i1].F = keySet.Array[i1][0].ReadUInt16();
                        KeySet.Keys[i1].V = keySet.Array[i1][1].ReadSingle();
                    }
                }
                else if (KeySet.Type == KeySetType.Interpolated)
                {
                    KeySet.Keys = new KFT2<ushort, float>[keySet.Array.Length];
                    for (i1 = 0; i1 < keySet.Array.Length; i1++)
                    {
                        KeySet.Keys[i1].F = keySet.Array[i1][0].ReadUInt16();
                        KeySet.Keys[i1].V = keySet.Array[i1][1].ReadSingle();
                        KeySet.Keys[i1].T = keySet.Array[i1][2].ReadSingle();
                    }
                }
            }

            if (MOT.ElementArray("BoneInfo", out Temp))
            {
                Mot.BoneInfo.Value = new BoneInfo[Temp.Array.Length];
                for (i = 0; i < Mot.BoneInfo.Value.Length; i++)
                    Mot.BoneInfo.Value[i].Id = Temp[i].ReadInt32();
            }
            else return;
        }

        public MsgPack MsgPackWriter(ref MotHeader Mot)
        {
            MsgPack MOT = MsgPack.NewReserve(4).Add("FrameCount", Mot.FrameCount).Add("HighBits", Mot.HighBits);

            MsgPack KeySets = new MsgPack(Mot.KeySet.Value.Length, "KeySets");
            for (i0 = 0; i0 < Mot.KeySet.Value.Length; i0++)
            {
                ref KeySet KeySet = ref Mot.KeySet.Value[i0];
                if (KeySet.Type == KeySetType.None) continue;

                KeySets[i0] = new MsgPack(2);
                KeySets[i0].Array[0] = (byte)KeySet.Type;
                KeySets[i0].Array[1] = new MsgPack(KeySet.Keys.Length);
                if (KeySet.Type == KeySetType.Static)
                {
                    KeySets[i0].Array[1][0] = new MsgPack(2);
                    KeySets[i0].Array[1][0].Array[0] = KeySet.Keys[0].F;
                    KeySets[i0].Array[1][0].Array[1] = KeySet.Keys[0].V;
                }
                else if (KeySet.Type == KeySetType.Linear)
                    for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                    {
                        KeySets[i0].Array[1][i1] = new MsgPack(2);
                        KeySets[i0].Array[1][i1].Array[0] = KeySet.Keys[i1].F;
                        KeySets[i0].Array[1][i1].Array[1] = KeySet.Keys[i1].V;
                    }
                else
                    for (i1 = 0; i1 < KeySet.Keys.Length; i1++)
                    {
                        KeySets[i0].Array[1][i1] = new MsgPack(3);
                        KeySets[i0].Array[1][i1].Array[0] = KeySet.Keys[i1].F;
                        KeySets[i0].Array[1][i1].Array[1] = KeySet.Keys[i1].V;
                        KeySets[i0].Array[1][i1].Array[2] = KeySet.Keys[i1].T;
                    }
            }
            MOT.Add(KeySets);

            MsgPack BoneInfo = new MsgPack(Mot.BoneInfo.Value.Length, "BoneInfo");
            for (i0 = 0; i0 < Mot.BoneInfo.Value.Length; i0++)
                BoneInfo[i0] = Mot.BoneInfo.Value[i0].Id;
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
            public KFT2<ushort, float>[] Keys;
            public KeySetType Type;

            public override string ToString() => $"Type: {Type}" + (Type == KeySetType.Static ?
                $"; Value: {Keys[0].Check()}" : Type > KeySetType.Static ? $"; Keys: {Keys.Length}" : "");
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
