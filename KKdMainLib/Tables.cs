using KKdBaseLib;
using KKdMainLib.IO;
using KKdBaseLib.Tables;
using TableDict = System.Collections.Generic.Dictionary<string, object>;

namespace KKdMainLib
{
    public struct Tables : System.IDisposable
    {
        private int i0;
        private Stream _IO;
        private TableDict dict;

        public      ButtonSE[]      ButtonSETable;
        public  ChainSlideSE[]  ChainSlideSETable;
        public       SlideSE[]       SlideSETable;
        public SliderTouchSE[] SliderTouchSETable;

        public CollectionCard[] CollectionCardTable;
        public  CustomizeItem[]  CustomizeItemTable;
        public         Module[]         ModuleTable;
        public          Plate[]          PlateTable;
        public             PV[]             PVList;

        private const string c = ".";

        public Tables(bool def = true)
        {
            i0 = 0;
            _IO = null;
            dict = null;
            disposed = false;
            ButtonSETable = null; ChainSlideSETable = null; SlideSETable = null; SliderTouchSETable = null;
            CollectionCardTable = null; CustomizeItemTable = null;
            ModuleTable = null; PlateTable = null; PVList = null;
        }

        public void BINReader(string file) =>
            BINReader(File.ReadAllBytes(file + ".bin"));

