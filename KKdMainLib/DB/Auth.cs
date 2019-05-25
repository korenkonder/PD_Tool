using System;
using System.Collections.Generic;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.DB
{
    public struct Auth
    {
        public    int  Signature { get; private set; }
        public string[] Category { get; private set; }
        public    UID[]     _UID { get; private set; }
        public Stream         IO { get; private set; }

        public int BINReader(string file)
        {
            Dictionary<string, object> Dict = new Dictionary<string, object>();
            string[] dataArray;

            IO = File.OpenReader(file + ".bin");

            IO.Format = Main.Format.F;
            Signature = IO.ReadInt32();
            if (Signature != 0x44334123) return 0;
            Signature = IO.ReadInt32();
            if (Signature != 0x5F5F5F41) return 0;
            IO.ReadInt64();

            string[] STRData = IO.ReadString(IO.Length - IO.Position).Replace("\r", "").Split('\n');
            for (int i = 0; i < STRData.Length; i++)
            {
                dataArray = STRData[i].Split('=');
                if (dataArray.Length == 2)
                    Dict.GetDictionary(dataArray[0], dataArray[1]);
            }

            if (Dict.FindValue(out string value, "category.length"))
            {
                Category = new string[int.Parse(value)];
                for (int i0 = 0; i0 < Category.Length; i0++)
                    if (Dict.FindValue(out value, "category." + i0 + ".value"))
                        Category[i0] = value;
            }

            if (Dict.FindValue(out value, "uid.length"))
            {
                _UID = new UID[int.Parse(value)];
                for (int i0 = 0; i0 < _UID.Length; i0++)
                {
                    Dict.FindValue(out _UID[i0].Category, "uid." + i0 + ".category");
                    Dict.FindValue(out _UID[i0].Size    , "uid." + i0 + ".size"    );
                    Dict.FindValue(out _UID[i0].Value   , "uid." + i0 + ".value"   );
                }
            }

            IO.Close();
            return 1;
        }

        public void BINWriter(string file)
        {
            IO = File.OpenWriter(file + ".bin", true);

            IO.Write("#A3DA__________\n");
            IO.Write("#", DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss yyyy",
                System.Globalization.CultureInfo.InvariantCulture));

            if (Category != null)
            {
                int[] SO = Category.Length.SortWriter();
                for (int i = 0; i < Category.Length; i++)
                    if (Category[SO[i]] != null)
                        IO.Write("category." + SO[i] + ".value=", Category[SO[i]]);
                IO.Write("category.length=", Category.Length);
            }

            if (_UID != null)
            {
                int[] SO = _UID.Length.SortWriter();
                for (int i = 0; i < _UID.Length; i++)
                {
                    if (_UID[SO[i]].Category != null)
                        if (_UID[SO[i]].Category != "")
                            IO.Write("uid." + SO[i] + ".category=", _UID[SO[i]].Category);
                    if (_UID[SO[i]].Size     != null)
                            IO.Write("uid." + SO[i] + ".size="    , _UID[SO[i]].Size    );
                    if (_UID[SO[i]].Value    != null)
                        if (_UID[SO[i]].Value    != "")
                            IO.Write("uid." + SO[i] + ".value="   , _UID[SO[i]].Value   );
                }
                IO.Write("uid.length=", _UID.Length);
            }

            IO.Close();
        }

        public void MsgPackReader(string file)
        {
            MPIO IO = new MPIO(File.OpenReader(file + ".mp"));
            MsgPack MsgPack = IO.Read();
            IO.Close();
            IO = null;

            if (MsgPack.Element("AuthDB", out MsgPack AuthDB))
            {
                if (AuthDB.Element("Category", out MsgPack Temp, typeof(object[])))
                {
                    this.Category = new string[((object[])Temp.Object).Length];
                    MsgPack Category = new MsgPack();
                    for (int i = 0; i < this.Category.Length; i++)
                        if (Temp[i].GetType() == typeof(MsgPack))
                        {
                            Category = (MsgPack)Temp[i];
                            this.Category[i] = Category.ReadString();
                        }
                }

                if (AuthDB.Element("UID", out Temp, typeof(object[])))
                {
                    _UID = new UID[((object[])Temp.Object).Length];
                    MsgPack UID = new MsgPack();
                    for (int i = 0; i < _UID.Length; i++)
                        if (Temp[i].GetType() == typeof(MsgPack))
                        {
                            UID = (MsgPack)Temp[i];
                            _UID[i].Category = UID.ReadString("C");
                            _UID[i].Size     = UID.ReadNInt32("S");
                            _UID[i].Value    = UID.ReadString("V");
                        }
                }
            }
            MsgPack = null;
        }

        public void MsgPackWriter(string file)
        {
            MsgPack AuthDB = new MsgPack("AuthDB");            
            if (Category != null)
            {
                MsgPack Category = new MsgPack("Category", this.Category.Length);
                for (int i = 0; i < this.Category.Length; i++)
                    Category[i] = this.Category[i];
                AuthDB.Add(Category);
            }

            if (_UID != null)
            {
                MsgPack UID = new MsgPack("UID", _UID.Length);
                for (int i = 0; i < _UID.Length; i++)
                    UID[i] = new MsgPack()
                        .Add("C", _UID[i].Category)
                        .Add("S", _UID[i].Size    )
                        .Add("V", _UID[i].Value   );
                AuthDB.Add(UID);
            }

            MsgPack MsgPack = new MsgPack(MsgPack.Types.FixMap).Add(AuthDB);

            MPIO IO = new MPIO(File.OpenWriter(file + ".mp", true));
            IO.Write(MsgPack, true);
            IO = null;
            MsgPack = null;
        }

        public struct UID
        {
            public int? Size;
            public string Value;
            public string Category;
        }
    }
}
