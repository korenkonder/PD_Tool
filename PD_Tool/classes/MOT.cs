using System;
using KKdMainLib;
using KKdMainLib.IO;

namespace PD_Tool
{
    public class MOT
    {
        public static void Processor(bool json)
        {
            Console.Title = "MOT Converter";
            Program.Choose(1, "bin", out string[] fileNames);
            if (fileNames.Length < 1) return;

            string filepath, ext;
            Mot mot;
            foreach (string file in fileNames)
                using (mot = new Mot())
                {
                    ext = Path.GetExtension(file);
                    filepath = file.Replace(ext, "");
                    ext = ext.ToLower();

                    Console.Title = "MOT Converter: " + Path.GetFileNameWithoutExtension(file);
                    if (ext == ".bin")
                    {
                        /*KKdBaseLib.KKdDict<int, int> a = KKdBaseLib.KKdDict<int, int>.New;
                        while (true)
                        {
                            string b = Console.ReadLine();
                            if (b == "") break;
                            string[] c = b.Split(' ');
                            if (c.Length == 2)
                                a.Add(int.Parse(c[0]), int.Parse(c[1]));
                        }
                        a.Add(0, 1);
                        a.Add(1648, 3);
                        a.Add(2139, 2);
                        a.Add(5416, 4);
                        a.Add(6589, 5);
                        a.Add(7783, 1);
                        mot.MOTReader(filepath);

                        Mot.KeySet[] oks = mot.MOT[1].KeySet.V;
                        Mot.MotHeader mh = mot.MOT[1];
                        mh.KeySet.V = new Mot.KeySet[mh.KeySet.V.Length];
                        Mot.KeySet[] ok = mh.KeySet.V;
                        for (int i = 0; i < mh.KeySet.V.Length; i++)
                        {
                            KKdBaseLib.Interpolation.PDI[] pdi = new KKdBaseLib.Interpolation.PDI[mot.MOT.Length];
                            for (int h = 0; h < mot.MOT.Length; h++)
                                pdi[h] = new KKdBaseLib.Interpolation.PDI(mot.MOT[h].KeySet.V[i].Keys);

                            KKdBaseLib.KKdList<KKdBaseLib.KFT2> kf = KKdBaseLib.KKdList<KKdBaseLib.KFT2>.New;

                            bool old = false;
                            KKdBaseLib.KFT2 v = default;
                            KKdBaseLib.KFT2 ov = default;
                            int k = -1;
                            for (int h = 0; h < mh.FrameCount; h++)
                            {
                                if (a.ContainsKey(h))
                                    k = a[h];
                                else if (k == -1) continue;

                                ref KKdBaseLib.Interpolation.PDI pdil = ref pdi[k];

                                v.F = h;
                                v.V = pdil.SetFrame(h);
                                if (h == 0 || ov.V != v.V)
                                {
                                    if (old) { old = false; kf.Add(ov); }
                                    kf.Add(v);
                                }
                                else old = true;
                                ov = v;
                            }
                            kf.Capacity = kf.Count;
                            ok[i].Keys = kf.ToArray();
                            ok[i].Type = Mot.KeySetType.Linear;

                            for (int h = 0; h < mot.MOT.Length; h++)
                                pdi[h] = default;
                        }
                        for (int i = 0; i < mh.KeySet.V.Length; i++)
                        {
                            if (ok[i].Keys.Length != 1) continue;

                            if (ok[i].Keys[0].V == 0)
                            {
                                ok[i].Keys[0].F = 0;
                                ok[i].Type = Mot.KeySetType.None;
                            }
                            else ok[i].Type = Mot.KeySetType.Static;
                        }
                        mot.MOT[1].KeySet.V = ok;
                        Array.Resize(ref mot.MOT, 2);
                        mot.MOTWriter(filepath + "_mod");*/
                        mot.    MOTReader(filepath);
                        mot.MsgPackWriter(filepath, json);
                    }
                    else if (ext == ".mp" || ext == ".json")
                    {
                        mot.MsgPackReader(filepath, ext == ".json");
                        mot.    MOTWriter(filepath);
                    }
                }
        }
    }
}
