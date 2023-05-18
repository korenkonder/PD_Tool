using KKdBaseLib;
using KKdBaseLib.F2;
using KKdMainLib.IO;

namespace KKdMainLib
{
    public struct MotHead : System.IDisposable
    {
        private int i, i0, i1;
        private Stream s;

        public HeaderData Header;

        public void MotHeadReader(string file, string ext)
        {
            bool x = false;
            int x_offset = 0;
            s = File.OpenReader(file + ext);
            Header = new HeaderData();
            int signature = s.RI32();
            if (signature == 0x484D4D4D)
            {
                x_offset = (int)s.ReadHeader(true).Length;
                Header.MotionSetID = s.RU32();
                int mot_count = s.RI32();
                int mot_hashes_offset = s.RI32();
                Header.SubHeaderOffset = s.RI32();
                if (Header.SubHeaderOffset == 0)
                {
                    if (mot_hashes_offset != 0) mot_hashes_offset += x_offset;
                    s.IsX = true;
                    x = true;
                    Header.SubHeaderOffset = (int)s.RIX();
                    if (Header.SubHeaderOffset != 0) Header.SubHeaderOffset += x_offset;
                }

                if (Header.SubHeaderOffset < 0x20) goto RETURN;

                s.P = mot_hashes_offset;
                Header.Data = new HeaderData.Sub[mot_count];
                for (i = 0; i < Header.Data.Length; i++) Header.Data[i].MotionID = s.RU32();
            }
            else
            {
                int offset = s.RI32();
                if (offset < 0x10) goto RETURN;
                s.O = offset;
                s.P = 0;

                Header.MotionSetID     = s.RU32();
                Header.FirstMotionID   = s.RU32();
                Header.LastMotionID    = s.RU32();
                Header.SubHeaderOffset = (int)s.RIX();

                if (Header.SubHeaderOffset < 0x10) goto RETURN;
                i0 = (int)(Header.LastMotionID - Header.FirstMotionID);
                if (i0 < 0) goto RETURN;

                Header.Data = new HeaderData.Sub[++i0];
                for (i = 0; i < Header.Data.Length; i++)
                    Header.Data[i].MotionID = Header.FirstMotionID + (uint)i;
            }

            s.P = Header.SubHeaderOffset;
            for (i = 0; i < Header.Data.Length; i++)
            {
                if (x) s.A(0x08);
                Header.Data[i].Offset = (int)s.RIX();
                if (x && Header.Data[i].Offset != 0) Header.Data[i].Offset += x_offset;
            }

            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].Offset < 0x10) continue;

                s.P = Header.Data[i].Offset;

                Header.Data[i].Field00 = s.RI32();
                if (!x)
                {
                    Header.Data[i].Field04 = s.RI32();
                    Header.Data[i].Field08 = s.RI32();
                    Header.Data[i].Field0C = s.RI32();
                }
                Header.Data[i].Field10 = s.RU16();
                Header.Data[i].Field12 = s.RU16();
                if (x) s.A(0x08);
                Header.Data[i].Offset2 = (int)s.RIX();
                if (x && Header.Data[i].Offset2 != 0) Header.Data[i].Offset2 += x_offset;
                if (x) s.A(0x08);
                Header.Data[i].DataHeaderOffset = (int)s.RIX();
                if (x && Header.Data[i].DataHeaderOffset != 0)
                    Header.Data[i].DataHeaderOffset += x_offset;

                if (Header.Data[i].DataHeaderOffset < 0x10) goto RETURN;
                s.P = Header.Data[i].DataHeaderOffset;
                i0 = 0;
                while (s.RI32() != -1) { s.RI32(); if (x) s.A(0x08); s.RIX(); i0++; }
                s.P = Header.Data[i].DataHeaderOffset;

                Header.Data[i].Array = new HeaderData.Sub.Data[i0];
                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    Header.Data[i].Array[i0].Type   = (Type)s.RI32();
                    Header.Data[i].Array[i0].Frame  = s.RI32();
                    if (x) s.A(0x08);
                    Header.Data[i].Array[i0].Offset = (int)s.RIX();
                    if (x && Header.Data[i].Array[i0].Offset != 0)
                        Header.Data[i].Array[i0].Offset += x_offset;
                }
            }

            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].Offset2 > 0x10)
                {
                    s.P = Header.Data[i].Offset2;
                    i0 = 0;
                    while (s.RI32() != -1) { s.RI32(); i0++; }
                    s.P = Header.Data[i].Offset2;

                    Header.Data[i].Array2 = new HeaderData.Sub.Data2[i0];
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        Header.Data[i].Array2[i0].Type   = s.RI32();
                        if (x) s.A(0x08);
                        Header.Data[i].Array2[i0].Offset = (int)s.RIX();
                        if (x && Header.Data[i].Array2[i0].Offset != 0)
                            Header.Data[i].Array2[i0].Offset += x_offset;
                    }

                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        ref HeaderData.Sub.Data2 Data = ref Header.Data[i].Array2[i0];
                        if ((i1 = GetSize2(Data.Type)) < 1) continue;

                        if (Header.Data[i].Array2[i0].Offset < 0x10) goto RETURN;
                        s.P = Data.Offset;
                        Data.Array = s.RBy(i1);
                    }
                }

                if (Header.Data[i].Array == null || Header.Data[i].Array.Length < 1) continue;

                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    ref HeaderData.Sub.Data Data = ref Header.Data[i].Array[i0];
                    if ((i1 = GetSize(Data.Type)) < 1) continue;

                    if (Data.Offset > 0)
                    {
                        s.P = Data.Offset;
                        Data.Array = s.RBy(i1);
                    }
                }
            }

