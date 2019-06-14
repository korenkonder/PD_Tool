using KKdMainLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public unsafe class VAG
    {
        private int c, d0, d1, e, g, i, i1, i2, j, s, PrNR, ShF, PrNRCount;
        private uint VBS;
        private float f;
        private ushort ch;
        private const uint BS = 28; //VAGBlockSize
        private int[] Samp1, Samp2, Samp3, Samp4;
        private int* S1Ptr, S2Ptr, S3Ptr, S4Ptr;
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

            if (reader.ReadUInt32() != 0x70474156) return;

            VAGData.Version = reader.ReadUInt32Endian(true);
            reader.ReadUInt32();
            VAGData.Size = reader.ReadUInt32Endian(true);
            VAGData.SampleRate = reader.ReadUInt32Endian(true);
            reader.ReadUInt32();
            reader.ReadUInt32();
            reader.ReadUInt16();
            VAGData.Channels = reader.ReadUInt16();
            if (VAGData.Channels < 2) VAGData.Channels = 1;
            VAGData.Name = reader.ReadString(0x10);

            bool HEVAG = VAGData.Version == 0x00020001 || VAGData.Version == 0x00030000;
            if (!HEVAG) VAGData.Channels = 1;
            ch = VAGData.Channels;
            VBS = BS * ch;

            four_bit    = new int[BS]; four_bitPtr    = four_bit   .GetPtr();
            temp_buffer = new int[BS]; temp_bufferPtr = temp_buffer.GetPtr();
            Samp1 = new int[VAGData.Channels]; S1Ptr = Samp1.GetPtr();
            Samp2 = new int[VAGData.Channels]; S2Ptr = Samp2.GetPtr();
            Samp3 = new int[VAGData.Channels]; S3Ptr = Samp3.GetPtr();
            Samp4 = new int[VAGData.Channels]; S4Ptr = Samp4.GetPtr();
            if (VAGData.Size + 0x30 > reader.LongLength) VAGData.Size = reader.UIntLength - 0x30;
            VAGData.Size = (VAGData.Size / VAGData.Channels) >> 4;
            VAGData.Flags = new byte [VAGData.Size];
            VAGData.Data = new int[VAGData.Size * VBS];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;

            if (HEVAG)
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                    for (c = 0; c < ch; c++)
                    {
                        s = reader.ReadByte();
                        PrNR = (s & 0xF0) >> 4;
                        ShF  =  s & 0x0F;
                        s = reader.ReadByte();
                        PrNR = (s & 0xF0) | PrNR;
                        VAGData.Flags[i1] = (byte)(s & 0xF);

                        for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                        {
                            s = reader.ReadByte();
                            four_bitPtr[i ] = s & 0x0F;
                            four_bitPtr[i2] = s & 0xF0;
                            four_bitPtr[i2] >>= 4;
                        }

                        HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];
                        HEVAG_3 = HEVAG3Ptr[PrNR]; HEVAG_4 = HEVAG4Ptr[PrNR];
                        tS1 = S1Ptr[c]; tS2 = S2Ptr[c]; tS3 = S3Ptr[c]; tS4 = S4Ptr[c];
                        DecodeHEVAG();
                        S1Ptr[c] = tS1; S2Ptr[c] = tS2; S3Ptr[c] = tS3; S4Ptr[c] = tS4;

                        for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                        {
                            VAGData.DataPtr[i  * ch + c] = temp_bufferPtr[i ];
                            VAGData.DataPtr[i2 * ch + c] = temp_bufferPtr[i2];
                        }

                    }
            else
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                {
                    s = reader.ReadByte();
                    PrNR = (s & 0xF0) >> 4;
                    ShF  =  s & 0x0F;
                    s = reader.ReadByte();
                    VAGData.Flags[i1] = (byte)(s & 0xF);

                    for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                    {
                        s = reader.ReadByte();
                        four_bitPtr[i ] = s & 0x0F;
                        four_bitPtr[i2] = s & 0xF0;
                        four_bitPtr[i2] >>= 4;
                    }

                    VAG_1 = HEVAG1Ptr[PrNR]; VAG_2 = HEVAG2Ptr[PrNR];
                    tS1 = S1Ptr[c]; tS2 = S2Ptr[c];

                    i = 0; i2 = 1;
                    while (i < BS)
                    {
                        d0 = four_bitPtr[i];
                        d1 = four_bitPtr[i2];
                        if (d0 > 7) d0 -= 16;
                        if (d1 > 7) d1 -= 16;
                        d0 = d0 << (20 - ShF);
                        d1 = d1 << (20 - ShF);

                        g = ((tS1 >> 8) * VAG_1 + (tS2 >> 8) * VAG_2) >> 5;
                        tS2 = tS1; tS1 = g + d0;

                        g = ((tS1 >> 8) * VAG_1 + (tS2 >> 8) * VAG_2) >> 5;
                        tS2 = tS1; tS1 = g + d1;

                        temp_bufferPtr[i ] = tS2;
                        temp_bufferPtr[i2] = tS1;
                        i  += 2;
                        i2 += 2;
                    }

                    S1Ptr[c] = tS1; S2Ptr[c] = tS2;

                    for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                    {
                        VAGData.DataPtr[i ] = temp_bufferPtr[i ];
                        VAGData.DataPtr[i2] = temp_bufferPtr[i2];
                    }
                }

            VAGData.DataPtr = VAGData.OriginDataPtr;
            reader.Close();
            Success = true;
        }

        private void DecodeHEVAG()
        {
            i = 0; i2 = 1;
            while (i < BS)
            {
                d0 = four_bitPtr[i ];
                d1 = four_bitPtr[i2];
                if (d0 > 7) d0 -= 16;
                if (d1 > 7) d1 -= 16;
                d0 = d0 << (20 - ShF);
                d1 = d1 << (20 - ShF);

                g = ((tS1 >> 8) * HEVAG_1 + (tS2 >> 8) * HEVAG_2 +
                     (tS3 >> 8) * HEVAG_3 + (tS4 >> 8) * HEVAG_4) >> 5;
                tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = g + d0;

                g = ((tS1 >> 8) * HEVAG_1 + (tS2 >> 8) * HEVAG_2 +
                     (tS3 >> 8) * HEVAG_3 + (tS4 >> 8) * HEVAG_4) >> 5;
                tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = g + d1;

                temp_bufferPtr[i ] = tS2;
                temp_bufferPtr[i2] = tS1;
                i  += 2;
                i2 += 2;
            }
        }
        
        public void WAVWriterStraight(string file, bool IgnoreEndFlags = false)
        {
            if (!Success) return;
            byte Flag = VAGData.Flags[0];
            if (Flag == 7) return;
            WAV.Header Header = new WAV.Header();
            Stream writer = File.OpenWriter(file + ".wav", true);
            writer.LongPosition = 0x2C;
            
            if (Flag < 8)
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                    {
                        f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                        writer.Write(f);
                    }
            else
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                        writer.Write(0f);

            for (i1 = 0, i2 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
            {
                Flag = VAGData.Flags[i1];
                if (!IgnoreEndFlags && Flag == 5 || Flag == 7) break;
                if (Flag < 8)
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                        {
                            f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                            writer.Write(f);
                        }
                else
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                            writer.Write(0f);

                if (!IgnoreEndFlags && Flag == 1) break;
            }
            VAGData.DataPtr = VAGData.OriginDataPtr;

            Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                VAGData.SampleRate, Size = writer.UIntPosition - 0x2C };
            writer.Write(Header, 0);
            writer.Close();
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
            writer.LongPosition = 0x2C;

            if (Flag < 8)
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                    {
                        f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                        writer.Write(f);
                    }
            else
                for (i = 0; i < BS; i++)
                    for (c = 0; c < ch; c++)
                        writer.Write(0f);

            VAGData.DataPtr += VBS;

            for (i1 = 1, i2 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
            {
                Flag = VAGData.Flags[i1];
                if (!IgnoreEndFlags && Flag == 5 || Flag == 7) break;
                else if (!IgnoreEndFlags && Flag == 6)
                {
                    Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                        VAGData.SampleRate, Size = writer.UIntPosition - 0x2C };
                    writer.Write(Header, 0);
                    writer.Close();
                    i2++;

                    writer = File.OpenWriter(file + "." + i2 + ".loop.wav", true);
                    writer.LongPosition = 0x2C;
                }

                if (!IgnoreEndFlags && Flag < 8)
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                        {
                            f = (float)(VAGData.DataPtr[i * ch + c] / 8388608.0);
                            writer.Write(f);
                        }
                else
                    for (i = 0; i < BS; i++)
                        for (c = 0; c < ch; c++)
                            writer.Write(0f);

                     if (!IgnoreEndFlags && Flag == 1) break;
                else if (!IgnoreEndFlags && Flag == 3)
                {
                    Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                        VAGData.SampleRate, Size = writer.UIntPosition - 0x2C };
                    writer.Write(Header, 0);
                    writer.Close();
                    i2++;

                    if (VAGData.Size == i1 + 1)
                        writer = File.OpenWriter();
                    else
                        writer = File.OpenWriter(file + "." + i2 + ".wav", true);
                    writer.LongPosition = 0x2C;
                }
            }
            VAGData.DataPtr = VAGData.OriginDataPtr;

            Header = new WAV.Header { Bytes = 4, Channels = ch, Format = 3, SampleRate =
                VAGData.SampleRate, Size = writer.UIntPosition - 0x2C };
            writer.Write(Header, 0);
            writer.Close();
        }

        public int WAVReaderStraight(string file, bool ExtendedFlagging = false)
        {
            Success = false;
            VAGData = new VAGFile();
            Stream reader = File.OpenReader(file + ".wav");
            WAV.Header Header = reader.ReadWAVHeader();
            if (!Header.IsSupported) { reader.Close(); return 1; }
            ch = Header.Channels;
            VBS = BS * ch;

            VAGData.Size = Header.Size / Header.Bytes;
            VAGData.Data = new int[VAGData.Size.Align(VBS)];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;
            
                 if (Header.Bytes == 1 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =      (reader.ReadByte  () - 0x80) << 16;
            else if (Header.Bytes == 2 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.ReadInt16 () << 8;
            else if (Header.Bytes == 3 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.ReadByte  () | (reader.ReadInt16() << 8);
            else if (Header.Bytes == 4 && Header.Format == 0x01)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr =       reader.ReadInt32 () >> 8;
            else if (Header.Bytes == 4 && Header.Format == 0x03)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr = (int)(reader.ReadSingle() * 8388608.0);
            else if (Header.Bytes == 8 && Header.Format == 0x03)
                for (int i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr++)
                    *VAGData.DataPtr = (int)(reader.ReadDouble() * 8388608.0);

            VAGData.Size = VAGData.Size.Align(VBS, VBS);
            VAGData.DataPtr = VAGData.OriginDataPtr;
            VAGData.Channels = ch;
            VAGData.SampleRate = Header.SampleRate;
            VAGData.Flags = new byte[VAGData.Size];
            if (ExtendedFlagging) VAGData.Flags[0] = 0x4;
            VAGData.Flags[VAGData.Size - 1] = 0x1;

            reader.Close();
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
                    {
                        files.Add(file + "." + i2 +      ".wav");
                        loop.Add(false);
                    }
                    else if (File.Exists(file + "." + i2 + ".loop.wav"))
                    {
                        files.Add(file + "." + i2 + ".loop.wav");
                        loop.Add(true);
                        HasLoop = true;
                    }
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

                if (!Header.IsSupported) { reader.Close(); return 2; }
                if (i == 0) { SampleRate = Header.SampleRate;
                    ch = Channels = Header.Channels; VBS = BS * ch; }
                if (Header.Channels   != Channels  ) { reader.Close(); return 3; }
                if (Header.SampleRate != SampleRate) { reader.Close(); return 4; }

                Sizes[i] = Header.Size / Header.Bytes;
                Size += Sizes[i].Align(VBS);
                reader.Close();
            }
            
            VAGData.Data = new int[Size];
            VAGData.DataPtr = VAGData.Data.GetPtr();
            VAGData.OriginDataPtr = VAGData.DataPtr;
            VAGData.Size = Size / VBS;
            VAGData.Flags = new byte[VAGData.Size];

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
                        *VAGData.DataPtr =      (reader.ReadByte  () - 0x80) << 16;
                else if (Header.Bytes == 2 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.ReadInt16 () << 8;
                else if (Header.Bytes == 3 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.ReadByte  () | (reader.ReadInt16() << 8);
                else if (Header.Bytes == 4 && Header.Format == 0x01)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr =       reader.ReadInt32 () >> 8;
                else if (Header.Bytes == 4 && Header.Format == 0x03)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr = (int)(reader.ReadSingle() * 8388608.0);
                else if (Header.Bytes == 8 && Header.Format == 0x03)
                    for (int i1 = 0; i1 < Sizes[i]; i1++, VAGData.DataPtr++)
                        *VAGData.DataPtr = (int)(reader.ReadDouble() * 8388608.0);
                reader.Close();

                AlignVAG = Sizes[i].Align(VBS) - Sizes[i];
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
            
            VAGData.DataPtr = VAGData.OriginDataPtr;
            VAGData.Channels = ch;
            VAGData.SampleRate = SampleRate;

            Success = true;
            return 0;
        }

        public void VAGWriter(string file, bool HEVAG = true)
        {
            if (!Success) return;
            VAGData.Name = Path.GetFileName(file);
            Stream writer = File.OpenWriter(file + ".vag", true);
            Samp1 = new int[ch]; S1Ptr = Samp1.GetPtr();
            Samp2 = new int[ch]; S2Ptr = Samp2.GetPtr();
            Samp3 = new int[ch]; S3Ptr = Samp3.GetPtr();
            Samp4 = new int[ch]; S4Ptr = Samp4.GetPtr();
            S_1 = new int[ch]; S_2 = new int[ch];
            S_3 = new int[ch]; S_4 = new int[ch];
            if (HEVAG) PrNRCount = 128;
            else       PrNRCount =   5;
            max         = new int[PrNRCount];         maxPtr = max        .GetPtr();
            error       = new int[PrNRCount];       errorPtr = error      .GetPtr();
            four_bit    = new int[BS];           four_bitPtr = four_bit   .GetPtr();
            data_buffer = new int[BS];        data_bufferPtr = data_buffer.GetPtr();
            temp_buffer = new int[BS];        temp_bufferPtr = temp_buffer.GetPtr();
            buffer = new int[PrNRCount, BS];

            writer.Write(0x70474156);
            if (HEVAG) writer.WriteEndian(0x00020001, true);
            else       writer.WriteEndian(0x00000020, true);
            writer.Write(0);
            if (HEVAG) writer.WriteEndian((VAGData.Size * ch + ch) << 4, true);
            else       writer.WriteEndian((VAGData.Size      +  1) << 4, true);
            writer.WriteEndian(VAGData.SampleRate, true);
            writer.Write(0);
            writer.Write(0);
            writer.Write((ushort)0);
            if (HEVAG) writer.Write(VAGData.Channels);
            else       writer.Write((ushort)0x1);
            writer.Write(VAGData.Name);
            writer.LongLength = 0x30;
            writer.LongPosition = 0x30;

            if (HEVAG)
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                    for (c = 0; c < ch; c++)
                    {
                        for (i = 0, s = 0; i < BS; i++)
                        {
                            data_bufferPtr[i] = VAGData.OriginDataPtr[i1 * BS * ch + i * ch + c];
                            s |= data_bufferPtr[i];
                        }

                        if (s == 0)
                        {
                            S_1[c] = S_2[c] = S_3[c] = S_4[c] = 0;
                            writer.WriteByte(0);
                            writer.WriteByte(VAGData.Flags[i1]);
                            writer.WriteByte(0);
                            writer.WriteByte(0);
                            writer.Write(0);
                            writer.Write(0);
                            writer.Write(0);
                            continue;
                        }

                        Calc4BitsHEVAG();

                        s = ((PrNR & 0xF) << 4) | (ShF & 0xF);
                        writer.WriteByte((byte)s);
                        s = (PrNR & 0xF0) | (VAGData.Flags[i1] & 0xF);
                        writer.WriteByte((byte)s);
                        for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                        {
                            s = (four_bitPtr[i2] << 4) | four_bitPtr[i];
                            writer.WriteByte((byte)s);
                        }
                    }
            else
                for (i1 = 0; i1 < VAGData.Size; i1++, VAGData.DataPtr += VBS)
                {
                    for (i = 0; i < BS; i++)
                    {
                        for (c = 0, data_bufferPtr[i] = 0, s = 0; c < ch; c++)
                            data_bufferPtr[i] += VAGData.OriginDataPtr[i1 * BS * ch + i * ch + c];
                        data_bufferPtr[i] /= ch;
                        s |=data_bufferPtr[i];
                        c = 0;
                    }

                    if (s == 0)
                    {
                        S_1[c] = S_2[c] = 0;
                        writer.WriteByte(0);
                        writer.WriteByte(VAGData.Flags[i1]);
                        writer.WriteByte(0);
                        writer.WriteByte(0);
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                        continue;
                    }

                    Calc4BitsVAG();

                    s = ((PrNR & 0xF) << 4) | (ShF & 0xF);
                    writer.WriteByte((byte)s);
                    writer.WriteByte(VAGData.Flags[i1]);
                    for (i = 0, i2 = 1; i < BS; i += 2, i2 += 2)
                    {
                        s = (four_bitPtr[i2] << 4) | four_bitPtr[i];
                        writer.WriteByte((byte)s);
                    }
                }

            if (!HEVAG) ch = 1;
            for (c = 0; c < ch; c++)
            {
                writer.Write(0x77770700);
                writer.Write(0x77777777);
                writer.Write(0x77777777);
                writer.Write(0x77777777);
            }
            VAGData.DataPtr = VAGData.OriginDataPtr;
            writer.Close();
        }

        private int min, ShM, S1, S2, S3, S4, tS1, tS2, tS3, tS4, PrNRf;
        private int VAG_1, VAG_2, HEVAG_1, HEVAG_2, HEVAG_3, HEVAG_4;
        private int[] data_buffer, error, four_bit, max, S_1, S_2, S_3, S_4, temp_buffer;
        private int[,] buffer;
        private int* data_bufferPtr, errorPtr, four_bitPtr, maxPtr, temp_bufferPtr;

        private void Calc4BitsVAG()
        {
            ShF = min = 134217728;
            for (j = 0; j < 5; j++)
            {
                maxPtr[j] = 0;
                S1 = S1Ptr[c]; S2 = S2Ptr[c];
                VAG_1 = HEVAG1Ptr[j]; VAG_2 = HEVAG2Ptr[j];
                for (i = 0; i < 28; i++)
                {
                    g = data_bufferPtr[i];
                    e = ((S1 >> 8) * VAG_1 + (S2 >> 8) * VAG_2) >> 5;
                    e = g - e;
                    if (e >  7864319) e =  7864319;
                    if (e < -7864320) e = -7864320;
                    buffer[j, i] = e;
                    if (e < 0) e = -e;
                    if (e > maxPtr[j]) maxPtr[j] = e;
                    S2 = S1; S1 = g;
                }

                if (maxPtr[j] < min) { PrNR = j; min = maxPtr[j]; }
            }
            
            S1Ptr[c] = S1; S2Ptr[c] = S2;

            ShF = 0;
            ShM = 0x4000;
            min = min >> 8;
            
            while (ShF < 12)
            {
                e = min + (ShM >> 3);
                if ((ShM & e) == ShM)
                    break;
                ShF++;
                ShM >>= 1;
            }

            S1 = S_1[c]; S2 = S_2[c];
            for (i = 0; i < 28; i++)
            {
                g = buffer[PrNR, i];
                e = ((S1 >> 8) * HEVAG1Ptr[PrNR] + (S2 >> 8) * HEVAG2Ptr[PrNR]) >> 5;
                e = g - e;

                d1 = e << ShF;
                d0 = (int)((uint)d1 + 0x80000) >> 20;
                if (d0 >  7) d0 =  7;
                if (d0 < -8) d0 = -8;
                four_bitPtr[i] = d0 & 0xF;
                d0 = d0 << (20 - ShF);

                S2 = S1; S1 = d0 - e;
            }
            S_1[c] = S1; S_2[c] = S2;
        }
        
        private void Calc4BitsHEVAG()
        {
            PrNRf = 0;
            min   = 134217728;
            for (j = 0; j < PrNRCount; j++)
            {
                PrNR = j;

                Calc4Bits_HEVAG();

                tS1 = S1Ptr[c]; tS2 = S2Ptr[c]; tS3 = S3Ptr[c]; tS4 = S4Ptr[c];
                DecodeHEVAG();
                i = 0;
                errorPtr[j] = 0;
                while (i < BS)
                {
                    e = data_bufferPtr[i] - temp_bufferPtr[i];
                    if (e < 0) e = -e;
                    errorPtr[j] += e;
                    i++;
                }

                if (errorPtr[j] < min) { PrNRf = j; min = errorPtr[j]; }
            }
            PrNR = PrNRf;

            Calc4Bits_HEVAG();
            S1Ptr[c] =  S1; S2Ptr[c] =  S2; S3Ptr[c] =  S3; S4Ptr[c] =  S4;
            S_1  [c] = tS1; S_2  [c] = tS2; S_3  [c] = tS3; S_4  [c] = tS4;
        }

        private void Calc4Bits_HEVAG()
        {
            S1 = S1Ptr[c]; S2 = S2Ptr[c]; S3 = S3Ptr[c]; S4 = S4Ptr[c];
            HEVAG_1 = HEVAG1Ptr[PrNR]; HEVAG_2 = HEVAG2Ptr[PrNR];
            HEVAG_3 = HEVAG3Ptr[PrNR]; HEVAG_4 = HEVAG4Ptr[PrNR];

            i = 0;
            maxPtr[PrNR] = 0;
            while (i < BS)
            {
                g = data_bufferPtr[i];
                e = ((S1 >> 8) * HEVAG_1 + (S2 >> 8) * HEVAG_2 +
                     (S3 >> 8) * HEVAG_3 + (S4 >> 8) * HEVAG_4) >> 5;
                e = g - e;
                if (e >  7864319) e =  7864319;
                if (e < -7864320) e = -7864320;
                temp_bufferPtr[i] = e;
                if (e < 0) e = -e;
                if (e > maxPtr[PrNR]) maxPtr[PrNR] = e;
                S4 = S3; S3 = S2; S2 = S1; S1 = g;
                i++;
            }

            for (ShF = 0, ShM = 0x400000; ShF < 15; ShF++, ShM >>= 1)
            { e = maxPtr[PrNR] + (ShM >> 3); if ((ShM & e) == ShM) break; }

            tS1 = S_1[c]; tS2 = S_2[c]; tS3 = S_3[c]; tS4 = S_4[c];
            i = 0;
            while (i < BS)
            {
                g = temp_bufferPtr[i];
                e = ((tS1 >> 8) * HEVAG_1 + (tS2 >> 8) * HEVAG_2 +
                     (tS3 >> 8) * HEVAG_3 + (tS4 >> 8) * HEVAG_4) >> 5;
                e = g - e;

                d1 = e << ShF;
                d0 = (d1 + 0x80000) >> 20;
                if (d0 >  7) d0 =  7;
                if (d0 < -8) d0 = -8;
                four_bitPtr[i] = d0 & 0xF;
                d0 = d0 << (20 - ShF);

                tS4 = tS3; tS3 = tS2; tS2 = tS1; tS1 = d0 - e;
                i++;
            }
        }

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
