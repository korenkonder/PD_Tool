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

            if (reader.RS(0x04) != "DIVA") { reader.C(); return; }

            reader.RI32();
            Data.Size = reader.RI32();
            Data.SampleRate = reader.RI32();
            Data.SamplesCount = reader.RI32();
            reader.RI64();
            Data.Channels = reader.RU16();
            reader.RU16();
            Data.Name = reader.RS(0x20);

            byte value = 0;
            byte[] data = new byte[Data.SamplesCount * Data.Channels * 4];
            int[] current = new int[Data.Channels];
            int[] currentclamp = new int[Data.Channels];
            sbyte[] stepindex = new sbyte[Data.Channels];

            fixed (int* currentPtr = current)
            fixed (int* currentclampPtr = currentclamp)
            fixed (sbyte* stepindexPtr = stepindex)
            fixed (byte* ptr = data)
            {
                float* dataPtr = (float*)ptr;
                for (i = 0; i < Data.SamplesCount; i++)
                    for (c = 0; c < Data.Channels; c++, dataPtr++)
                    {
                        value = reader.RHB();
                        IMADecoder(value, ref currentPtr[c], ref currentclampPtr[c], ref stepindexPtr[c]);
                        *dataPtr = (float)(currentPtr[c] / 32768.0);
                    }
            }
            reader.CR();


            if (ToArray) Data.Data = data;
            else
            {
                Stream writer = File.OpenWriter(file + ".wav", true);
                WAV.Header Header = new WAV.Header { Bytes = 4, Channels = Data.Channels, Format = 3,
                    SampleRate = Data.SampleRate, Size = Data.SamplesCount * Data.Channels * 4 };
                writer.W(Header, 0);
                writer.W(data);
                writer.C();
            }

            reader.C();
        }

        public void DIVAWriter(string file)
        {
            if (!File.Exists(file + ".wav")) return;

            Stream reader = File.OpenReader(file + ".wav");

            Data = new DIVAFile();
            WAV.Header Header = reader.ReadWAVHeader();
            if (!Header.IsSupported) { reader.C(); return; }

            Stream writer = File.OpenWriter(file + ".diva", true);
            Data.Channels = Header.Channels;
            Data.SampleRate = Header.SampleRate;
            Data.SamplesCount = Header.Size / Header.Channels / Header.Bytes;
            writer.PI64 = 0x40;
            writer.LI64 = 0x40 + (Data.SamplesCount * Data.Channels).A(2, 2);

            byte value = 0;
            int[] sample = new int[Data.Channels];
            int[] current = new int[Data.Channels];
            int[] currentclamp = new int[Data.Channels];
            sbyte[] stepindex = new sbyte[Data.Channels];

            fixed (int* samplePtr = sample)
            fixed (int* currentPtr = current)
            fixed (int* currentclampPtr = currentclamp)
            fixed (sbyte* stepindexPtr = stepindex)
            {
                for (i = 0; i < Data.SamplesCount; i++)
                    for (c = 0; c < Header.Channels; c++)
                    {
                        samplePtr[c] = (reader.RS(Header.Bytes, Header.Format) * 0x8000).CFTI();
                        value = IMAEncoder(samplePtr[c], ref currentPtr[c],
                            ref currentclampPtr[c], ref stepindexPtr[c]);
                        writer.W(value, 4);
                    }
            }

            writer.CW();

            writer.PI64 = 0x00;
            writer.W("DIVA");
            writer.W(0x00);
            writer.W((Data.SamplesCount * Data.Channels).A(2, 2));
            writer.W(Data.SampleRate);
            writer.W(Data.SamplesCount);
            writer.W(0x00);
            writer.W(0x00);
            writer.W(Data.Channels);
            writer.C();
            reader.C();
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
            public int Size;
            public int SampleRate;
            public int SamplesCount;
            public string Name;
            public ushort Channels;
            public byte[] Data;
        }
    }
}
