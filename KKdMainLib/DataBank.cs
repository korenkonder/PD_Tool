using System;
using System.Net;
using KKdMainLib.IO;
using KKdMainLib.MessagePack;

namespace KKdMainLib
{
    public class DataBank
    {
        public DataBank() { Success = false; IO = null; pvList = null; }

        private Stream IO;
        private int i;

        private PvList[] pvList;

        private const string d = ".";
        private const string c = ",";

        public bool Success { get; private set; }

        public void DBReader(string file)
        {
            Success = false;
            if (!File.Exists(file)) return;
            string text = File.ReadAllText(file);
            while (text.Contains("%")) text = WebUtility.UrlDecode(text);
            string[] array = text.Split(',');

            if (file.Contains("PvList") && array.Length % 7 < 2)
            {
                pvList = new PvList[array.Length / 7];
                for (i = 0; i < pvList.Length; i++) pvList[i].SetValue(array, i);
                Success = true;
            }
        }

        public void DBWriter(string file)
        {
            if (!Success) return;

            IO = File.OpenWriter();
            if (file.Contains("PvList"))
            {
                if (pvList.Length != 0)
                    for (i = 0; i < pvList.Length; i++)
                        IO.Write(UrlEncode(pvList[i].ToString() +
                            ((i + 1 != pvList.Length) ? "," : "")));
                else IO.Write("%2A%2A%2A");
            }

            byte[] data = IO.ToArray(Close: true);
            ushort num = DCC.CalculateChecksum(data);
            uint num2 = (uint)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            File.WriteAllBytes(file + "_" + num + "_" + num2 + ".dat", data);
        }

        public void MsgPackReader(string file, bool JSON)
        {
            Success = false;
            MsgPack msgPack = file.ReadMP(JSON);
            bool compact = msgPack.ReadBoolean("Compact");

            if (file.Contains("PvList") && msgPack.Element<MsgPack>("PvList", out MsgPack PvList))
            {
                pvList = new PvList[PvList.Array.Length];
                for (i = 0; i < pvList.Length; i++)
                    pvList[i].SetValue((MsgPack)PvList[i], compact);
                Success = true;
            }
            msgPack = null;
        }

        public void MsgPackWriter(string file, bool JSON, bool Compact = true)
        {
            if (!Success) return;
            MsgPack msgPack = new MsgPack();

            if (file.Contains("PvList"))
            {
                if (Compact) msgPack.Add("Compact", Compact);

                MsgPack PvList = new MsgPack("PvList", pvList.Length);
                for (i = 0; i < pvList.Length; i++) PvList[i] = pvList[i].WriteMP(Compact);
                msgPack.Add(PvList);
            }
            msgPack.Write(file, JSON).Dispose();
        }

        public static string UrlEncode(string value) =>
            WebUtility.UrlEncode(value).Replace("+", "%20");

        public struct PvList
        {
            public int ID;
            public bool Enable;
            public bool Extra;
            public Date AdvDemoStart;
            public Date AdvDemoEnd;
            public Date StartShow;
            public Date   EndShow;

            public void SetValue(string[] data, int i = 0)
            {
                    ID = int.Parse(data[i * 7]);
                Enable = int.Parse(data[i * 7 + 1]) == 1;
                 Extra = int.Parse(data[i * 7 + 2]) == 1;
                AdvDemoStart.SetValue(data[i * 7 + 3]);
                AdvDemoEnd  .SetValue(data[i * 7 + 4]);
                StartShow   .SetValue(data[i * 7 + 5]);
                  EndShow   .SetValue(data[i * 7 + 6]);
            }

            public void SetValue(MsgPack msg, bool Compact)
            {
                MsgPack Temp = new MsgPack();
                this.Enable =  true;
                this.Extra  = false;

                ID = msg.ReadInt32("ID");
                bool? Enable = msg.ReadNBoolean("Enable");
                bool? Extra  = msg.ReadNBoolean("Extra");
                if (Enable.HasValue) this.Enable = (bool)Enable;
                if (Extra .HasValue) this.Extra  = (bool)Extra ;
                if (Compact)
                {
                    AdvDemoStart.SetValue(msg.ReadNInt32("AdvDemoStart"),  true);
                    AdvDemoEnd  .SetValue(msg.ReadNInt32("AdvDemoEnd"  ), false);
                    StartShow   .SetValue(msg.ReadNInt32("StartShow"   ), false);
                      EndShow   .SetValue(msg.ReadNInt32(  "EndShow"   ),  true);
                    return;
                }
                if (msg.Element("AdvDemoStart", out Temp)) AdvDemoStart.SetValue(Temp,  true);
                if (msg.Element("AdvDemoEnd"  , out Temp)) AdvDemoEnd  .SetValue(Temp, false);
                if (msg.Element("StartShow"   , out Temp)) StartShow   .SetValue(Temp, false);
                if (msg.Element(  "EndShow"   , out Temp))   EndShow   .SetValue(Temp,  true);
            }

