// Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary

using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.Types;
using KKdMainLib.MessagePack;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.DB
{
    public class BoneData
    {
        public Stream IO;
        public BONE Data;

        private int i, i0;

        public BoneData()
        { Data = new BONE(); }
        
        public void BONReader(string file, string ext)
        {
            Data = new BONE { Header = new PDHead() };
            IO = File.OpenReader(file + ext);

            IO.Format = Main.Format.F;
            Data.Header.Signature = IO.ReadInt32();
            if (Data.Header.Signature == 0x454E4F42)
            {
                Data.Header = IO.ReadHeader(true);
                Data.POF = Data.Header.AddPOF();
            }
            if (Data.Header.Signature != 0x09102720)
                return;
            
            Data.Skeleton = new SkeletonEntry[IO.ReadInt32Endian()];
            Data.SkeletonsOffset = IO.ReadUInt32Endian(ref Data.POF);
            Data.SkeletonNamesOffset = IO.ReadUInt32Endian();
            if (Data.SkeletonNamesOffset == 0)
            {
                IO.IsX = true;
                IO.Format = Main.Format.X;
                Data.POF.POFOffsets = new List<long>();
                IO.Seek(Data.Header.Lenght, 0);
                IO.LongOffset = Data.Header.Lenght;
                Data.Header.Signature = IO.ReadInt32();
                Data.Skeleton = new SkeletonEntry[IO.ReadInt32Endian()];
                Data.SkeletonsOffset = IO.ReadInt64();
                Data.SkeletonNamesOffset = IO.ReadInt64(ref Data.POF);
            }
            else
            {
                IO.LongPosition -= 4;
                IO.GetOffset(ref Data.POF);
                IO.LongPosition += 4;
            }

            Data.SkeletonEntryOffset = new long[Data.Skeleton.Length];
            IO.LongPosition = Data.SkeletonsOffset;
            for (i0 = 0; i0 < Data.Skeleton.Length; i0++)
                Data.SkeletonEntryOffset[i0] = IO.ReadIntX(ref Data.POF);

            for (i0 = 0; i0 < Data.Skeleton.Length; i0++)
            {
                Data.Skeleton[i0] = new SkeletonEntry();
                IO.LongPosition = Data.SkeletonEntryOffset[i0];

                Data.Skeleton[i0].BoneOffset         =                    IO.ReadIntX(ref Data.POF);
                Data.Skeleton[i0].Position           = new Vector3<float>[IO.ReadIntX()];
                Data.Skeleton[i0].PositionOffset     =                    IO.ReadIntX(ref Data.POF);
                Data.Skeleton[i0].Field02Offset      =                    IO.ReadIntX(ref Data.POF);
                Data.Skeleton[i0].BoneName1          = new         string[IO.ReadIntX()];
                Data.Skeleton[i0].BoneName1Offset    =                    IO.ReadIntX(ref Data.POF);
                Data.Skeleton[i0].BoneName2          = new         string[IO.ReadIntX()];
                Data.Skeleton[i0].BoneName2Offset    =                    IO.ReadIntX(ref Data.POF);
                Data.Skeleton[i0].ParentIndiceOffset =                    IO.ReadIntX(ref Data.POF);

                IO.LongPosition = Data.SkeletonNamesOffset + i0 << (IO.IsX ? 3 : 2);

                Data.Skeleton[i0].Name = IO.ReadStringAtOffset(ref Data.POF);
                IO.LongPosition = Data.Skeleton[i0].BoneOffset;

                long Count = 0;
                string Name = "";
                while (true)
                {
                    IO.ReadUInt64();
                    Name = IO.ReadStringAtOffset(ref Data.POF);
                    if (Name == "End") break;
                    Count++;
                }

                Data.Skeleton[i0].Bone = new BoneEntry[Count];
                for (i = 0; i < Count; i++)
                {
                    Data.Skeleton[i0].Bone[i].Type  = (BoneType)IO.ReadByte   ();
                    Data.Skeleton[i0].Bone[i].HasParent       = IO.ReadBoolean();
                    Data.Skeleton[i0].Bone[i].ParentNameIndex = IO.ReadByte   ();
                    Data.Skeleton[i0].Bone[i].Field01         = IO.ReadByte   ();
                    Data.Skeleton[i0].Bone[i].PairNameIndex   = IO.ReadByte   ();
                    Data.Skeleton[i0].Bone[i].Field02         = IO.ReadByte   ();
                    IO.ReadInt16Endian();
                    Data.Skeleton[i0].Bone[i].Name = IO.ReadStringAtOffset(ref Data.POF);
                }

                IO.LongPosition = Data.Skeleton[i0].PositionOffset;
                for (i = 0; i < Data.Skeleton[i0].Position.Length; i++)
                    Data.Skeleton[i0].Position[i] = new Vector3<float>(IO.ReadSingleEndian(),
                        IO.ReadSingleEndian(), IO.ReadSingleEndian());

                IO.LongPosition = Data.Skeleton[i0].Field02Offset;
                Data.Skeleton[i0].Field02 = IO.ReadIntX();

                IO.LongPosition = Data.Skeleton[i0].BoneName1Offset;
                for (i = 0; i < Data.Skeleton[i0].BoneName1.Length; i++)
                    Data.Skeleton[i0].BoneName1[i] = IO.ReadStringAtOffset(ref Data.POF);

                IO.LongPosition = Data.Skeleton[i0].BoneName2Offset;
                for (i = 0; i < Data.Skeleton[i0].BoneName2.Length; i++)
                    Data.Skeleton[i0].BoneName2[i] = IO.ReadStringAtOffset(ref Data.POF);

                IO.LongPosition = Data.Skeleton[i0].ParentIndiceOffset;
                for (i = 0; i < Data.Skeleton[i0].BoneName2.Length; i++)
                    Data.Skeleton[i0].ParentIndice[i] = IO.ReadInt16Endian();
            }
            if (IO.Format > Main.Format.F)
            {
                IO.LongPosition = Data.POF.Offset;
                IO.ReadPOF(ref Data.POF);
            }
            IO.Close();
        }

        public void BONWriter(string file, Main.Format Format)
        {
            IO = File.OpenWriter(file + ".bon", true);
            IO.Close();
        }
        
        public void MsgPackReader(string file, bool JSON)
        {
            MsgPack MsgPack = file.ReadMP(JSON);

            MsgPack = null;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            MsgPack BoneDB = new MsgPack("BoneDB", Data.Skeleton.Length);
            for (i0 = 0; i0 < Data.Skeleton.Length; i0++)
            {
                MsgPack Skeleton = new MsgPack().Add("Name"   , Data.Skeleton[i0].Name   )
                                                .Add("Field02", Data.Skeleton[i0].Field02);

                MsgPack Bone = new MsgPack("Bone", Data.Skeleton[i0].Bone.Length);
                for (i = 0; i < Data.Skeleton[i0].Bone.Length; i++)
                    Bone[i] = new MsgPack().Add("Type"           , (byte)  Data.Skeleton[i0].Bone[i].Type   )
                                           .Add("HasParent"      , Data.Skeleton[i0].Bone[i].HasParent      )
                                           .Add("ParentNameIndex", Data.Skeleton[i0].Bone[i].ParentNameIndex)
                                           .Add("Field01"        , Data.Skeleton[i0].Bone[i].Field01        )
                                           .Add("PairNameIndex"  , Data.Skeleton[i0].Bone[i].PairNameIndex  )
                                           .Add("Field02"        , Data.Skeleton[i0].Bone[i].Field02        )
                                           .Add("Name"           , Data.Skeleton[i0].Bone[i].Name           );
                Skeleton.Add(Bone);

                MsgPack Position = new MsgPack("Position", Data.Skeleton[i0].Position.Length);
                for (i = 0; i < Data.Skeleton[i0].Position.Length; i++)
                    Position[i] = new MsgPack().Add("X", Data.Skeleton[i0].Position[i].X)
                                               .Add("Y", Data.Skeleton[i0].Position[i].Y)
                                               .Add("Z", Data.Skeleton[i0].Position[i].Z);
                Skeleton.Add(Position);

                MsgPack BoneName1 = new MsgPack("BoneName1", Data.Skeleton[i0].BoneName1.Length);
                for (i = 0; i < Data.Skeleton[i0].BoneName1.Length; i++)
                    BoneName1[i] = Data.Skeleton[i0].BoneName1[i];
                Skeleton.Add(BoneName1);

                MsgPack BoneName2 = new MsgPack("BoneName2", Data.Skeleton[i0].BoneName2.Length);
                for (i = 0; i < Data.Skeleton[i0].BoneName2.Length; i++)
                    BoneName2[i] = Data.Skeleton[i0].BoneName2[i];
                Skeleton.Add(BoneName2);

                MsgPack ParentIndice = new MsgPack("ParentIndice", Data.Skeleton[i0].ParentIndice.Length);
                for (i = 0; i < Data.Skeleton[i0].ParentIndice.Length; i++)
                    ParentIndice[i] = Data.Skeleton[i0].ParentIndice[i];
                BoneDB[i0] = Skeleton.Add(ParentIndice);
            }

            BoneDB.Write(true, file, JSON);
        }

        /*public struct Bone
        {
            public List<string> Names;
            public List<long> Offsets;
        }*/

        public struct BONE
        {
            public long SkeletonsOffset;
            public long SkeletonNamesOffset;
            public POF POF;
            public PDHead Header;
            public long[] SkeletonEntryOffset;
            public SkeletonEntry[] Skeleton;
        }

        public struct BoneEntry
        {
            public bool HasParent;
            public byte Field01;
            public byte Field02;
            public byte PairNameIndex;
            public byte ParentNameIndex;
            public string Name;
            public BoneType Type;
        }

        public enum BoneType : byte
        {
            Rotation = 0,
            Type1    = 1,
            Position = 2,
            Type3    = 3,
            Type4    = 4,
            Type5    = 5,
            Type6    = 6,
        };

        public struct SkeletonEntry
        {
            public long BoneOffset;
            public long Field02Offset;
            public long PositionOffset;
            public long BoneName1Offset;
            public long BoneName2Offset;
            public long ParentIndiceOffset;

            public long Field02;
            public string Name;
            public short[] ParentIndice;
            public string[] BoneName1;
            public string[] BoneName2;
            public BoneEntry[] Bone;
            public Vector3<float>[] Position;
        }
    }
}