        public void BINReader(byte[] data)
        {
            ButtonSETable = null; ChainSlideSETable = null; SlideSETable = null; SliderTouchSETable = null;
            CollectionCardTable = null; CustomizeItemTable = null;
            ModuleTable = null; PlateTable = null; PVList = null;

            dict = new TableDict();
            string[] strData = data.ToUTF8().Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            for (i0 = 0; i0 < strData.Length; i0++)
                dict.GD(strData[i0]);
            strData = null;

            int length;
            string name;
            if (dict.FV(out length, "btn_se.data_list.length"))
            {
                ButtonSETable = new ButtonSE[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "btn_se" + c + i0 + c;

                    ref ButtonSE btn = ref ButtonSETable[i0];
                    dict.FV(out btn.    ID   , name +      "id"   );
                    dict.FV(out btn.    Name , name +      "name" );
                    dict.FV(out btn.  SEName , name +   "se_name" );
                    dict.FV(out btn.SortIndex, name + "sort_index");

                    btn.Start = GetStartTableDate(dict, name);
                    btn.End   =   GetEndTableDate(dict, name);
                }
            }
            else if (dict.FV(out length, "chainslide_se.data_list.length"))
            {
                ChainSlideSETable = new ChainSlideSE[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "chainslide_se" + c + i0 + c;

                    ref ChainSlideSE csld = ref ChainSlideSETable[i0];
                    dict.FV(out csld.         ID   , name +            "id"   );
                    dict.FV(out csld.         Name , name +            "name" );
                    dict.FV(out csld.  FirstSEName , name +   "first_se_name" );
                    dict.FV(out csld.FailureSEName , name + "failure_se_name" );
                    dict.FV(out csld.     SortIndex, name +       "sort_index");
                    dict.FV(out csld.    SubSEName , name +     "sub_se_name" );
                    dict.FV(out csld.SuccessSEName , name + "success_se_name" );

                    csld.Start = GetStartTableDate(dict, name);
                    csld.End   =   GetEndTableDate(dict, name);
                }
            }
            else if (dict.FV(out length, "collection_card.data_list.length"))
            {
                CollectionCardTable = new CollectionCard[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "collection_card" + c + i0 + c;

                    ref CollectionCard clcrd = ref CollectionCardTable[i0];
                    dict.FV(out clcrd.Chara       , name + "chara"        );
                    dict.FV(out clcrd.DispNum     , name + "disp_num"     );
                    dict.FV(out clcrd.DivaProParam, name + "divapro_param");
                    dict.FV(out clcrd.ID          , name + "id"           );
                    dict.FV(out clcrd.ModuleAuthor, name + "module_author");
                    dict.FV(out clcrd.Name        , name + "name"         );
                    dict.FV(out clcrd.NameReading , name + "name_reading" );
                    dict.FV(out clcrd.NG          , name + "ng"           );
                    dict.FV(out clcrd.SortIndex   , name + "sort_idx"     );
                    dict.FV(out clcrd.Type        , name + "type"         );
                    dict.FV(out clcrd.TypeParam   , name + "type_param"   );
                    dict.FV(out clcrd.WallParam   , name + "wall_param"   );

                    clcrd.Start = GetStartTableDate(dict, name);
                    clcrd.End   =   GetEndTableDate(dict, name);
                }
            }
            else if (dict.FV(out length, "cstm_item.data_list.length"))
            {
                CustomizeItemTable = new CustomizeItem[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "cstm_item" + c + i0 + c;

                    ref CustomizeItem czitm = ref CustomizeItemTable[i0];
                    dict.FV(out czitm.Chara    , name + "chara"     );
                    dict.FV(out czitm.ID       , name + "id"        );
                    dict.FV(out czitm.Name     , name + "name"      );
                    dict.FV(out czitm.NG       , name + "ng"        );
                    dict.FV(out czitm.ObjID    , name + "obj_id"    );
                    dict.FV(out czitm.Parts    , name + "parts"     );
                    dict.FV(out czitm.SellType , name + "sell_type" );
                    dict.FV(out czitm.ShopPrice, name + "shop_price");
                    dict.FV(out czitm.SortIndex, name + "sort_index");

                    czitm.ShopStart = GetStartTableDate(dict, name + "shop_");
                    czitm.ShopEnd   =   GetEndTableDate(dict, name + "shop_");
                }
            }
            else if (dict.FV(out length, "module.data_list.length"))
            {
                ModuleTable = new Module[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "module" + c + i0 + c;

                    ref Module mdl = ref ModuleTable[i0];
                    dict.FV(out mdl.Attribute, name + "attr"      );
                    dict.FV(out mdl.Chara    , name + "chara"     );
                    dict.FV(out mdl.Costume  , name + "cos"       );
                    dict.FV(out mdl.ID       , name + "id"        );
                    dict.FV(out mdl.Name     , name + "name"      );
                    dict.FV(out mdl.NG       , name + "ng"        );
                    dict.FV(out mdl.ShopPrice, name + "shop_price");
                    dict.FV(out mdl.SortIndex, name + "sort_index");

                    mdl.ShopStart = GetStartTableDate(dict, name + "shop_");
                    mdl.ShopEnd   =   GetEndTableDate(dict, name + "shop_");
                }
            }
            else if (dict.FV(out length, "plate.data_list.length"))
            {
                PlateTable = new Plate[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "plate" + c + i0 + c;

                    ref Plate plt = ref PlateTable[i0];
                    dict.FV(out plt.Alpha       , name + "alpha"         );
                    dict.FV(out plt.BaseID      , name + "base_id"       );
                    dict.FV(out plt.Border      , name + "border"        );
                    dict.FV(out plt.ColorB      , name + "color_b"       );
                    dict.FV(out plt.ColorG      , name + "color_g"       );
                    dict.FV(out plt.ColorR      , name + "color_r"       );
                    dict.FV(out plt.Font        , name + "font"          );
                    dict.FV(out plt.ID          , name + "id"            );
                    dict.FV(out plt.Shadow      , name + "shadow"        );
                    dict.FV(out plt.ShadowAlpha , name + "shadow_alpha"  );
                    dict.FV(out plt.ShadowColorB, name + "shadow_color_b");
                    dict.FV(out plt.ShadowColorG, name + "shadow_color_g");
                    dict.FV(out plt.ShadowColorR, name + "shadow_color_r");
                }
            }
            else if (dict.FV(out length, "pv_list.data_list.length"))
            {
                PVList = new PV[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "pv_list" + c + i0 + c;

                    ref PV pv = ref PVList[i0];
                    dict.FV(out pv.ID    , name + "id"    );
                    dict.FV(out pv.Ignore, name + "ignore");
                    dict.FV(out pv.Name  , name + "name"  );

                    pv.AdvStart = GetStartTableDate(dict, name + "adv_demo_");
                    pv.AdvEnd   =   GetEndTableDate(dict, name + "adv_demo_");

                    pv.Easy    = GetDifficulty(dict, name + "easy"    + c);
                    pv.Encore  = GetDifficulty(dict, name + "encore"  + c);
                    pv.Extreme = GetDifficulty(dict, name + "extreme" + c);
                    pv.Hard    = GetDifficulty(dict, name + "hard"    + c);
                    pv.Normal  = GetDifficulty(dict, name + "normal"  + c);
                }

                static PV.Difficulty[] GetDifficulty(TableDict dict, string name)
                {
                    int len, val;
                    if (!dict.FV(out len, name + "length") || len < 0) return null;

                    PV.Difficulty[] arr = new PV.Difficulty[len];
                    for (int i = 0; i < len; i++)
                    {
                        if (dict.FV(out val, $"{name}{i}.edition")) arr[i].Edition = val;
                        if (dict.FV(out val, $"{name}{i}.ver"    )) arr[i].Version = val;

                        arr[i].Start = GetStartTableDate(dict, $"{name}{i}.");
                        arr[i].End   =   GetEndTableDate(dict, $"{name}{i}.");
                    }
                    return arr;
                }
            }
            else if (dict.FV(out length, "slide_se.data_list.length"))
            {
                SlideSETable = new SlideSE[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "slide_se" + c + i0 + c;

                    ref SlideSE sld = ref SlideSETable[i0];
                    dict.FV(out sld.    ID   , name +      "id"   );
                    dict.FV(out sld.    Name , name +      "name" );
                    dict.FV(out sld.  SEName , name +   "se_name" );
                    dict.FV(out sld.SortIndex, name + "sort_index");

                    sld.Start = GetStartTableDate(dict, name);
                    sld.End   =   GetEndTableDate(dict, name);
                }
            }
            else if (dict.FV(out length, "slidertouch_se.data_list.length"))
            {
                SliderTouchSETable = new SliderTouchSE[length];
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "slidertouch_se" + c + i0 + c;

                    ref SliderTouchSE stld = ref SliderTouchSETable[i0];
                    dict.FV(out stld.    ID   , name +      "id"   );
                    dict.FV(out stld.    Name , name +      "name" );
                    dict.FV(out stld.  SEName , name +   "se_name" );
                    dict.FV(out stld.SortIndex, name + "sort_index");

                    stld.Start = GetStartTableDate(dict, name);
                    stld.End   =   GetEndTableDate(dict, name);
                }
            }

