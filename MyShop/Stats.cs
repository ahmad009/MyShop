using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using InfinityScript;

namespace MyShop
{
    public class MyStats : BaseScript
    {
        static string adminxuids;
        public static List<string> AdminsXUIDs = new List<string>();
        public static List<string> MasterAdminsXUIDs = new List<string>();

        public static void ConfigReader()
        {
            if (File.Exists("scripts\\MyShop\\Config.cfg"))
            {
                try
                {
                    string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Config.cfg");
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str = strArrays[i];
                        if (str.StartsWith("Admins="))
                        {
                            char[] chrArray = new char[] { '=' };
                            adminxuids = str.Split(chrArray)[i];

                            string[] words = adminxuids.Split(',');
                            foreach (string word in words)
                            {
                                AdminsXUIDs.Add(word);
                            }
                        }
                    }

                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str = strArrays[i];
                        if (str.StartsWith("MasterAdmins="))
                        {
                            char[] chrArray = new char[] { '=' };
                            adminxuids = str.Split(chrArray)[i];

                            string[] words = adminxuids.Split(',');
                            foreach (string word in words)
                            {
                                MasterAdminsXUIDs.Add(word);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        public static int PointsReader(Entity player)
        {
            int point = 0;
            if (File.Exists("scripts\\MyShop\\Points\\Points.txt"))
            {
                try
                {
                    string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Points\\Points.txt");
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str = strArrays[i];
                        if (str.StartsWith(player.Name + " , ID: " + player.UserID + " , Point"))
                        {
                            char[] chrArray = new char[] { '=' };
                            point = int.Parse(str.Split(chrArray)[1]);
                            MyShop.hud1.SetText(point.ToString());
                            return point;
                        }

                        if (i == ((int)strArrays.Length - 1))
                        {
                            if (str != (player.Name + " , ID: " + player.UserID + " , Point"))
                            {
                                File.AppendAllText("scripts\\MyShop\\Points\\Points.txt", string.Concat(player.Name + " , ID: " + player.UserID + " , Point=0" + "\n"));
                                MyShop.hud1.SetText(point.ToString());
                                return point;
                            }
                        }
                    }
                }
                catch { }
            }
            return point;
        }

        public static void PointWriter(Entity player)
        {
            try
            {
                string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Points\\Points.txt");
                int oldpoint = PointsReader(player);
                int newpoint = oldpoint + 100;
                if (player.GetField<string>("sessionteam") == "axis")
                {
                    newpoint = oldpoint + 150;
                }

                string str5 = File.ReadAllText("scripts\\MyShop\\Points\\Points.txt");
                str5 = str5.Replace(string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + oldpoint), string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + newpoint));
                File.WriteAllText("scripts\\MyShop\\Points\\Points.txt", str5);
                PointsReader(player);
            }
            catch { }
        }

        public static void PointEditor(Entity player, int newpoint)
        {
            try
            {
                string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Points\\Points.txt");
                int oldpoint = PointsReader(player);

                string str5 = File.ReadAllText("scripts\\MyShop\\Points\\Points.txt");
                str5 = str5.Replace(string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + oldpoint), string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + newpoint));
                File.WriteAllText("scripts\\MyShop\\Points\\Points.txt", str5);
                PointsReader(player);
            }
            catch { }
        }

