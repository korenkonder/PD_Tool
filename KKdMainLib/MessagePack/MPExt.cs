using KKdMainLib.IO;
using MPIO = KKdMainLib.MessagePack.IO;

namespace KKdMainLib.MessagePack
{
    public static class MPExt
    {
        public static MsgPack ReadMPAllAtOnce(this string file, bool JSON = false)
        {
            MsgPack MsgPack;
            if (JSON)
            { JSONIO IO = new JSONIO(File.OpenReader(file + ".json", true));
                MsgPack = IO.Read(); IO.Close(); IO = null; }
            else
            {   MPIO IO = new   MPIO(File.OpenReader(file + ".mp"  , true));
                MsgPack = IO.Read(); IO.Close(); IO = null; }
            return MsgPack;
        }
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
        { if (Temp) MsgPack.New.Add(mp).Write(file, JSON).Dispose();
          else                      mp .Write(file, JSON); }

        public static MsgPack Write(this MsgPack mp, string file, bool JSON = false)
        {
            if (JSON)
            { JSONIO IO = new JSONIO(File.OpenWriter(file + ".json", true));
                IO.Write(mp, "\n", "  ").Close(); IO = null; }
            else
            {   MPIO IO = new   MPIO(File.OpenWriter(file + ".mp"  , true));
                IO.Write(mp            ).Close(); IO = null; }
            return mp;
        }

        public static void WriteAfterAll(this MsgPack mp, bool Temp, string file, bool JSON = false)
        { if (Temp) MsgPack.New.Add(mp).WriteAfterAll(file, JSON).Dispose();
          else                        mp .WriteAfterAll(file, JSON); }

        public static MsgPack WriteAfterAll(this MsgPack mp, string file, bool JSON = false)
        {
            byte[] data = null;
            if (JSON)
            { JSONIO IO = new JSONIO(File.OpenWriter());
                IO.Write(mp, true); data = IO.ToArray(true); }
            else
            {   MPIO IO = new   MPIO(File.OpenWriter());
                IO.Write(mp      ); data = IO.ToArray(true); }
            File.WriteAllBytes(file + (JSON ? ".json" : ".mp"), data);
            return mp;
        }

        public static void ToJSON   (this string file) =>
            file.ReadMP(    ).Write(file, true).Dispose();

        public static void ToMsgPack(this string file) =>
            file.ReadMP(true).Write(file      ).Dispose();
    }
}
