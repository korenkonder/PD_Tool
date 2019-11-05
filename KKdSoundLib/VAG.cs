using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public unsafe class VAG : System.IDisposable
    {
        private int c, d0, d1, e, g, i, i1, i2, j, s, PrNR, ShF, PrNRCount;
        private uint VBS;
        private float f;
        private ushort ch;
        private const uint BS = 28; //VAGBlockSize
        private bool Success = false;

        public VAGFile VAGData = new VAGFile();

        public VAG() { VAGData = new VAGFile();
            HEVAG1Ptr = HEVAG1.GetPtr(); HEVAG2Ptr = HEVAG2.GetPtr();
            HEVAG3Ptr = HEVAG3.GetPtr(); HEVAG4Ptr = HEVAG4.GetPtr(); }
        
        public void VAGReader(string file)
        {
            Success = false;
            if (!File.Exists(file + ".vag")) return;
            
            VAGData = new VAGFile();
            Stream reader = File.OpenReader(file + ".vag", true);

            if (reader.RU32() != 0x70474156) return;

            VAGData.Version = reader.RU32E(true);
            reader.RU32();
            VAGData.Size = reader.RU32E(true);
            VAGData.SampleRate = reader.RU32E(true);
            reader.RU32();
            reader.RU32();
            reader.RU16();
            VAGData.Channels = reader.RU16();
            if (VAGData.Channels < 2) VAGData.Channels = 1;
            VAGData.Name = reader.RS(0x10);

            bool HEVAG = VAGData.Version == 0x00020001 || VAGData.Version == 0x00030000;
            if (!HEVAG) VAGData.Channels = 1;
            ch = VAGData.Channels;
            VBS = BS * ch;

            four_bit    = new int[BS]; four_bitPtr    = four_bit   .GetPtr();
            temp_buffer = new int[BS]; temp_bufferPtr = temp_buffer.GetPtr();
            S1 = new int[VAGData.Channels]; S1Ptr = S1.GetPtr();
            S2 = new int[VAGData.Channels]; S2Ptr = S2.GetPtr();
            S3 = new int[VAGData.Channels]; S3Ptr = S3.GetPtr();
            S4 = new int[VAGData.Channels]; S4Ptr = S4.GetPtr();
            if (VAGData.Size + 0x30 > reader.I64L) VAGData.Size = reader.U32L - 0x30;
            VAGData.Size = (VAGData.Size / VAGData.Channels) >> 4;
            VAGData.Flags = new byte [VAGData.Size];
            VAGData.Data = new int[VAGData.Size * VBS];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;

            if (HEVAG)
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                    for (c = 0; c < ch; c++)
                    {
                        s = reader.RU8();
                        PrNR = (s & 0xF0) >> 4;
                        ShF  =  s & 0x0F;
                        s = reader.RU8();
                        PrNR = (s & 0xF0) | PrNR;
                        VAGData.Flags[i1] = (byte)(s & 0xF);

                        for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                        {
                            s = reader.RU8();
                            four_bitPtr[i ] = s & 0x0F;
                            four_bitPtr[i2] = s & 0xF0;
                            four_bitPtr[i2] >>= 4;
                        }

                        HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];
                        HEVAG_3 = HEVAG3Ptr[PrNR]; HEVAG_4 = HEVAG4Ptr[PrNR];
                        tS1 = S1Ptr[c]; tS2 = S2Ptr[c]; tS3 = S3Ptr[c]; tS4 = S4Ptr[c];
                        ShFf = 20 - ShF;
                        for (i = 0; i < BS; i++)
                        {
                            d0 = four_bitPtr[i];
                            d0 = (d0 > 7 ? d0 - 16 : d0) << ShFf;

                            g = ((tS1 * HEVAG_1 + tS2 * HEVAG_2 +
                                  tS3 * HEVAG_3 + tS4 * HEVAG_4) >> 5) + d0;
                            tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = g >> 8;
                            VAGData.DataPtr[i * ch + c] = g;
                        }
                        S1Ptr[c] = tS1; S2Ptr[c] = tS2; S3Ptr[c] = tS3; S4Ptr[c] = tS4;
                    }
            else
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                {
                    s = reader.RU8();
                    PrNR = (s & 0xF0) >> 4;
                    ShF  =  s & 0x0F;
                    s = reader.RU8();
                    VAGData.Flags[i1] = (byte)(s & 0xF);

                    for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                    {
                        s = reader.RU8();
                        four_bitPtr[i ] = s & 0x0F;
                        four_bitPtr[i2] = s & 0xF0;
                        four_bitPtr[i2] >>= 4;
                    }

                    HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];
                    tS1 = S1Ptr[c]; tS2 = S2Ptr[c];
                    ShFf = 20 - ShF;
                    i = 0;
                    for (i = 0; i < BS; i++)
                    {
                        d0 = four_bitPtr[i];
                        d0 = (d0 > 7 ? d0 - 16 : d0) << ShFf;

                        g = ((tS1 * HEVAG_1 + tS2 * HEVAG_2) >> 5) + d0;
                        tS2 = tS1; tS1 = g >> 8;
                        VAGData.DataPtr[i * ch + c] = g;
                    }
                    S1Ptr[c] = tS1; S2Ptr[c] = tS2;
                }

            reader.C();
            Success = true;
        }
        
        public void WAVWriterStraight(string file, bool IgnoreEndFlags = false)
        {
            if (!Success) return;
            byte Flag = VAGData.Flags[0];
            if (Flag == 7) return;
            WAV.Header Header = new WAV.Header();
            Stream writer = File.OpenWriter(file + ".wav", true);
            writer.I64P = 0x2C;

            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;
            if (Flag < 8)
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                    {
                        f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                        writer.W(f);
                    }
            else
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                        writer.W(0f);

            for (i1 = 0, i2 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
            {
                Flag = VAGData.Flags[i1];
                if (!IgnoreEndFlags && (Flag == 5 || Flag == 7)) break;
                if (Flag < 8)
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                        {
                            f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                            writer.W(f);
                        }
                else
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                            writer.W(0f);

                if (!IgnoreEndFlags && Flag == 1) break;
            }

            Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                VAGData.SampleRate, Size = writer.U32P - 0x2C };
            writer.W(Header, 0);
            writer.C();
        }

        public void WAVWriter(string file, bool IgnoreEndFlags = false)
        {
            if (!Success) return;
            byte Flag = VAGData.Flags[0];
            if (Flag == 7) return;
            WAV.Header Header = new WAV.Header();
            Stream writer;
            if (!IgnoreEndFlags && Flag == 6) writer = File.OpenWriter(file + ".loop.0.wav", true);
            else                              writer = File.OpenWriter(file +      ".0.wav", true);
            writer.I64P = 0x2C;

            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;
            if (Flag < 8)
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                    {
                        f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                        writer.W(f);
                    }
            else
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                        writer.W(0f);

            VAGData.DataPtr += VBS;

            for (i1 = 1, i2 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
            {
                Flag = VAGData.Flags[i1];
                if (!IgnoreEndFlags && (Flag == 5 || Flag == 7)) break;
                else if (!IgnoreEndFlags && Flag == 6)
                {
                    Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                        VAGData.SampleRate, Size = writer.U32P - 0x2C };
                    writer.W(Header, 0);
                    writer.C();
                    i2++;

                    writer = File.OpenWriter(file + "." + i2 + ".loop.wav", true);
                    writer.I64P = 0x2C;
                }

                if (!IgnoreEndFlags && Flag < 8)
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                        {
                            f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                            writer.W(f);
                        }
                else
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                            writer.W(0f);

                     if (!IgnoreEndFlags && Flag == 1) break;
                else if (!IgnoreEndFlags && Flag == 3)
                {
                    Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                        VAGData.SampleRate, Size = writer.U32P - 0x2C };
                    writer.W(Header, 0);
                    writer.C();
                    i2++;

                    if (VAGData.Size == i1 + 1)
                        writer = File.OpenWriter();
                    else
                        writer = File.OpenWriter(file + "." + i2 + ".wav", true);
                    writer.I64P = 0x2C;
                }
            }

            Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                VAGData.SampleRate, Size = writer.U32P - 0x2C };
            writer.W(Header, 0);
            writer.C();
        }

        public int WAVReaderStraight(string file, bool ExtendedFlagging = false)
        {
            Success = false;
            VAGData = new VAGFile();
            Stream reader = File.OpenReader(file + ".wav");
            WAV.Header Header = reader.ReadWAVHeader();
            if (!Header.IsSupported) { reader.C(); return 1; }
            ch = Header.Channels;
            VBS = BS * ch;

            VAGData.Size = Header.Size / Header.Bytes;
            VAGData.Data = new int[VAGData.Size.A(VBS)];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;
            
                 if (Header.Bytes == 1 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =      (reader.RU8  () - 0x80) << 16;
            else if (Header.Bytes == 2 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.RI16 () << 8;
            else if (Header.Bytes == 3 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.RU8  () | (reader.RI16() << 8);
            else if (Header.Bytes == 4 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.RI32 () >> 8;
            else if (Header.Bytes == 4 && Header.Format == 0x03)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr = (int)(reader.RF32() * 8388608.0);
            else if (Header.Bytes == 8 && Header.Format == 0x03)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr = (int)(reader.RF64() * 8388608.0);

            VAGData.Size = VAGData.Size.A(VBS, VBS);
            VAGData.Channels = ch;
            VAGData.SampleRate = Header.SampleRate;
            VAGData.Flags = new byte[VAGData.Size];
            if (ExtendedFlagging) VAGData.Flags[0] = 0x4;
            VAGData.Flags[VAGData.Size - 1] = 0x1;

            reader.C();
            Success = true;
            return 0;
        }

        public int WAVReader(string file, bool ExtendedFlagging = false)
        {
            Success = false;
            string[] Files;
            bool HasLoop = false;
            bool[] Loop;
            {
                if (!file.EndsWith(".0")) return WAVReaderStraight(file);
                file = file.Remove(file.Length - 2);
                i2 = 0;
                System.Collections.Generic.List<string> files = 
                    new System.Collections.Generic.List<string>();
                System.Collections.Generic.List<  bool>  loop =
                    new System.Collections.Generic.List<  bool>();
                while (true)
                {
                         if (File.Exists(file + "." + i2 +      ".wav"))
                    { files.Add(file + "." + i2 +      ".wav"); loop.Add(false);                 }
                    else if (File.Exists(file + "." + i2 + ".loop.wav"))
                    { files.Add(file + "." + i2 + ".loop.wav"); loop.Add( true); HasLoop = true; }
                    else break;
                    i2++;
                }
                Files = files.ToArray();
                Loop  = loop .ToArray();
            }

            uint[] Sizes = new uint[Files.Length];
            ushort Channels = 0;
            uint AlignVAG, Size = 0, SampleRate = 0;

            VAGData = new VAGFile();
            Stream reader;
            WAV.Header Header;
            for (i = 0; i < Files.Length; i++)
            {
                reader = File.OpenReader(Files[i]);
                Header = reader.ReadWAVHeader();

                if (!Header.IsSupported) { reader.C(); return 2; }
                if (i == 0) { SampleRate = Header.SampleRate;
                    ch = Channels = Header.Channels; VBS = BS * ch; }
                if (Header.Channels   != Channels  ) { reader.C(); return 3; }
                if (Header.SampleRate != SampleRate) { reader.C(); return 4; }

                Sizes[i] = Header.Size / Header.Bytes;
                Size += Sizes[i].A(VBS);
                reader.C();
            }
            
            VAGData.Size = Size / VBS;
            VAGData.Data = new int[Size];
            VAGData.Flags = new byte[VAGData.Size];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;

            if (HasLoop)
                for (i = 0; i < VAGData.Size; i++)
                    VAGData.Flags[i] = 0x2;

            i2 = 0;
            uint Start = 0, End = 0;
            for (i = 0; i < Files.Length; i++)
            {
                reader = File.OpenReader(Files[i]);
                Header = reader.ReadWAVHeader();
                     if (Header.Bytes == 1 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =      (reader.RU8  () - 0x80) << 16;
                else if (Header.Bytes == 2 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.RI16 () << 8;
                else if (Header.Bytes == 3 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.RU8  () | (reader.RI16() << 8);
                else if (Header.Bytes == 4 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.RI32 () >> 8;
                else if (Header.Bytes == 4 && Header.Format == 0x03)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr = (int)(reader.RF32() * 8388608.0);
                else if (Header.Bytes == 8 && Header.Format == 0x03)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr = (int)(reader.RF64() * 8388608.0);
                reader.C();

                AlignVAG = Sizes[i].A(VBS) - Sizes[i];
                VAGData.DataPtr += AlignVAG;

                AlignVAG = (Sizes[i] + AlignVAG) / VBS;
                End += AlignVAG;
                End--;
                     if (Loop[i]         ) { VAGData.Flags[Start] = 0x6; VAGData.Flags[End] = 0x3; }
                else if (ExtendedFlagging)   VAGData.Flags[Start] = 0x4;
                if (i + 1 == Files.Length && !Loop[i]) VAGData.Flags[End] = 0x1;
                Start += AlignVAG;
                End++;
            }
            
            VAGData.Channels = ch;
            VAGData.SampleRate = SampleRate;

            Success = true;
            return 0;
        }

        public void VAGWriter(string file, string choose)
        {
            bool HEVAG = choose != "1";
            if (!Success) return;
            byte[] data = new byte[0x30 + ((HEVAG ? (VAGData.Size * ch + ch) : (VAGData.Size + 1)) << 4)];
            Stream writer = File.OpenWriter(data);
            VAGData.Name = Path.GetFileName(file);
            S1 = new int[ch]; S2 = new int[ch];
            S3 = new int[ch]; S4 = new int[ch]; 
            S_1 = new int[ch]; S_2 = new int[ch];
            S_3 = new int[ch]; S_4 = new int[ch];
            PrNRCount = choose == "1" ? 5 : choose == "2" ? 8 : choose == "3" ? 32 :
                choose == "4" ? 64 : choose == "5" ? 96 : 128;
            error = new int[PrNRCount]; four_bit = new int[BS];
            data_buffer = new int[BS]; temp_buffer = new int[BS];
            errorPtr = error.GetPtr();
            four_bitPtr = four_bit.GetPtr();
            data_bufferPtr = data_buffer.GetPtr();
            temp_bufferPtr = temp_buffer.GetPtr();
            S1Ptr = S1.GetPtr(); S2Ptr = S2.GetPtr();
            S3Ptr = S3.GetPtr(); S4Ptr = S4.GetPtr();
            S_1Ptr = S_1.GetPtr(); S_2Ptr = S_2.GetPtr();
            S_3Ptr = S_3.GetPtr(); S_4Ptr = S_4.GetPtr();

            writer.W(0x70474156);
            writer.WE(HEVAG ? 0x00020001 : 0x00000020, true);
            writer.W(0);
            writer.WE((HEVAG ? (VAGData.Size * ch + ch) : (VAGData.Size + 1)) << 4, true);
            writer.WE(VAGData.SampleRate, true);
            writer.W(0);
            writer.W(0);
            writer.W((ushort)0);
            writer.W((ushort)(HEVAG ? VAGData.Channels : 0x1));
            writer.W(VAGData.Name);
            writer.I64L = 0x30;
            writer.I64P = 0x30;

            if (HEVAG)
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                    for (c = 0; c < ch; c++)
                    {
                        for (i = 0, s = 0; i < BS; i++)
                            data_bufferPtr[i] = VAGData.OriginDataPtr[i1 * BS * ch + i * ch + c];

                        Calc4BitsHEVAG();

                        s = ((PrNR & 0xF) << 4) | (ShF & 0xF);
                        writer.W((byte)s);
                        s = (PrNR & 0xF0) | (VAGData.Flags[i1] & 0xF);
                        writer.W((byte)s);
                        for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                        {
                            s = ((four_bitPtr[i2] & 0xF) << 4) | (four_bitPtr[i] & 0xF);
                            writer.W((byte)s);
                        }
                    }
            else
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                {
                    for (i = 0; i < BS; i++)
                    {
                        for (c = 0, s = 0, data_bufferPtr[i] = 0; c < ch; c++)
                            data_bufferPtr[i] += VAGData.OriginDataPtr[i1 * BS * ch + i * ch + c];
                        data_bufferPtr[i] /= ch;
                    }

                    Calc4BitsVAG();

                    s = ((PrNR & 0xF) << 4) | (ShF & 0xF);
                    writer.W((byte)s);
                    s = VAGData.Flags[i1] & 0xF;
                    writer.W((byte)s);
                    for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                    {
                        s = ((four_bitPtr[i2] & 0xF) << 4) | (four_bitPtr[i] & 0xF);
                        writer.W((byte)s);
                    }
                }

            if (!HEVAG) ch = 1;
            for (c = 0; c < ch; c++)
            {
                writer.W(0x77770700);
                writer.W(0x77777777);
                writer.W(0x77777777);
                writer.W(0x77777777);
            }
            writer.C();
            File.WriteAllBytes(file + ".vag", data);
        }

        private int max, min, ShM, s1, s2, s3, s4, tS1, tS2, tS3, tS4, PrNRf, ShFf;
        private int HEVAG_1, HEVAG_2, HEVAG_3, HEVAG_4;
        private int[] data_buffer, error, four_bit, temp_buffer;
        private int* data_bufferPtr, errorPtr, four_bitPtr, temp_bufferPtr;
        private int[] S_1, S_2, S_3, S_4, S1, S2, S3, S4;
        private int* S_1Ptr, S_2Ptr, S_3Ptr, S_4Ptr, S1Ptr, S2Ptr, S3Ptr, S4Ptr;
        
        private void Calc4BitsVAG()
        {
            min = 0x7FFFFFFF;
            PrNRf = 0;
            for (j = 0; j < PrNRCount; j++)
            {
                PrNR = j;

                Calc4Bits_VAG();
                
                tS1 = S1Ptr[c]; tS2 = S2Ptr[c];
                errorPtr[j] = 0;
                for (i = 0; i < BS; i++)
                {
                    d0 = four_bitPtr[i];
                    d0 = (d0 > 7 ? d0 - 16 : d0) << ShFf;

                    g = ((tS1 * HEVAG_1 + tS2 * HEVAG_2) >> 5) + d0;
                    tS2 = tS1; tS1 = g >> 8;
                    e = data_bufferPtr[i] - g;
                    if (e < 0) e = -e;
                    errorPtr[j] += e;
                }

                if (errorPtr[j] < min) { PrNRf = j; min = errorPtr[j]; }
            }
            PrNR = PrNRf;

            Calc4Bits_VAG();
             S1Ptr[c] =  s1;  S2Ptr[c] =  s2;
            S_1Ptr[c] = tS1; S_2Ptr[c] = tS2;
        }

        private void Calc4Bits_VAG()
        {
            s1 = S1Ptr[c]; s2 = S2Ptr[c]; s3 = S3Ptr[c]; s4 = S4Ptr[c];
            HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];

            for (i = 0, max = 0; i < BS; i++)
            {
                g = data_bufferPtr[i];
                e = (s1 * HEVAG_1 + s2 * HEVAG_2) >> 5;
                e = g - e;
                if (e >  0x77FFFF) e =  0x77FFFF;
                if (e < -0x780000) e = -0x780000;
                temp_bufferPtr[i] = e;
                if (e < 0) e = -e;
                if (e > max) max = e;
                s4 = s3; s3 = s2; s2 = s1; s1 = g >> 8;
            }

            for (ShF = 0, ShM = 0x400000; ShF < 15; ShF++, ShM >>= 1)
            { e = max + (ShM >> 3); if ((ShM & e) == ShM) break; }
            ShFf = 20 - ShF;

            tS1 = S_1Ptr[c]; tS2 = S_2Ptr[c];
            for (i = 0; i < BS; i++)
            {
                g = temp_bufferPtr[i];
                e = (tS1 * HEVAG_1 + tS2 * HEVAG_2) >> 5;
                e = g - e;

                d1 = e << ShF;
                d0 = (d1 + 0x80000) >> 20;
                if (d0 >  7) d0 =  7;
                if (d0 < -8) d0 = -8;
                four_bitPtr[i] = d0;
                d0 <<= ShFf;

                tS2 = tS1; tS1 = (d0 - e) >> 8;
            }
        }
        
        private void Calc4BitsHEVAG()
        {
            min = 0x7FFFFFFF;
            PrNRf = 0;
            for (j = 0; j < PrNRCount; j++)
            {
                PrNR = j;

                Calc4Bits_HEVAG();
                
                tS1 = S1Ptr[c]; tS2 = S2Ptr[c]; tS3 = S3Ptr[c]; tS4 = S4Ptr[c];
                errorPtr[j] = 0;
                for (i = 0; i < BS; i++)
                {
                    d0 = four_bitPtr[i];
                    d0 = (d0 > 7 ? d0 - 16 : d0) << ShFf;

                    g = ((tS1 * HEVAG_1 + tS2 * HEVAG_2 +
                          tS3 * HEVAG_3 + tS4 * HEVAG_4) >> 5) + d0;
                    tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = g >> 8;
                    e = data_bufferPtr[i] - g;
                    if (e < 0) e = -e;
                    errorPtr[j] += e;
                }

                if (errorPtr[j] < min) { PrNRf = j; min = errorPtr[j]; }
            }
            PrNR = PrNRf;

            Calc4Bits_HEVAG();
             S1Ptr[c] =  s1;  S2Ptr[c] =  s2;  S3Ptr[c] =  s3;  S4Ptr[c] =  s4;
            S_1Ptr[c] = tS1; S_2Ptr[c] = tS2; S_3Ptr[c] = tS3; S_4Ptr[c] = tS4;
        }

        private void Calc4Bits_HEVAG()
        {
            s1 = S1Ptr[c]; s2 = S2Ptr[c]; s3 = S3Ptr[c]; s4 = S4Ptr[c];
            HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];
            HEVAG_3 = HEVAG3Ptr[PrNR]; HEVAG_4 = HEVAG4Ptr[PrNR];

            for (i = 0, max = 0; i < BS; i++)
            {
                g = data_bufferPtr[i];
                e = (s1 * HEVAG_1 + s2 * HEVAG_2 +
                     s3 * HEVAG_3 + s4 * HEVAG_4) >> 5;
                e = g - e;
                if (e >  0x77FFFF) e =  0x77FFFF;
                if (e < -0x780000) e = -0x780000;
                temp_bufferPtr[i] = e;
                if (e < 0) e = -e;
                if (e > max) max = e;
                s4 = s3; s3 = s2; s2 = s1; s1 = g >> 8;
            }

            for (ShF = 0, ShM = 0x400000; ShF < 15; ShF++, ShM >>= 1)
            { e = max + (ShM >> 3); if ((ShM & e) == ShM) break; }
            ShFf = 20 - ShF;

            tS1 = S_1Ptr[c]; tS2 = S_2Ptr[c]; tS3 = S_3Ptr[c]; tS4 = S_4Ptr[c];
            for (i = 0; i < BS; i++)
            {
                g = temp_bufferPtr[i];
                e = (tS1 * HEVAG_1 + tS2 * HEVAG_2 +
                     tS3 * HEVAG_3 + tS4 * HEVAG_4) >> 5;
                e = g - e;

                d1 = e << ShF;
                d0 = (d1 + 0x80000) >> 20;
                if (d0 >  7) d0 =  7;
                if (d0 < -8) d0 = -8;
                four_bitPtr[i] = d0;
                d0 <<= ShFf;

                tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = (d0 - e) >> 8;
            }
        }

        public void Dispose()
        { data_buffer = null; error = null; four_bit = null; temp_buffer = null;
            S_1 = null; S_2 = null; S_3 = null; S_4 = null;
            S1 = null; S2 = null; S3 = null; S4 = null; }

        public struct VAGFile
        {
            public uint Size;
            public uint Version;
            public uint SampleRate;
            public ushort Channels;
            public string Name;
            public byte[] Flags;
            public int[] Data;
            public int* DataPtr;
            public int* OriginDataPtr;
        }

        private int* HEVAG1Ptr, HEVAG2Ptr, HEVAG3Ptr, HEVAG4Ptr;

        private readonly int[] HEVAG1 = new int[]
        {
                 0,   7680,  14720,  12544,  15616,  14731,  14507,  13920,
             13133,  12028,  10764,   9359,   7832,   6201,   4488,   2717,
               910,   -910,  -2717,  -4488,  -6201,  -7832,  -9359, -10764,
            -12028, -13133, -13920, -14507, -14731,   5376,  -6400, -10496,
              -167,  -7430,  -8001,   6018,   3798,  -8237,   9199,  13021,
             13112,  -1668,   7819,   9571,  10032,  -4745,  -5896,  -1193,
              2783,  -7334,   6127,   9457,   7876,  -7172,  -7358,  -9170,
             -2638,   1873,   9214,  13204,  12437,  -2653,   9331,   1642,
              4246,  -8988,  -2562,   3182,   7937,  10069,   8400,  -8529,
              9477,     75,  -9143,  -7270,  -2740,   8993,  13101,   9543,
              5272,  -7696,   7309,  10275,  10940,     24,  -8122,  -8511,
               326,   8895,  12073,   8729,  12950,  10038,   9385,  -4720,
              7869,   2450,  10192,  11313,  10154,   9638,   3854,   6699,
             11082,  -1026,  10396,  10287,   7953,  12689,   6641,  -2348,
              9290,   4633,  11247,   9807,   9736,   8440,   9307,   1698,
             10214,   8390,   7201,    -88,   6193,  12325,  13064,   5333,
        };

        private readonly int[] HEVAG2 = new int[]
        {
                0,     0, -6656, -7040, -7680, -7059, -7366, -7522,
            -7680, -7680, -7680, -7680, -7680, -7680, -7680, -7680,
            -7680, -7680, -7680, -7680, -7680, -7680, -7680, -7680,
            -7680, -7680, -7522, -7366, -7059, -9216, -7168, -7424,
            -2722, -2221, -3166, -4750, -6946, -2596,  1982, -3044,
            -4487, -3744, -4328, -1336, -2562, -4122,  2378, -9117,
            -7108, -2062, -2577, -1858, -4483, -1795, -2102, -3509,
            -2647,  9183,  1859, -3012, -4792, -1144, -1048,  -620,
            -7585, -3891, -2735,  -483, -3844, -2609, -3297, -2775,
            -1882, -2241, -4160, -1958,  3745,  1948, -2835, -1961,
            -4270, -3383,  2523, -2867, -3721,  -310, -2411, -3067,
            -3846,  2194, -1876, -3423, -3847, -2570, -2757, -5006,
            -4326, -8597, -2763, -4213, -2716, -1417, -4554, -5659,
            -3908, -9810, -3746,   988,  3878, -3375,  3166, -7354,
            -4039, -6403, -4125, -2284, -1536, -3436, -1021, -9025,
            -2791,  3248,  3316, -7809, -5189, -1290, -4075,  2999,
        };

        private readonly int[] HEVAG3 = new int[]
        {
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0,  3328, -3328, -3584,
             -494, -2298, -2814,  2649,  3875, -2071, -1382, -3792,
            -2250, -6456,  2111,  -757,   300, -5486, -4787, -1237,
            -1575, -2212,  -315,   102,  2126, -2069, -2233, -2674,
            -1929,  1860, -1124, -4139,  -256, -3182,  -828,  -946,
             -533, -2807, -1730,  -714,  2821,   314,  1551, -2432,
              108,  -298, -2963, -2156,  5936,  -683, -3854,   130,
             3124, -2907,   434,   391,   665, -1262, -2311, -2337,
              419,  -541, -2017,  1674, -3007,   302,  1008, -2852,
             2135,  1299,   360,   833,   345,  -737,  2843,  2249,
              728,  -805,  1367, -1915,  -764, -3354,   231, -1944,
             1885,  1748,   802,   219,  -706,  1562,  -835,   688,
              368,  -758,    46,  -538,  2760, -3284, -2824,   775,
        };

        private readonly int[] HEVAG4 = new int[]
        {
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0,     0,     0,     0,
                0,     0,     0,     0,     0, -3072, -2304, -1024,
             -541,   424,   289, -1298, -1216,   227, -2316,  1267,
             1665,   840,  -506,   487,   199, -1493, -6947, -3114,
            -1447,   446,   -18,   258,  -538,   482,   440,  -391,
            -1637, -5746, -2427,  1370,   622, -6878,   507, -4229,
            -2259,    44, -1899, -1421, -1019,   195,  -155,  -336,
              256, -6937,     5,   460, -1089, -2704,  1055,   250,
            -3157,  -456, -2461,   172,    97,   320,  -271,   163,
             -933, -2880,  -601,  -169,  1946,   198,    41, -1161,
             -501, -2780,   181,    53,   185,   482, -3397, -1074,
               80, -3462,   -96, -1437, -3263,  2079, -2089, -4122,
             -246, -1619,    61,   222,   473,  -176,   509, -3037,
              179, -2989, -2614, -4571, -1245,   253,  1877, -1132,
        };
    }
}
