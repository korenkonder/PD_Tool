using System;
using System.Net;
using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct DataBank : IDisposable
    {
        private Stream _IO;
        private int i;

        public PvList[] pvList;
        public PsrData[] psrData;

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

            pvList = null;
            psrData = null;
            if (file.Contains("psrData") && array.Length % 13 < 2)
            {
                psrData = new PsrData[array.Length / 13];
                for (i = 0; i < psrData.Length; i++) psrData[i].SetValue(array, i);
                Success = true;
            }
            else if (file.Contains("psrData")) Success = true;
            else if (file.Contains("PvList") && array.Length % 7 < 2)
            {
                pvList = new PvList[array.Length / 7];
                for (i = 0; i < pvList.Length; i++) pvList[i].SetValue(array, i);
                Success = true;
            }
            else if (file.Contains("PvList")) Success = true;
        }

        public void DBWriter(string file, uint num2)
        {
            if (!Success) return;

            _IO = File.OpenWriter();
            if (file.Contains("psrData"))
            {
                if (psrData != null && psrData.Length > 0)
                    for (i = 0; i < psrData.Length; i++)
                        _IO.W(psrData[i].ToString() + c);
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null && pvList.Length > 0)
                    for (i = 0; i < pvList.Length; i++)
                        _IO.W(UrlEncode(pvList[i].ToString() +
                            (i < pvList.Length ? c : "")));
                else _IO.W("%2A%2A%2A");
            }

            byte[] data = _IO.ToArray(true);
            ushort num = DCC.CalculateChecksum(data);
            File.WriteAllBytes(file + "_" + num + "_" + num2 + ".dat", data);
        }

        public void MsgPackReader(string file, bool json)
        {
            Success = false;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            bool compact = msgPack.RB("Compact");

            psrData = null;
            pvList = null;
            if (file.Contains("psrData"))
            {
                MsgPack psrData;
                if ((psrData = msgPack["psrData", true]).NotNull)
                {
                    this.psrData = new PsrData[psrData.Array.Length];
                    for (i = 0; i < this.psrData.Length; i++)
                        this.psrData[i].SetValue(psrData[i]);
                }
                Success = true;
                psrData.Dispose();
            }
            else if (file.Contains("PvList"))
            {
                MsgPack pvList;
                if ((pvList = msgPack["PvList", true]).NotNull)
                {
                    this.pvList = new PvList[pvList.Array.Length];
                    for (i = 0; i < this.pvList.Length; i++)
                        this.pvList[i].SetValue(pvList[i], compact);
                }
                Success = true;
                pvList.Dispose();
            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json, bool Compact = true)
        {
            if (!Success) return;
            MsgPack msgPack = MsgPack.New;

            if (file.Contains("psrData"))
            {
                if (psrData != null)
                {
                    MsgPack psrData = new MsgPack(this.psrData.Length, "psrData");
                    for (i = 0; i < this.psrData.Length; i++) psrData[i] = this.psrData[i].WriteMP();
                    msgPack.Add(psrData);
                }
                else msgPack.Add(new MsgPack("psrData", null));
            }
            else if (file.Contains("PvList"))
            {
                if (pvList != null)
                {
                    if (Compact) msgPack.Add("Compact", Compact);

                    MsgPack PvList = new MsgPack(pvList.Length, "PvList");
                    for (i = 0; i < pvList.Length; i++) PvList[i] = pvList[i].WriteMP(Compact);
                    msgPack.Add(PvList);
                }
                else msgPack.Add(new MsgPack("PvList", null));
            }
            msgPack.Write(file, json).Dispose();
        }

        public static string UrlEncode(string value) =>
            WebUtility.UrlEncode(value).Replace("+", "%20");

        private bool disposed;
        public void Dispose()
        { if (!disposed) { if (_IO != null) _IO.Dispose(); psrData = null; pvList = null; Success = false; disposed = true; } }

        public struct PsrData
        {
            public Player p1;
            public Player p2;
            public Player p3;
            public int PV_ID;

            public void SetValue(string[] data, int i = 0)
            {
                p1.SetValue(data, i, 0);
                p2.SetValue(data, i, 1);
                p3.SetValue(data, i, 2);
                PV_ID = int.Parse(data[i * 13 + 12]);
            }

            public void SetValue(MsgPack msg)
            {
                int? id = msg.RnI32("PV_ID");
                if (id != null) PV_ID = (int)id;
                else { id = msg.RnI32("ID"); if (id != null) PV_ID = (int)id; }

                MsgPack temp;
                if ((temp = msg["P1", true]).NotNull) p1.SetValue(temp);
                if ((temp = msg["P2", true]).NotNull) p2.SetValue(temp);
                if ((temp = msg["P3", true]).NotNull) p3.SetValue(temp);
                temp.Dispose();
            }

            public MsgPack WriteMP() =>
                MsgPack.New.Add("PV_ID", PV_ID).Add(p1.WriteMP("P1"))
                     .Add(p2.WriteMP("P2")).Add(p3.WriteMP("P3"));

            public override string ToString() =>
                UrlEncode(p1.ToString() + c + p2.ToString() + c + p3.ToString() + c + PV_ID);
        }

        public struct Player
        {
            public int Score0;
            public int Score1;
            public string Name0;
            public string Name1;
            public Difficulty Diff;
            public bool Has2P => Name1 != null;

            public void SetValue(string[] data, int i = 0, int offset = 0)
            {
                string[] array = data[i * 13 + offset * 4].Split('.');
                Score0 = int.Parse(array[0]);
                if (array.Length > 1) Score1 = int.Parse(array[1]);
                string text = "";
                for (int j = 0; j < data[i * 13 + 1 + offset * 4].Length; j++)
                {
                    text += data[i * 13 + 1 + offset * 4][j].ToString();
                    if (text.EndsWith("xxx"))
                    {
                        Name0 = text.Remove(text.Length - 3);
                        text = "";
                    }
                }
                if (array.Length == 1) Name0 = text;
                else Name1 = text;
                if (int.TryParse(data[i * 13 + 2 + offset * 4], out int Diff))
                    this.Diff = (Difficulty)Diff;
                else
                    Enum.TryParse(data[i * 13 + 2 + offset * 4], out this.Diff);
            }

            public void SetValue(MsgPack msg)
            {
                 Diff  = (Difficulty)msg.RI32("Diff");
                Score0 = msg.RI32 ("Score");
                 Name0 = msg.RS( "Name");
                if (Name0 == null)
                {
                    Score0 = msg.RI32("Score0");
                    Score1 = msg.RI32("Score1");
                     Name0 = msg.RS  ( "Name0");
                     Name1 = msg.RS  ( "Name1");
                }
                else Name1 = null;
            }

            public MsgPack WriteMP(string name) =>
                Has2P ? new MsgPack(name).Add("Diff", (int)Diff).Add("Score0", Score0)
                .Add("Score1", Score1).Add("Name0", Name0).Add("Name1", Name1) :
                        new MsgPack(name).Add("Diff", (int)Diff)
                .Add("Score" , Score0).Add("Name" , Name0);

            public override string ToString() =>
                (Score0 + (Has2P ? (d + Score1) : "") + c + UrlEncode(Name0) +
                (Has2P ? ("xxx" + UrlEncode(Name1)) : "") + c +
                Diff + c + (Has2P ? "0.1" : "0")).Replace("*", "%2A");
        }

        public struct PvList
        {
            public int PV_ID;
            public bool Enable;
            public bool Extra;
            public Date AdvDemoStart;
            public Date AdvDemoEnd;
            public Date StartShow;
            public Date   EndShow;

            public void SetValue(string[] data, int i = 0)
            {
                PV_ID  = int.Parse(data[i * 7]);
                Enable = int.Parse(data[i * 7 + 1]) == 1;
                Extra  = int.Parse(data[i * 7 + 2]) == 1;
                AdvDemoStart.SV(data[i * 7 + 3]);
                AdvDemoEnd  .SV(data[i * 7 + 4]);
                StartShow   .SV(data[i * 7 + 5]);
                  EndShow   .SV(data[i * 7 + 6]);
            }

            public void SetValue(MsgPack msg, bool Compact)
            {
                Enable =  true;
                Extra  = false;

                int? id = msg.RnI32("PV_ID");
                if (id != null) PV_ID = (int)id;
                else { id = msg.RnI32("ID"); if (id != null) PV_ID = (int)id; }
                bool? enable = msg.RnB("Enable");
                bool? extra  = msg.RnB("Extra");
                if (enable != null) Enable = (bool)enable;
                if (extra  != null) Extra  = (bool)extra ;
                if (Compact)
                {
                    AdvDemoStart.SV(msg.RnI32("AdvDemoStart"),  true);
                    AdvDemoEnd  .SV(msg.RnI32("AdvDemoEnd"  ), false);
                    StartShow   .SV(msg.RnI32("StartShow"   ), false);
                      EndShow   .SV(msg.RnI32(  "EndShow"   ),  true);
                    return;
                }

                MsgPack temp;
                if ((temp = msg["AdvDemoStart", true]).NotNull) AdvDemoStart.SV(temp,  true);
                if ((temp = msg["AdvDemoEnd"  , true]).NotNull) AdvDemoEnd  .SV(temp, false);
                if ((temp = msg["StartShow"   , true]).NotNull) StartShow   .SV(temp, false);
                if ((temp = msg[  "EndShow"   , true]).NotNull)   EndShow   .SV(temp,  true);
                temp.Dispose();
            }

            public MsgPack WriteMP(bool Compact)
            {
                MsgPack msgPack = MsgPack.New;
                msgPack.Add("ID", PV_ID);
                if (!Enable) msgPack.Add("Enable", Enable);
                if ( Extra ) msgPack.Add("Extra" , Extra );
                if (Compact)
                {
                    if (AdvDemoStart.WU) msgPack.Add("AdvDemoStart", AdvDemoStart.WI());
                    if (AdvDemoEnd  .WL) msgPack.Add("AdvDemoEnd"  , AdvDemoEnd  .WI());
                    if (StartShow   .WL) msgPack.Add("StartShow"   , StartShow   .WI());
                    if (  EndShow   .WU) msgPack.Add(  "EndShow"   ,   EndShow   .WI());
                }
                else
                {
                    if (AdvDemoStart.WU) msgPack.Add(AdvDemoStart.WriteMP("AdvDemoStart"));
                    if (AdvDemoEnd  .WL) msgPack.Add(AdvDemoEnd  .WriteMP("AdvDemoEnd"  ));
                    if (StartShow   .WL) msgPack.Add(StartShow   .WriteMP("StartShow"   ));
                    if (  EndShow   .WU) msgPack.Add(  EndShow   .WriteMP(  "EndShow"   ));
                }
                return msgPack;
            }

            public override string ToString() =>
                UrlEncode(PV_ID + c + (Enable ? 1 : 0) + c + (Extra ? 1 : 0) + c +
                    AdvDemoStart.ToString() + c + AdvDemoEnd.ToString() + c +
                    StartShow.ToString() + c + EndShow.ToString());
        }

        public struct Date
        {
            private int  year;
            private int month;
            private int  day;

            public int  Year { get =>  year; set {  year = value; CheckDate(); } }

            public int Month { get => month; set { month = value; CheckDate(); } }

            public int   Day { get =>   day; set {   day = value; CheckDate(); } }

            public bool WU => Year != 2029 || Month != 1 || Day != 1;
            public bool WL => Year != 2000 || Month != 1 || Day != 1;

            public void SDL() => Year = 2000;
            public void SDU() => Year = 2029;

            public void SV(string data)
            {
                string[] array = data.Split('-');
                if (array.Length == 3)
                {
                     Year = int.Parse(array[0]);
                    Month = int.Parse(array[1]);
                      Day = int.Parse(array[2]);
                }
            }

            public void SV(int? ymd, bool setDefaultUpper)
            {
                if (!setDefaultUpper) SDL();
                else                  SDU();
                if (ymd != null)
                {
                    year  = ymd.Value / 10000;
                    month = ymd.Value / 100 % 100;
                    day   = ymd.Value % 100;
                    CheckDate();
                }
            }

            public void SV(MsgPack msg, bool setDefaultUpper)
            {
                if (!setDefaultUpper) SDL();
                else                  SDU();
                int?  Year = msg.RnI32( "Year");
                int? Month = msg.RnI32("Month");
                int?   Day = msg.RnI32(  "Day");
                if ( Year != null)  year =  Year.Value;
                if (Month != null) month = Month.Value;
                if (  Day != null)   day =   Day.Value;
                CheckDate();
            }

            public int WI() =>
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

        public enum Difficulty
        {
            Easy    = 0,
            Normal  = 1,
            Hard    = 2,
            Extreme = 3,
        }
    }
}
