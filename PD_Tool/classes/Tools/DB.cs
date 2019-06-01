using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool.Tools
{
    public class DB
    {
        public static void Processor()
        {
            Console.Title = "DataBank Converter";
            Main.Choose(1, "databank", out string[] FileNames);

            DataBank DB;
            string[] file_split;
            int File_Checksum = 0, Get_Checksum;
            foreach (string file in FileNames)
                //try
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    file_split = filename.Split('_');
                    DB = new DataBank();
                    if (file_split.Length == 5 && file.EndsWith(".dat"))
                    {
                        if (int.TryParse(file_split[3], out File_Checksum))
                        {
                            Get_Checksum = DCC.CalculateChecksum(file);
                            if (File_Checksum == Get_Checksum)
                            {
                                string filepath = file.Replace(filename + ".dat", "");
                                Console.Title = "DataBank Converter: " + filename;
                                DB.DBReader(file);
                                DB.XMLWriter(filepath + file_split[0] + "_" +
                                    file_split[1] + "_" + file_split[2] + ".xml");
                            }
                        }
                    }
                    else if (file.EndsWith(".xml"))
                    {
                        string filepath = file.Replace(Path.GetExtension(file), "");
                        Console.Title = "DataBank Converter: " + Path.GetFileNameWithoutExtension(file);
                        DB.XMLReader(file);
                        DB.DBWriter(filepath);
                    }
                }
                //catch (Exception e) { Console.WriteLine(e.Message); }

        }
    }
}