            public MsgPack WriteMP(bool Compact)
            {
                MsgPack msgPack = new MsgPack();
                msgPack.Add("ID", ID);
                if (!Enable) msgPack.Add("Enable", Enable);
                if ( Extra ) msgPack.Add("Extra" , Extra );
                if (Compact)
                {
                    if (AdvDemoStart.WriteLower) msgPack.Add("AdvDemoStart", AdvDemoStart.WriteInt());
                    if (AdvDemoEnd  .WriteLower) msgPack.Add("AdvDemoEnd"  , AdvDemoEnd  .WriteInt());
                    if (StartShow   .WriteLower) msgPack.Add("StartShow"   , StartShow   .WriteInt());
                    if (  EndShow   .WriteUpper) msgPack.Add(  "EndShow"   ,   EndShow   .WriteInt());
                }
                else
                {
                    if (AdvDemoStart.WriteLower) msgPack.Add(AdvDemoStart.WriteMP("AdvDemoStart"));
                    if (AdvDemoEnd  .WriteLower) msgPack.Add(AdvDemoEnd  .WriteMP("AdvDemoEnd"  ));
                    if (StartShow   .WriteLower) msgPack.Add(StartShow   .WriteMP("StartShow"   ));
                    if (  EndShow   .WriteUpper) msgPack.Add(  EndShow   .WriteMP(  "EndShow"   ));
                }
                return msgPack;
            }

            public override string ToString() =>
                UrlEncode(ID + "," + (Enable ? 1 : 0) + "," + (Extra ? 1 : 0) + "," +
                    AdvDemoStart.ToString() + "," + AdvDemoEnd.ToString() + "," +
                    StartShow.ToString() + "," + EndShow.ToString());
        }

        public struct Date
        {
            private int  year;
            private int month;
            private int  day;

            public int  Year { get =>  year; set {  year = value; CheckDate(); } }

            public int Month { get => month; set { month = value; CheckDate(); } }

            public int   Day { get =>   day; set {   day = value; CheckDate(); } }

            public bool WriteUpper => Year != 2029 || Month != 1 || Day != 1;
            public bool WriteLower => Year != 2000 || Month != 1 || Day != 1;

            public void SetDefaultLower() => Year = 2000;
            public void SetDefaultUpper() => Year = 2029;

            public void SetValue(string data)
            {
                string[] array = data.Split('-');
                if (array.Length == 3)
                {
                     Year = int.Parse(array[0]);
                    Month = int.Parse(array[1]);
                      Day = int.Parse(array[2]);
                }
            }

            public void SetValue(int? YMD, bool SetDefaultUpper)
            {
                if (!SetDefaultUpper) SetDefaultLower();
                else             this.SetDefaultUpper();
                if (YMD != null)
                {
                    year  = YMD.Value / 10000;
                    month = YMD.Value / 100 % 100;
                    day   = YMD.Value % 100;
                    CheckDate();
                }
            }

            public void SetValue(MsgPack msg, bool SetDefaultUpper)
            {
                if (!SetDefaultUpper) SetDefaultLower();
                else             this.SetDefaultUpper();
                int?  Year = msg.ReadNInt32( "Year");
                int? Month = msg.ReadNInt32("Month");
                int?   Day = msg.ReadNInt32(  "Day");
                if ( Year != null)  year =  Year.Value;
                if (Month != null) month = Month.Value;
                if (  Day != null)   day =   Day.Value;
                CheckDate();
            }

            public int WriteInt() =>
                (Year * 100 + Month) * 100 + Day;

            public MsgPack WriteMP(string name) =>
                new MsgPack(name).Add("Year", Year).Add("Month", Month).Add("Day", Day);

            private void CheckDate()
            {
                if (year <  2000) { year = 2000; month = 1; day = 1; return; }
                if (year >= 2029) { year = 2029; month = 1; day = 1; return; }
                     if (month <  1) month = 1;
                else if (month > 12) month = 12;
                     if (day <  1) day = 1;
                else if (day > 31 && (month == 1 || month ==  3 || month ==  5 ||
                        month == 7 || month == 8 || month == 10 || month == 12)) day = 31;
                else if (day > 30 && (month == 4 || month ==  6 ||
                                      month == 9 || month == 11)) day = 30;
                else if (day > 29 && month == 2 && year % 4 == 0) day = 29;
                else if (day > 28 && month == 2 && year % 4 != 0) day = 28;
            }

            public override string ToString() =>
                Year.ToString("d4") + "-" + Month.ToString("d2") + "-" + Day.ToString("d2");
        }
    }
}
