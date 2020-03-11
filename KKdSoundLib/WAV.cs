namespace KKdSoundLib
{
    public static class WAV
    {
        public struct Header
        {
            public int Size;
            public int SampleRate;
            public int HeaderSize;
            public uint ChannelMask;
            public bool IsSupported;
            public ushort Bytes;
            public ushort Format;
            public ushort Channels;
        }
    }
}
