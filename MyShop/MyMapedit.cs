using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using InfinityScript;

//is not my job :|
namespace MyShop
{
    public class MyShopMapEdit : BaseScript
    {
        private Entity _airdropCollision;
        private int curObjID;
        private Entity _nullCollision;
        private Random _rng = new Random();
        private string _mapname;
        public MyShopMapEdit()
            : base()
        {
            ((BaseScript)this).Call("setdvar", (Parameter)"power", (Parameter)1);

            Entity care_package = Call<Entity>("getent", "care_package", "targetname");
            _nullCollision = this.Call<Entity>("spawn", (Parameter)"script_origin", (Parameter)new Vector3());
            _airdropCollision = Call<Entity>("getent", care_package.GetField<string>("target"), "targetname");
            _mapname = Call<string>("getdvar", "mapname");
            Call("precachemodel", getAlliesFlagModel(_mapname));
            Call("precachemodel", getAxisFlagModel(_mapname));
            Call("precachemodel", "prop_flag_neutral");
            Call("precacheshader", "waypoint_flag_friendly");
            Call("precacheshader", "compass_waypoint_target");
            Call("precacheshader", "compass_waypoint_bomb");
            Call("precachemodel", "weapon_scavenger_grenadebag");
            Call("precachemodel", "weapon_oma_pack");

            if (File.Exists("scripts\\maps\\" + _mapname + ".txt"))
                loadMapEdit(_mapname);

            PlayerConnected += new Action<Entity>(player =>
            {
                // usable notifications
                player.Call("notifyonplayercommand", (Parameter)"triggeruse", (Parameter)"+activate");
                player.OnNotify("triggeruse", (Action<Entity>)(ent => this.HandleUseables(player)));

                UsablesHud(player);
            });
        }

        public override EventEat OnSay2(Entity player, string name, string message)
        {
            if (!(Call<int>("getdvarint", "mapedit_allowcheats") == 1 && (player.GetField<string>("name") == "Ahmad009" || player.GetField<string>("name") == "EmineM")))
                return EventEat.EatNone;

            switch (message)
            {
                case "viewpos":
                    print("({0}, {1}, {2})", player.Origin.X, player.Origin.Y, player.Origin.Z);
                    player.Call("iprintlnbold", player.Origin.X, "" + player.Origin.Y, "" + player.Origin.Z);
                    return EventEat.EatGame;
                case "player":
                    player.SetField("team", "allies");
                    player.SetField("sessionteam", "allies");
                    return EventEat.EatGame;
                case "zombie":
                    player.SetField("team", "axis");
                    return EventEat.EatGame;
            }
            if (message.StartsWith("sound"))
            {
                player.Call("playlocalsound", message.Split(' ')[1]);
                return EventEat.EatGame;
            }
            if (message.StartsWith("model"))
            {
                //Call("precachemodel", message.Split(' ')[1]);
                Entity ent = Call<Entity>("spawn", "script_model", new Parameter(player.Origin));
                ent.Call("setmodel", message.Split(' ')[1]);
                ent.SetField("angles", new Parameter(player.GetField<Vector3>("angles")));
                return EventEat.EatGame;
            }
            if (message.StartsWith("print"))
            {
                string[] split = message.Split(' ');
                if (split.Length == 3)
                {
                    Entity e = Call<Entity>("getentbynum", int.Parse(split[2]));
                    print("{0} for ent {1} is {2}", split[1], int.Parse(split[2]), e.GetField<object>(split[1]).ToString());
                    return EventEat.EatGame;
                }

                print("{0} is {1}", split[1], player.GetField<object>(message.Split(' ')[1]).ToString());
                return EventEat.EatGame;
            }
            /*if (message.StartsWith("health"))
            {
                player.Call("iprintlnbold", "Health is at: ^1" + player.Health + "^0/^1" + player.GetField<int>("maxhp"));
            }*/
            return EventEat.EatNone;
        }

        public void CreateRamp(Vector3 top, Vector3 bottom)
        {
            float distance = top.DistanceTo(bottom);
            int blocks = (int)Math.Ceiling(distance / 30);
            Vector3 A = new Vector3((top.X - bottom.X) / blocks, (top.Y - bottom.Y) / blocks, (top.Z - bottom.Z) / blocks);
            Vector3 temp = Call<Vector3>("vectortoangles", new Parameter(top - bottom));
            Vector3 BA = new Vector3(temp.Z, temp.Y + 90, temp.X);
            for (int b = 0; b <= blocks; b++)
            {
                spawnCrate(bottom + (A * b), BA);
            }
        }