RETURN:
            s.C();
        }

        public void MotHeadWriter(string file)
        {
            if (Header.Data == null || Header.Data.Length < 1) return;

            Header.FirstMotionID = 0x7FFFFFFF;
            Header. LastMotionID = 0x00000000;
            for (i = 0; i < Header.Data.Length; i++)
            {
                if (Header.Data[i].MotionID < Header.FirstMotionID)
                    Header.FirstMotionID = Header.Data[i].MotionID;
                if (Header.Data[i].MotionID > Header. LastMotionID)
                    Header. LastMotionID = Header.Data[i].MotionID;
            }

            s = File.OpenWriter(file + ".bin", true);
            s.W(0x01);
            s.W(0x20);
            s.A(0x20, true);
            s.O = 0x20;
            s.P = 0;
            s.W(0);
            for (int I = (int)(Header.LastMotionID - Header.FirstMotionID); I > -1; I--)
                for (i = 0; i < Header.Data.Length; i++)
                    if (Header.FirstMotionID + I == Header.Data[i].MotionID)
                    {
                        s.A(0x20, true);
                        Header.Data[i].Offset = s.P;
                        Header.Data[i].DataHeaderOffset = Header.Data[i].Offset + 0x20;
                        s.W(0L);
                        s.W(0L);
                        s.W(0L);
                        s.W(0L);

                        for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                        { s.W( 0); s.W( 0); s.W(0); }
                          s.W(-1); s.W(-1); s.W(0);

                        for (i0 = Header.Data[i].Array.Length - 1; i0 > -1; i0--)
                        {
                            ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                            int Size = GetSize(data.Type);
                            if (data.Array == null || data.Array.Length < 1 || Size < 1) continue;

                            switch ((int)data.Type)
                            {
                                case 0x3E:
                                case 0x49:
                                case 0x4B:
                                    s.A(0x20, true);
                                    break;
                            }

                            if (data.Array.Length >= 4 && s.P % 0x4 != 0)
                                s.A(0x04, true);
                            else if (data.Array.Length == 2 && s.P % 0x2 != 0)
                                s.A(0x02, true);

                            data.Offset = s.P;
                            s.W(data.Array);

                            if ((int)data.Type == 0x3C)
                                s.A(0x20, true);
                        }

                        if (Header.Data[i].Array2 != null && Header.Data[i].Array2.Length > 0)
                        {
                            Header.Data[i].Offset2 = s.P;

                            for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                            { s.W( 0); s.W(0); }
                              s.W(-1); s.W(0);

                            for (i0 = Header.Data[i].Array2.Length - 1; i0 > -1; i0--)
                            {
                                ref HeaderData.Sub.Data2 data = ref Header.Data[i].Array2[i0];
                                int Size = GetSize2(data.Type);
                                if (data.Array == null || data.Array.Length < 1 || Size < 1) continue;

                                if (data.Array.Length >= 4 && s.P % 0x4 != 0)
                                    s.A(0x04, true);
                                else if (data.Array.Length == 2 && s.P % 0x2 != 0)
                                    s.A(0x02, true);

                                data.Offset = s.P;
                                s.W(data.Array);
                            }
                            break;
                        }
                    }
            s.A(0x10, true);

            Header.SubHeaderOffset = s.P;
            i1 = (int)(Header.LastMotionID - Header.FirstMotionID);

            int offset = 0;
            for (i = 0, i1++; i < i1; i++)
            {
                offset = 0;
                for (i0 = 0; i0 < Header.Data.Length; i0++)
                    if (Header.FirstMotionID + i == Header.Data[i0].MotionID)
                    { offset = Header.Data[i0].Offset; break; }
                s.W(offset);
            }
            s.A(0x20, true);

            for (i = 0; i < Header.Data.Length; i++)
            {
                s.P = Header.Data[i].Offset;
                s.W(Header.Data[i].Field00);
                s.W(Header.Data[i].Field04);
                s.W(Header.Data[i].Field08);
                s.W(Header.Data[i].Field0C);
                s.W(Header.Data[i].Field10);
                s.W(Header.Data[i].Field12);
                s.W(Header.Data[i].Offset2);
                s.W(Header.Data[i].DataHeaderOffset);
                s.W(0);

                s.P = Header.Data[i].DataHeaderOffset;
                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    s.W((int)Header.Data[i].Array[i0].Type  );
                    s.W(Header.Data[i].Array[i0].Frame );
                    s.W(Header.Data[i].Array[i0].Offset);
                }

                s.P = Header.Data[i].Offset2;
                if (Header.Data[i].Array2 != null)
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        s.W(Header.Data[i].Array2[i0].Type  );
                        s.W(Header.Data[i].Array2[i0].Offset);
                    }
            }

            s.P = 0;
            s.W(Header.MotionSetID    );
            s.W(Header.FirstMotionID  );
            s.W(Header. LastMotionID  );
            s.W(Header.SubHeaderOffset);
            s.C();
        }

        public void MsgPackReader(string file, bool json)
        {
            Header = new HeaderData();

            MsgPack motHead;
            MsgPack msgPack = file.ReadMPAllAtOnce(json);
            if ((motHead = msgPack["MotHead"]).IsNull) return;
            Header.MotionSetID = motHead.RU32("MotionSetID");

            MsgPack motions, array;
            if ((motions = motHead["Motions", true]).IsNull) return;
            Header.Data = new HeaderData.Sub[motions.Array.Length];
            for (i = 0; i < Header.Data.Length; i++)
            {
                Header.Data[i].MotionID = motions[i].RU32("MotionID");
                Header.Data[i].Field00 = motions[i].RI32("Field00");
                Header.Data[i].Field04 = motions[i].RI32("Field04");
                Header.Data[i].Field08 = motions[i].RI32("Field08");
                Header.Data[i].Field0C = motions[i].RI32("Field0C");
                Header.Data[i].Field10 = motions[i].RU16("Field10");
                Header.Data[i].Field12 = motions[i].RU16("Field12");

                MsgPack array2;
                if ((array2 = motions[i]["Array2", true]).NotNull)
                {
                    Header.Data[i].Array2 = new HeaderData.Sub.Data2[array2.Array.Length];

                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        ref HeaderData.Sub.Data2 data = ref Header.Data[i].Array2[i0];

                        data.Type = array2[i0].RI32("Type");
                        data.Array = new byte[GetSize2(data.Type)];

                        MsgPack arrayArray;
                        if ((arrayArray = array2[i0]["Array", true]).IsNull) continue;

                        data.Array = new byte[arrayArray.Array.Length];
                        for (i1 = 0; i1 < arrayArray.Array.Length; i1++)
                            data.Array.GBy(arrayArray[i1].RU8(), i1);
                    }
                }

                if ((array = motions[i]["Array", true]).IsNull) { continue; }

                Header.Data[i].Array = new HeaderData.Sub.Data[array.Array.Length];

                for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                {
                    ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                    System.Enum.TryParse(array[i0].RS("Type"), out data.Type);
                    data.Frame = array[i0].RI32("Frame");
                    MsgPack temp = array[i0]["Data"];

                    if (temp.IsNull)
                    {
                        MsgPack arrayArray = array[i0]["Array"];
                        if (arrayArray.IsNull)
                            continue;

                        data.Array = new byte[arrayArray.Array.Length];
                        for (i1 = 0; i1 < data.Array.Length; i1++)
                            data.Array[i1] = arrayArray[i1].RU8();
                        continue;
                    }

                    data.Array = new byte[GetSize(data.Type)];

                    switch ((int)data.Type)
                    {
                        case 0x02:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            break;
                        case 0x03:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            break;
                        case 0x04:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            break;
                        case 0x07:
                            data.Array.GBy(temp.RI32("i00_16"), 0x00);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            break;
                        case 0x08:
                            data.Array.GBy(temp.RI16("i04_16"), 0x04);
                            break;
                        case 0x0D:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            data.Array.GBy(temp.RI32("i04"), 0x04);
                            data.Array.GBy(temp.RI32("i08"), 0x08);
                            break;
                        case 0x11:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            data.Array.GBy(temp.RI16("i02_16"), 0x02);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            data.Array.GBy(temp.RF32("f0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            data.Array.GBy(temp.RF32("f14"), 0x14);
                            data.Array.GBy(temp.RF32("f18"), 0x18);
                            break;
                        case 0x1D:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            data.Array.GBy(temp.RI16("i02_16"), 0x02);
                            data.Array.GBy(temp.RI16("i04_16"), 0x04);
                            data.Array.GBy(temp.RI16("i06_16"), 0x06);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            data.Array.GBy(temp.RF32("f0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            data.Array.GBy(temp.RI16("i14_16"), 0x14);
                            data.Array.GBy(temp.RF32("f18"), 0x18);
                            data.Array.GBy(temp.RF32("f1C"), 0x1C);
                            data.Array.GBy(temp.RF32("f20"), 0x20);
                            data.Array.GBy(temp.RF32("f24"), 0x24);
                            data.Array.GBy(temp.RF32("f28"), 0x28);
                            data.Array.GBy(temp.RF32("f2C"), 0x2C);
                            data.Array.GBy(temp.RI16("i30_16"), 0x30);
                            data.Array.GBy(temp.RI32("i34"), 0x34);
                            data.Array.GBy(temp.RI32("i38"), 0x38);
                            break;
                        case 0x20:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            data.Array.GBy(temp.RI32("i04"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            break;
                        case 0x21:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            data.Array.GBy(temp.RU8("i02_8"), 0x02);
                            data.Array.GBy(temp.RI32("i04"), 0x04);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            data.Array.GBy(temp.RF32("f0C"), 0x0C);
                            data.Array.GBy(temp.RI32("i10"), 0x10);
                            data.Array.GBy(temp.RI32("i14"), 0x14);
                            break;
                        case 0x32: // SetFaceMotionId
                            data.Array.GBy(temp.RI32("MotionID"), 0x00);
                            data.Array.GBy(temp.RF32("Frame"), 0x04);
                            break;
                        case 0x35: // SetFaceMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x36: // SetHandRMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x37: // SetHandLMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x38: // SetMouthMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x39: // SetEyesMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x3A: // SetEyelidMottblMotion
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            data.Array.GBy(temp.RF32("Duration"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RF32("f10"), 0x10);
                            break;
                        case 0x3B: // SetRobCharaHeadObject
                            data.Array.GBy(temp.RI32("Index"), 0x00);
                            break;
                        case 0x3C:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            data.Array.GBy(temp.RF32("f0C"), 0x0C);
                            break;
                        case 0x3D: // SetEyelidMotionFromFace
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            data.Array.GBy(temp.RF32("Duration"), 0x04);
                            break;
                        case 0x3E: // RobPartsAdjust
                            data.Array.GBy(temp.RI32("TransitionDuration"), 0x00);
                            data.Array.GBy(temp.RU8("Parts"), 0x04);
                            data.Array.GBy(temp.RU8("Type"), 0x05);
                            data.Array.GBy((byte)(temp.RB("IgnoreGravity") ? 1 : 0), 0x06);
                            data.Array.GBy(temp.RU8("CycleType"), 0x07);
                            data.Array.GBy(temp.RF32("ExternalForceX"), 0x08);
                            data.Array.GBy(temp.RF32("ExternalForceY"), 0x0C);
                            data.Array.GBy(temp.RF32("ExternalForceZ"), 0x10);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthX"), 0x14);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthY"), 0x18);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthZ"), 0x1C);
                            data.Array.GBy(temp.RF32("ExternalForceCycleX"), 0x20);
                            data.Array.GBy(temp.RF32("ExternalForceCycleY"), 0x24);
                            data.Array.GBy(temp.RF32("ExternalForceCycleZ"), 0x28);
                            data.Array.GBy(temp.RF32("Cycle"), 0x2C);
                            data.Array.GBy(temp.RF32("Phase"), 0x30);
                            data.Array.GBy(temp.RF32("Force"), 0x34);
                            data.Array.GBy(temp.RF32("Strength"), 0x38);
                            data.Array.GBy(temp.RI32("StrengthTransition"), 0x3C);
                            break;
                        case 0x41: // OsageReset
                            data.Array.GBy(temp.RI32("Iterations"), 0x00);
                            break;
                        case 0x42: // OsageStep
                            data.Array.GBy(temp.RF32("Value"), 0x00);
                            break;
                        case 0x43:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            break;
                        case 0x44:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            break;
                        case 0x45:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            break;
                        case 0x46:
                            data.Array.GBy(temp.RI32("i00"), 0x00);
                            break;
                        case 0x47: // OsageMoveCancel
                            data.Array.GBy(temp.RU8("ID"), 0x00);                       // 0 - All, 1 - Kami, 2 - Outer
                            data.Array.GBy(temp.RF32("Value"), 0x04);
                            break;
                        case 0x48:
                            data.Array.GBy(temp.RF32("f00"), 0x00);
                            data.Array.GBy(temp.RF32("f04"), 0x04);
                            data.Array.GBy(temp.RF32("f08"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RI32("i10"), 0x10);
                            data.Array.GBy(temp.RI32("i14"), 0x14);
                            data.Array.GBy(temp.RU8("i18_8"), 0x18);
                            data.Array.GBy(temp.RU8("i19_8"), 0x19);
                            data.Array.GBy(temp.RU8("i1A_8"), 0x1A);
                            break;
                        case 0x49: // RobHandAdjust
                            data.Array.GBy(temp.RI16("Hand"), 0x00);
                            data.Array.GBy(temp.RI16("ScaleSelect"), 0x02);
                            data.Array.GBy(temp.RF32("Duration"), 0x04);
                            data.Array.GBy(temp.RI16("Type"), 0x08);
                            data.Array.GBy(temp.RF32("Scale"), 0x0C);
                            data.Array.GBy(temp.RF32("RotBlend"), 0x10);
                            data.Array.GBy(temp.RF32("OffsetX"), 0x14);
                            data.Array.GBy(temp.RF32("OffsetY"), 0x18);
                            data.Array.GBy(temp.RF32("OffsetZ"), 0x1C);
                            data.Array.GBy((byte)(temp.RB("EnableScale") ? 1 : 0), 0x20);
                            data.Array.GBy((byte)(temp.RB("DisableX") ? 1 : 0), 0x21);
                            data.Array.GBy((byte)(temp.RB("DisableY") ? 1 : 0), 0x22);
                            data.Array.GBy((byte)(temp.RB("DisableZ") ? 1 : 0), 0x23);
                            data.Array.GBy(temp.RF32("OffsetBlend"), 0x24);
                            data.Array.GBy(temp.RF32("ArmLength"), 0x28);
                            data.Array.GBy(temp.RI32("i2C"), 0x2C);
                            break;
                        case 0x4A:
                            data.Array.GBy(temp.RU8("i00_8"), 0x00);
                            data.Array.GBy(temp.RU8("i01_8"), 0x01);
                            break;
                        case 0x4B: // RobAdjustGlobal
                            data.Array.GBy(temp.RI32("TransitionDuration"), 0x00);
                            data.Array.GBy(temp.RU8("Type"), 0x04);
                            data.Array.GBy(temp.RU8("CycleType"), 0x05);
                            data.Array.GBy(temp.RF32("ExternalForceX"), 0x08);
                            data.Array.GBy(temp.RF32("ExternalForceY"), 0x0C);
                            data.Array.GBy(temp.RF32("ExternalForceZ"), 0x10);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthX"), 0x14);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthY"), 0x18);
                            data.Array.GBy(temp.RF32("ExternalForceCycleStrengthZ"), 0x1C);
                            data.Array.GBy(temp.RF32("ExternalForceCycleX"), 0x20);
                            data.Array.GBy(temp.RF32("ExternalForceCycleY"), 0x24);
                            data.Array.GBy(temp.RF32("ExternalForceCycleZ"), 0x28);
                            data.Array.GBy(temp.RF32("Cycle"), 0x2C);
                            data.Array.GBy(temp.RF32("Phase"), 0x30);
                            break;
                        case 0x4C: // RobArmAdjust
                            data.Array.GBy(temp.RI16("Index"), 0x00);
                            data.Array.GBy(temp.RF32("Duration"), 0x04);
                            data.Array.GBy(temp.RF32("Value"), 0x08);
                            break;
                        case 0x4D: // DisableEyeMotion
                            data.Array.GBy((byte)(temp.RB("Value") ? 1 : 0), 0x00);
                            break;
                        case 0x4E:
                            data.Array.GBy(temp.RI16("i00_16"), 0x00);
                            data.Array.GBy(temp.RU8("i02_8"), 0x02);
                            data.Array.GBy(temp.RU8("i03_8"), 0x03);
                            data.Array.GBy(temp.RI32("i04"), 0x04);
                            data.Array.GBy(temp.RI32("i08"), 0x08);
                            data.Array.GBy(temp.RI32("i0C"), 0x0C);
                            data.Array.GBy(temp.RI32("i10"), 0x10);
                            data.Array.GBy(temp.RI32("i14"), 0x14);
                            data.Array.GBy(temp.RI32("i18"), 0x18);
                            break;
                        case 0x4F: // RobCharaColiRing
                            data.Array.GBy(temp.RU8("Index"), 0x00);
                            break;
                        case 0x50: // AdjustGetGlobalTrans
                            data.Array.GBy((byte)(temp.RB("Value") ? 1 : 0), 0x00);
                            break;
                        case 0x40: // WindReset
                        default:
                            break;
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
                    .Add("Field00", Header.Data[i].Field00)
                    .Add("Field04", Header.Data[i].Field04)
                    .Add("Field08", Header.Data[i].Field08)
                    .Add("Field0C", Header.Data[i].Field0C)
                    .Add("Field10", Header.Data[i].Field10)
                    .Add("Field12", Header.Data[i].Field12);

                if (Header.Data[i].Array != null && Header.Data[i].Array.Length > 0)
                {
                    MsgPack array = new MsgPack(Header.Data[i].Array.Length, "Array");
                    for (i0 = 0; i0 < Header.Data[i].Array.Length; i0++)
                    {
                        ref HeaderData.Sub.Data data = ref Header.Data[i].Array[i0];
                        MsgPack arrayEntry = MsgPack.New;
                        arrayEntry.Add("Type", data.Type.ToString());
                        arrayEntry.Add("Frame", data.Frame);
                        switch ((int)data.Type)
                        {
                            case 0x02:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00)));
                                break;
                            case 0x03:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00)));
                                break;
                            case 0x04:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00)));
                                break;
                            case 0x07:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("f04", data.Array.TF32(0x04))
                                    .Add("f08", data.Array.TF32(0x08)));
                                break;
                            case 0x08:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i04_16", data.Array.TI16(0x04)));
                                break;
                            case 0x0D:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00))
                                    .Add("i04", data.Array.TI32(0x04))
                                    .Add("i08", data.Array.TI32(0x08)));
                                break;
                            case 0x11:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("i02_16", data.Array.TI16(0x02))
                                    .Add("f04", data.Array.TF32(0x04))
                                    .Add("f08", data.Array.TF32(0x08))
                                    .Add("f0C", data.Array.TF32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10))
                                    .Add("f14", data.Array.TF32(0x14))
                                    .Add("f18", data.Array.TF32(0x18)));
                                break;
                            case 0x1D:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("i02_16", data.Array.TI16(0x02))
                                    .Add("i04_16", data.Array.TI16(0x04))
                                    .Add("i06_16", data.Array.TI16(0x06))
                                    .Add("f08", data.Array.TF32(0x08))
                                    .Add("f0C", data.Array.TF32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10))
                                    .Add("i14_16", data.Array.TI16(0x14))
                                    .Add("f18", data.Array.TF32(0x18))
                                    .Add("f1C", data.Array.TF32(0x1C))
                                    .Add("f20", data.Array.TF32(0x20))
                                    .Add("f24", data.Array.TF32(0x24))
                                    .Add("f28", data.Array.TF32(0x28))
                                    .Add("f2C", data.Array.TF32(0x2C))
                                    .Add("i30_16", data.Array.TI16(0x30))
                                    .Add("i34", data.Array.TI32(0x34))
                                    .Add("i38", data.Array.TI32(0x38)));
                                break;
                            case 0x20:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("i04", data.Array.TI32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C)));
                                break;
                            case 0x21:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("i02_8", data.Array.TU8(0x02))
                                    .Add("i04", data.Array.TI32(0x04))
                                    .Add("f08", data.Array.TF32(0x08))
                                    .Add("f0C", data.Array.TF32(0x0C))
                                    .Add("i10", data.Array.TI32(0x10))
                                    .Add("i14", data.Array.TI32(0x14)));
                                break;
                            case 0x32: // SetFaceMotionId
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("MotionID", data.Array.TI32(0x00))
                                    .Add("Frame", data.Array.TF32(0x04)));
                                break;
                            case 0x35: // SetFaceMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x36: // SetHandRMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x37: // SetHandLMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x38: // SetMouthMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x39: // SetEyesMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x3A: // SetEyelidMottblMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00))
                                    .Add("Value", data.Array.TF32(0x04))
                                    .Add("Duration", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("f10", data.Array.TF32(0x10)));
                                break;
                            case 0x3B: // SetRobCharaHeadObject
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI32(0x00)));
                                break;
                            case 0x3C:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00))
                                    .Add("f04", data.Array.TF32(0x04))
                                    .Add("f08", data.Array.TF32(0x08))
                                    .Add("f0C", data.Array.TF32(0x0C)));
                                break;
                            case 0x3D: // SetEyelidMotionFromFace
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00))
                                    .Add("Duration", data.Array.TF32(0x04)));
                                break;
                            case 0x3E: // RobPartsAdjust
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("TransitionDuration", data.Array.TI32(0x00))
                                    .Add("Parts", data.Array.TU8(0x04))
                                    .Add("Type", data.Array.TU8(0x05))
                                    .Add("IgnoreGravity", data.Array.TU8(0x06) != 0)
                                    .Add("CycleType", data.Array.TU8(0x07))
                                    .Add("ExternalForceX", data.Array.TF32(0x08))
                                    .Add("ExternalForceY", data.Array.TF32(0x0C))
                                    .Add("ExternalForceZ", data.Array.TF32(0x10))
                                    .Add("ExternalForceCycleStrengthX", data.Array.TF32(0x14))
                                    .Add("ExternalForceCycleStrengthY", data.Array.TF32(0x18))
                                    .Add("ExternalForceCycleStrengthZ", data.Array.TF32(0x1C))
                                    .Add("ExternalForceCycleX", data.Array.TF32(0x20))
                                    .Add("ExternalForceCycleY", data.Array.TF32(0x24))
                                    .Add("ExternalForceCycleZ", data.Array.TF32(0x28))
                                    .Add("Cycle", data.Array.TF32(0x2C))
                                    .Add("Phase", data.Array.TF32(0x30))
                                    .Add("Force", data.Array.TF32(0x34))
                                    .Add("Strength", data.Array.TF32(0x38))
                                    .Add("StrengthTransition", data.Array.TI32(0x3C)));
                                break;
                            case 0x41: // OsageReset
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Iterations", data.Array.TI32(0x00)));
                                break;
                            case 0x42: // OsageStep
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Value", data.Array.TF32(0x00)));
                                break;
                            case 0x43:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00))
                                    .Add("f04", data.Array.TF32(0x04)));
                                break;
                            case 0x44:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00))
                                    .Add("f04", data.Array.TF32(0x04)));
                                break;
                            case 0x45:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00)));
                                break;
                            case 0x46:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00", data.Array.TI32(0x00)));
                                break;
                            case 0x47: // OsageMoveCancel
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("ID", data.Array.TU8(0x00))                    // 0 - All, 1 - Kami, 2 - Outer
                                    .Add("Value", data.Array.TF32(0x04)));
                                break;
                            case 0x48:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("f00", data.Array.TF32(0x00))
                                    .Add("f04", data.Array.TF32(0x04))
                                    .Add("f08", data.Array.TF32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("i10", data.Array.TI32(0x10))
                                    .Add("i14", data.Array.TI32(0x14))
                                    .Add("i18_8", data.Array.TU8(0x18))
                                    .Add("i19_8", data.Array.TU8(0x19))
                                    .Add("i1A_8", data.Array.TU8(0x1A)));
                                break;
                            case 0x49: // RobHandAdjust
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Hand", data.Array.TI16(0x00))
                                    .Add("ScaleSelect", data.Array.TI16(0x02))
                                    .Add("Duration", data.Array.TF32(0x04))
                                    .Add("Type", data.Array.TI16(0x08))
                                    .Add("Scale", data.Array.TF32(0x0C))
                                    .Add("RotBlend", data.Array.TF32(0x10))
                                    .Add("OffsetX", data.Array.TF32(0x14))
                                    .Add("OffsetY", data.Array.TF32(0x18))
                                    .Add("OffsetZ", data.Array.TF32(0x1C))
                                    .Add("EnableScale", data.Array.TU8(0x20) != 0)
                                    .Add("DisableX", data.Array.TU8(0x21) != 0)
                                    .Add("DisableY", data.Array.TU8(0x22) != 0)
                                    .Add("DisableZ", data.Array.TU8(0x23) != 0)
                                    .Add("OffsetBlend", data.Array.TF32(0x24))
                                    .Add("ArmLength", data.Array.TF32(0x28))
                                    .Add("i2C", data.Array.TI32(0x2C)));
                                break;
                            case 0x4A:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_8", data.Array.TU8(0x00))
                                    .Add("i01_8", data.Array.TU8(0x01)));
                                break;
                            case 0x4B: // RobAdjustGlobal
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("TransitionDuration", data.Array.TI32(0x00))
                                    .Add("Type", data.Array.TU8(0x04))
                                    .Add("CycleType", data.Array.TU8(0x05))
                                    .Add("ExternalForceX", data.Array.TF32(0x08))
                                    .Add("ExternalForceY", data.Array.TF32(0x0C))
                                    .Add("ExternalForceZ", data.Array.TF32(0x10))
                                    .Add("ExternalForceCycleStrengthX", data.Array.TF32(0x14))
                                    .Add("ExternalForceCycleStrengthY", data.Array.TF32(0x18))
                                    .Add("ExternalForceCycleStrengthZ", data.Array.TF32(0x1C))
                                    .Add("ExternalForceCycleX", data.Array.TF32(0x20))
                                    .Add("ExternalForceCycleY", data.Array.TF32(0x24))
                                    .Add("ExternalForceCycleZ", data.Array.TF32(0x28))
                                    .Add("Cycle", data.Array.TF32(0x2C))
                                    .Add("Phase", data.Array.TF32(0x30)));
                                break;
                            case 0x4C: // RobArmAdjust
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TI16(0x00))
                                    .Add("Duration", data.Array.TF32(0x04))
                                    .Add("Value", data.Array.TF32(0x08)));
                                break;
                            case 0x4D: // DisableEyeMotion
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Value", data.Array.TU8(0x00) != 0));
                                break;
                            case 0x4E:
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("i00_16", data.Array.TI16(0x00))
                                    .Add("i02_8", data.Array.TU8(0x02))
                                    .Add("i03_8", data.Array.TU8(0x03))
                                    .Add("i04", data.Array.TI32(0x04))
                                    .Add("i08", data.Array.TI32(0x08))
                                    .Add("i0C", data.Array.TI32(0x0C))
                                    .Add("i10", data.Array.TI32(0x10))
                                    .Add("i14", data.Array.TI32(0x14))
                                    .Add("i18", data.Array.TI32(0x18)));
                                break;
                            case 0x4F: // RobCharaColiRing
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Index", data.Array.TU8(0)));
                                break;
                            case 0x50: // AdjustGetGlobalTrans
                                arrayEntry.Add(new MsgPack("Data")
                                    .Add("Value", data.Array.TU8(0) != 0));
                                break;
                            case 0x40: // WindReset
                            default:
                                if (data.Array != null && data.Array.Length > 0)
                                {
                                    MsgPack arrayArray = new MsgPack(data.Array.Length, "Array");
                                    for (i1 = 0; i1 < data.Array.Length; i1++)
                                        arrayArray[i1] = data.Array[i1];
                                    arrayEntry.Add(arrayArray);
                                }
                                break;
                        }

                        array[i0] = arrayEntry;
                    }
                    motion.Add(array);
                }

                if (Header.Data[i].Array2 != null && Header.Data[i].Array2.Length > 0)
                {
                    MsgPack array2 = new MsgPack(Header.Data[i].Array2.Length, "Array2");
                    for (i0 = 0; i0 < Header.Data[i].Array2.Length; i0++)
                    {
                        MsgPack arrayEntry = MsgPack.New.Add("Type", Header.Data[i].Array2[i0].Type);
                        if (Header.Data[i].Array2[i0].Array != null && Header.Data[i].Array2[i0].Array.Length > 0)
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
            motHead.WriteAfterAll(false, true, file, json);
        }

        private static int GetSize(Type Type) =>
            (int)Type switch
            {
                0x02 => 0x02,
                0x03 => 0x04,
                0x04 => 0x04,
                0x07 => 0x0C,
                0x08 => 0x08,
                0x0D => 0x0C,
                0x11 => 0x1C,
                0x1D => 0x3C,
                0x20 => 0x10,
                0x21 => 0x18,
                0x32 => 0x08, // SetFaceMotionId
                0x35 => 0x14, // SetFaceMottblMotion
                0x36 => 0x14, // SetHandRMottblMotion
                0x37 => 0x14, // SetHandLMottblMotion
                0x38 => 0x14, // SetMouthMottblMotion
                0x39 => 0x14, // SetEyesMottblMotion
                0x3A => 0x14, // SetEyelidMottblMotion
                0x3B => 0x04, // SetRobCharaHeadObject
                0x3C => 0x10,
                0x3D => 0x08, // SetEyelidMotionFromFace
                0x3E => 0x40, // RobPartsAdjust
                0x40 => 0x00, // WindReset
                0x41 => 0x04, // OsageReset
                0x42 => 0x04, // OsageStep
                0x43 => 0x08,
                0x44 => 0x08,
                0x45 => 0x04,
                0x46 => 0x04,
                0x47 => 0x08, // OsageMoveCancel
                0x48 => 0x1C,
                0x49 => 0x30, // RobHandAdjust
                0x4A => 0x02,
                0x4B => 0x34, // RobAdjustGlobal
                0x4C => 0x0C, // RobArmAdjust
                0x4D => 0x01, // DisableEyeMotion
                0x4E => 0x1C,
                0x4F => 0x01, // RobCharaColiRing
                0x50 => 0x01, // AdjustGetGlobalTrans
                _    => 0x00,
            };

        private static int GetSize2(int Type) =>
            (int)Type switch {
                0x00 => 0x1C,
                0x01 => 0x08,
                0x02 => 0x14,
                0x28 => 0x10,
                0x2E => 0x02,
                0x33 => 0x02,
                0x37 => 0x0C,
                0x40 => 0x0C,
                0x42 => 0x04,
                0x44 => 0x04,
                _    => 0x00,
            };

    public void Dispose()
        { if (s != null) s.D(); s = null; Header = default; }

        public struct HeaderData
        {
            public uint MotionSetID;
            public uint FirstMotionID;
            public uint LastMotionID;
            public int SubHeaderOffset;

            public Sub[] Data;

            public struct Sub
            {
                public int Offset;
                public uint MotionID;

                public int Field00;
                public int Field04;
                public int Field08;
                public int Field0C;
                public ushort Field10;
                public ushort Field12;
                public int Offset2;
                public int DataHeaderOffset;

                public Data[] Array;
                public Data2[] Array2;

                public struct Data
                {
                    public Type Type;
                    public int Frame;
                    public int Offset;

                    public byte[] Array;
                }

                public struct Data2
                {
                    public int Offset;

                    public int Type;
                    public byte[] Array;
                }
            }
        }

        public enum Type
        {
            SetFaceMotionId         = 0x32,
            SetFaceMottblMotion     = 0x35,
            SetHandRMottblMotion    = 0x36,
            SetHandLMottblMotion    = 0x37,
            SetMouthMottblMotion    = 0x38,
            SetEyesMottblMotion     = 0x39,
            SetEyelidMottblMotion   = 0x3A,
            SetRobCharaHeadObject   = 0x3B,
            SetEyelidMotionFromFace = 0x3D,
            RobPartsAdjust          = 0x3E,
            WindReset               = 0x40,
            OsageReset              = 0x41,
            OsageStep               = 0x42,
            OsageMoveCancel         = 0x47,
            RobHandAdjust           = 0x49,
            RobAdjustGlobal         = 0x4B,
            RobArmAdjust            = 0x4C,
            DisableEyeMotion        = 0x4D,
            RobCharaColiRing        = 0x4F,
            AdjustGetGlobalTrans    = 0x50,
        }
    }
}
