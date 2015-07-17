using InfinityScript;
using System;
using System.IO;

//by zxz0O0
public class MapRotate : BaseScript
{
    private static string OldRotFilename = "OldRotation.cfg";

    public MapRotate()
    {
        base.Call("setdvarifuninitialized", new Parameter[] { "sv_oldrotation", "OldRotation.cfg" });
        OldRotFilename = base.Call<string>("getdvar", new Parameter[] { "sv_oldrotation" });
        //Log.Debug("\n OldRotation Plugin loaded\n Author: zxz0O0");
        if (base.Call<int>("getdvarint", new Parameter[] { "HasJustStarted", 1 }) == 1)
        {
            Log.Debug("First Start...");
            if (File.Exists(@"scripts\" + OldRotFilename + ".tmp"))
            {
                File.Delete(@"scripts\" + OldRotFilename + ".tmp");
            }
            File.Copy(@"scripts\" + OldRotFilename, @"scripts\" + OldRotFilename + ".tmp", true);
            base.Call("setdvar", new Parameter[] { "HasJustStarted", 0 });
        }
        if (!File.Exists(@"scripts\" + OldRotFilename + ".tmp"))
        {
            File.Copy(@"scripts\" + OldRotFilename, @"scripts\" + OldRotFilename + ".tmp", true);
        }
        if (!File.Exists(@"scripts\" + OldRotFilename))
        {
            Log.Debug("File " + OldRotFilename + " not found. OldRotation Plugin will not work");
        }
        else
        {
            FileInfo info = new FileInfo(@"scripts\" + OldRotFilename);
            if (info.Length < 5L)
            {
                Log.Debug("Syntax error in Rotation file. OldRotation Plugin will not work");
            }
            else
            {
                base.Call("setdvar", new Parameter[] { "sv_maprotation", OldRotFilename });
                if (this.HasRotated())
                {
                    Log.Debug("Rotating...");
                    this.SetRotation();
                }
            }
        }
    }

    private bool HasRotated()
    {
        string strA = base.Call<string>("getdvar", new Parameter[] { "mapname" });
        string str2 = base.Call<string>("getdvar", new Parameter[] { "sv_nextmap" });
        return (string.IsNullOrEmpty(str2) || (string.Compare(strA, str2, true) == 0));
    }

    private void SetRotation()
    {
        string contents = File.ReadAllText(@"scripts\" + OldRotFilename + ".tmp");
        string[] strArray = contents.Split(new char[] { ' ' }, 5);
        if (((strArray.Length < 4) || (strArray[0] != "playlist")) || (strArray[2] != "map"))
        {
            Log.Debug("Syntax error in tmp Rotation file. OldRotation Plugin will not work");
        }
        else
        {
            string str3 = strArray[1];
            string str2 = strArray[3];
            for (int i = 0; i < 4; i++)
            {
                string str4 = strArray[i];
                int index = contents.IndexOf(str4);
                int length = str4.Length;
                contents = contents.Remove(index, length).TrimStart(new char[] { ' ' });
                if (contents[contents.Length - 1] != ' ')
                {
                    contents = contents + " ";
                }
                contents = contents + str4;
            }
            File.WriteAllText(@"scripts\" + OldRotFilename + ".tmp", contents);
            base.Call("setdvar", new Parameter[] { "sv_nextmap", str2 });
            File.WriteAllText(@"admin\" + OldRotFilename + ".dspl", str2 + "," + str3 + ",1000");
        }
    }
}