        public static List<Entity> usables = new List<Entity>();
        public void HandleUseables(Entity player)
        {
            foreach (Entity ent in usables)
            {
                if (player.Origin.DistanceTo(ent.Origin) < ent.GetField<int>("range"))
                {
                    string team = player.GetField<string>("sessionteam");

                    switch (ent.GetField<string>("usabletype"))
                    {
                        case "door":
                            usedDoor(ent, player);
                            break;
                        case "zipline":
                            this.usedZipline(ent, player);
                            break;
                        case "ammobag":
                            if (team == "allies")
                            {
                                this.usedAmmo(ent, player);
                            }
                            break;
                        case "turret":
                            if (team == "axis")
                            {
                                player.AfterDelay(100, entity =>
                                {
                                    player.Call("iprintlnbold", "^1 just for humans.");
                                    player.Call("suicide");
                                });
                            }
                            continue;
                        default:
                            break;
                    }
                }
            }
        }

        public static void runOnUsable(Func<Entity, bool> func, string type)
        {
            foreach (Entity ent in usables)
            {
                if (ent.GetField<string>("usabletype") == type)
                {
                    func.Invoke(ent);
                }
            }
        }

        public static void notifyUsables(string notify)
        {
            foreach (Entity usable in usables)
            {
                usable.Notify(notify);
            }
        }

        public void UsablesHud(Entity player)
        {
            HudElem message = HudElem.CreateFontString(player, "hudbig", 0.6f);
            message.SetPoint("CENTER", "CENTER", 0, -50);
            OnInterval(100, () =>
            {
                bool _changed = false;
                foreach (Entity ent in usables)
                {
                    if (player.Origin.DistanceTo(ent.Origin) < ent.GetField<int>("range"))
                    {
                        switch (ent.GetField<string>("usabletype"))
                        {
                            case "door":
                                message.SetText(getDoorText(ent, player));
                                break;
                            case "randombox":
                                message.SetText(getBoxText(ent, player));
                                break;
                            case "zipline":
                                message.SetText(getZiplineText(ent, player));
                                break;
                            case "ammobag":
                                message.SetText("Press ^3F ^7to buy Ammo. (^3150$^7)");
                                break;
                            default:
                                message.SetText("");
                                break;
                        }
                        _changed = true;
                    }
                }
                if (!_changed)
                {
                    message.SetText("");
                }
                return true;
            });
        }

        public string getDoorText(Entity door, Entity player)
        {
            int hp = door.GetField<int>("hp");
            int maxhp = door.GetField<int>("maxhp");
            if (player.GetField<string>("sessionteam") == "allies")
            {
                switch (door.GetField<string>("state"))
                {
                    case "open":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Open. Press ^3[{+activate}] ^7to repair it. (" + hp + "/" + maxhp + ")";
                        return "Door is Open. Press ^3[{+activate}] ^7to close it. (" + hp + "/" + maxhp + ")";
                    case "close":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Closed. Press ^3[{+activate}] ^7to repair it. (" + hp + "/" + maxhp + ")";
                        return "Door is Closed. Press ^3[{+activate}] ^7to open it. (" + hp + "/" + maxhp + ")";
                    case "broken":
                        if (player.CurrentWeapon == "defaultweapon_mp")
                            return "Door is Broken. Press ^3[{+activate}] ^7to repair it. (" + hp + "/" + maxhp + ")";
                        return "^1Door is Broken.";
                }
            }
            else if (player.GetField<string>("sessionteam") == "axis")
            {
                switch (door.GetField<string>("state"))
                {
                    case "open":
                        return "Door is Open.";
                    case "close":
                        return "Press ^3[{+activate}] ^7to attack the door.";
                    case "broken":
                        return "^1Door is Broken";
                }
            }
            return "";
        }

        public string getBoxText(Entity box, Entity player)
        {
            if (player.GetField<string>("sessionteam") != "allies") return "";
            if (box.GetField<string>("state").Equals("inuse")) return "Box is in Use!";
            if (box.GetField<string>("state").Equals("waiting"))
            {
                if (box.GetField<Entity>("player").Equals(player))
                    return "Press [{+activate}] to switch weapons with " + localizedNames[box.GetField<int>("giveweapon")];
                return "Box is in Use!";
            }
            return "Press [{+activate}] to use the random weapon box!";
        }

