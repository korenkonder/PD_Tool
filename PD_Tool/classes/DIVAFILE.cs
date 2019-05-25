using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DIVAFILE
    {
        public static void Decrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            if (reader.ReadInt64() != 0x454C494641564944)
            {
                reader.Close();
                Encrypt(file);
                return;
            }
            reader.Close();
            file.Decrypt();
        }

        public static void Encrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            if (reader.ReadInt64() == 0x454C494641564944)
            {
                reader.Close();
                Decrypt(file);
                return;
            }

            file.Encrypt();
        }
    }
}
