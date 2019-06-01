using KKdMainLib.IO;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.MessagePack
{
    public static class MPExt
    {
        public static MsgPack ReadMP(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON)
            { JSONIO IO = new JSONIO(File.OpenReader(file + ".json"));
                MsgPack = IO.Read(); IO.Close(); IO = null; }
            else
            {   MPIO IO = new   MPIO(File.OpenReader(file + ".mp"  ));
                MsgPack = IO.Read(); IO.Close(); IO = null; }
            return MsgPack;
        }

        public static void Write(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) new MsgPack().Add(mp).Write(file, JSON).Dispose();
          else                        mp .Write(file, JSON); }

        public static MsgPack Write(this MsgPack mp, string file, bool JSON = false)
        {
            if (JSON)
            { JSONIO IO = new JSONIO(File.OpenWriter(file + ".json", true));
                IO.Write(mp, true, true); IO = null; }
            else
            {   MPIO IO = new   MPIO(File.OpenWriter(file + ".mp"  , true));
                IO.Write(mp, true      ); IO = null; }
            return mp;
        }
    }
}