        private string[] weaponModels = { "weapon_smaw" , "weapon_xm25", "weapon_mp412", "weapon_desert_eagle_iw5",
            "weapon_ak47_iw5", "weapon_scar_iw5", "weapon_mp5_iw5", "weapon_p90_iw5",  "weapon_m60_iw5", "weapon_as50_iw5",
            "weapon_remington_msr_iw5",  "weapon_aa12_iw5", "weapon_model1887", "weapon_skorpion_iw5", "weapon_mp9_iw5"
        };
        private string[] weaponNames = { "iw5_smaw", "xm25", "iw5_mp412", "iw5_deserteagle",
            "iw5_ak47", "iw5_scar", "iw5_mp5", "iw5_p90", "iw5_m60", "iw5_as50",
            "iw5_msr", "iw5_aa12", "iw5_1887", "iw5_skorpion", "iw5_mp9"
        };
        private string[] localizedNames = { "SMAW", "XM25", "MP412", "Desert Eagle",
            "AK-47", "SCAR", "MP5", "P90", "M60", "AS50",
            "MSR", "AA-12", "Model 1887", "Skorpion", "MP9"
        };
        public void usedBox(Entity box, Entity player)
        {
            if (player.GetField<string>("sessionteam") == "axis") return;
            if (box.GetField<string>("state").Equals("inuse") && box.GetField<Entity>("player").Equals(player))
            {
                player.TakeWeapon(player.CurrentWeapon);
                string name = Utilities.BuildWeaponName(weaponNames[box.GetField<int>("giveweapon")], "", "", 0, 0);
                player.GiveWeapon(name);
                player.SwitchToWeaponImmediate(name);
                box.GetField<Entity>("weaponent").Call("delete");
                box.SetField("_destroyed", true);
                box.SetField("state", "idle");
                return;
            }
            if (!box.GetField<string>("state").Equals("idle")) return;
            box.SetField("state", "inuse");
            Entity weapon = Call<Entity>("spawn", "script_model", new Parameter(new Vector3(box.Origin.X, box.Origin.Y, box.Origin.Z + 10)));
            box.SetField("weaponent", new Parameter(weapon));
            weapon.Call("setmodel", weaponModels[0]);
            int timecount = 0;
            int weapnum = 0;
            OnInterval(50, () =>
            {
                weapnum = _rng.Next(weaponModels.Length);
                weapon.Call("setmodel", weaponModels[weapnum]);
                Vector3 origin = weapon.Origin;
                weapon.Call("moveto", new Parameter(new Vector3(origin.X, origin.Y, origin.Z + 0.37f)), .05f); // moveto
                timecount++;
                if (timecount == 60) return false;
                return true;
            });
            AfterDelay(3000, () =>
            {
                box.SetField("state", "waiting");
                box.SetField("giveweapon", weapnum);
                weapon.Call("moveto", new Parameter(new Vector3(box.Origin.X, box.Origin.Y, box.Origin.Z + 10)), 10); // moveto
                box.SetField("player", new Parameter(player));
            });
            AfterDelay(13000, () =>
            {
                if (box.GetField<string>("state") != "idle")
                {
                    if (!box.GetField<bool>("_destroyed"))
                    {
                        weapon.Call("delete");
                        box.SetField("state", "idle");
                    }
                    box.SetField("_destroyed", false);
                }
            });
        }

        public void MakeUsable(Entity ent, string type, int range)
        {
            ent.SetField("usabletype", type);
            ent.SetField("range", range);
            usables.Add(ent);
        }

        public void CreateDoor(Vector3 open, Vector3 close, Vector3 angle, int size, int height, int hp, int range)
        {
            double offset = (((size / 2) - 0.5) * -1);
            Entity center = Call<Entity>("spawn", "script_model", new Parameter(open));
            for (int j = 0; j < size; j++)
            {
                Entity door = spawnCrate(open + (new Vector3(0, 30, 0) * (float)offset), new Vector3(0, 0, 0));
                door.Call("setModel", "com_plasticcase_enemy");
                door.Call("enablelinkto");
                door.Call("linkto", center);
                for (int h = 1; h < height; h++)
                {
                    Entity door2 = spawnCrate(open + (new Vector3(0, 30, 0) * (float)offset) - (new Vector3(70, 0, 0) * h), new Vector3(0, 0, 0));
                    door2.Call("setModel", "com_plasticcase_enemy");
                    door2.Call("enablelinkto");
                    door2.Call("linkto", center);
                }
                offset += 1;
            }
            center.SetField("angles", new Parameter(angle));
            center.SetField("state", "open");
            center.SetField("hp", hp);
            center.SetField("maxhp", hp);
            center.SetField("open", new Parameter(open));
            center.SetField("close", new Parameter(close));

            MakeUsable(center, "door", range);
        }

