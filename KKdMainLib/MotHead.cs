using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct MotHead
    {
        private int i, i0, i1;
        private Stream _IO;

        public HeaderData Header;

        public void MotHeadReader(string file)
        {
            _IO = File.OpenReader(file + ".bin");
            _IO.RI32();
            Header = new HeaderData();

            int offset = _IO.RI32();
            if (offset < 0x10) goto RETURN;
            _IO.O = offset;
            _IO.P = 0;

            Header.  MotionSetID   = _IO.RI32();
            Header.StartMotionID   = _IO.RI32();
            Header.  EndMotionID   = _IO.RI32();
            Header.SubHeaderOffset = _IO.RI32();

            if (Header.SubHeaderOffset < 0x10) goto RETURN;
            i0 = Header.EndMotionID - Header.StartMotionID;
            if (i0 < 0) goto RETURN;

            _IO.P = Header.SubHeaderOffset;
            Header.Data = new HeaderData.Sub[++i0];
            for (i = 0; i < Header.Data.Length; i++) Header.Data[i].Offset = _IO.RI32();
            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].Offset < 0x10) continue;

                _IO.P = Header.Data[i].Offset;

                Header.Data[i].MotionID = Header.StartMotionID + i;
                Header.Data[i].SomeData = _IO.RI32();
                _IO.RI64();
                _IO.RI64();
                Header.Data[i].Offset2 = _IO.RI32();
                Header.Data[i].DataHeaderOffset = _IO.RI32();

                if (Header.Data[i].DataHeaderOffset < 0x10) goto RETURN;
                _IO.P = Header.Data[i].DataHeaderOffset;
                i0 = 0;
                while (_IO.RI32() != -1) { _IO.RI32(); _IO.RI32(); i0++; }
                _IO.P = Header.Data[i].DataHeaderOffset;

                Header.Data[i].Array = new HeaderData.Sub.Data[i0];
                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    Header.Data[i].Array[i0].Type   = _IO.RI32();
                    Header.Data[i].Array[i0].Frame  = _IO.RI32();
                    Header.Data[i].Array[i0].Offset = _IO.RI32();
                }
            }

            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].Offset2 > 0x10)
                {
                    _IO.P = Header.Data[i].Offset2;
                    i0 = 0;
                    while (_IO.RI32() != -1) { _IO.RI32(); i0++; }
                    _IO.P = Header.Data[i].Offset2;

                    Header.Data[i].Array2 = new HeaderData.Sub.Data2[i0];
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        Header.Data[i].Array2[i0].Frame  = _IO.RI32();
                        Header.Data[i].Array2[i0].Offset = _IO.RI32();
                    }

                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        if (Header.Data[i].Array2[i0].Offset < 0x10) goto RETURN;
                        _IO.P = Header.Data[i].Array2[i0].Offset;
                        Header.Data[i].Array2[i0].Array = new int[7];
                        for (i1 = 0; i1 < Header.Data[i].Array2[i0].Array.Length; i1++)
                            Header.Data[i].Array2[i0].Array[i1] = _IO.RI32();
                    }
                }

                if (Header.Data[i].Array == null || Header.Data[i].Array.Length < 1) continue;

                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    ref HeaderData.Sub.Data Data = ref Header.Data[i].Array[i0];
                    if (!GetSize(Data.Type, out i1)) goto RETURN;

                    if (Data.Offset > 0)
                    {
                    _IO.P = Data.Offset;
                             if (i1 == 0x01) Data.Array = new int[] { _IO.RU8 () };
                        else if (i1 == 0x02) Data.Array = new int[] { _IO.RU16() };
                        else
                        {
                            Data.Array = new int[i1 / 4];
                            for (i1 = 0; i1 < Data.Array.Length; i1++) Data.Array[i1] = _IO.RI32();
                        }
                    }
                }
            }

