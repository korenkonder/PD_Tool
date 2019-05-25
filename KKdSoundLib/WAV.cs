using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKdSoundLib
{
    public static class WAV
    {
        public struct Header
        {
            public uint Size;
            public uint SampleRate;
            public uint HeaderSize;
            public uint ChannelMask;
            public bool IsSupported;
            public ushort Bytes;
            public ushort Format;
            public ushort Channels;
        }
    }
}