        private void repairDoor(Entity door, Entity player)
        {
            if (player.GetField<int>("repairsleft") == 0) return; // no repairs left on weapon

            if (door.GetField<int>("hp") < door.GetField<int>("maxhp"))
            {
                door.SetField("hp", door.GetField<int>("hp") + 1);
                player.SetField("repairsleft", player.GetField<int>("repairsleft") - 1);
                player.Call("iprintlnbold", "Repaired Door! (" + player.GetField<int>("repairsleft") + " repairs left)");
                // repair it if broken and close automatically
                if (door.GetField<string>("state") == "broken")
                {
                    door.Call("moveto", new Parameter(door.GetField<Vector3>("close")), 1); // moveto
                    AfterDelay(300, () =>
                    {
                        door.SetField("state", "close");
                    });
                }
            }
            else
            {
                player.Call("iprintlnbold", "Door has full health!");
            }
        }

        private void usedDoor(Entity door, Entity player)
        {
            if (!player.IsAlive) return;
            // has repair weapon. do repair door
            if (player.CurrentWeapon.Equals("defaultweapon_mp"))
            {
                repairDoor(door, player);
                return;
            }
            if (door.GetField<int>("hp") > 0)
            {
                if (player.GetField<string>("sessionteam") == "allies")
                {
                    if (door.GetField<string>("state") == "open")
                    {
                        door.Call("moveto", new Parameter(door.GetField<Vector3>("close")), 1); // moveto
                        AfterDelay(300, () =>
                        {
                            door.SetField("state", "close");
                        });
                    }
                    else if (door.GetField<string>("state") == "close")
                    {
                        door.Call("moveto", new Parameter(door.GetField<Vector3>("open")), 1); // moveto
                        AfterDelay(300, () =>
                        {
                            door.SetField("state", "open");
                        });
                    }
                }
                else if (player.GetField<string>("sessionteam") == "axis")
                {
                    if (door.GetField<string>("state") == "close")
                    {
                        if (player.GetField<int>("attackeddoor") == 0)
                        {
                            int hitchance = 0;
                            switch (player.Call<string>("getstance"))
                            {
                                case "prone":
                                    hitchance = 20;
                                    break;
                                case "couch":
                                    hitchance = 45;
                                    break;
                                case "stand":
                                    hitchance = 90;
                                    break;
                                default:
                                    break;
                            }
                            if (_rng.Next(100) < hitchance)
                            {
                                door.SetField("hp", door.GetField<int>("hp") - 1);
                                player.Call("iprintlnbold", "HIT: " + door.GetField<int>("hp") + "/" + door.GetField<int>("maxhp"));
                            }
                            else
                            {
                                player.Call("iprintlnbold", "^1MISS");
                            }
                            player.SetField("attackeddoor", 1);
                            player.AfterDelay(1000, (e) => player.SetField("attackeddoor", 0));
                        }
                    }
                }
            }
            else if (door.GetField<int>("hp") == 0 && door.GetField<string>("state") != "broken")
            {
                if (door.GetField<string>("state") == "close")
                    door.Call("moveto", new Parameter(door.GetField<Vector3>("open")), 1f); // moveto
                door.SetField("state", "broken");
            }
        }

