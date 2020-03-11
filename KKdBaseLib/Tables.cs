namespace KKdBaseLib
{
    public struct ButtonSE
    {
        public TableDate Start;
        public TableDate End;

        public int ID;
        public int SortIndex;
        public string   Name;
        public string SEName;
    }

    public struct ChainSlideSE
    {
        public TableDate Start;
        public TableDate End;

        public int ID;
        public int SortIndex;
        public string          Name;
        public string   FirstSEName;
        public string FailureSEName;
        public string     SubSEName;
        public string SuccessSEName;
    }

    public struct CollectionCard
    {
        public TableDate Start;
        public TableDate End;

        public int DispNum;
        public int ID;
        public int NG;
        public int SortIndex;
        public int TypeParam;
        public string Chara;
        public string DivaProParam;
        public string ModuleAuthor;
        public string Name;
        public string NameReading;
        public string Type;
        public string WallParam;
    }

    public struct CustomizeItem
    {
        public TableDate ShopStart;
        public TableDate ShopEnd;

        public int    ID;
        public int ObjID;
        public int NG;
        public int SellType;
        public int ShopPrice;
        public int SortIndex;
        public string Chara;
        public string Name;
        public string Parts;
    }

    public struct Module
    {
        public TableDate ShopStart;
        public TableDate ShopEnd;

        public int Attribute;
        public int ID;
        public int NG;
        public int ShopPrice;
        public int SortIndex;
        public string Chara;
        public string Costume;
        public string Name;
    }

    public struct Plate
    {
        public int Alpha;
        public int BaseID;
        public int Border;
        public int ColorB;
        public int ColorG;
        public int ColorR;
        public int ID;
        public int Shadow;
        public int ShadowAlpha;
        public int ShadowColorB;
        public int ShadowColorG;
        public int ShadowColorR;
        public string Font;
    }

    public struct PV
    {
        public TableDate AdvStart;
        public TableDate AdvEnd;

        public int ID;
        public int Ignore;
        public string Name;
        public Difficulty[] Easy;
        public Difficulty[] Encore;
        public Difficulty[] Extreme;
        public Difficulty[] Normal;
        public Difficulty[] Hard;

        public struct Difficulty
        {
            public TableDate Start;
            public TableDate End;

            public int Edition;
            public int Version;
        }
    }

    public struct SlideSE
    {
        public TableDate Start;
        public TableDate End;

        public int ID;
        public int SortIndex;
        public string   Name;
        public string SEName;
    }

    public struct SliderTouchSE
    {
        public TableDate Start;
        public TableDate End;

        public int ID;
        public int SortIndex;
        public string   Name;
        public string SEName;
    }
    
    public struct TableDate : INull
    {
        private int  year;
        private int month;
        private int  day;
        
        public int  Year { get =>  year; set {  year = value; CheckDate(); } }
        public int Month { get => month; set { month = value; CheckDate(); } }
        public int   Day { get =>   day; set {   day = value; CheckDate(); } }

        public bool  IsNull => Year == -1 || Month == -1 || Day != -1;
        public bool NotNull => Year != -1 && Month != -1 && Day != -1;

        public bool WU => Year != 2029 || Month != 1 || Day != 1;
        public bool WL => Year != 2000 || Month != 1 || Day != 1;

        public void SN () => Year =   -1;
        public void SDL() => Year = 2000;
        public void SDU() => Year = 2029;
        
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
        
        public int Int => NotNull ? (Year * 100 + Month) * 100 + Day : -1;
        
        private void CheckDate()
        {
            if (year == -1 || month == -1 || day == -1) { year = -1; month = -1; day = -1; return; }
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
                $"{Year.ToString("d4")}-{Month.ToString("d2")}-{Day.ToString("d2")}";
    }
}
