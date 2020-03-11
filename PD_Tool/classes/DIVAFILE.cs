using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class DIVAFILE
    {
        public static void Decrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            ulong head = reader.RU64();
            reader.C();

            if (head != 0x454C494641564944) return;
            System.Console.Title = "DIVAFILE Decrypt: " + Path.GetFileName(file);
            file.Decrypt();
        }

        public static void Encrypt(string file)
        {
            Stream reader = File.OpenReader(file);
            ulong head = reader.RU64();
            reader.C();

            if (head == 0x454C494641564944) return;
            System.Console.Title = "DIVAFILE Encrypt: " + Path.GetFileName(file);
            file.Encrypt();
        }
    }
}