            static TableDate GetStartTableDate(TableDict dict, string name)
            {
                TableDate date = default; date.SDL(); int val;
                if (dict.FV(out val, name + "st_year" )) date.Year  = val;
                if (dict.FV(out val, name + "st_month")) date.Month = val;
                if (dict.FV(out val, name + "st_day"  )) date.Day   = val;
                return date;
            }

            static TableDate GetEndTableDate(TableDict dict, string name)
            {
                TableDate date = default; date.SDU(); int val;
                if (dict.FV(out val, name + "ed_year" )) date.Year  = val;
                if (dict.FV(out val, name + "ed_month")) date.Month = val;
                if (dict.FV(out val, name + "ed_day"  )) date.Day   = val;
                return date;
            }
        }

        public void BINWriter(string file)
        {
            int[] so;
            int length;
            string name;

            if (ButtonSETable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = ButtonSETable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "btn_se" + c + so[i0] + c;

                    ref ButtonSE btn = ref ButtonSETable[so[i0]];
                    WriteEndTableDate(_IO, name, btn.End);
                    W(name +      "id"   , btn.    ID   );
                    W(name +      "name" , btn.    Name );
                    W(name +   "se_name" , btn.  SEName );
                    W(name + "sort_index", btn.SortIndex);
                    WriteStartTableDate(_IO, name, btn.Start);
                }
                W("btn_se.data_list.length", length);
                _IO.W("kind=0\n");
                _IO.D();
            }
            else if (ChainSlideSETable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = ChainSlideSETable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "chainslide_se" + c + so[i0] + c;

                    ref ChainSlideSE csld = ref ChainSlideSETable[so[i0]];
                    WriteEndTableDate(_IO, name, csld.End);
                    W(name + "failure_se_name" , csld.FailureSEName );
                    W(name +   "first_se_name" , csld.  FirstSEName );
                    W(name +            "id"   , csld.         ID   );
                    W(name +            "name" , csld.         Name );
                    W(name +       "sort_index", csld.     SortIndex);
                    WriteStartTableDate(_IO, name, csld.Start);
                    W(name +     "sub_se_name" , csld.    SubSEName );
                    W(name + "success_se_name" , csld.SuccessSEName );
                }
                W("chainslide_se.data_list.length", length);
                _IO.W("kind=2\n");
                _IO.D();
            }
            else if (CollectionCardTable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = CollectionCardTable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "collection_card" + c + so[i0] + c;

                    ref CollectionCard clcrd = ref CollectionCardTable[so[i0]];
                    W(name + "chara"        , clcrd.Chara       );
                    W(name + "disp_num"     , clcrd.DispNum     );
                    W(name + "divapro_param", clcrd.DivaProParam);
                    WriteEndTableDate(_IO, name, clcrd.End);
                    W(name + "id"           , clcrd.ID          );
                    W(name + "module_author", clcrd.ModuleAuthor);
                    W(name + "name"         , clcrd.Name        );
                    W(name + "name_reading" , clcrd.NameReading );
                    W(name + "ng"           , clcrd.NG          );
                    W(name + "sort_idx"     , clcrd.SortIndex   );
                    WriteStartTableDate(_IO, name, clcrd.Start);
                    W(name + "type"         , clcrd.Type        );
                    W(name + "type_param"   , clcrd.TypeParam   );
                    W(name + "wall_param"   , clcrd.WallParam   );
                }
                W("collection_card.data_list.length", length);
                _IO.W("patch=0\n");
                _IO.W("version=0\n");
                _IO.D();
            }
            else if (CustomizeItemTable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = CustomizeItemTable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "cstm_item" + c + so[i0] + c;

                    ref CustomizeItem czitm = ref CustomizeItemTable[so[i0]];
                    W(name + "chara"     , czitm.Chara    );
                    W(name + "id"        , czitm.ID       );
                    W(name + "name"      , czitm.Name     );
                    W(name + "ng"        , czitm.NG       );
                    W(name + "obj_id"    , czitm.ObjID    );
                    W(name + "parts"     , czitm.Parts    );
                    W(name + "sell_type" , czitm.SellType );
                    WriteEndTableDate(_IO, name + "shop_", czitm.ShopEnd);
                    W(name + "shop_price", czitm.ShopPrice);
                    WriteStartTableDate(_IO, name + "shop_", czitm.ShopStart);
                    W(name + "sort_index", czitm.SortIndex);
                }
                W("cstm_item.data_list.length", length);
                _IO.W("patch=0\n");
                _IO.W("version=0\n");
                _IO.D();
            }
            else if (ModuleTable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = ModuleTable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "module" + c + so[i0] + c;

                    ref Module mdl = ref ModuleTable[so[i0]];
                    W(name + "attr"      , mdl.Attribute);
                    W(name + "chara"     , mdl.Chara    );
                    W(name + "cos"       , mdl.Costume  );
                    W(name + "id"        , mdl.ID       );
                    W(name + "name"      , mdl.Name     );
                    W(name + "ng"        , mdl.NG       );
                    WriteEndTableDate(_IO, name + "shop_", mdl.ShopEnd);
                    W(name + "shop_price", mdl.ShopPrice);
                    WriteStartTableDate(_IO, name + "shop_", mdl.ShopStart);
                    W(name + "sort_index", mdl.SortIndex);
                }
                W("module.data_list.length", length);
                _IO.D();
            }
            else if (PlateTable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = PlateTable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "plate" + c + so[i0] + c;

                    ref Plate plt = ref PlateTable[so[i0]];
                    W(name + "alpha"         , plt.Alpha       );
                    W(name + "base_id"       , plt.BaseID      );
                    W(name + "border"        , plt.Border      );
                    W(name + "color_b"       , plt.ColorB      );
                    W(name + "color_g"       , plt.ColorG      );
                    W(name + "color_r"       , plt.ColorR      );
                    W(name + "font"          , plt.Font        );
                    W(name + "id"            , plt.ID          );
                    W(name + "shadow"        , plt.Shadow      );
                    W(name + "shadow_alpha"  , plt.ShadowAlpha );
                    W(name + "shadow_color_b", plt.ShadowColorB);
                    W(name + "shadow_color_g", plt.ShadowColorG);
                    W(name + "shadow_color_r", plt.ShadowColorR);
                }
                W("plate.data_list.length", length);
                _IO.D();
            }
            else if (PVList != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = PVList.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "pv_list" + c + so[i0] + c;

                    ref PV pv = ref PVList[so[i0]];
                      WriteEndTableDate(_IO, name + "adv_demo_", pv.AdvEnd  );
                    WriteStartTableDate(_IO, name + "adv_demo_", pv.AdvStart);

                    WriteDifficulty(_IO, name +    "easy.", pv.Easy   );
                    WriteDifficulty(_IO, name +  "encore.", pv.Encore );
                    WriteDifficulty(_IO, name + "extreme.", pv.Extreme);
                    WriteDifficulty(_IO, name +    "hard.", pv.Hard   );
                    W(name + "id"    , pv.ID    );
                    W(name + "ignore", pv.Ignore);
                    W(name + "name"  , pv.Name  );
                    WriteDifficulty(_IO, name +  "normal.", pv.Normal );
                }
                W("pv_list.data_list.length", length);
                _IO.D();

                static void WriteDifficulty(Stream _IO, string name, PV.Difficulty[] arr)
                {
                    if (arr == null) { _IO.W($"{name}length=0\n"); return; }

                    int length = arr.Length;
                    int[] so = length.SW();
                    for (int i = 0; i < length; i++)
                    {
                        ref PV.Difficulty diff = ref arr[so[i]];

                          WriteEndTableDate(_IO, $"{name}{i}.", diff.End  );
                        _IO.W($"{name}{i}.edition" + $"={diff.Edition}\n");
                        WriteStartTableDate(_IO, $"{name}{i}.", diff.Start);
                        _IO.W($"{name}{i}.ver"     + $"={diff.Version}\n");
                    }
                    _IO.W($"{name}length={length}\n");
                }
            }
            else if (SlideSETable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = SlideSETable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                _IO.W("kind=1\n");
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "slide_se" + c + so[i0] + c;

                    ref SlideSE sld = ref SlideSETable[so[i0]];
                    WriteEndTableDate(_IO, name, sld.End);
                    W(name +      "id"   , sld.    ID   );
                    W(name +      "name" , sld.    Name );
                    W(name +   "se_name" , sld.  SEName );
                    W(name + "sort_index", sld.SortIndex);
                    WriteStartTableDate(_IO, name, sld.Start);
                }
                W("slide_se.data_list.length", length);
                _IO.D();
            }
            else if (SliderTouchSETable != null)
            {
                _IO = File.OpenWriter(file + ".bin", true);
                length = SliderTouchSETable.Length;
                so = length.SW();
                WriteEmptyLines(_IO, length);
                _IO.W("kind=3\n");
                for (i0 = 0; i0 < length; i0++)
                {
                    name = "slidertouch_se" + c + so[i0] + c;

                    ref SliderTouchSE sldt = ref SliderTouchSETable[so[i0]];
                    WriteEndTableDate(_IO, name, sldt.End);
                    W(name +      "id"   , sldt.    ID   );
                    W(name +      "name" , sldt.    Name );
                    W(name +   "se_name" , sldt.  SEName );
                    W(name + "sort_index", sldt.SortIndex);
                    WriteStartTableDate(_IO, name, sldt.Start);
                }
                W("slidertouch_se.data_list.length", length);
                _IO.D();
            }

            static void WriteEmptyLines(Stream _IO, int count)
            { for (int i = 0; i < count; i++) _IO.W("#---------------------------------------------\n"); }

            static void WriteStartTableDate(Stream _IO, string name, TableDate date)
            {
                _IO.W(name + "st_day"   + "=" + date.Day   + "\n");
                _IO.W(name + "st_month" + "=" + date.Month + "\n");
                _IO.W(name + "st_year"  + "=" + date.Year  + "\n");
            }

            static void WriteEndTableDate(Stream _IO, string name, TableDate date)
            {
                _IO.W(name + "ed_day"   + "=" + date.Day   + "\n");
                _IO.W(name + "ed_month" + "=" + date.Month + "\n");
                _IO.W(name + "ed_year"  + "=" + date.Year  + "\n");
            }
        }

        private void W(string Data,   long  val) =>
                           _IO.W(Data + "=" + val + "\n");
        private void W(string Data, string  val)
        { if (val != null) _IO.W(Data + "=" + val + "\n"); }

        public void MsgPackReader(string file, bool json)
        {
            ButtonSETable = null; ChainSlideSETable = null; SlideSETable = null; SliderTouchSETable = null;
            CollectionCardTable = null; CustomizeItemTable = null;
            ModuleTable = null; PlateTable = null; PVList = null;

            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            MsgPack temp;
            if ((temp = msgPack["ButtonSETable", true]).NotNull)
            {
                ButtonSETable = new ButtonSE[temp.Array.Length];
                for (i0 = 0; i0 < ButtonSETable.Length; i0++)
                {
                    MsgPack btnMP = temp[i0];
                    ref ButtonSE btn = ref ButtonSETable[i0];
                    btn.End  .SV(btnMP.RnI32("End"  ),  true);
                    btn.Start.SV(btnMP.RnI32("Start"), false);

                    btnMP.R(    "ID"   , out btn.    ID   );
                    btnMP.R(    "Name" , out btn.    Name );
                    btnMP.R(  "SEName" , out btn.  SEName );
                    btnMP.R("SortIndex", out btn.SortIndex);
                }
            }
            else if ((temp = msgPack["ChainSlideSETable", true]).NotNull)
            {
                ChainSlideSETable = new ChainSlideSE[temp.Array.Length];
                for (i0 = 0; i0 < ChainSlideSETable.Length; i0++)
                {
                    MsgPack csldMP = temp[i0];
                    ref ChainSlideSE csld = ref ChainSlideSETable[i0];
                    csld.End  .SV(csldMP.RnI32("End"  ),  true);
                    csld.Start.SV(csldMP.RnI32("Start"), false);

                    csldMP.R("FailureSEName" , out csld.FailureSEName );
                    csldMP.R(  "FirstSEName" , out csld.  FirstSEName );
                    csldMP.R(         "ID"   , out csld.         ID   );
                    csldMP.R(         "Name" , out csld.         Name );
                    csldMP.R(     "SortIndex", out csld.     SortIndex);
                    csldMP.R(    "SubSEName" , out csld.    SubSEName );
                    csldMP.R("SuccessSEName" , out csld.SuccessSEName );
                }
            }
            else if ((temp = msgPack["CollectionCardTable", true]).NotNull)
            {
                CollectionCardTable = new CollectionCard[temp.Array.Length];
                for (i0 = 0; i0 < CollectionCardTable.Length; i0++)
                {
                    MsgPack clcrdMP = temp[i0];
                    ref CollectionCard clcrd = ref CollectionCardTable[i0];
                    clcrd.End  .SV(clcrdMP.RnI32("End"  ),  true);
                    clcrd.Start.SV(clcrdMP.RnI32("Start"), false);

                    clcrdMP.R("Chara"       , out clcrd.Chara       );
                    clcrdMP.R("DispNum"     , out clcrd.DispNum     );
                    clcrdMP.R("DivaProParam", out clcrd.DivaProParam);
                    clcrdMP.R("ID"          , out clcrd.ID          );
                    clcrdMP.R("ModuleAuthor", out clcrd.ModuleAuthor);
                    clcrdMP.R("Name"        , out clcrd.Name        );
                    clcrdMP.R("NameReading" , out clcrd.NameReading );
                    clcrdMP.R("NG"          , out clcrd.NG          );
                    clcrdMP.R("SortIndex"   , out clcrd.SortIndex   );
                    clcrdMP.R("Type"        , out clcrd.Type        );
                    clcrdMP.R("TypeParam"   , out clcrd.TypeParam   );
                    clcrdMP.R("WallParam"   , out clcrd.WallParam   );
                }
            }
            else if ((temp = msgPack["CustomizeItemTable", true]).NotNull)
            {
                CustomizeItemTable = new CustomizeItem[temp.Array.Length];
                for (i0 = 0; i0 < CustomizeItemTable.Length; i0++)
                {
                    MsgPack czitmMP = temp[i0];
                    ref CustomizeItem czitm = ref CustomizeItemTable[i0];
                    czitm.ShopEnd  .SV(czitmMP.RnI32("ShopEnd"  ),  true);
                    czitm.ShopStart.SV(czitmMP.RnI32("ShopStart"), false);

                    czitmMP.R("Chara"    , out czitm.Chara    );
                    czitmMP.R("ID"       , out czitm.ID       );
                    czitmMP.R("Name"     , out czitm.Name     );
                    czitmMP.R("NG"       , out czitm.NG       );
                    czitmMP.R("ObjID"    , out czitm.ObjID    );
                    czitmMP.R("Parts"    , out czitm.Parts    );
                    czitmMP.R("SellType" , out czitm.SellType );
                    czitmMP.R("ShopPrice", out czitm.ShopPrice);
                    czitmMP.R("SortIndex", out czitm.SortIndex);
                }
            }
            else if ((temp = msgPack["ModuleTable", true]).NotNull)
            {
                ModuleTable = new Module[temp.Array.Length];
                for (i0 = 0; i0 < ModuleTable.Length; i0++)
                {
                    MsgPack mdlMP = temp[i0];
                    ref Module mdl = ref ModuleTable[i0];
                    mdl.ShopEnd  .SV(mdlMP.RnI32("ShopEnd"  ),  true);
                    mdl.ShopStart.SV(mdlMP.RnI32("ShopStart"), false);

                    mdlMP.R("Attribute", out mdl.Attribute);
                    mdlMP.R("Costume"  , out mdl.Costume  );
                    mdlMP.R("Chara"    , out mdl.Chara    );
                    mdlMP.R("ID"       , out mdl.ID       );
                    mdlMP.R("Name"     , out mdl.Name     );
                    mdlMP.R("NG"       , out mdl.NG       );
                    mdlMP.R("ShopPrice", out mdl.ShopPrice);
                    mdlMP.R("SortIndex", out mdl.SortIndex);
                }
            }
            else if ((temp = msgPack["PlateTable", true]).NotNull)
            {
                PlateTable = new Plate[temp.Array.Length];
                for (i0 = 0; i0 < PlateTable.Length; i0++)
                {
                    MsgPack pltMP = temp[i0];
                    ref Plate plt = ref PlateTable[i0];
                    pltMP.R("Alpha"       , out plt.Alpha       );
                    pltMP.R("BaseID"      , out plt.BaseID      );
                    pltMP.R("Border"      , out plt.Border      );
                    pltMP.R("ColorB"      , out plt.ColorB      );
                    pltMP.R("ColorG"      , out plt.ColorG      );
                    pltMP.R("ColorR"      , out plt.ColorR      );
                    pltMP.R("Font"        , out plt.Font        );
                    pltMP.R("ID"          , out plt.ID          );
                    pltMP.R("Shadow"      , out plt.Shadow      );
                    pltMP.R("ShadowAlpha" , out plt.ShadowAlpha );
                    pltMP.R("ShadowColorB", out plt.ShadowColorB);
                    pltMP.R("ShadowColorG", out plt.ShadowColorG);
                    pltMP.R("ShadowColorR", out plt.ShadowColorR);
                }
            }
            else if ((temp = msgPack["PVList", true]).NotNull)
            {
                PVList = new PV[temp.Array.Length];
                for (i0 = 0; i0 < PVList.Length; i0++)
                {
                    MsgPack pvMP = temp[i0];
                    ref PV pv = ref PVList[i0];
                    pv.AdvEnd  .SV(pvMP.RnI32("AdvEnd"  ),  true);
                    pv.AdvStart.SV(pvMP.RnI32("AdvStart"), false);

                    pvMP.R("ID"    , out pv.ID    );
                    pvMP.R("Ignore", out pv.Ignore);
                    pvMP.R("Name"  , out pv.Name  );

                    pv.Easy    = GetDifficulty(pvMP, "Easy"   );
                    pv.Normal  = GetDifficulty(pvMP, "Normal" );
                    pv.Hard    = GetDifficulty(pvMP, "Hard"   );
                    pv.Extreme = GetDifficulty(pvMP, "Extreme");
                    pv.Encore  = GetDifficulty(pvMP, "Encore" );
                }

                static PV.Difficulty[] GetDifficulty(MsgPack mp, string name)
                {
                    MsgPack t;
                    if ((t = mp[name, true]).IsNull) return null;

                    PV.Difficulty[] arr = new PV.Difficulty[t.Array.Length];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        MsgPack pvMP = t[i];
                        ref PV.Difficulty diff = ref arr[i];
                        diff.End  .SV(pvMP.RnI32("End"  ),  true);
                        diff.Start.SV(pvMP.RnI32("Start"), false);

                        pvMP.R("Edition", out diff.Edition);
                        pvMP.R("Version", out diff.Version);
                    }
                    return arr;
                }
            }
            else if ((temp = msgPack["SlideSETable", true]).NotNull)
            {
                SlideSETable = new SlideSE[temp.Array.Length];
                for (i0 = 0; i0 < SlideSETable.Length; i0++)
                {
                    MsgPack sldMP = temp[i0];
                    ref SlideSE sld = ref SlideSETable[i0];
                    sld.End  .SV(sldMP.RnI32("End"  ),  true);
                    sld.Start.SV(sldMP.RnI32("Start"), false);

                    sldMP.R(    "ID"   , out sld.    ID   );
                    sldMP.R(    "Name" , out sld.    Name );
                    sldMP.R(  "SEName" , out sld.  SEName );
                    sldMP.R("SortIndex", out sld.SortIndex);
                }
            }
            else if ((temp = msgPack["SliderTouchSETable", true]).NotNull)
            {
                SliderTouchSETable = new SliderTouchSE[temp.Array.Length];
                for (i0 = 0; i0 < SliderTouchSETable.Length; i0++)
                {
                    MsgPack TEMP = temp[i0];
                    ref SliderTouchSE sldt = ref SliderTouchSETable[i0];
                    sldt.End  .SV(TEMP.RnI32("End"  ),  true);
                    sldt.Start.SV(TEMP.RnI32("Start"), false);

                    TEMP.R(    "ID"   , out sldt.    ID   );
                    TEMP.R(    "Name" , out sldt.    Name );
                    TEMP.R(  "SEName" , out sldt.  SEName );
                    TEMP.R("SortIndex", out sldt.SortIndex);
                }
            }
            temp.Dispose();
            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            MsgPack msgPack = MsgPack.New;
            if (ButtonSETable != null)
            {
                MsgPack buttonSETable = new MsgPack(ButtonSETable.Length, "ButtonSETable");
                for (i0 = 0; i0 < ButtonSETable.Length; i0++)
                {
                    ref ButtonSE btn = ref ButtonSETable[i0];
                    buttonSETable[i0] = MsgPack.New.Add(    "End"  , btn.End.Int  )
                                                   .Add(    "ID"   , btn.    ID   )
                                                   .Add(    "Name" , btn.    Name )
                                                   .Add(  "SEName" , btn.  SEName )
                                                   .Add("SortIndex", btn.SortIndex)
                                                   .Add(    "Start", btn.Start.Int);
                }
                msgPack.Add(buttonSETable);
            }
            else if (ChainSlideSETable != null)
            {
                MsgPack chainSlideSETable = new MsgPack(ChainSlideSETable.Length, "ChainSlideSETable");
                for (i0 = 0; i0 < ChainSlideSETable.Length; i0++)
                {
                    ref ChainSlideSE csld = ref ChainSlideSETable[i0];
                    chainSlideSETable[i0] = MsgPack.New.Add(         "End"  , csld.     End.Int  )
                                                       .Add("FailureSEName" , csld.FailureSEName )
                                                       .Add(  "FirstSEName" , csld.  FirstSEName )
                                                       .Add(         "ID"   , csld.         ID   )
                                                       .Add(         "Name" , csld.         Name )
                                                       .Add(    "SubSEName" , csld.    SubSEName )
                                                       .Add("SuccessSEName" , csld.SuccessSEName )
                                                       .Add(     "SortIndex", csld.     SortIndex)
                                                       .Add(         "Start", csld.     Start.Int);
                }
                msgPack.Add(chainSlideSETable);
            }
            else if (CollectionCardTable != null)
            {
                MsgPack collectionCardTable = new MsgPack(CollectionCardTable.Length, "CollectionCardTable");
                for (i0 = 0; i0 < CollectionCardTable.Length; i0++)
                {
                    ref CollectionCard clcrd = ref CollectionCardTable[i0];
                    collectionCardTable[i0] = MsgPack.New.Add("Chara"       , clcrd.Chara       )
                                                         .Add("DispNum"     , clcrd.DispNum     )
                                                         .Add("DivaProParam", clcrd.DivaProParam)
                                                         .Add("End"         , clcrd.End.Int     )
                                                         .Add("ID"          , clcrd.ID          )
                                                         .Add("ModuleAuthor", clcrd.ModuleAuthor)
                                                         .Add("Name"        , clcrd.Name        )
                                                         .Add("NameReading" , clcrd.NameReading )
                                                         .Add("NG"          , clcrd.NG          )
                                                         .Add("SortIndex"   , clcrd.SortIndex   )
                                                         .Add("Start"       , clcrd.Start.Int   )
                                                         .Add("Type"        , clcrd.Type        )
                                                         .Add("TypeParam"   , clcrd.TypeParam   )
                                                         .Add("WallParam"   , clcrd.WallParam   );
                }
                msgPack.Add(collectionCardTable);
            }
            else if (CustomizeItemTable != null)
            {
                MsgPack customizeItemTable = new MsgPack(CustomizeItemTable.Length, "CustomizeItemTable");
                for (i0 = 0; i0 < CustomizeItemTable.Length; i0++)
                {
                    ref CustomizeItem czitm = ref CustomizeItemTable[i0];
                    customizeItemTable[i0] = MsgPack.New.Add("Chara"    , czitm.Chara        )
                                                        .Add("ID"       , czitm.ID           )
                                                        .Add("Name"     , czitm.Name         )
                                                        .Add("NG"       , czitm.NG           )
                                                        .Add("ObjID"    , czitm.ObjID        )
                                                        .Add("Parts"    , czitm.Parts        )
                                                        .Add("SellType" , czitm.SellType     )
                                                        .Add("ShopEnd"  , czitm.ShopEnd  .Int)
                                                        .Add("ShopPrice", czitm.ShopPrice    )
                                                        .Add("ShopStart", czitm.ShopStart.Int)
                                                        .Add("SortIndex", czitm.SortIndex    );
                }
                msgPack.Add(customizeItemTable);
            }
            else if (ModuleTable != null)
            {
                MsgPack moduleTable = new MsgPack(ModuleTable.Length, "ModuleTable");
                for (i0 = 0; i0 < ModuleTable.Length; i0++)
                {
                    ref Module mdl = ref ModuleTable[i0];
                    moduleTable[i0] = MsgPack.New.Add("Attribute", mdl.Attribute    )
                                                 .Add("Chara"    , mdl.Chara        )
                                                 .Add("Costume"  , mdl.Costume      )
                                                 .Add("ID"       , mdl.ID           )
                                                 .Add("Name"     , mdl.Name         )
                                                 .Add("NG"       , mdl.NG           )
                                                 .Add("ShopEnd"  , mdl.ShopEnd  .Int)
                                                 .Add("ShopPrice", mdl.ShopPrice    )
                                                 .Add("ShopStart", mdl.ShopStart.Int)
                                                 .Add("SortIndex", mdl.SortIndex    );
                }
                msgPack.Add(moduleTable);
            }
            else if (PlateTable != null)
            {
                MsgPack plateTable = new MsgPack(PlateTable.Length, "PlateTable");
                for (i0 = 0; i0 < PlateTable.Length; i0++)
                {
                    ref Plate plt = ref PlateTable[i0];
                    plateTable[i0] = MsgPack.New.Add("Alpha"       , plt.Alpha       )
                                                .Add("BaseID"      , plt.BaseID      )
                                                .Add("Border"      , plt.Border      )
                                                .Add("ColorB"      , plt.ColorB      )
                                                .Add("ColorG"      , plt.ColorG      )
                                                .Add("ColorR"      , plt.ColorR      )
                                                .Add("Font"        , plt.Font        )
                                                .Add("ID"          , plt.ID          )
                                                .Add("Shadow"      , plt.Shadow      )
                                                .Add("ShadowAlpha" , plt.ShadowAlpha )
                                                .Add("ShadowColorB", plt.ShadowColorB)
                                                .Add("ShadowColorG", plt.ShadowColorG)
                                                .Add("ShadowColorR", plt.ShadowColorR);
                }
                msgPack.Add(plateTable);
            }
            else if (PVList != null)
            {
                MsgPack pvList = new MsgPack(PVList.Length, "PVList");
                for (i0 = 0; i0 < PVList.Length; i0++)
                {
                    ref PV pv = ref PVList[i0];
                    MsgPack mp = MsgPack.New.Add("AdvEnd"  , pv.AdvEnd  .Int)
                                            .Add("AdvStart", pv.AdvStart.Int);

                    mp = mp.Add("ID"    , pv.ID    )
                           .Add("Name"  , pv.Name  )
                           .Add("Ignore", pv.Ignore);

                    mp.Add(WriteDifficulty("Easy"   , pv.Easy   ));
                    mp.Add(WriteDifficulty("Normal" , pv.Normal ));
                    mp.Add(WriteDifficulty("Hard"   , pv.Hard   ));
                    mp.Add(WriteDifficulty("Extreme", pv.Extreme));
                    mp.Add(WriteDifficulty("Encore" , pv.Encore ));
                    pvList[i0] = mp;
                }
                msgPack.Add(pvList);

                static MsgPack WriteDifficulty(string name, PV.Difficulty[] arr)
                {
                    if (arr == null) return default;
                    MsgPack mp = new MsgPack(arr.Length, name);
                    for (int i = 0; i < arr.Length; i++)
                    {
                        ref PV.Difficulty diff = ref arr[i];
                        mp[i] = MsgPack.New.Add("Edition", diff.Edition  )
                                           .Add("End"    , diff.End.Int  )
                                           .Add("Start"  , diff.Start.Int)
                                           .Add("Version", diff.Version  );
                    }
                    return mp;
                }
            }
            else if (SlideSETable != null)
            {
                MsgPack slideSETable = new MsgPack(SlideSETable.Length, "SlideSETable");
                for (i0 = 0; i0 < SlideSETable.Length; i0++)
                {
                    ref SlideSE sld = ref SlideSETable[i0];
                    slideSETable[i0] = MsgPack.New.Add(    "End"  , sld.End.Int  )
                                                  .Add(    "ID"   , sld.    ID   )
                                                  .Add(    "Name" , sld.    Name )
                                                  .Add(  "SEName" , sld.  SEName )
                                                  .Add("SortIndex", sld.SortIndex)
                                                  .Add(    "Start", sld.Start.Int);
                }
                msgPack.Add(slideSETable);
            }
            else if (SliderTouchSETable != null)
            {
                MsgPack sliderTouchSETable = new MsgPack(SliderTouchSETable.Length, "SliderTouchSETable");
                for (i0 = 0; i0 < SliderTouchSETable.Length; i0++)
                {
                    ref SliderTouchSE sldt = ref SliderTouchSETable[i0];
                    sliderTouchSETable[i0] = MsgPack.New.Add(    "End"  , sldt.End.Int  )
                                                        .Add(    "ID"   , sldt.    ID   )
                                                        .Add(    "Name" , sldt.    Name )
                                                        .Add(  "SEName" , sldt.  SEName )
                                                        .Add("SortIndex", sldt.SortIndex)
                                                        .Add(    "Start", sldt.Start.Int);
                }
                msgPack.Add(sliderTouchSETable);
            }
            if (msgPack.List.Count == 1) msgPack.Write(file, false, json);
        }

        private bool disposed;
        public void Dispose()
        {
            if (disposed) return;
            if (_IO != null) _IO.D(); _IO = null;
            if (dict != null) { dict.Clear(); dict = null; }
            ButtonSETable = null; ChainSlideSETable = null; SlideSETable = null; SliderTouchSETable = null;
            CollectionCardTable = null; CustomizeItemTable = null;
            ModuleTable = null; PlateTable = null; PVList = null;
        }
    }
}
