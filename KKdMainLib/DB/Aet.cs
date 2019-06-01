//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.DB
{
    public class Aet
    {
        public AetSet[] AetSets;

        private Stream IO;
        private int i, i0, i1, i2;

        public void BINReader(string file)
        {
            IO = File.OpenReader(file + ".bin");

            int aetSetsLength = IO.ReadInt32();
            int aetSetsOffset = IO.ReadInt32();
            int aetsLength    = IO.ReadInt32();
            int aetsOffset    = IO.ReadInt32();

            IO.Position = aetSetsOffset;
            AetSets = new AetSet[aetSetsLength];
            for (i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Id          = IO.ReadInt32();
                AetSets[i].Name        = IO.ReadStringAtOffset();
                AetSets[i].FileName    = IO.ReadStringAtOffset();
                IO.ReadInt32();
                AetSets[i].SpriteSetId = IO.ReadInt32();
            }

            int setIndex;
            AET aet = new AET();
            int[] AetCount = new int[aetSetsLength];

            IO.Position = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                IO.LongPosition += 10;
                setIndex = IO.ReadInt16();
                AetCount[setIndex]++;
            }

            for (int i = 0; i < aetSetsLength; i++)
            {
                AetSets[i].Aets  = new AET[AetCount[i]];
                AetCount[i] = 0;
            }

            IO.Position = aetsOffset;
            for (i = 0; i < aetsLength; i++)
            {
                aet.Id    = IO.ReadInt32();
                aet.Name  = IO.ReadStringAtOffset();
                IO.ReadInt16();
                setIndex  = IO.ReadInt16();

                AetSets[setIndex].Aets[AetCount[setIndex]] = aet; AetCount[setIndex]++;
            }

            IO.Close();
        }


        public void BINWriter(string file)
        {
            if (AetSets        == null) return;
            if (AetSets.Length ==    0) return;

            List<string> SetName     = new List<string>();
            List<string> SetFileName = new List<string>();

            List<int>    Ids        = new List<int>();
            List<int> SetIds        = new List<int>();

            List<int> NotAdd = new List<int>();
            AET temp;
            AetSet set;

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (set.    Name  != null)
                    if  (SetName    .Contains(set.    Name)) { NotAdd.Add(i); continue; }
                    else SetName    .Add     (set.    Name);
                if (set.FileName  != null)
                    if  (SetFileName.Contains(set.FileName)) { NotAdd.Add(i); continue; }
                    else SetFileName.Add     (set.FileName);

                if (set.NewId)
                {
                    AetSets[i].Id = null;
                    for (i0 = 0; i0 < set.Aets.Length; i0++) AetSets[i].Aets[i0].Id = null;
                    continue;
                }

                if (set.Id    != null)
                    if  (SetIds.Contains((int)set.Id)) { NotAdd.Add(i); continue; }
                    else SetIds.Add     ((int)set.Id);

                for (i0 = 0; i0 < set.Aets.Length; i0++)
                {
                    temp = set.Aets[i0];
                    if (temp.Id != null)
                        if ( Ids.Contains((int)temp.Id)) { NotAdd.Add(i); break; }
                        else Ids.Add     ((int)temp.Id);
                }
            }
            SetName     = null;
            SetFileName = null;

            for (i = 0; i < AetSets.Length; i++)
            {
                set = AetSets[i];
                if (NotAdd.Contains(i)) continue;
                if (!set.NewId) continue;

                i1 = 0;
                if (set.Id    == null) while (true)
                    { if (!SetIds.Contains(i1)) { AetSets[i].Id = i1; SetIds.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Aets.Length; i0++)
                    if (set.Aets[i0].Id == null) while (true) { if (!Ids.Contains(i1))
                            { AetSets[i].Aets[i0].Id = i1; Ids.Add(i1); break; } i1++; }
            }

               Ids = null;
            SetIds = null;

            for (i = 0, i0 = 0, i2 = 0; i < AetSets.Length; i++)
                if (!NotAdd.Contains(i)) { i0 += AetSets[i].Aets.Length; i2++; }
            
            i1 = i0 * 12;
            i1 = i1.Align(0x20) + 0x20;

            IO = File.OpenWriter(file + ".bin", true);
            IO.Write(i2);
            IO.Write(i1);
            IO.Write(i0);
            IO.Write(0x20);
            IO.Write(0x9066906690669066);
            IO.Write(0x9066906690669066);

            IO.Position = (i1 + i2 * 0x14).Align(0x20);
            for (i = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                AetSets[i].    NameOffset = IO.Position; IO.Write(AetSets[i].    Name + "\0");
                AetSets[i].FileNameOffset = IO.Position; IO.Write(AetSets[i].FileName + "\0");
            }

            for (i = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                { AetSets[i].Aets[i0].NameOffset = IO.Position; IO.Write(AetSets[i].Aets[i0].Name + "\0"); }
            }
            IO.Align(0x08, true);
            
            IO.Position = 0x20;
            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }
                
                for (i0 = 0; i0 < AetSets[i].Aets.Length; i0++)
                {
                    IO.Write(AetSets[i].Aets[i0].Id        );
                    IO.Write(AetSets[i].Aets[i0].NameOffset);
                    IO.Write((ushort)     i0 );
                    IO.Write((ushort)(i - i2));
                }
            }
            IO.Align(0x20);
            for (i = 0, i2 = 0; i < AetSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }

                IO.Write(AetSets[i].Id            );
                IO.Write(AetSets[i].    NameOffset);
                IO.Write(AetSets[i].FileNameOffset);
                IO.Write(i - i2);
                IO.Write(AetSets[i].SpriteSetId   );
            }
            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON)
        {
            MsgPack MsgPack = file.ReadMP(JSON);

            if (MsgPack.Element("AetDB", out MsgPack AetDB, typeof(object[])))
            {
                AetSets = new AetSet[((object[])AetDB.Object).Length];
                for (int i = 0; i < AetSets.Length; i++)
                    if (AetDB[i].GetType() == typeof(MsgPack))
                        AetSets[i].ReadMsgPack((MsgPack)AetDB[i]);
            }
            MsgPack = null;
        }


        public void MsgPackWriter(string file, bool JSON)
        {
            if (AetSets        == null) return;
            if (AetSets.Length ==    0) return;

            MsgPack AetDB = new MsgPack("AetDB", AetSets.Length);
            for (i = 0; i < AetSets.Length; i++)
                AetDB[i] = AetSets[i].WriteMsgPack();

            AetDB.Write(true, file, JSON);
        }

        public struct AET
        {
            public int NameOffset;
            public int? Id;
            public string Name;

            public void ReadMsgPack(MsgPack msg)
            { Id = msg.ReadNUInt16("Id"); Name  = msg.ReadString("Name"); }
            
            public MsgPack WriteMsgPack() =>
                new MsgPack().Add("Id", Id).Add("Name", Name);
        }

        public struct AetSet
        {
            public int NameOffset;
            public int FileNameOffset;
            public int? Id;
            public int? SpriteSetId;
            public bool NewId;
            public string Name;
            public string FileName;
            public AET[] Aets;
            
            public void ReadMsgPack(MsgPack msg)
            {
                FileName    = msg.ReadString ("FileName"   );
                Id          = msg.ReadNUInt16(         "Id");
                Name        = msg.ReadString (    "Name"   );
                NewId       = msg.ReadBoolean("NewId"      );
                SpriteSetId = msg.ReadNUInt16("SpriteSetId");

                if (msg.Element("Aets", out MsgPack Aets, typeof(object[])))
                {
                    object Aet;
                    this  .Aets = new AET[((object[])Aets.Object).Length];
                    for (int i0 = 0; i0 < this.Aets.Length; i0++)
                    { Aet = Aets[i0]; if (Aet.GetType() == typeof(MsgPack))
                           this.Aets[i0].ReadMsgPack((MsgPack)Aet); }
                }
            }

            public MsgPack WriteMsgPack()
            {
                MsgPack Aets = new MsgPack("Aets", this.Aets.Length);
                for (int i0 = 0; i0 < this.Aets.Length; i0++)
                    Aets[i0] = this.Aets[i0].WriteMsgPack();

                return new MsgPack().Add("FileName", FileName).Add("Id", Id)
                    .Add("Name", Name).Add("SpriteSetId", SpriteSetId).Add(Aets);
            }
        }
    }
}