RETURN:
            _IO.C();
        }

        public void MotHeadWriter(string file)
        {
            if (Header.Data == null || Header.Data.Length < 1) return;

            Header.StartMotionID = 0x7FFFFFFF;
            Header.EndMotionID   = 0x00000000;
            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].MotionID < Header.StartMotionID)
                    Header.StartMotionID = Header.Data[i].MotionID;
                if (Header.Data[i].MotionID > Header.  EndMotionID)
                    Header.  EndMotionID = Header.Data[i].MotionID;
            }

            _IO = File.OpenWriter(file + ".bin");
            _IO.W(0x01);
            _IO.W(0x20);
            _IO.A(0x20, true);
            _IO.O = 0x20;
            _IO.P = 0;
            _IO.W(0);
            for (int I = Header.EndMotionID - Header.StartMotionID; I > -1; I--)
                for (i = 0; i < Header.Data.Length; i++)
                    if (Header.StartMotionID + I == Header.Data[i].MotionID)
                    {
                        _IO.A(0x20, true);
                        Header.Data[i].Offset = _IO.P;
                        Header.Data[i].DataHeaderOffset = Header.Data[i].Offset + 0x20;
                        _IO.W(0L);
                        _IO.W(0L);
                        _IO.W(0L);
                        _IO.W(0L);

                        for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                        { _IO.W( 0); _IO.W( 0); _IO.W(0); }
                          _IO.W(-1); _IO.W(-1); _IO.W(0);

                        for (i0 = Header.Data[i].Array.Length - 1; i0 > -1; i0--)
                        {
                            ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                            if (data.Array != null)
                                if (data.Array.Length > 0 &&
                                    GetSize(data.Type, out int Size))
                                {
                                    if (data.Type == 0x3E) _IO.A(0x20, true);
                                    data.Offset = _IO.P;

                                         if (Size == 0x01) _IO.W(( byte)data.Array[0]);
                                    else if (Size == 0x02) _IO.W((short)data.Array[0]);
                                    else
                                    {
                                        if (_IO.P % 0x4 != 0) _IO.A(0x04, true);
                                        Size /= 4;

                                        data.Offset = _IO.P;
                                        for (i1 = 0; i1 < data.Array.Length && i1 < Size; i1++)
                                            _IO.W(data.Array[i1]);

                                        for (; i1 < Size; i1++) _IO.W(0);
                                    }
                                }
                        }

                        if (Header.Data[i].Array2 != null)
                            if (Header.Data[i].Array2.Length > 0)
                            {
                                Header.Data[i].Offset2 = _IO.P;

                                for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                                { _IO.W( 0); _IO.W(0); }
                                  _IO.W(-1); _IO.W(0);

                                for (i0 = Header.Data[i].Array2.Length - 1; i0 > -1; i0--)
                                {
                                    Header.Data[i].Array2[i0].Offset = _IO.P;
                                    for (i1 = 0; i1 < Header.Data[i].Array2[i0].Array.Length && i1 < 7; i1++)
                                        _IO.W(Header.Data[i].Array2[i0].Array[i1]);

                                    for (; i1 < 7; i1++) _IO.W(0);
                                }
                            }
                        break;
                    }
            _IO.A(0x20, true);
            Header.SubHeaderOffset = _IO.P;
            i1 = Header.EndMotionID - Header.StartMotionID;

            int offset = 0;
            for (i = 0, i1++; i < i1; i++)
            {
                offset = 0;
                for (i0 = 0; i0 < Header.Data.Length; i0++)
                    if (Header.StartMotionID + i == Header.Data[i0].MotionID)
                    { offset = Header.Data[i0].Offset; break; }
                _IO.W(offset);
            }
            _IO.A(0x20, true);

            for (i = 0; i < Header.Data.Length; i++)
            {
                _IO.P = Header.Data[i].Offset;
                _IO.W(Header.Data[i].SomeData);
                _IO.W(0);
                _IO.W(0L);
                _IO.W(0x040004);
                _IO.W(Header.Data[i].Offset2);
                _IO.W(Header.Data[i].DataHeaderOffset);
                _IO.W(0);

                _IO.P = Header.Data[i].DataHeaderOffset;
                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    _IO.W(Header.Data[i].Array[i0].Type  );
                    _IO.W(Header.Data[i].Array[i0].Frame );
                    _IO.W(Header.Data[i].Array[i0].Offset);
                }

                _IO.P = Header.Data[i].Offset2;
                if (Header.Data[i].Array2 != null)
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        _IO.W(Header.Data[i].Array2[i0].Frame );
                        _IO.W(Header.Data[i].Array2[i0].Offset);
                    }
            }

            _IO.P = 0;
            _IO.W(Header.MotionSetID    );
            _IO.W(Header.StartMotionID  );
            _IO.W(Header.  EndMotionID  );
            _IO.W(Header.SubHeaderOffset);
            _IO.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            Header = new HeaderData();

            MsgPack motHead;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            if ((motHead = msgPack["MotHead"]).IsNull) return;
            Header.MotionSetID = motHead.RI32("MotionSetID");

            MsgPack temp = MsgPack.New;
            MsgPack motions, array;
            if ((motions = motHead["Motions", true]).IsNull) return;
            Header.Data = new HeaderData.Sub[motions.Array.Length];
            for (i = 0; i < Header.Data.Length; i++)
            {
                Header.Data[i].MotionID = motions[i].RI32("MotionID");
                Header.Data[i].SomeData = motions[i].RI32("SomeData");

                MsgPack array2;
                if ((array2 = motions[i]["Array2", true]).NotNull)
                {
                    Header.Data[i].Array2 = new HeaderData.Sub.Data2[array2.Array.Length];

                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        Header.Data[i].Array2[i0].Frame = array2[i0].RI32("Frame");

                        MsgPack arrayArray;
                        if ((arrayArray = array2[i0]["Array", true]).IsNull) continue;

                        Header.Data[i].Array2[i0].Array = new int[arrayArray.Array.Length];
                        for (i1 = 0; i1 < Header.Data[i].Array2[i0].Array.Length; i1++)
                            Header.Data[i].Array2[i0].Array[i1] = arrayArray[i1].RI32();
                    }
                }

                if ((array = motions[i]["Array", true]).IsNull) { continue; }

                Header.Data[i].Array = new HeaderData.Sub.Data[array.Array.Length];

                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                    data.Type = array[i0].RI32("Type");
                    data.Frame = array[i0].RI32("Frame");

                    if ((temp = array[i0]["Type3"]).NotNull)
                    {
                        data.Type = 0x03;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type53"]).NotNull)
                    {
                        data.Type = 0x35;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type54"]).NotNull)
                    {
                        data.Type = 0x36;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type55"]).NotNull)
                    {
                        data.Type = 0x37;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type56"]).NotNull)
                    {
                        data.Type = 0x38;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type57"]).NotNull)
                    {
                        data.Type = 0x39;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type60"]).NotNull)
                    {
                        data.Type = 0x3C;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RF32("f3").ToI32() };
                    }
                    else if ((temp = array[i0]["Type61"]).NotNull)
                    {
                        data.Type = 0x3D;
                        data.Array = new int[] { temp.RI32("i0"), temp.RF32("f1").ToI32() };
                    }
                    else if ((temp = array[i0]["Type62"]).NotNull)
                    {
                        data.Type = 0x3E;
                        data.Array = new int[0x10];
                        data.Array[ 0] = temp.RI32("i0");
                        data.Array[ 1] =
                            ((byte)temp.RI32("i1_3") << 24) | ((byte)temp.RI32("i1_2") << 16) |
                            ((byte)temp.RI32("i1_1") <<  8) |  (byte)temp.RI32("i1_0");
                        data.Array[ 2] = temp.RF32("f2" ).ToI32();
                        data.Array[ 3] = temp.RF32("f3" ).ToI32();
                        data.Array[ 4] = temp.RF32("f4" ).ToI32();
                        data.Array[ 5] = temp.RF32("f5" ).ToI32();
                        data.Array[ 6] = temp.RF32("f6" ).ToI32();
                        data.Array[ 7] = temp.RF32("f7" ).ToI32();
                        data.Array[ 8] = temp.RF32("f8" ).ToI32();
                        data.Array[ 9] = temp.RF32("f9" ).ToI32();
                        data.Array[10] = temp.RF32("f10").ToI32();
                        data.Array[11] = temp.RF32("f11").ToI32();
                        data.Array[12] = temp.RF32("f12").ToI32();
                        data.Array[13] = temp.RF32("f13").ToI32();
                        data.Array[14] = temp.RF32("f14").ToI32();
                        data.Array[15] = temp.RI32("i15");
                    }
                    else if ((temp = array[i0]["Type65"]).NotNull)
                    {
                        data.Type = 0x41;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type66"]).NotNull)
                    {
                        data.Type = 0x42;
                        data.Array = new int[] { temp.RF32("f0").ToI32() };
                    }
                    else if ((temp = array[i0]["Type67"]).NotNull)
                    {
                        data.Type = 0x43;
                        data.Array = new int[] { temp.RI32("i0"), temp.RF32("f1").ToI32() };
                    }
                    else if ((temp = array[i0]["Type68"]).NotNull)
                    {
                        data.Type = 0x44;
                        data.Array = new int[] { temp.RI32("i0"), temp.RF32("f1").ToI32() };
                    }
                    else if ((temp = array[i0]["Type69"]).NotNull)
                    {
                        data.Type = 0x45;
                        data.Array = new int[] { temp.RI32("i0"), temp.RI32("i1") };
                    }
                    else if ((temp = array[i0]["Type70"]).NotNull)
                    {
                        data.Type = 0x46;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type71"]).NotNull)
                    {
                        data.Type = 0x47;
                        data.Array = new int[] { temp.RI32("i0"), temp.RF32("f1").ToI32() };
                    }
                    else if ((temp = array[i0]["Type72"]).NotNull)
                    {
                        data.Type = 0x48;
                        data.Array = new int[] { temp.RI32("i0"), temp.RF32("f1").ToI32() };
                    }
                    else if ((temp = array[i0]["Type73"]).NotNull)
                    {
                        data.Type = 0x49;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RI32("i2"),
                                                 temp.RF32("f3").ToI32() };
                    }
                    else if ((temp = array[i0]["Type74"]).NotNull)
                    {
                        data.Type = 0x4A;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type62"]).NotNull)
                    {
                        data.Type = 0x4B;
                        data.Array = new int[0x0D];
                        data.Array[ 0] = temp.RI32("i0");
                        data.Array[ 1] = ((byte)temp.RI32("i1_1") << 8) | (byte)temp.RI32("i1_0");
                        data.Array[ 2] = temp.RF32("f2" ).ToI32();
                        data.Array[ 3] = temp.RF32("f3" ).ToI32();
                        data.Array[ 4] = temp.RF32("f4" ).ToI32();
                        data.Array[ 5] = temp.RF32("f5" ).ToI32();
                        data.Array[ 6] = temp.RF32("f6" ).ToI32();
                        data.Array[ 7] = temp.RF32("f7" ).ToI32();
                        data.Array[ 8] = temp.RF32("f8" ).ToI32();
                        data.Array[ 9] = temp.RF32("f9" ).ToI32();
                        data.Array[10] = temp.RF32("f10").ToI32();
                        data.Array[11] = temp.RF32("f11").ToI32();
                        data.Array[12] = temp.RF32("f12").ToI32();
                    }
                    else if ((temp = array[i0]["Type76"]).NotNull)
                    {
                        data.Type = 0x4C;
                        data.Array = new int[] { temp.RI32("i0"),
                                                 temp.RF32("f1").ToI32(),
                                                 temp.RF32("f2").ToI32(),
                                                 temp.RI32("i3") };
                    }
                    else if ((temp = array[i0]["Type77"]).NotNull)
                    {
                        data.Type = 0x4D;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type79"]).NotNull)
                    {
                        data.Type = 0x4F;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Type80"]).NotNull)
                    {
                        data.Type = 0x50;
                        data.Array = new int[] { temp.RI32("i0") };
                    }
                    else if ((temp = array[i0]["Array", true]).NotNull)
                    {
                        data.Array = new int[temp.Array.Length];
                        for (i1 = 0; i1 < data.Array.Length; i1++)
                            data.Array[i1] = temp[i1].RI32();
                    }
                }
            }

            msgPack.Dispose();
        }

        public void MsgPackWriter(string file, bool json)
        {
            if (Header.Data == null || Header.Data.Length < 1) return;

            for (i = 0, i1 = 0; i < Header.Data.Length; i++)
                if (Header.Data[i].Offset >= 0x10) i1++;
            MsgPack motHead = new MsgPack("MotHead").Add("MotionSetID", Header.MotionSetID);
            MsgPack motions = new MsgPack(i1, "Motions");
            for (int I = 0, i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].Offset < 0x10) continue;
                MsgPack motion = MsgPack.New
                    .Add("MotionID", Header.Data[i].MotionID)
                    .Add("SomeData", Header.Data[i].SomeData);

                if (Header.Data[i].Array == null || Header.Data[i].Array.Length < 1) { }
                else
                {
                    MsgPack array = new MsgPack(Header.Data[i].Array.Length, "Array");
                    for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                    {
                        ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                        MsgPack arrayEntry = MsgPack.New;
                        arrayEntry.Add("Frame", data.Frame);
                        if (data.Array == null || data.Array.Length < 1)
                            arrayEntry.Add("Type", data.Type);
                        else if (data.Type == 0x03)
                            arrayEntry.Add(new MsgPack("Type3")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x35)
                            arrayEntry.Add(new MsgPack("Type53")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x36)
                            arrayEntry.Add(new MsgPack("Type54")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x37)
                            arrayEntry.Add(new MsgPack("Type55")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x38)
                            arrayEntry.Add(new MsgPack("Type56")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x39)
                            arrayEntry.Add(new MsgPack("Type57")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x3C)
                            arrayEntry.Add(new MsgPack("Type60")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("f3", data.Array[3].ToF32()));
                        else if (data.Type == 0x3D)
                            arrayEntry.Add(new MsgPack("Type61")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32()));
                        else if (data.Type == 0x3E)
                            arrayEntry.Add(new MsgPack("Type62")
                                .Add("i0", data.Array[0])
                                .Add("i1_0", (byte) data.Array[1])
                                .Add("i1_1", (byte)(data.Array[1] >>  8))
                                .Add("i1_2", (byte)(data.Array[1] >> 16))
                                .Add("i1_3", (byte)(data.Array[1] >> 24))
                                .Add("f2" , data.Array[ 2].ToF32())
                                .Add("f3" , data.Array[ 3].ToF32())
                                .Add("f4" , data.Array[ 4].ToF32())
                                .Add("f5" , data.Array[ 5].ToF32())
                                .Add("f6" , data.Array[ 6].ToF32())
                                .Add("f7" , data.Array[ 7].ToF32())
                                .Add("f8" , data.Array[ 8].ToF32())
                                .Add("f9" , data.Array[ 9].ToF32())
                                .Add("f10", data.Array[10].ToF32())
                                .Add("f11", data.Array[11].ToF32())
                                .Add("f12", data.Array[12].ToF32())
                                .Add("f13", data.Array[13].ToF32())
                                .Add("f14", data.Array[14].ToF32())
                                .Add("i15", data.Array[15]));
                        else if (data.Type == 0x41)
                            arrayEntry.Add(new MsgPack("Type65")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x42)
                            arrayEntry.Add(new MsgPack("Type66")
                                .Add("f0", data.Array[0].ToF32()));
                        else if (data.Type == 0x43)
                            arrayEntry.Add(new MsgPack("Type67")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32()));
                        else if (data.Type == 0x44)
                            arrayEntry.Add(new MsgPack("Type68")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32()));
                        else if (data.Type == 0x45)
                            arrayEntry.Add(new MsgPack("Type69")
                                .Add("i0", data.Array[0])
                                .Add("i1", data.Array[1]));
                        else if (data.Type == 0x46)
                            arrayEntry.Add(new MsgPack("Type70")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x47)
                            arrayEntry.Add(new MsgPack("Type71")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32()));
                        else if (data.Type == 0x48)
                            arrayEntry.Add(new MsgPack("Type72")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32()));
                        else if (data.Type == 0x49)
                            arrayEntry.Add(new MsgPack("Type73")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("i2", (short)data.Array[2])
                                .Add("f3", data.Array[3].ToF32()));
                        else if (data.Type == 0x4A)
                            arrayEntry.Add(new MsgPack("Type74")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x4B)
                            arrayEntry.Add(new MsgPack("Type75")
                                .Add("i0", data.Array[0])
                                .Add("i1_0", (byte) data.Array[1])
                                .Add("i1_1", (byte)(data.Array[1] >> 8))
                                .Add("f2" , data.Array[ 2].ToF32())
                                .Add("f3" , data.Array[ 3].ToF32())
                                .Add("f4" , data.Array[ 4].ToF32())
                                .Add("f5" , data.Array[ 5].ToF32())
                                .Add("f6" , data.Array[ 6].ToF32())
                                .Add("f7" , data.Array[ 7].ToF32())
                                .Add("f8" , data.Array[ 8].ToF32())
                                .Add("f9" , data.Array[ 9].ToF32())
                                .Add("f10", data.Array[10].ToF32())
                                .Add("f11", data.Array[11].ToF32())
                                .Add("f12", data.Array[12].ToF32()));
                        else if (data.Type == 0x4C)
                            arrayEntry.Add(new MsgPack("Type76")
                                .Add("i0", data.Array[0])
                                .Add("f1", data.Array[1].ToF32())
                                .Add("f2", data.Array[2].ToF32())
                                .Add("i3", data.Array[3]));
                        else if (data.Type == 0x4D)
                            arrayEntry.Add(new MsgPack("Type77")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x4F)
                            arrayEntry.Add(new MsgPack("Type79")
                                .Add("i0", data.Array[0]));
                        else if (data.Type == 0x50)
                            arrayEntry.Add(new MsgPack("Type80")
                                .Add("i0", data.Array[0]));
                        else
                        {
                            arrayEntry.Add("Type", data.Type);
                            MsgPack arrayArray = new MsgPack(data.Array.Length, "Array");
                            for (i1 = 0; i1 < data.Array.Length; i1++)
                                arrayArray[i1] = data.Array[i1];
                            arrayEntry.Add(arrayArray);
                        }
                        array[i0] = arrayEntry;
                    }
                    motion.Add(array);
                }

                if (Header.Data[i].Array2 == null || Header.Data[i].Array2.Length < 1) { }
                else
                {
                    MsgPack array2 = new MsgPack(Header.Data[i].Array2.Length, "Array2");
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        MsgPack arrayEntry = MsgPack.New.Add("Frame", Header.Data[i].Array2[i0].Frame);
                        if (Header.Data[i].Array2[i0].Array == null || Header.Data[i].Array2[i0].Array.Length < 1) { }
                        else
                        {
                            MsgPack arrayArray = new MsgPack(Header.Data[i].Array2[i0].Array.Length, "Array");
                            for (i1 = 0; i1 < Header.Data[i].Array2[i0].Array.Length; i1++)
                                arrayArray[i1] = Header.Data[i].Array2[i0].Array[i1];
                            arrayEntry.Add(arrayArray);
                        }
                        array2[i0] = arrayEntry;
                    }
                    motion.Add(array2);
                }

                motions[I] = motion;
                I++;
            }
            motHead.Add(motions);
            motHead.WriteAfterAll(true, file, json);
        }

        private static bool GetSize(int Type, out int Size)
        {
            Size = Type switch
            {
                0x03 => 0x04,
                0x35 => 0x14,
                0x36 => 0x14,
                0x37 => 0x14,
                0x38 => 0x14,
                0x39 => 0x14,
                0x3C => 0x10,
                0x3D => 0x08,
                0x3E => 0x40,
                0x40 => 0x00,
                0x41 => 0x04,
                0x42 => 0x04,
                0x43 => 0x08,
                0x44 => 0x08,
                0x45 => 0x08,
                0x46 => 0x04,
                0x47 => 0x08,
                0x48 => 0x08,
                0x49 => 0x10,
                0x4A => 0x02,
                0x4B => 0x34,
                0x4C => 0x10,
                0x4D => 0x04,
                0x4F => 0x01,
                0x50 => 0x04,
                _    => 0x00,
            };
            return Size > 0;
        }

        public struct HeaderData
        {
            public int MotionSetID;
            public int StartMotionID;
            public int EndMotionID;
            public int SubHeaderOffset;

            public Sub[] Data;

            public struct Sub
            {
                public int Offset;
                public int MotionID;

                public int SomeData;
                public int Offset2;
                public int DataHeaderOffset;

                public Data[] Array;
                public Data2[] Array2;

                public struct Data
                {
                    public int Type;
                    public int Frame;
                    public int Offset;

                    public int[] Array;
                }

                public struct Data2
                {
                    public int Offset;

                    public int Frame;
                    public int[] Array;
                }
            }
        }
    }
}
