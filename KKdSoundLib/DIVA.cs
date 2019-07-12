using KKdBaseLib;
using KKdMainLib.IO;

namespace KKdSoundLib
{
    public unsafe class DIVA
    {
        public DIVAFile Data = new DIVAFile();
        private int c, i;

        public DIVA() { Data = new DIVAFile(); }

        public void DIVAReader(string file, bool ToArray = false)
        {
            if (!File.Exists(file + ".diva")) return;

            Data = new DIVAFile();
            Stream reader = File.OpenReader(file + ".diva");

            if (reader.ReadString(0x04) != "DIVA") { reader.Close(); return; }

            reader.ReadInt32();
            Data.Size = reader.ReadUInt32();
            Data.SampleRate = reader.ReadUInt32();
            Data.SamplesCount = reader.ReadUInt32();
            reader.ReadInt64();
            Data.Channels = reader.ReadUInt16();
            reader.ReadUInt16();
            Data.Name = reader.ReadString(0x20);

            Stream writer = File.OpenWriter();
            if (!ToArray) writer = File.OpenWriter(file + ".wav", true);
            writer.LongPosition = 0x2C;

            byte value = 0;
            int[] current = new int[Data.Channels];
            int[] currentclamp = new int[Data.Channels];
            sbyte[] stepindex = new sbyte[Data.Channels];
            float f;

            int* currentPtr = current.GetPtr();
            int* currentclampPtr = currentclamp.GetPtr();
            sbyte* stepindexPtr = stepindex.GetPtr();

            for (i = 0; i < Data.SamplesCount; i++)
                for (c = 0; c < Data.Channels; c++)
                {
                    value = reader.ReadHalfByte();
                    IMADecoder(value, ref currentPtr[c], ref currentclampPtr[c], ref stepindexPtr[c]);
                    f = (float)(currentPtr[c] / 32768.0);
                    writer.Write(f);
                }

            WAV.Header Header = new WAV.Header { Bytes = 4, Channels = Data.Channels, Format = 3,
                SampleRate = Data.SampleRate, Size = Data.SamplesCount * Data.Channels * 4 };
            writer.Write(Header, 0);
            if (ToArray) Data.Data = writer.ToArray();
            writer.Close();

            reader.Close();
        }

        public void DIVAWriter(string file)
        {
            if (!File.Exists(file + ".wav")) return;

            Stream reader = File.OpenReader(file + ".wav");

            Data = new DIVAFile();
            WAV.Header Header = reader.ReadWAVHeader();
            if (!Header.IsSupported) { reader.Close(); return; }

            Stream writer = File.OpenWriter(file + ".diva", true);
            Data.Channels = Header.Channels;
            Data.SampleRate = Header.SampleRate;
            writer.LongPosition = 0x40;

            byte value = 0;
            int[] sample = new int[Data.Channels];
            int[] current = new int[Data.Channels];
            int[] currentclamp = new int[Data.Channels];
            sbyte[] stepindex = new sbyte[Data.Channels];

            int* samplePtr = sample.GetPtr();
            int* currentPtr = current.GetPtr();
            int* currentclampPtr = currentclamp.GetPtr();
            sbyte* stepindexPtr = stepindex.GetPtr();
            Data.SamplesCount = Header.Size / Header.Channels / Header.Bytes;

            for (i = 0; i < Data.SamplesCount; i++)
                for (c = 0; c < Header.Channels; c++)
                {
                    samplePtr[c] = (reader.ReadWAVSample(Header.Bytes, Header.Format) * 0x8000).CFTI();
                    value = IMAEncoder(samplePtr[c], ref currentPtr[c],
                        ref currentclampPtr[c], ref stepindexPtr[c]);
                    writer.Write(value, 4);
                }
            writer.CheckWrited();

            writer.LongPosition = 0x00;
            writer.Write("DIVA");
            writer.Write(0x00);
            writer.Write((Data.SamplesCount * Data.Channels).Align(2, 2));
            writer.Write(Data.SampleRate);
            writer.Write(Data.SamplesCount);
            writer.Write(0x00);
            writer.Write(0x00);
            writer.Write(Data.Channels);
            writer.Close();
            reader.Close();
        }

        private void IMADecoder(byte value, ref int current, ref int currentclamp, ref sbyte stepindex)
        {
            step = ima_step_table[stepindex];

            diff = step >> 3;
            if ((value & 1) == 1) diff += step >> 2;
            if ((value & 2) == 2) diff += step >> 1;
            if ((value & 4) == 4) diff += step;
            
            if ((value & 8) == 8)
            {       currentclamp -=   diff; current = currentclamp;
                if (currentclamp < -0x8000) currentclamp = -0x8000; }
            else
            {       currentclamp +=   diff; current = currentclamp;
                if (currentclamp >  0x7FFF) currentclamp =  0x7FFF; }
            
            stepindex += ima_index_table[value & 7];
            if (stepindex <  0) stepindex =  0;
            if (stepindex > 88) stepindex = 88;
        }

        private int delta, diff;
        private byte value;
        private short step;

        private byte IMAEncoder(int sample, ref int current, ref int currentclamp, ref sbyte stepindex)
        {
            value = 0;
            step = ima_step_table[stepindex];
            
            delta = sample - current;
            
            if (delta < 0)
            { value |= 8; delta = -delta; }
            diff = step >> 3;
            if (delta > step)
            { value |= 4; diff += step; delta -= step; }
            step >>= 1;
            if (delta > step)
            { value |= 2; diff += step; delta -= step; }
            step >>= 1;
            if (delta > step)
            { value |= 1; diff += step; }
            
            if ((value & 8) == 8)
            {       currentclamp -=   diff; current = currentclamp;
                if (currentclamp < -0x8000) currentclamp = -0x8000; }
            else
            {       currentclamp +=   diff; current = currentclamp;
                if (currentclamp >  0x7FFF) currentclamp =  0x7FFF; }
            
            stepindex += ima_index_table[value & 0x07];
            if (stepindex <  0) stepindex =  0;
            if (stepindex > 88) stepindex = 88;
            return value;
        }

        private readonly sbyte[] ima_index_table =
        { -1, -1, -1, -1, 2, 4, 6, 8 };

        private readonly short[] ima_step_table = {
            7,     8,     9,     10,    11,    12,    13,    14,
            16,    17,    19,    21,    23,    25,    28,    31,
            34,    37,    41,    45,    50,    55,    60,    66,
            73,    80,    88,    97,    107,   118,   130,   143,
            157,   173,   190,   209,   230,   253,   279,   307,
            337,   371,   408,   449,   494,   544,   598,   658,
            724,   796,   876,   963,   1060,  1166,  1282,  1411,
            1552,  1707,  1878,  2066,  2272,  2499,  2749,  3024,
            3327,  3660,  4026,  4428,  4871,  5358,  5894,  6484,
            7132,  7845,  8630,  9493,  10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
            32767
        };

        public struct DIVAFile
        {
            public uint Size;
            public uint SampleRate;
            public uint SamplesCount;
            public string Name;
            public ushort Channels;
            public byte[] Data;
        }
    }
}