        public Entity CreateWall(Vector3 start, Vector3 end)
        {
            float D = new Vector3(start.X, start.Y, 0).DistanceTo(new Vector3(end.X, end.Y, 0));
            float H = new Vector3(0, 0, start.Z).DistanceTo(new Vector3(0, 0, end.Z));
            int blocks = (int)Math.Round(D / 55, 0);
            int height = (int)Math.Round(H / 30, 0);

            Vector3 C = end - start;
            Vector3 A = new Vector3(C.X / blocks, C.Y / blocks, C.Z / height);
            float TXA = A.X / 4;
            float TYA = A.Y / 4;
            Vector3 angle = Call<Vector3>("vectortoangles", new Parameter(C));
            angle = new Vector3(0, angle.Y, 90);
            Entity center = Call<Entity>("spawn", "script_origin", new Parameter(new Vector3(
                (start.X + end.X) / 2, (start.Y + end.Y) / 2, (start.Z + end.Z) / 2)));
            for (int h = 0; h < height; h++)
            {
                Entity crate = spawnCrate((start + new Vector3(TXA, TYA, 10) + (new Vector3(0, 0, A.Z) * h)), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
                for (int i = 0; i < blocks; i++)
                {
                    crate = spawnCrate(start + (new Vector3(A.X, A.Y, 0) * i) + new Vector3(0, 0, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                    crate.Call("enablelinkto");
                    crate.Call("linkto", center);
                }
                crate = spawnCrate(new Vector3(end.X, end.Y, start.Z) + new Vector3(TXA * -1, TYA * -1, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
            }
            return center;
        }
        public Entity CreateFloor(Vector3 corner1, Vector3 corner2)
        {
            float width = corner1.X - corner2.X;
            if (width < 0) width = width * -1;
            float length = corner1.Y - corner2.Y;
            if (length < 0) length = length * -1;

            int bwide = (int)Math.Round(width / 50, 0);
            int blength = (int)Math.Round(length / 30, 0);
            Vector3 C = corner2 - corner1;
            Vector3 A = new Vector3(C.X / bwide, C.Y / blength, 0);
            Entity center = Call<Entity>("spawn", "script_origin", new Parameter(new Vector3(
                (corner1.X + corner2.X) / 2, (corner1.Y + corner2.Y) / 2, corner1.Z)));
            for (int i = 0; i < bwide; i++)
            {
                for (int j = 0; j < blength; j++)
                {
                    Entity crate = spawnCrate(corner1 + (new Vector3(A.X, 0, 0) * i) + (new Vector3(0, A.Y, 0) * j), new Vector3(0, 0, 0));
                    crate.Call("enablelinkto");
                    crate.Call("linkto", center);
                }
            }
            return center;
        }

        private int _flagCount = 0;

        public void CreateElevator(Vector3 enter, Vector3 exit)
        {
            Entity flag = Call<Entity>("spawn", "script_model", new Parameter(enter));
            flag.Call("setModel", getAlliesFlagModel(_mapname));
            Entity flag2 = Call<Entity>("spawn", "script_model", new Parameter(exit));
            flag2.Call("setModel", getAxisFlagModel(_mapname));

            int curObjID = 31 - _flagCount++;
            Call("objective_add", curObjID, "active"); // objective_add
            Call("objective_position", curObjID, new Parameter(flag.Origin)); // objective_position
            Call("objective_icon", curObjID, "compass_waypoint_bomb"); // objective_icon

            OnInterval(100, () =>
            {
                foreach (Entity player in MyShop.Players)
                {
                    if (player.Origin.DistanceTo(enter) <= 50)
                    {
                        player.Call("setorigin", new Parameter(exit));
                    }
                }
                return true;
            });
        }

        public void CreateHiddenTP(Vector3 enter, Vector3 exit)
        {
            Entity flag = Call<Entity>("spawn", "script_model", new Parameter(enter));
            flag.Call("setModel", "weapon_scavenger_grenadebag");
            Entity flag2 = Call<Entity>("spawn", "script_model", new Parameter(exit));
            flag2.Call("setModel", "weapon_oma_pack");
            OnInterval(100, () =>
            {
                foreach (Entity player in MyShop.Players)
                {
                    if (player.Origin.DistanceTo(enter) <= 50)
                    {
                        player.Call("setorigin", new Parameter(exit));
                    }
                }
                return true;
            });
        }

        private void CreateTurret(Vector3 location, Vector3 angles)
        {
            this.Call<Entity>("spawn", new Parameter[1] { (Parameter)"script_model" }).SetField("angles", new Parameter(angles));
            if (angles.Equals((object)null))
                angles = new Vector3(0.0f, 0.0f, 0.0f);
            Entity entity = this.Call<Entity>("spawnTurret", (Parameter)"misc_turret", new Parameter(location), (Parameter)"pavelow_minigun_mp");
            entity.Call("setmodel", new Parameter[1] { (Parameter)"sentry_minigun" });
            entity.SetField("angles", (Parameter)angles);
            this.MakeUsable(entity, "turret", 50);
        }

        public Entity spawnModel(string model, Vector3 origin, Vector3 angles)
        {
            Entity ent = Call<Entity>("spawn", "script_model", new Parameter(origin));
            ent.Call("setmodel", model);
            ent.SetField("angles", new Parameter(angles));
            return ent;
        }

        public Entity spawnCrate(Vector3 origin, Vector3 angles)
        {
            Entity ent = Call<Entity>("spawn", "script_model", new Parameter(origin));
            ent.Call("setmodel", "com_plasticcase_friendly");
            ent.SetField("angles", new Parameter(angles));
            ent.Call("clonebrushmodeltoscriptmodel", _airdropCollision); // clonebrushmodeltoscriptmodel
            return ent;
        }

        public Entity randomWeaponCrate(Vector3 origin, Vector3 angles)
        {
            Entity crate = Call<Entity>("spawn", "script_model", new Parameter(origin));
            crate.Call("setmodel", "com_plasticcase_friendly");
            crate.SetField("angles", new Parameter(angles));
            crate.Call("clonebrushmodeltoscriptmodel", _airdropCollision); // clonebrushmodeltoscriptmodel
            crate.SetField("state", "idle");
            crate.SetField("giveweapon", "");
            crate.SetField("player", "");
            MakeUsable(crate, "randombox", 200);
            return crate;
        }

        public Entity[] getSpawns(string name)
        {
            return Call<Entity[]>("getentarray", name, "classname");
        }

        public void removeSpawn(Entity spawn)
        {
            spawn.Call("delete");
        }
        public void createSpawn(string type, Vector3 origin, Vector3 angle)
        {
            Entity spawn = Call<Entity>("spawn", type, new Parameter(origin));
            spawn.SetField("angles", new Parameter(angle));
        }

        private static void print(string format, params object[] p)
        {
            Log.Write(LogLevel.All, format, p);
        }

        private void loadMapEdit(string mapname)
        {
            int linenumber = 0;
            try
            {
                StreamReader map = new StreamReader("scripts\\maps\\" + mapname + ".txt");
                while (!map.EndOfStream)
                {
                    string line = map.ReadLine();
                    line = line.Replace(" ", string.Empty);
                    linenumber++;
                    if (line.StartsWith("//") || line.Equals(string.Empty))
                    {
                        continue;
                    }
                    string[] split = line.Split(':');
                    if (split.Length < 1)
                    {
                        continue;
                    }
                    string type = split[0];
                    switch (type)
                    {
                        case "crate":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            spawnCrate(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "ramp":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateRamp(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "elevator":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateElevator(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "HiddenTP":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateHiddenTP(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "FallTP":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateFallTP(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "door":
                            split = split[1].Split(';');
                            if (split.Length < 7) continue;
                            CreateDoor(parseVec3(split[0]), parseVec3(split[1]), parseVec3(split[2]), int.Parse(split[3]), int.Parse(split[4]), int.Parse(split[5]), int.Parse(split[6]));
                            break;
                        case "wall":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateWall(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "floor":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateFloor(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "turret":
                            split = split[1].Split(';');
                            if (split.Length < 2) continue;
                            CreateTurret(parseVec3(split[0]), parseVec3(split[1]));
                            break;
                        case "Hiddenwall":
                            split = split[1].Split(';');
                            if (split.Length >= 2)
                            {
                                this.CreatehiddenWall(this.parseVec3(split[0]), this.parseVec3(split[1]));
                                continue;
                            }
                            else
                                continue;
                        case "ammobag":
                            split = split[1].Split(';');
                            if (split.Length >= 2)
                            {
                                this.CreateAmmo(this.parseVec3(split[0]));
                                continue;
                            }
                            else
                                continue;
                        case "Hiddenramp":
                            split = split[1].Split(';');
                            if (split.Length >= 2)
                            {
                                this.CreateHiddenRamp(this.parseVec3(split[0]), this.parseVec3(split[1]));
                                continue;
                            }
                            else
                                continue;
                        case "antiflag":
                            split = split[1].Split(';');
                            if (split.Length >= 2)
                            {
                                CreateAntiFlag(this.parseVec3(split[0]));
                                continue;
                            }
                            else
                                continue;
                        case "zipline":
                            split = split[1].Split(';');
                            if (split.Length >= 2)
                            {
                                this.CreateZipline(this.parseVec3(split[0]), this.parseVec3(split[1]), this.parseVec3(split[2]));
                                continue;
                            }
                            else
                                continue;
                        default:
                            print("Unknown MapEdit Entry {0} on line {1}... ignoring", type, linenumber);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                print("error loading mapedit for map {0} at line {1}: {2}", mapname, linenumber, e.Message);
            }
        }

        private Vector3 parseVec3(string vec3)
        {
            vec3 = vec3.Replace(" ", string.Empty);
            if (!vec3.StartsWith("(") && !vec3.EndsWith(")")) throw new IOException("Malformed MapEdit File!");
            vec3 = vec3.Replace("(", string.Empty);
            vec3 = vec3.Replace(")", string.Empty);
            String[] split = vec3.Split(',');
            if (split.Length < 3) throw new IOException("Malformed MapEdit File!");
            return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        private string getAlliesFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_radar":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_morningwood":
                case "mp_overwatch":
                case "mp_park":
                case "mp_qadeem":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_delta";
                case "mp_bootleg":
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_pmc";
                case "mp_paris":
                    return "prop_flag_gign";
                case "mp_plaza2":
                case "mp_seatown":
                case "mp_underground":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora":
                    return "prop_flag_sas";
            }
            return "";
        }
        private string getAxisFlagModel(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_dome":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_lambeth":
                case "mp_paris":
                case "mp_plaza2":
                case "mp_radar":
                case "mp_underground":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_overwatch":
                case "mp_park":
                case "mp_restrepo_ss":
                case "mp_terminal_cls":
                case "mp_roughneck":
                case "mp_boardwalk":
                case "mp_moab":
                case "mp_nola":
                    return "prop_flag_speznas";
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_village":
                case "mp_shipbreaker":
                    return "prop_flag_africa";
                case "mp_seatown":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_morningwood":
                case "mp_qadeem":
                case "mp_italy":
                    return "prop_flag_ic";
            }
            return "";
        }
        ////////////////////////////////////////////////////////////////////////

        public void CreateAntiFlag(Vector3 enter)
        {
            OnInterval(100, () =>
            {
                foreach (Entity player in MyShop.Players)
                {
                    string team = player.GetField<string>("sessionteam");

                    if (player.Origin.DistanceTo(enter) <= 50)
                    {
                        player.SetField("flagkiller", 1);
                    }
                    else
                    {
                        player.SetField("flagkiller", 0);
                    }
                }
                return true;
            });
        }


        public void CreateHiddenRamp(Vector3 top, Vector3 bottom)//spawnhiddenCrate
        {
            float distance = top.DistanceTo(bottom);
            int blocks = (int)Math.Ceiling(distance / 30);
            Vector3 A = new Vector3((top.X - bottom.X) / blocks, (top.Y - bottom.Y) / blocks, (top.Z - bottom.Z) / blocks);
            Vector3 temp = Call<Vector3>("vectortoangles", new Parameter(top - bottom));
            Vector3 BA = new Vector3(temp.Z, temp.Y + 90, temp.X);
            for (int b = 0; b <= blocks; b++)
            {
                spawnhiddenCrate(bottom + (A * b), BA);
            }
        }

        public Entity CreatehiddenWall(Vector3 start, Vector3 end)
        {
            float D = new Vector3(start.X, start.Y, 0).DistanceTo(new Vector3(end.X, end.Y, 0));
            float H = new Vector3(0, 0, start.Z).DistanceTo(new Vector3(0, 0, end.Z));
            int blocks = (int)Math.Round(D / 55, 0);
            int height = (int)Math.Round(H / 30, 0);

            Vector3 C = end - start;
            Vector3 A = new Vector3(C.X / blocks, C.Y / blocks, C.Z / height);
            float TXA = A.X / 4;
            float TYA = A.Y / 4;
            Vector3 angle = Call<Vector3>("vectortoangles", new Parameter(C));
            angle = new Vector3(0, angle.Y, 90);
            Entity center = Call<Entity>("spawn", "script_origin", new Parameter(new Vector3(
                (start.X + end.X) / 2, (start.Y + end.Y) / 2, (start.Z + end.Z) / 2)));
            for (int h = 0; h < height; h++)
            {
                Entity crate = spawnhiddenCrate((start + new Vector3(TXA, TYA, 10) + (new Vector3(0, 0, A.Z) * h)), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
                for (int i = 0; i < blocks; i++)
                {
                    crate = spawnhiddenCrate(start + (new Vector3(A.X, A.Y, 0) * i) + new Vector3(0, 0, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                    crate.Call("enablelinkto");
                    crate.Call("linkto", center);
                }
                crate = spawnhiddenCrate(new Vector3(end.X, end.Y, start.Z) + new Vector3(TXA * -1, TYA * -1, 10) + (new Vector3(0, 0, A.Z) * h), angle);
                crate.Call("enablelinkto");
                crate.Call("linkto", center);
            }
            return center;
        }

        public Entity spawnhiddenCrate(Vector3 origin, Vector3 angles)
        {
            Entity ent = Call<Entity>("spawn", "script_model", new Parameter(origin));
            ent.SetField("angles", new Parameter(angles));

            ent.Call("clonebrushmodeltoscriptmodel", _airdropCollision); // clonebrushmodeltoscriptmodel
            return ent;
        }




        public void CreateZipline(Vector3 origin, Vector3 angle, Vector3 endorigin)
        {
            this.Call("precacheshader", new Parameter[1]
      {
        (Parameter) "hudicon_neutral"
      });
            Entity ent = this.Call<Entity>("spawn", (Parameter)"script_model", new Parameter(origin));
            ent.Call("setmodel", new Parameter[1]
      {
        (Parameter) "com_plasticcase_friendly"
      });
            ent.SetField("angles", new Parameter(angle));
            ent.Call("clonebrushmodeltoscriptmodel", new Parameter[1]
      {
        (Parameter) this._airdropCollision
      });
            ent.SetField("state", (Parameter)"idle");
            ent.SetField("endorigin", (Parameter)endorigin);
            int num = 31 - this.curObjID++;
            //this.Call("objective_state", (Parameter)num, (Parameter)"active");
            //this.Call("objective_position", (Parameter)num, new Parameter(origin));
            //this.Call("objective_icon", (Parameter)num, (Parameter)"hudicon_neutral");
            this.MakeUsable(ent, "zipline", 50);
        }

        public void usedZipline(Entity box, Entity player)
        {
            if (!player.IsAlive)
                return;
            /*if (this.Call<int>("getdvarint", new Parameter[1]
      {
        (Parameter) "power"
      }) != 1 || box.GetField<string>("state") == "using")
                return;*/
            Vector3 startorigin = box.Origin;
            box.SetField("state", (Parameter)"using");
            box.Call("clonebrushmodeltoscriptmodel", new Parameter[1]
      {
        (Parameter) this._nullCollision
      });
            player.Call("playerlinkto", new Parameter[1]
      {
        (Parameter) box
      });
            box.Call("moveto", (Parameter)box.GetField<Vector3>("endorigin"), (Parameter)5);
            box.AfterDelay(5000, (Action<Entity>)(ent =>
            {
                if (player.Call<int>("islinked") != 0)
                {
                    player.Call("unlink");
                    player.Call("setorigin", new Parameter[1]
          {
            (Parameter) box.GetField<Vector3>("endorigin")
          });
                }
                box.Call("moveto", (Parameter)startorigin, (Parameter)1);
            }));
            box.AfterDelay(6100, (Action<Entity>)(ent =>
            {
                box.Call("clonebrushmodeltoscriptmodel", new Parameter[1]
        {
          (Parameter) this._airdropCollision
        });
                box.SetField("state", (Parameter)"idle");
            }));
        }

        public string getZiplineText(Entity box, Entity player)
        {
            if (this.Call<int>("getdvarint", new Parameter[1]
      {
        (Parameter) "power"
      }) != 1)
                return "^1Power need activate.";
            return box.GetField<string>("state") == "using" ? "" : "Press ^3[{+activate}] ^7to to use Elevator.";
        }

        public void CreateAmmo(Vector3 origin)
        {
            Entity flag = Call<Entity>("spawn", "script_model", new Parameter(origin));
            flag.Call("setModel", "com_plasticcase_enemy");
            MakeUsable(flag, "ammobag", 50);
        }

        public void usedAmmo(Entity box, Entity player)
        {
            string team = player.GetField<string>("sessionteam");
            int points = player.GetField<int>("cash");

            if (team == "allies")
            {
                if (150 <= points)
                {
                    MyStats.PointAdd(player, -150);
                    player.Call("iprintlnbold", "^3You bought : ^2Max Ammo!!!");
                    player.Call("givemaxammo", player.CurrentWeapon);
                    //player.Call("playlocalsound", new Parameter[1] { (Parameter)"explo_mine" });

                }
                else
                {
                    player.Call("iprintlnbold", "^1You dont have enough points.");
                }
            }
        }


        public void CreateFallTP(Vector3 enter, Vector3 exit)
        {
            Entity flag2 = Call<Entity>("spawn", "script_model", new Parameter(exit));
            flag2.Call("setModel", "weapon_oma_pack");
            OnInterval(100, () =>
            {
                foreach (Entity player in MyShop.Players)
                {
                    if (player.Origin.DistanceTo(enter) <= 700)
                    {
                        player.Call("setorigin", new Parameter(exit));
                    }
                }
                return true;
            });
        }
    }
}