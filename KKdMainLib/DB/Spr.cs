//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System.Collections.Generic;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib.DB
{
    public class Spr
    {
        public SpriteSet[] SpriteSets;

        private Stream IO;
        private int i, i0, i1, i2;

        public void BINReader(string file)
        {
            IO = File.OpenReader(file + ".bin");

            int spriteSetsLength = IO.RI32();
            int spriteSetsOffset = IO.RI32();
            int spritesLength    = IO.RI32();
            int spritesOffset    = IO.RI32();

            IO.P = spriteSetsOffset;
            SpriteSets = new SpriteSet[spriteSetsLength];
            for (i = 0; i < spriteSetsLength; i++)
            {
                SpriteSets[i].Id       = IO.RI32();
                SpriteSets[i].Name     = IO.RSaO();
                SpriteSets[i].FileName = IO.RSaO();
                IO.RI32();
            }

            int setIndex;
            bool IsTexture;
            SpriteTexture st = new SpriteTexture();
            int[] SprCount = new int[spriteSetsLength];
            int[] TexCount = new int[spriteSetsLength];

            IO.P = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                IO.I64P += 10;
                setIndex = IO.RI16();
                IsTexture = (setIndex & 0x1000) == 0x1000;
                setIndex &= 0xFFF;

                if (IsTexture) TexCount[setIndex]++;
                else           SprCount[setIndex]++;
            }

            for (int i = 0; i < spriteSetsLength; i++)
            {
                SpriteSets[i].Sprites  = new SpriteTexture[SprCount[i]];
                SpriteSets[i].Textures = new SpriteTexture[TexCount[i]];

                SprCount[i] = 0;
                TexCount[i] = 0;
            }

            IO.P = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                st.Id    = IO.RI32();
                st.Name  = IO.RSaO();
                IO.RI16();
                setIndex = IO.RI16();
                IsTexture = (setIndex & 0x1000) == 0x1000;
                setIndex &= 0xFFF;
                
                if (IsTexture) { SpriteSets[setIndex].Textures[TexCount[setIndex]] = st; TexCount[setIndex]++; }
                else           { SpriteSets[setIndex]. Sprites[SprCount[setIndex]] = st; SprCount[setIndex]++; }
            }

            IO.C();
        }

        public void BINWriter(string file)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            List<string> SetName     = new List<string>();
            List<string> SetFileName = new List<string>();

            List<int>    Ids = new List<int>();
            List<int> SetIds = new List<int>();

            List<int> NotAdd = new List<int>();
            SpriteTexture temp;
            SpriteSet set;

            for (i = 0; i < SpriteSets.Length; i++)
            {
                set = SpriteSets[i];
                if (set.    Name  != null)
                    if  (SetName    .Contains(set.    Name)) { NotAdd.Add(i); continue; }
                    else SetName    .Add     (set.    Name);
                if (set.FileName  != null)
                    if  (SetFileName.Contains(set.FileName)) { NotAdd.Add(i); continue; }
                    else SetFileName.Add     (set.FileName);

                if (set.NewId)
                {
                    SpriteSets[i].Id = null;
                    for (i0 = 0; i0 < set.Textures.Length; i0++) SpriteSets[i].Textures[i0].Id = null;
                    for (i0 = 0; i0 < set. Sprites.Length; i0++) SpriteSets[i]. Sprites[i0].Id = null;
                    continue;
                }

                if (set.Id    != null)
                    if  (SetIds.Contains((int)set.Id)) { NotAdd.Add(i); continue; }
                    else SetIds.Add     ((int)set.Id);

                for (i0 = 0; i0 < set. Sprites.Length; i0++)
                {
                    temp = set. Sprites[i0];
                    if (temp.Id != null)
                        if ( Ids.Contains((int)temp.Id)) { NotAdd.Add(i); break; }
                        else Ids.Add     ((int)temp.Id);
                }
                if (i0 < set.Sprites.Length) continue;

                for (i0 = 0; i0 < set.Textures.Length; i0++)
                {
                    temp = set.Textures[i0];
                    if (temp.Id != null)
                        if ( Ids.Contains((int)temp.Id)) { NotAdd.Add(i); break; }
                        else Ids.Add     ((int)temp.Id);
                }
            }
            SetName     = null;
            SetFileName = null;

            for (i = 0; i < SpriteSets.Length; i++)
            {
                set = SpriteSets[i];
                if (NotAdd.Contains(i)) continue;
                if (!set.NewId) continue;

                i1 = 0;
                if (set.Id    == null) while (true)
                    { if (!SetIds.Contains(i1)) { SpriteSets[i].Id = i1; SetIds.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Textures.Length; i0++)
                    while (set.Textures[i0].Id == null)
                        if (!Ids.Contains(i1)) Ids.Add((int)(SpriteSets[i].Textures[i0].Id = i1)); else i1++;

                for (i0 = 0, i1 = 0; i0 < set. Sprites.Length; i0++)
                    while (set. Sprites[i0].Id == null)
                        if (!Ids.Contains(i1)) Ids.Add((int)(SpriteSets[i]. Sprites[i0].Id = i1)); else i1++;
            }

               Ids = null;
            SetIds = null;



            for (i = 0, i0 = 0, i2 = 0; i < SpriteSets.Length; i++)
                if (!NotAdd.Contains(i)) { i0 += SpriteSets[i].Sprites.Length + SpriteSets[i].Textures.Length; i2++; }
            
            i1 = i0 * 12;
            i1 = i1.Align(0x20) + 0x20;

            IO = File.OpenWriter(file + ".bin", true);
            IO.W(i2);
            IO.W(i1);
            IO.W(i0);
            IO.W(0x20);
            IO.W(0x9066906690669066);
            IO.W(0x9066906690669066);

            IO.P = (i1 + i2 * 0x10).Align(0x20);
            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                { SpriteSets[i].Textures[i0].NameOffset = IO.P;
                    IO.W(SpriteSets[i].Textures[i0].Name + "\0"); }

                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                { SpriteSets[i]. Sprites[i0].NameOffset = IO.P;
                    IO.W(SpriteSets[i]. Sprites[i0].Name + "\0"); }
            }

            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                SpriteSets[i].    NameOffset = IO.P; IO.W(SpriteSets[i].    Name + "\0");
                SpriteSets[i].FileNameOffset = IO.P; IO.W(SpriteSets[i].FileName + "\0");
            }
            
            IO.A(0x08, true);
            IO.P = 0x20;
            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }
                
                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                {
                    IO.W(SpriteSets[i].Textures[i0].Id        );
                    IO.W(SpriteSets[i].Textures[i0].NameOffset);
                    IO.W((ushort)               i0  );
                    IO.W((ushort)(0x1000 | (i - i2)));
                }
                
                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                {
                    IO.W(SpriteSets[i]. Sprites[i0].Id        );
                    IO.W(SpriteSets[i]. Sprites[i0].NameOffset);
                    IO.W((ushort)     i0 );
                    IO.W((ushort)(i - i2));
                }
            }
            IO.A(0x20);
            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }

                IO.W(SpriteSets[i].Id            );
                IO.W(SpriteSets[i].    NameOffset);
                IO.W(SpriteSets[i].FileNameOffset);
                IO.W(i - i2);
            }

            IO.C();
        }

        public void MsgPackReader(string file, bool JSON = false)
        {
            MsgPack MsgPack = file.ReadMPAllAtOnce(JSON);

            MsgPack SprDB;
            if ((SprDB = MsgPack["SprDB", true]).NotNull)
            {
                SpriteSets = new SpriteSet[SprDB.Array.Length];
                for (int i = 0; i < SpriteSets.Length; i++)
                    SpriteSets[i].ReadMsgPack(SprDB[i]);
            }
            SprDB.Dispose();
            MsgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            MsgPack SprDB = new MsgPack(SpriteSets.Length, "SprDB");
            for (i = 0; i < SpriteSets.Length; i++) SprDB[i] = SpriteSets[i].WriteMsgPack();

            SprDB.Write(true, file, JSON);
        }

        public struct SpriteTexture
        {
            public int NameOffset;
            public int? Id;
            public string Name;

            public void ReadMsgPack(MsgPack msg)
            { Id = msg.RnI32("Id"); Name = msg.RS("Name"); }

            public MsgPack WriteMsgPack() =>
                MsgPack.New.Add("Id", Id).Add("Name", Name);
        }

        public struct SpriteSet
        {
            public int NameOffset;
            public int FileNameOffset;
            public int? Id;
            public bool NewId;
            public string Name;
            public string FileName;
            public SpriteTexture[] Sprites;
            public SpriteTexture[] Textures;
            
            public void ReadMsgPack(MsgPack msg)
            {
                FileName     = msg.RS   ("FileName");
                Id           = msg.RnI32(   "Id"   );
                Name         = msg.RS   ("Name"    );
                NewId        = msg.RB   ("NewId"   );

                MsgPack Temp;
                if ((Temp = msg["Sprites", true]).NotNull)
                {
                    Sprites = new SpriteTexture[Temp.Array.Length];
                    for (int i0 = 0; i0 < Sprites.Length; i0++)
                        Sprites[i0].ReadMsgPack(Temp[i0]);
                }

                if ((Temp = msg["Textures", true]).NotNull)
                {
                    Textures = new SpriteTexture[Temp.Array.Length];
                    for (int i0 = 0; i0 < Textures.Length; i0++)
                        Textures[i0].ReadMsgPack(Temp[i0]);
                }
                Temp.Dispose();
            }

            public MsgPack WriteMsgPack()
            {
                MsgPack  Sprites = new MsgPack(this. Sprites.Length,  "Sprites");
                for (int i0 = 0; i0 < this.Sprites.Length; i0++)
                     Sprites[i0] = this.Sprites[i0].WriteMsgPack();

                MsgPack Textures = new MsgPack(this.Textures.Length, "Textures");
                for (int i0 = 0; i0 <  this.Textures.Length; i0++)
                    Textures[i0] = this.Textures[i0].WriteMsgPack();

                return MsgPack.New.Add("FileName", FileName).Add("Id", Id).Add("Name", Name).Add(Sprites).Add(Textures);
            }
        }
    }
}