        public static void PointAdd(Entity player, int point)
        {
            try
            {
                string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Points\\Points.txt");
                int oldpoint = PointsReader(player);
                int newpoint = oldpoint + point;

                string str5 = File.ReadAllText("scripts\\MyShop\\Points\\Points.txt");
                str5 = str5.Replace(string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + oldpoint), string.Concat(player.Name + " , ID: " + player.UserID + " , Point=" + newpoint));
                File.WriteAllText("scripts\\MyShop\\Points\\Points.txt", str5);
                PointsReader(player);
            }
            catch { }
        }


        public static int KillsReader(Entity player)
        {
            int Kills = 0;
            if (File.Exists("scripts\\MyShop\\Kills\\Kills.txt"))
            {
                try
                {
                    string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Kills\\Kills.txt");
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str = strArrays[i];
                        if (str.StartsWith(player.Name + " , ID: " + player.UserID + " , Kills"))
                        {
                            char[] chrArray = new char[] { '=' };
                            Kills = int.Parse(str.Split(chrArray)[1]);
                            return Kills;
                        }

                        if (i == ((int)strArrays.Length - 1))
                        {
                            if (str != (player.Name + " , ID: " + player.UserID + " , Kills"))
                            {
                                File.AppendAllText("scripts\\MyShop\\Kills\\Kills.txt", string.Concat(player.Name + " , ID: " + player.UserID + " , Kills=0" + "\n"));
                                return Kills;
                            }
                        }
                    }
                }
                catch { }
            }
            return Kills;
        }

        public static void KillsWriter(Entity player)
        {
            try
            {
                string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Kills\\Kills.txt");
                int oldKills = KillsReader(player);
                int newKill = oldKills + 1;

                string str5 = File.ReadAllText("scripts\\MyShop\\Kills\\Kills.txt");
                str5 = str5.Replace(string.Concat(player.Name + " , ID: " + player.UserID + " , Kills=" + oldKills), string.Concat(player.Name + " , ID: " + player.UserID + " , Kills=" + newKill));
                File.WriteAllText("scripts\\MyShop\\Kills\\Kills.txt", str5);
            }
            catch { }
        }

        public static int max = 0;
        public static int max1 = 0;
        public static int max2 = 0;
        public static int kills = 0;
        public static string playername = "";
        public static string playername1 = "";
        public static string playername2 = "";
        public static List<int> list = new List<int>();

        public static void HighestkillsReader()
        {
            list.Clear();
            string[] myListOfStrings = File.ReadAllLines("scripts\\MyShop\\Kills\\Kills.txt");
            foreach (string line in myListOfStrings)
            {
                char[] chrArray = new char[] { '=' };
                int nums = int.Parse(line.Split(chrArray)[1]);
                //int nums = int.Parse(line);
                list.Add(nums);
            }

            max = list.Max();
            list.Remove(list.Max());
            max1 = list.Max();
            list.Remove(list.Max());
            max2 = list.Max();

            for (int i = 0; i < (int)myListOfStrings.Length; i++)
            {
                string str = myListOfStrings[i];
                //if (str.StartsWith(" , Kills"))
                //{
                    char[] chrArray = new char[] { '=' };
                    kills = int.Parse(str.Split(chrArray)[1]);
                    if (kills == max2)
                    {
                        string text = str.Split(chrArray)[0];
                        string[] Arr = new string[] { " , ID:" };
                        string[] spl = text.Split(Arr, StringSplitOptions.RemoveEmptyEntries);
                        playername2 = spl[0];
                    }
                    if (kills == max1)
                    {
                        string text = str.Split(chrArray)[0];
                        string[] Arr = new string[] { " , ID:" };
                        string[] spl = text.Split(Arr, StringSplitOptions.RemoveEmptyEntries);
                        playername1 = spl[0];
                    }
                    if (kills == max)
                    {
                        string text = str.Split(chrArray)[0];
                        string[] Arr = new string[] { " , ID:" };
                        string[] spl = text.Split(Arr, StringSplitOptions.RemoveEmptyEntries);
                        playername = spl[0];
                    }
                //}
            }
        }

        public static int zpoint = 10000;
        public static int hpoint = 10000;

        public static void PointsReaderHZ(Entity player)
        {
            char[] chrArray = new char[] { '=' };
            char[] chrArray2 = new char[] { ',' };
            string[] points;

            if (File.Exists("scripts\\MyShop\\Points\\Points.txt"))
            {
                try
                {
                    string[] strArrays = File.ReadAllLines("scripts\\MyShop\\Points\\Points.txt");
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str = strArrays[i];
                        if (str.StartsWith(player.UserID + "="))
                        {
                            string raw = str.Split(chrArray)[1];
                            points = raw.Split((chrArray2)[0]);
                            zpoint = int.Parse(points[0]);
                            hpoint = int.Parse(points[1]);
                        }

                        if (i == ((int)strArrays.Length - 1))
                        {
                            if (str != (player.UserID + "="))
                            {
                                File.AppendAllText("scripts\\MyShop\\Points\\Points.txt", string.Concat(player.UserID + "=10000,10000" + "\n"));
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
