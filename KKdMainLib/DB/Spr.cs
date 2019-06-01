//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/

using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.DB
{
    public class Spr
    {
        public SpriteSet[] SpriteSets;

        private Stream IO;
        private int i, i0, i1, i2;

        public void BINReader(string file)
        {
            /*Stream IO0 = File.OpenReader(file + "0.bin");
            Stream IO1 = File.OpenReader(file + "1.bin");

            List<int> _0 = new List<int>();
            List<int> _1 = new List<int>();
            IO0.Position = 0x20;
            for (i = 0; i < 124; i++)
            { _0.Add(IO0.ReadInt32()); IO0.LongPosition += 8; }
            IO1.Position = 0x20;
            for (i = 0; i < 124; i++)
            { _1.Add(IO1.ReadInt32()); IO1.LongPosition += 8; }
            IO0.Close();
            IO1.Close();

            IO = File.OpenWriter(@"F:\Source\MikuMikuModel\DatabaseConverter\msgpack-json-tools\aet_gam_pv643.bin");
            IO.Position = 0x10;
            for (i = 0; i < 108; i++)
            { IO.ReadInt32(); i1 = _1[_0.IndexOf(IO.ReadInt32())]; IO.Position -= 4; IO.Write(i1); }
            IO.Close();*/

            IO = File.OpenReader(file + ".bin");

            int spriteSetsLength = IO.ReadInt32();
            int spriteSetsOffset = IO.ReadInt32();
            int spritesLength    = IO.ReadInt32();
            int spritesOffset    = IO.ReadInt32();

            IO.Position = spriteSetsOffset;
            SpriteSets = new SpriteSet[spriteSetsLength];
            for (i = 0; i < spriteSetsLength; i++)
            {
                SpriteSets[i].Id       = IO.ReadInt32();
                SpriteSets[i].Name     = IO.ReadStringAtOffset();
                SpriteSets[i].FileName = IO.ReadStringAtOffset();
                IO.ReadInt32();
            }

            int setIndex;
            bool IsTexture;
            SpriteTexture st = new SpriteTexture();
            int[] SprCount = new int[spriteSetsLength];
            int[] TexCount = new int[spriteSetsLength];

            IO.Position = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                IO.LongPosition += 10;
                setIndex = IO.ReadInt16();
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

            IO.Position = spritesOffset;
            for (i = 0; i < spritesLength; i++)
            {
                st.Id    = IO.ReadInt32();
                st.Name  = IO.ReadStringAtOffset();
                IO.ReadInt16();
                setIndex = IO.ReadInt16();
                IsTexture = (setIndex & 0x1000) == 0x1000;
                setIndex &= 0xFFF;
                
                if (IsTexture) { SpriteSets[setIndex].Textures[TexCount[setIndex]] = st; TexCount[setIndex]++; }
                else           { SpriteSets[setIndex]. Sprites[SprCount[setIndex]] = st; SprCount[setIndex]++; }
            }

            IO.Close();
        }

        public void BINWriter(string file)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            List<string> SetName     = new List<string>();
            List<string> SetFileName = new List<string>();

            List<int>    Ids        = new List<int>();
            List<int> SetIds        = new List<int>();

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
                if (i0 + 1 != set.Sprites.Length) continue;

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

                for (i0 = 0, i1 = 0; i0 < set. Sprites.Length; i0++)
                    if (set. Sprites[i0].Id == null) while (true) { if (!Ids.Contains(i1))
                            { SpriteSets[i]. Sprites[i0].Id = i1; Ids.Add(i1); break; } i1++; }

                for (i0 = 0, i1 = 0; i0 < set.Textures.Length; i0++)
                    if (set.Textures[i0].Id == null) while (true) { if (!Ids.Contains(i1))
                            { SpriteSets[i].Textures[i0].Id = i1; Ids.Add(i1); break; } i1++; }
            }

               Ids = null;
            SetIds = null;



            for (i = 0, i0 = 0, i2 = 0; i < SpriteSets.Length; i++)
                if (!NotAdd.Contains(i)) { i0 += SpriteSets[i].Sprites.Length + SpriteSets[i].Textures.Length; i2++; }
            
            i1 = i0 * 12;
            i1 = i1.Align(0x20) + 0x20;

            IO = File.OpenWriter(file + ".bin", true);
            IO.Write(i2);
            IO.Write(i1);
            IO.Write(i0);
            IO.Write(0x20);
            IO.Write(0x9066906690669066);
            IO.Write(0x9066906690669066);

            IO.Position = (i1 + i2 * 0x10).Align(0x20);
            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                SpriteSets[i].    NameOffset = IO.Position; IO.Write(SpriteSets[i].    Name + "\0");
                SpriteSets[i].FileNameOffset = IO.Position; IO.Write(SpriteSets[i].FileName + "\0");
            }

            for (i = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) continue;
                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                { SpriteSets[i].Textures[i0].NameOffset = IO.Position; IO.Write(SpriteSets[i].Textures[i0].Name + "\0"); }

                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                { SpriteSets[i]. Sprites[i0].NameOffset = IO.Position; IO.Write(SpriteSets[i]. Sprites[i0].Name + "\0"); }
            }
            
            IO.Align(0x08, true);
            IO.Position = 0x20;
            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }
                
                for (i0 = 0; i0 < SpriteSets[i].Textures.Length; i0++)
                {
                    IO.Write(SpriteSets[i].Textures[i0].Id        );
                    IO.Write(SpriteSets[i].Textures[i0].NameOffset);
                    IO.Write((ushort)               i0  );
                    IO.Write((ushort)(0x1000 | (i - i2)));
                }
                
                for (i0 = 0; i0 < SpriteSets[i]. Sprites.Length; i0++)
                {
                    IO.Write(SpriteSets[i]. Sprites[i0].Id        );
                    IO.Write(SpriteSets[i]. Sprites[i0].NameOffset);
                    IO.Write((ushort)     i0 );
                    IO.Write((ushort)(i - i2));
                }
            }
            IO.Align(0x20);
            for (i = 0, i2 = 0; i < SpriteSets.Length; i++)
            {
                if (NotAdd.Contains(i)) { i2++; continue; }

                IO.Write(SpriteSets[i].Id            );
                IO.Write(SpriteSets[i].    NameOffset);
                IO.Write(SpriteSets[i].FileNameOffset);
                IO.Write(i - i2);
            }

            IO.Close();
        }

        public void MsgPackReader(string file, bool JSON = false)
        {
            MsgPack MsgPack = file.ReadMP(JSON);

            if (MsgPack.Element("SprDB", out MsgPack SprDB, typeof(object[])))
            {
                SpriteSets = new SpriteSet[((object[])SprDB.Object).Length];
                for (int i = 0; i < SpriteSets.Length; i++)
                    if (SprDB[i].GetType() == typeof(MsgPack))
                        SpriteSets[i].ReadMsgPack((MsgPack)SprDB[i]);
            }
            MsgPack = null;
        }

        public void MsgPackWriter(string file, bool JSON)
        {
            if (SpriteSets        == null) return;
            if (SpriteSets.Length ==    0) return;

            MsgPack SprDB = new MsgPack("SprDB", SpriteSets.Length);
            for (i = 0; i < SpriteSets.Length; i++) SprDB[i] = SpriteSets[i].WriteMsgPack();

            SprDB.Write(true, file, JSON);
        }

        public struct SpriteTexture
        {
            public int NameOffset;
            public int? Id;
            public string Name;

            public void ReadMsgPack(MsgPack msg)
            { Id = msg.ReadNInt32("Id"); Name = msg.ReadString("Name"); }

            public MsgPack WriteMsgPack() =>
                new MsgPack().Add("Id", Id).Add("Name", Name);
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
                FileName     = msg.ReadString ("FileName");
                Id           = msg.ReadNInt32 (   "Id"   );
                Name         = msg.ReadString ("Name"    );
                NewId        = msg.ReadBoolean("NewId"   );

                if (msg.Element( "Sprites", out MsgPack  Sprites, typeof(object[])))
                {
                    object Sprite;
                    this  .Sprites = new SpriteTexture[((object[])Sprites.Object).Length];
                    for (int i0 = 0; i0 < this.Sprites.Length; i0++)
                    { Sprite = Sprites[i0]; if (Sprites.GetType() == typeof(MsgPack))
                          this.Sprites[i0].ReadMsgPack((MsgPack)Sprite); }
                }

                if (msg.Element("Textures", out MsgPack Textures, typeof(object[])))
                {
                    object Texture;
                    this  .Textures = new SpriteTexture[((object[])Textures.Object).Length];
                    for (int i0 = 0; i0 < this.Textures.Length; i0++)
                    { Texture = Textures[i0]; if (Texture.GetType() == typeof(MsgPack))
                           this.Textures[i0].ReadMsgPack((MsgPack)Texture); }
                }
            }

            public MsgPack WriteMsgPack()
            {
                MsgPack  Sprites = new MsgPack( "Sprites",  this.Sprites.Length);
                for (int i0 = 0; i0 < this.Sprites.Length; i0++)
                     Sprites[i0] = this.Sprites[i0].WriteMsgPack();

                MsgPack Textures = new MsgPack("Textures", this.Textures.Length);
                for (int i0 = 0; i0 <  this.Textures.Length; i0++)
                    Textures[i0] = this.Textures[i0].WriteMsgPack();

                return new MsgPack().Add("FileName", FileName).Add("Id", Id).Add("Name", Name).Add(Sprites).Add(Textures);
            }
        }
    }
}
