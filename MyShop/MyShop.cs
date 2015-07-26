using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace MyShop
{
    public class MyShop : BaseScript
    {
        public static List<Entity> Players = new List<Entity>();
        public List<string> PlayerStop = new List<string>();
        public Dictionary<int, int> Client_HudElem_Shop = new Dictionary<int, int>();

        Random _rnd = new Random();
        HudElem motd;

        bool ok = false;

        public MyShop()
            : base()
        {
            //check Log and Config files
            string file = "";
            StreamWriter writer;
            try
            {
                if (!File.Exists("scripts\\MyShop\\reports.txt"))
                {
                    Directory.CreateDirectory("scripts\\MyShop");
                    file = "scripts\\MyShop\\reports.txt";
                    writer = new StreamWriter(file);
                    writer.Write("");
                    writer.Close();
                }
                
                if (!File.Exists("scripts\\MyShop\\ConnectedPlayers\\ConnectedPlayers.txt"))
                {
                    Directory.CreateDirectory("scripts\\MyShop\\ConnectedPlayers");
                    file = "scripts\\MyShop\\ConnectedPlayers\\ConnectedPlayers.txt";
                    writer = new StreamWriter(file);
                    writer.Write("");
                    writer.Close();
                }

                if (!File.Exists("scripts\\MyShop\\Kills\\Kills.txt"))
                {
                    Directory.CreateDirectory("scripts\\MyShop\\Kills");
                    file = "scripts\\MyShop\\Kills\\Kills.txt";
                    writer = new StreamWriter(file);
                    writer.Write("");
                    writer.Close();
                }

                if (!File.Exists("scripts\\MyShop\\PointTransfers\\Transfers.txt"))
                {
                    Directory.CreateDirectory("scripts\\MyShop\\PointTransfers");
                    file = "scripts\\MyShop\\PointTransfers\\Transfers.txt";
                    writer = new StreamWriter(file);
                    writer.Write("");
                    writer.Close();
                }

                if (!File.Exists("scripts\\MyShop\\Config.cfg"))
                {
                    Directory.CreateDirectory("scripts\\MyShop");
                    file = "scripts\\MyShop\\Config.cfg";
                    writer = new StreamWriter(file);
                    writer.WriteLine("Admins=");
                    writer.WriteLine("MasterAdmins=");
                    writer.Close();
                }

                if (!File.Exists("scripts\\MyShop\\CustomNames.cfg"))
                {
                    Directory.CreateDirectory("scripts\\MyShop");
                    file = "scripts\\MyShop\\CustomNames.cfg";
                    writer = new StreamWriter(file);
                    writer.Write("ahmad009=^:Ahmad009");
                    writer.Close();
                }
            }
            catch { }

            base.PlayerConnecting += new Action<Entity>(this.playerConnecting);
            base.PlayerDisconnected += (new Action<Entity>(this.playerDisconnected));

            createServerHud();
            OnNotify("game_ended", (Parameter level) => MyStats.AllPointsWriter());
            OnNotify("game_ended", (Parameter level2) =>
            {
                foreach (Entity player in Players)
                {
                    MyStats.KillsWriter(player);
                }
            });

            /*OnInterval(60000, () =>
            {
                MyStats.AllPointsWriter();
                return true;
            });*/

            PlayerConnected += new Action<Entity>(ent =>
            {
                MyStats.PlayerPointsReader(ent);

                /*HudElem hud = HudElem.CreateFontString(ent, "default", 2f);
                Parameter[] parameterArray = new Parameter[] { 200, 8000, 1000 };
                hud.Call("setpulsefx", parameterArray);
                hud.HideWhenInMenu = false;
                hud.Foreground = true;
                hud.Alpha = 1f;
                hud.GlowAlpha = 5f;
                hud.SetPoint("LEFT", "LEFT", 300, 100);
                hud.SetText("^1Welcome to ^5IN^3FecT^5ED ^1Server");*/

                MyShop.Players.Add(ent);

                ent.SetField("customname", "null");
                ent.SetField("ismaster", 0);

                MyStats.ConfigReader();
                MyStats.CustomNameReader(ent);


                ent.SetPerk("specialty_blindeye", true, false);
                ent.SetPerk("specialty_quieter", true, false);
                ent.SetPerk("specialty_fastreload", true, false);
                ent.SetPerk("specialty_quickdraw", true, false);
                ent.SetPerk("specialty_longersprint", true, false);
                ent.SetPerk("specialty_fastermelee", true, false);
                ent.SetPerk("specialty_lightweight", true, false);
                ent.SetPerk("specialty_quickswap", true, false);

                if (MyStats.MasterAdminsXUIDs.Contains(ent.GUID.ToString()))
                {
                    ent.SetField("ismaster", 1);
                }

                ent.SetField("hasatk", 0);
                ent.SetField("hasttk", 0);
                ent.SetField("hashp", 0);
                ent.SetField("isjff", 0);
                ent.SetField("isgod", 0);
                ent.SetField("isgold", 0);
                ent.SetField("troll", 0);
                ent.SetField("hasesp", 0);
                File.AppendAllText("scripts\\MyShop\\ConnectedPlayers\\ConnectedPlayers.txt", string.Concat(DateTime.Now + "  ==> Name : " + ent.Name + " ,XUID : " + ent.GUID + " ,ID : " + ent.UserID + " ,IP : " + ent.IP + "\n"));
                ent.Call("freezeControls", new Parameter[] { false });

                if (ent.GetField<int>("ismaster") == 1)
                {
                    MyStats.PointEditor(ent, 100000);
                    ent.SetField("cash", 100000);
                }

                settings(ent);
                setPlayerHud(ent);

                /*ent.SetPerk("specialty_fastreload", true, true);
                ent.SetPerk("specialty_quickdraw", true, true);
                ent.SetPerk("specialty_longersprint", true, true);
                ent.SetPerk("specialty_fastermelee", true, true);
                ent.SetPerk("specialty_lightweight", true, true);
                ent.SetPerk("specialty_quickswap", true, false);
                ent.SetPerk("specialty_extraammo", true, false);*/

                ent.SpawnedPlayer += new Action(() =>
                {
                    ent.SetPerk("specialty_blindeye", true, false);
                    ent.SetPerk("specialty_fastreload", true, false);
                    ent.SetPerk("specialty_quickdraw", true, false);
                    ent.SetPerk("specialty_quieter", true, false);
                    ent.SetPerk("specialty_longersprint", true, false);
                    ent.SetPerk("specialty_fastermelee", true, false);
                    ent.SetPerk("specialty_lightweight", true, false);
                    ent.SetPerk("specialty_quickswap", true, false);

                    string team = ent.GetField<string>("sessionteam");

                    ent.SetField("hasatk", 0);
                    ent.SetField("hasttk", 0);
                    //ent.SetField("hashp", 0);
                    ent.SetField("isjff", 0);
                    //ent.SetField("isgod", 0);
                    ent.SetField("isgold", 0);

                    ent.Call("ThermalVisionFOFOverlayOff");
                    ent.SetField("incomingweapon", "nope");
                    ent.SetField("incomingweaponfrom", "null");

                    //perks
                    /*if (team == "axis")
                    {
                        AfterDelay(100, () =>
                        {
                            ent.SetPerk("specialty_longersprint", true, true);//
                            ent.SetPerk("specialty_fastermelee", true, true);//
                        });
                    }*/

                    if (team == "axis")
                    {
                        AfterDelay(100, () =>
                        {
                            if (ent.CurrentWeapon == "riotshield_mp")
                            {
                                ent.TakeWeapon("iw5_p99_mp_tactical");
                                ent.Call("giveweapon", "iw5_mp7_mp");
                                doCustomWeapon(ent, "iw5_mp7_mp", "rpg_mp");
                                ent.Call("givemaxammo", ent.CurrentWeapon);
                            }
                        });
                    }

                    mdlchanger(ent);

                    AfterDelay(100, () =>
                    {
                        ent.SetField("movespeed", 1.15f);
                        OnInterval(100, () =>
                        {
                            ent.Call("setmovespeedscale", new Parameter(ent.GetField<float>("movespeed")));
                            return true;
                        });
                    });
                });
            }
            );
        }

        private void playerDisconnected(Entity player)
        {
            ServerSay("^6" + player.Name + " ^5Left The Game.");
            MyShop.Players.Remove(player);
        }

        private void settings(Entity player)
        {
            Call("setdvar", "g_ScoresColor_Spectator", ".25 .25 .25");
            Call("setdvar", "g_ScoresColor_Free", ".76 .78 .10");
            Call("setdvar", "g_teamColor_MyTeam", ".6 .8 .6");
            Call("setdvar", "g_teamColor_EnemyTeam", "1 .45 .5");
            Call("setdvar", "g_teamTitleColor_MyTeam", ".6 .8 .6");
            Call("setdvar", "g_teamTitleColor_EnemyTeam", "1 .45 .5");
            Call("setdvar", "ui_allow_teamchange", "0");

            AfterDelay(100, () =>
            {
                string env = getMapEnv(Call<string>("getdvar", "mapname"));
                Call("setdvar", "env1", env);

                if (Call<string>("getdvar", "mapname").Equals("mp_radar"))
                    env = "arctic";
                Call("setdvar", "env2", env);
                Call("precachemodel", "viewhands_iw5_ghillie_" + env);
            });

            Call("setdvar", "g_TeamName_Allies", "^1H^3uman^5s");
            Call("setdvar", "g_TeamName_Axis", "^1Z^3ombie^6s");
            Call("setdvar", "g_teamColor_MyTeam", ".6 .8 .6");
            Call("setdvar", "g_teamColor_EnemyTeam", "1 .45 .5");
            Call("setdvar", "g_teamTitleColor_MyTeam", ".6 .8 .6");
            Call("setdvar", "g_teamTitleColor_EnemyTeam", "1 .45 .5");
            player.Call("notifyonplayercommand", "ShowShop", "+reload");
            player.Call("notifyonplayercommand", "HideShop", "-reload");
        }

        private void mdlchanger(Entity ent)
        {
            if (ent.GetField<int>("isjff") == 1)
            {
                AfterDelay(100, () =>
                {
                    ent.Call("setmodel", "mp_body_ally_ghillie_" + Call<string>("getdvar", "env1") + "_sniper");
                    ent.Call("setviewmodel", "viewhands_iw5_ghillie_" + Call<string>("getdvar", "env2"));
                    ent.Call("giveweapon", "iw5_usp45_mp_tactical");
                    ent.SwitchToWeaponImmediate("iw5_usp45_mp_tactical");

                    ent.SetPerk("specialty_paint", true, true);
                    ent.SetPerk("specialty_longersprint", true, true);
                    ent.SetPerk("specialty_blindeye", true, false);
                    ent.SetPerk("specialty_coldblooded", true, false);
                    ent.SetPerk("specialty_quickdraw", true, false);
                    ent.SetPerk("specialty_assists", true, false);
                    ent.SetPerk("specialty_moredamage", true, false);
                    ent.SetPerk("specialty_holdbreathwhileads", true, false);
                    ent.SetPerk("specialty_marksman", true, false);
                    ent.SetPerk("specialty_lightweight", true, true);
                    ent.SetPerk("specialty_blastshield", true, false);
                    ent.SetPerk("specialty_autospot", true, false);
                    ent.SetPerk("specialty_bulletaccuracy", true, false);
                    ent.SetPerk("specialty_stalker", true, true);

                    /*if (ent.GetField<int>("ismaster") == 1)
                    {
                        doCustomWeapon(ent, "iw5_usp45", "rpg_mp");
                        ((BaseScript)this).Call("playfx", (Parameter)((BaseScript)this).Call<int>("loadfx", new Parameter[1] { (Parameter)"props/cash_player_drop" }), (Parameter)ent.Call<Vector3>("gettagorigin", new Parameter[1] { (Parameter)"j_spine4" }));//explosions
                        ent.Call("playsound", new Parameter[1] { (Parameter)"mp_killconfirm_tags_pickup" });
                    }*/
                    //doCustomWeapon(ent, "iw5_usp45", "rpg_mp");
                });
            }

            string team = ent.GetField<string>("sessionteam");

            if (team == "axis")
            {
                AfterDelay(100, () =>
                {
                    ent.Call("setmodel", "mp_body_ally_ghillie_" + Call<string>("getdvar", "env1") + "_sniper");
                    ent.Call("setviewmodel", "viewhands_iw5_ghillie_" + Call<string>("getdvar", "env2"));

                    ent.SetPerk("specialty_lightweight", true, true);
                    ent.SetPerk("specialty_longersprint", true, true);

                    //red Eyes
                    /*OnInterval(1000, () =>
                    {
                        base.Call("playfxontag", new Parameter[] { base.Call<int>("loadfx", new Parameter[] { "misc/aircraft_light_wingtip_red" }), ent, "j_eyeball_le" });
                        base.Call("playfxontag", new Parameter[] { base.Call<int>("loadfx", new Parameter[] { "misc/aircraft_light_wingtip_red" }), ent, "j_eyeball_ri" });
                        return true;
                    });*/
                });
            }

            if (ent.GetField<int>("hasesp") == 1)
            {
                ent.SetField("movespeed", 1.7f);
                OnInterval(100, () =>
                {
                    ent.Call("setmovespeedscale", new Parameter(ent.GetField<float>("movespeed")));
                    return true;
                });
            }
        }

        public string getMapEnv(string mapname)
        {
            switch (mapname)
            {
                case "mp_alpha":
                case "mp_bootleg":
                case "mp_exchange":
                case "mp_hardhat":
                case "mp_interchange":
                case "mp_mogadishu":
                case "mp_paris":
                case "mp_plaza2":
                case "mp_underground":
                case "mp_cement":
                case "mp_hillside_ss":
                case "mp_overwatch":
                case "mp_terminal_cls":
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_meteora":
                case "mp_morningwood":
                case "mp_qadeem":
                case "mp_crosswalk_ss":
                case "mp_italy":
                case "mp_boardwalk":
                case "mp_roughneck":
                case "mp_nola":
                    return "urban";
                case "mp_dome":
                case "mp_radar":
                case "mp_restrepo_ss":
                case "mp_burn_ss":
                case "mp_seatown":
                case "mp_shipbreaker":
                case "mp_moab":
                    return "desert";
                case "mp_bravo":
                case "mp_carbon":
                case "mp_park":
                case "mp_six_ss":
                case "mp_village":
                case "mp_lambeth":
                    return "woodland";
            }
            return "";
        }

        private void playerConnecting(Entity player)
        {
            ServerSay("^1" + player.Name + " ^3 is Incomming!!!");
        }

        public void ServerSay(string message)
        {
            Utilities.RawSayAll(message);
        }

        private void setPlayerHud(Entity player)
        {
            HudElem hud0;
            HudElem hud1;
            HudElem rHud;
            HudElem zmhud1;
            HudElem zmhud2;
            HudElem zmhud3;

            HudElem hmhud1;
            HudElem hmhud2;
            HudElem hmhud3;
            HudElem hmhud4;
            HudElem hmhud5;
            HudElem hmhud6;

            Log.Debug("creating player hud");
            hud0 = HudElem.CreateFontString(player, "default", 1.3f);
            hud0.SetPoint("TOP LEFT", "TOP LEFT", 110, 165);
            hud0.HideWhenInMenu = true;
            //hud0.GlowAlpha = 2f;

            hud1 = HudElem.CreateFontString(player, "default", 1.3f);
            hud1.SetPoint("TOP LEFT", "TOP LEFT", 140, 165); //50 original
            hud1.HideWhenInMenu = true;
            //hud1.GlowAlpha = 2f;

            rHud = HudElem.CreateFontString(player, "default", 1.15f);
            rHud.SetPoint("TOP LEFT", "TOP LEFT", +110, 3);
            rHud.HideWhenInMenu = true;
            //rHud.GlowAlpha = 2f;

            zmhud1 = HudElem.CreateFontString(player, "default", 1f);
            zmhud1.SetPoint("TOP LEFT", "TOP LEFT", +110, 15); //25 original
            zmhud1.HideWhenInMenu = true;

            zmhud2 = HudElem.CreateFontString(player, "default", 1f);
            zmhud2.SetPoint("TOP LEFT", "TOP LEFT", +110, 75);
            zmhud2.HideWhenInMenu = true;

            zmhud3 = HudElem.CreateFontString(player, "default", 1f);
            zmhud3.SetPoint("TOP LEFT", "TOP LEFT", 300, 15); //50 original
            zmhud3.HideWhenInMenu = true;


            //humans
            hmhud1 = HudElem.CreateFontString(player, "default", 1f);
            hmhud1.SetPoint("TOP LEFT", "TOP LEFT", +110, 15);
            hmhud1.HideWhenInMenu = true;

            hmhud2 = HudElem.CreateFontString(player, "default", 1f);
            hmhud2.SetPoint("TOP LEFT", "TOP LEFT", +110, 65);
            hmhud2.HideWhenInMenu = true;

            hmhud3 = HudElem.CreateFontString(player, "default", 1f);
            hmhud3.SetPoint("TOP LEFT", "TOP LEFT", +110, 115);
            hmhud3.HideWhenInMenu = true;

            hmhud4 = HudElem.CreateFontString(player, "default", 1f);
            hmhud4.SetPoint("TOP LEFT", "TOP LEFT", +300, 15);
            hmhud4.HideWhenInMenu = true;

            hmhud5 = HudElem.CreateFontString(player, "default", 1f);
            hmhud5.SetPoint("TOP LEFT", "TOP LEFT", +300, 65);
            hmhud5.HideWhenInMenu = true;

            hmhud6 = HudElem.CreateFontString(player, "default", 1f);
            hmhud6.SetPoint("TOP LEFT", "TOP LEFT", +300, 115);
            hmhud6.HideWhenInMenu = true;

            hud0.Alpha = 0;
            hud1.Alpha = 0;
            zmhud1.Alpha = 0;
            zmhud2.Alpha = 0;
            zmhud3.Alpha = 0;
            hmhud1.Alpha = 0;
            hmhud2.Alpha = 0;
            hmhud3.Alpha = 0;
            hmhud4.Alpha = 0;
            hmhud5.Alpha = 0;
            hmhud6.Alpha = 0;
            rHud.Alpha = 1;

            rHud.SetText("^5Hold ^:R ^5To Show Shop");
            hud0.SetText("^3Point:");

            zmhud1.SetText("^3!Sui ^1= ^5Suicide          ^:(free)\n^3!hp ^1= ^5Double Health    ^:(1500)\n^3!esp ^1= ^5More Speed       ^:(500)\n^3!ttk ^1= ^5Teleporter tk    ^:(2000)\n^3!la  ^1= ^5Stinger          ^:(500)");
            zmhud2.SetText("^3!sx  ^1= ^5Semtex          ^:(6000)\n^3!wh  ^1= ^5Wallhack         ^:(500)\n^3!fg  ^1= ^5Flashbang        ^:(600)\n^3!sm  ^1= ^5Smoke            ^:(400)\n^3!cl  ^1= ^5Claymore        ^:(300)");
            zmhud3.SetText("^6Other:\n^5!gp  ^1= ^5give point   ^:!gp ^1[^3Part Of Player Name^1]\n^5!re  ^1= ^5Restart Map      ^:(50000)\n^5!report ^1[^3Message or Bug^1] ^1= ^5Report Message or Bug :D");

            hmhud1.SetText("^3!1    ^1= ^5Random Pistol               ^:(150)\n^3!2    ^1= ^5Random Auto Pistol          ^:(200)\n^3!3    ^1= ^5Random SMG                  ^:(250)\n^3!4    ^1= ^5Random Assault              ^:(600)");
            hmhud2.SetText("^3!5    ^1= ^5Random Shotgun              ^:(600)\n^3!6    ^1= ^5Random Snipe                ^:(750)\n^3!7    ^1= ^5Random Light                ^:(850)\n^3!8    ^1= ^5Random Launcher             ^:(400)");
            hmhud3.SetText("^3!9    ^1= ^5Random Weapon               ^:(500)\n^3!aug  ^1= ^5aug weapon                  ^:(2500)\n^3!go   ^1= ^5Gold ACR + Unlimited Ammo   ^:(7000)\n^3!sui  ^1= ^5Suicide                     ^:(Free)");
            hmhud4.SetText("^3!hp   ^1= ^5Double Health               ^:(1000)\n^3!Hide ^1= ^5Invisable For 60 second     ^:(5000)\n^3!ri   ^1= ^5Riot Shield                 ^:(500)\n^3!up   ^1= ^5upgrade weapon               ^:(1000)");
            hmhud5.SetText("^3!am   ^1= ^5Max ammo                    ^:(150)\n^3!jff  ^1= ^5Just For Fun                 ^:(4000)\n^3!atk   ^1= ^5Anti Trowing Knife     ^:(2000)");
            hmhud6.SetText("^6Other:\n^5!gp  ^1= ^5give point   ^:!gp ^1[^3Part Of Player Name^1]\n^5!gg  ^1= ^5give Gun   ^:!gg ^1[^3Part Of Player Name^1]\n^5!re  ^1= ^5Restart Map      ^:(50000)\n^3MyShop By Ahmad009,^5Updated By EmineM");

            player.OnNotify("ShowShop", (entity) =>
            {
                string team = player.GetField<string>("sessionteam");

                AfterDelay(20, () =>
                {
                    if (team == "axis")
                    {
                        hud0.Alpha = 1;
                        hud1.Alpha = 1;
                        zmhud1.Alpha = 1;
                        zmhud2.Alpha = 1;
                        zmhud3.Alpha = 1;

                        hmhud1.Alpha = 0;
                        hmhud2.Alpha = 0;
                        hmhud3.Alpha = 0;
                        hmhud4.Alpha = 0;
                        hmhud5.Alpha = 0;
                        hmhud6.Alpha = 0;
                    }

                    if (team == "allies")
                    {
                        zmhud1.Alpha = 0;
                        zmhud2.Alpha = 0;
                        zmhud3.Alpha = 0;

                        hud0.Alpha = 1;
                        hud1.Alpha = 1;
                        hmhud1.Alpha = 1;
                        hmhud2.Alpha = 1;
                        hmhud3.Alpha = 1;
                        hmhud4.Alpha = 1;
                        hmhud5.Alpha = 1;
                        hmhud6.Alpha = 1;
                    }

                    rHud.Alpha = 0;

                    hud1.SetText(player.GetField<int>("cash").ToString());
                    //hud1.Call("setvalue", player.GetField<int>("cash"));
                });
            });

            player.OnNotify("HideShop", (entity) =>
            {
                AfterDelay(20, () =>
                {
                    hud0.Alpha = 0;
                    hud1.Alpha = 0;
                    zmhud1.Alpha = 0;
                    zmhud2.Alpha = 0;
                    zmhud3.Alpha = 0;

                    hmhud1.Alpha = 0;
                    hmhud2.Alpha = 0;
                    hmhud3.Alpha = 0;
                    hmhud4.Alpha = 0;
                    hmhud5.Alpha = 0;
                    hmhud6.Alpha = 0;

                    rHud.Alpha = 1;
                });
            });
        }

        private void createServerHud()
        {
            motd = HudElem.CreateServerFontString("objective", 0.8f);
            motd.SetPoint("CENTER", "BOTTOM", 0, -19);
            motd.Foreground = true;
            motd.HideWhenInMenu = true;
            OnInterval(50000, () =>
            {
                //motd.SetText("^3Server Is Created By ^5A^3h^5m^3a^5d^3009 ^0& ^2E^;m^2i^;n^2e^;M                                                       ^7More information: www.PlusMaster.Net");
                motd.SetText("^6Most Kills = ^5" + MyStats.playername + "^3 = ^1" + MyStats.max + "   ^5" + MyStats.playername1 + "^3 = ^1" + MyStats.max1 + "   ^5" + MyStats.playername2 + "^3 = ^1" + MyStats.max2);
                motd.SetPoint("CENTER", "BOTTOM", 1100, -10);
                motd.Call("moveovertime", 25);
                motd.X = -700;
                return true;
            });
        }

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            //ServerSay("damage = " + damage + " dFlags = " + dFlags + " mod = " + mod + " weapon = " + weapon + " hitloc = " + hitLoc);
            //knife => damage = 138     mod = MOD_MELLE
            //TK => DAMAGE = 135       MOD = MOD_IMPACT
            //Utilities.RawSayAll(damage.ToString());

            if (attacker.GetField<int>("isjff") == 1)
            {
                damage *= 5;
            }
            if (weapon == "throwingknife_mp")
            {
                if (player.GetField<int>("hasatk") == 0)
                {
                    if (attacker.GetField<int>("hasttk") == 1)
                    {
                        attacker.AfterDelay(100, entity =>
                        {
                            attacker.Call("setorigin", player.Origin);
                        });
                    }
                }
                if (player.GetField<int>("hasatk") == 1)
                {
                    player.Health += damage;
                }
            }

            if (player.GetField<int>("hashp") == 1)
            {
                if (mod != "MOD_FALLING")
                {
                    player.Health += damage / 2;
                }
            }

            /*if (player.GetField<int>("isgod") == 1)
            {
                damage *= 2/10;
                player.Health += damage;
            }*/
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (player == inflictor && inflictor == attacker) return;

            if (attacker != null)
            {
                if (attacker.IsPlayer && attacker.IsAlive)
                {
                    player.SetField("cash", player.GetField<int>("cash") + 100);
                    attacker.SetField("cash", attacker.GetField<int>("cash") + 100);

                    //MyStats.PointEditor(attacker, 100);
                    //MyStats.PointEditor(player, 100);
                    //MyStats.KillsWriter(attacker);
                    /*if (attacker.GetField<string>("sessionteam") == "allies")
                    {
                        if (attacker.GetField<int>("flagkiller") == 1)
                        {
                            AfterDelay(1, () =>
                            {
                                attacker.Call("iprintlnbold", "^1 -300 point for flag kill.");
                                MyStats.PointAdd(attacker, -300);
                            });
                        }
                    }*/
                }
            }

            /*Parameter[] origin = new Parameter[] { player.Origin };
            
            if (weapon == "throwingknife_mp")
            {
                if (attacker.GetField<int>("hasttk") == 1)
                {
                    attacker.AfterDelay(100, entity =>
                    {
                        attacker.Call("setorigin", origin);
                    });
                }
            }*/

            //if (player.GetField<string>("sessionteam") == "allies")
            //{
            //MyStats.MostkillsReader(player, player.GetField<int>("kills"));
            //}
        }

        public override BaseScript.EventEat OnSay3(Entity player, BaseScript.ChatType type, string name, ref string message)
        {
            if (message.ToLower().StartsWith("!"))
            {
                int points = player.GetField<int>("cash");

                string guid = player.GUID.ToString();
                string team = player.GetField<string>("sessionteam");
                int price;

                string[] readArray = message.Split('!');
                string cmd = readArray[1].ToString();

                string[] strArrays = cmd.Split(' ');

                if ((int)strArrays.Length > 1)
                {
                    if (strArrays[0].ToLower().ToString() == "gp")
                    {
                        Entity entity = this.FindByName(strArrays[1]);

                        if (entity == null)
                        {
                            this.TellClient(player, "^1That user wasn't found or multiple were found.");
                            return EventEat.EatGame;
                        }

                        int gp = 0;
                        gp = int.Parse(strArrays[2]);

                        int playerPoint = player.GetField<int>("cash");

                        if (gp >= 0)
                        {
                            if (gp <= playerPoint)
                            {

                                player.Call("iprintlnbold", gp + "^1 Points sent to ^2" + entity.Name);
                                entity.Call("iprintlnbold", "^2" + gp + " ^1Points recieved from ^2" + player.Name);
                                MyStats.PointAdd(player, -gp);
                                MyStats.PointAdd(entity, gp);
                            }
                            else
                            {
                                player.Call("iprintlnbold", "^1You dont have enough points. Your points = ^6" + playerPoint);
                            }
                        }

                        File.AppendAllText("scripts\\MyShop\\PointTransfers\\Transfers.txt", string.Concat(DateTime.Now + "  ==>  " + gp + " points from " + player.Name + " to " + entity.Name + "\n"));

                        return EventEat.EatGame;
                    }
                    else if (strArrays[0].ToLower().ToString() == "gg")
                    {
                        Entity entity = this.FindByName(strArrays[1]);

                        if (entity == null)
                        {
                            this.TellClient(player, "^1That user wasn't found or multiple were found.");
                            return EventEat.EatGame;
                        }

                        string team2 = entity.GetField<string>("sessionteam");

                        if (team2 == "axis")
                        {
                            return EventEat.EatGame;
                        }

                        if (entity.Name == player.Name)
                        {
                            TellClient(player, "^1You cant Give Gun to Yourself :| .");
                            return EventEat.EatGame;
                        }

                        TellClient(player, "^1Waiting for ^2" + entity.Name + " ^1accept.");
                        TellClient(entity, "^2" + player.Name + " ^1Want's To Send You a Gun! ^3if You Accept,Type : ^6!yes ^3and if You Don't Accept ^6!no");

                        string weaponname = player.CurrentWeapon.ToString();

                        entity.SetField("incomingweapon", weaponname);
                        entity.SetField("incomingweaponfrom", player.Name);

                        return EventEat.EatGame;
                    }

                    if (player.GetField<int>("ismaster") == 1)
                    {
                        if (strArrays[0].ToLower() == "tele")
                        {
                            Entity entity = this.FindByName(strArrays[1]);

                            if (entity == null)
                            {
                                this.TellClient(player, "^1That user wasn't found or multiple were found.");
                                return EventEat.EatGame;
                            }

                            Parameter[] origin = new Parameter[] { entity.Origin };
                            player.Call("setorigin", origin);
                            return EventEat.EatGame;
                        }
                        else if (strArrays[0].ToLower() == "tele2")
                        {
                            Entity entity = this.FindByName(strArrays[1]);

                            if (entity == null)
                            {
                                this.TellClient(player, "^1That user wasn't found or multiple were found.");
                                return EventEat.EatGame;
                            }

                            Parameter[] origin = new Parameter[] { player.Origin };
                            entity.Call("setorigin", origin);
                            return EventEat.EatGame;
                        }
                        else if (strArrays[0].ToLower() == "setpoint")
                        {
                            Entity entity = this.FindByName(strArrays[1]);

                            if (entity == null)
                            {
                                this.TellClient(player, "^1That user wasn't found or multiple were found.");
                                return EventEat.EatGame;
                            }
                            
                            int gp = 0;
                            gp = int.Parse(strArrays[2]);

                            entity.SetField("cash", gp);

                            player.Call("iprintlnbold", "^1" + entity.Name + "^3Point set to = ^5" + gp);

                            return EventEat.EatGame;
                        }
                        else if (strArrays[0].ToLower() == "tw")
                        {
                            Entity entity = this.FindByName(strArrays[1]);

                            if (entity == null)
                            {
                                this.TellClient(player, "^1That user wasn't found or multiple were found.");
                                return EventEat.EatGame;
                            }

                            entity.TakeAllWeapons();

                            return EventEat.EatGame;
                        }
                        else if (strArrays[0].ToLower() == "gpall")
                        {
                            int gpall = int.Parse(strArrays[1].ToString());

                            HudElem hud = HudElem.CreateServerFontString("default", 2f);
                            Parameter[] parameterArray = new Parameter[] { 200, 8000, 1000 };
                            hud.Call("setpulsefx", parameterArray);
                            hud.HideWhenInMenu = false;
                            hud.Foreground = true;
                            hud.Alpha = 1f;
                            hud.GlowAlpha = 5f;
                            hud.SetPoint("LEFT", "LEFT", 300, 0);
                            hud.SetText("^5" + gpall + " ^1Points from ^5" + player.Name + " ^1to all :D");

                            foreach (Entity entity2 in Players)
                            {
                                entity2.AfterDelay(50, entity =>
                                {
                                    MyStats.PointAdd(entity2, gpall);
                                });
                            }
                            return EventEat.EatGame;
                        }
                    }
                }
                

                //string cmd = message.Split('!')[0].ToLower();



                //for humans
                if (team == "allies")
                {
                    switch (cmd)
                    {
                        case "sui":
                            {
                                player.AfterDelay(200, entity =>
                                {
                                    player.Call("suicide");
                                });

                                return EventEat.EatGame;
                            }
                        case "1":
                            {
                                if (150 <= points)
                                {
                                    MyStats.PointAdd(player, -150);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Pistol!");
                                    player.TakeWeapon(player.CurrentWeapon);
                                    string weaponname = WeaponUtils.getRandomPistol(1, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "2":
                            {
                                if (200 <= points)
                                {
                                    MyStats.PointAdd(player, -200);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random AutoPistol!");
                                    player.TakeWeapon(player.CurrentWeapon);
                                    string weaponname = WeaponUtils.getRandomAutoPistol(2, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "3":
                            {
                                if (250 <= points)
                                {
                                    MyStats.PointAdd(player, -250);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random SMG!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomSMG(3, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "4":
                            {
                                if (600 <= points)
                                {
                                    MyStats.PointAdd(player, -600);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Assault Rifle!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomAR(4, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "5":
                            {
                                if (600 <= points)
                                {
                                    MyStats.PointAdd(player, -600);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Shotgun!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomShotgun(5, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "6":
                            {
                                if (750 <= points)
                                {
                                    MyStats.PointAdd(player, -750);
                                    player.Call("iprintlnbold", "^1You Bought : ^5Random Sniper Rifle!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomSniper(6, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "7":
                            {
                                if (850 <= points)
                                {
                                    MyStats.PointAdd(player, -850);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Light Machine Gun!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomLMG(7, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "8":
                            {
                                if (400 <= points)
                                {
                                    MyStats.PointAdd(player, -400);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Launcher!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomLauncher(8, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "9":
                            {
                                if (500 <= points)
                                {
                                    MyStats.PointAdd(player, -500);
                                    player.Call("iprintlnbold", "^1You bought : ^5Random Weapon!");
                                    player.TakeWeapon(player.CurrentWeapon);

                                    string weaponname = WeaponUtils.getRandomWeapon(11, 1);
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Dont Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "aug":
                            {
                                if (2500 <= points)
                                {
                                    MyStats.PointAdd(player, -2500);
                                    player.Call("iprintlnbold", "^1You bought : ^5AUG!");
                                    player.Call("giveweapon", "iw5_m60jugg_mp_silencer_camo01");
                                    player.SwitchToWeaponImmediate("iw5_m60jugg_mp_silencer_camo01");
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "up":
                            {
                                WeaponUpgrade(player);
                                ShopFX(player);
                                return EventEat.EatGame;
                            }
                        case "hp":
                            {
                                price = 1500;
                                //price = 2500;
                                if (price <= points)
                                {
                                    if (player.GetField<int>("hashp") == 1)
                                    {
                                        player.Call("iprintlnbold", "^1You already have ^2Double HP");
                                    }

                                    if (player.GetField<int>("hashp") == 0)
                                    {
                                        MyStats.PointAdd(player, -price);
                                        player.SetField("hashp", 1);
                                        ShopFX(player);

                                        player.Call("iprintlnbold", "^1You bought : ^5Double HP");
                                    }
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "atk":
                            {
                                if (2000 <= points)
                                {
                                    MyStats.PointAdd(player, -2000);
                                    player.Call("iprintlnbold", "^1You bought : ^5Anti Throwing Knife!");
                                    player.SetField("hasatk", 1);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "sx":
                            {
                                if (100 <= points)
                                {
                                    MyStats.PointAdd(player, -100);
                                    player.Call("iprintlnbold", "^3You bought : ^2Semtex Grenade!");
                                    player.Call("SetOffhandPrimaryClass", "other");
                                    player.Call("giveweapon", "semtex_mp");
                                    player.Call("givemaxammo", "semtex_mp");
                                    //player.Call("setweaponammoclip", "semtex_mp", 3);
                                    ShopFX(player);
                                    if (player.GetField<int>("ismaster") == 1)
                                    {
                                        OnInterval(1000, () =>
                                        {
                                            Nades(player, 99);
                                            return true;
                                        });
                                    }
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You dont have enough points.");
                                }
                                return EventEat.EatGame;
                            }
                        case "am":
                            {
                                if (150 <= points)
                                {
                                    MyStats.PointAdd(player, -150);
                                    player.Call("iprintlnbold", "^1You bought : ^5Maximum Ammo!");
                                    player.Call("givemaxammo", player.CurrentWeapon);
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "ri":
                            {
                                if (500 <= points)
                                {
                                    MyStats.PointAdd(player, -500);
                                    player.Call("iprintlnbold", "^1You bought : ^5Riot Shield!");
                                    player.Call("giveweapon", "riotshield_mp");
                                    player.SwitchToWeaponImmediate("riotshield_mp");
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "jff":
                            {
                                if (4000 <= points)
                                {
                                    MyStats.PointAdd(player, -3500);
                                    player.Call("iprintlnbold", "^1You bought : ^5Just For Fun :)");
                                    player.SetField("isjff", 1);
                                    mdlchanger(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "hide":
                            {
                                if (5000 <= points)
                                {
                                    MyStats.PointAdd(player, -5000);
                                    player.Call("iprintlnbold", "^1You bought : ^5Invisable For 60 Seconds!!!");
                                    player.Call("hide");
                                    ShopFX(player);
                                    player.AfterDelay(60000, entity =>
                                    {
                                        player.Call("iprintlnbold", "^1Invisibility Off");
                                        player.Call("show");
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "go":
                            {
                                if (7000 <= points)
                                {
                                    MyStats.PointAdd(player, -7000);
                                    player.Call("iprintlnbold", "^1You bought : ^5Gold ACR + ^3Unlimited Ammo!");

                                    player.TakeWeapon(player.CurrentWeapon);
                                    string weaponname = Utilities.BuildWeaponName("iw5_acr", "reflex", "none", 11, 0);
                                    player.Call("giveweapon", weaponname);
                                    player.SetField("isgold", 1);
                                    ShopFX(player);

                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                        OnInterval(100, () =>
                                        {
                                            if (player.CurrentWeapon.StartsWith("iw5_acr"))
                                            {
                                                Ammo(entity, 99);
                                                Stock(entity, 99);
                                            }
                                            return true;
                                        });
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "yes":
                            {
                                string weapname = player.GetField<string>("incomingweapon");

                                if (weapname == "nope")
                                {
                                    player.Call("iprintlnbold", "^1Nothing Found.");
                                }
                                string weapowner;

                                if (weapname != "nope")
                                {
                                    weapowner = player.GetField<string>("incomingweaponfrom");
                                    Entity entity = this.FindByName(weapowner);
                                    entity.TakeWeapon(weapname);
                                    player.Call("giveweapon", weapname);
                                    player.SwitchToWeapon(weapname);
                                    player.SetField("incomingweaponfrom", "null");
                                    player.SetField("incomingweapon", "nope");
                                }
                                return EventEat.EatGame;
                            }
                        case "no":
                            {
                                player.SetField("incomingweaponfrom", "null");
                                player.SetField("incomingweapon", "nope");
                                return EventEat.EatGame;
                            }
                    }
                }

                //for zombies
                if (team == "axis")
                {
                    switch (cmd)
                    {
                        case "hp":
                            {
                                price = 2500;
                                if (price <= points)
                                {
                                    if (player.GetField<int>("hashp") == 1)
                                    {
                                        player.Call("iprintlnbold", "^1You already have ^2Double HP");
                                    }

                                    if (player.GetField<int>("hashp") == 0)
                                    {
                                        MyStats.PointAdd(player, -price);
                                        player.SetField("hashp", 1);
                                        ShopFX(player);

                                        player.Call("iprintlnbold", "^1You bought : ^5Double HP");
                                    }
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "sx":
                            {
                                if (5000 <= points)
                                {
                                    MyStats.PointAdd(player, -5000);
                                    player.Call("iprintlnbold", "^1You bought : ^5Semtex Grenade!");
                                    player.Call("SetOffhandPrimaryClass", "other");
                                    player.Call("giveweapon", "semtex_mp");
                                    player.Call("givemaxammo", "semtex_mp");
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "cl":
                            {
                                if (300 <= points)
                                {
                                    MyStats.PointAdd(player, -300);
                                    player.Call("iprintlnbold", "^1You bought : ^5Claymore!");
                                    player.Call("SetOffhandSecondaryClass", "claymore");
                                    player.Call("giveweapon", "claymore_mp");
                                    player.Call("setweaponammoclip", "claymore_mp", 1);
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "sm":
                            {
                                if (400 <= points)
                                {
                                    MyStats.PointAdd(player, -400);
                                    player.Call("iprintlnbold", "^1You bought : ^5Smoke Grenade!");
                                    player.Call("SetOffhandSecondaryClass", "smoke");
                                    player.Call("giveweapon", "smoke_grenade_mp");
                                    player.Call("setweaponammoclip", "smoke_grenade_mp", 1);
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "ttk":
                            {
                                if (2000 <= points)
                                {
                                    MyStats.PointAdd(player, -2000);
                                    player.Call("iprintlnbold", "^1You bought : ^5Teleporter tk!");
                                    player.SetField("hasttk", 1);
                                    player.Call("SetOffhandPrimaryClass", "throwingknife");
                                    player.Call("giveweapon", "throwingknife_mp");
                                    player.Call("setweaponammoclip", "throwingknife_mp", 1);
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "fg":
                            {
                                if (600 <= points)
                                {
                                    MyStats.PointAdd(player, -600);
                                    player.Call("iprintlnbold", "^1You bought : ^52 Flash Grenades!");
                                    player.Call("SetOffhandSecondaryClass", "flash");
                                    player.Call("giveweapon", "flash_grenade_mp");
                                    player.Call("setweaponammoclip", "flash_grenade_mp", 2);
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "la":
                            {
                                if (500 <= points)
                                {
                                    MyStats.PointAdd(player, -500);
                                    player.Call("iprintlnbold", "^1You bought : ^5Stinger!");
                                    string weaponname = "stinger_mp";
                                    player.Call("giveweapon", weaponname);
                                    ShopFX(player);
                                    player.AfterDelay(100, entity =>
                                    {
                                        player.SwitchToWeaponImmediate(weaponname);
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "wh":
                            {
                                if (500 <= points)
                                {
                                    MyStats.PointAdd(player, -500);
                                    player.Call("iprintlnbold", "^1You bought : ^5WallHack!");
                                    player.Call("thermalvisionfofoverlayon");
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "esp":
                            {
                                if (500 <= points)
                                {
                                    MyStats.PointAdd(player, -500);
                                    player.Call("iprintlnbold", "^1You bought : ^5More Speed!");
                                    player.SetField("movespeed", 1.7f);
                                    player.SetField("hasesp", 1);
                                    ShopFX(player);
                                    OnInterval(100, () =>
                                    {
                                        player.Call("setmovespeedscale", new Parameter(player.GetField<float>("movespeed")));
                                        return true;
                                    });
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }
                                return EventEat.EatGame;
                            }
                        case "re":
                            {
                                if (50000 <= points)
                                {
                                    MyStats.PointAdd(player, -50000);
                                    MyStats.AllPointsWriter();

                                    player.Call("iprintlnbold", "^1You bought : ^5Restart Map!");
                                    ServerSay("^2" + player.Name + " ^1Bought Restart Map.");
                                    AfterDelay(4000, () =>
                                    {
                                        Utilities.ExecuteCommand("fast_restart");
                                    });
                                    ShopFX(player);
                                }
                                else
                                {
                                    player.Call("iprintlnbold", "^1You Don't Have Enough Points!");
                                }

                                return EventEat.EatGame;
                            }
                    }
                }

                //for Master Admins
                if (player.GetField<int>("ismaster") == 1)
                {
                    switch (cmd)
                    {
                        case "tpall":
                            {
                                Parameter[] origin = new Parameter[] { player.Origin };

                                foreach (Entity entity2 in Players)
                                {
                                    entity2.AfterDelay(50, entity =>
                                    {
                                        entity2.Call("setorigin", origin);
                                    });
                                }
                                return EventEat.EatGame;
                            }
                        case "suiall":
                            {
                                foreach (Entity entity2 in Players)
                                {
                                    entity2.AfterDelay(200, entity =>
                                    {
                                        entity2.Call("suicide");
                                    });
                                }
                                return EventEat.EatGame;
                            }
                        case "spall":
                            {
                                foreach (Entity entity2 in Players)
                                {
                                    TellClient(player, entity2.Name + "^6 point = ^5" + entity2.GetField<int>("cash"));
                                }
                                return EventEat.EatGame;
                            }
                        case "weap":
                            {
                                player.Call("iprintlnbold", player.CurrentWeapon.ToString());
                                return EventEat.EatGame;
                            }
                        case "speed":
                            {
                                player.SetField("movespeed", 2f);

                                OnInterval(100, () =>
                                {
                                    player.Call("setmovespeedscale", new Parameter(player.GetField<float>("movespeed")));
                                    return true;
                                });
                                return EventEat.EatGame;
                            }
                    }
                }
            }

            return EventEat.EatNone;
        }

        public void doCustomWeapon(Entity player, string name, string bullet)
        {
            player.OnNotify("weapon_fired", (p, weaponName) =>
            {
                if (((string)weaponName).Contains(name))
                {
                    Call("magicbullet", bullet,
                        new Parameter(player.Call<Vector3>("getTagOrigin", "tag_weapon_left")),
                        new Parameter(Call<Vector3>("anglestoforward", player.Call<Vector3>("getPlayerAngles")) * 1000000),
                        new Parameter(player));
                }
            });
        }

        public bool Ammo(Entity player, int amount)
        {
            if (PlayerStop.Contains(player.GetField<string>("name")))
                return false;
            var wep = player.CurrentWeapon;
            player.Call("setweaponammoclip", wep, amount);
            player.Call("setweaponammoclip", wep, amount, "left");
            player.Call("setweaponammoclip", wep, amount, "right");
            return true;
        }

        public bool Stock(Entity player, int amount)
        {
            if (PlayerStop.Contains(player.GetField<string>("name")))
                return false;
            var wep = player.CurrentWeapon;
            player.Call("setweaponammostock", wep, amount);
            return true;
        }

        public bool Weapon(Entity player, string weapon, string add = "", string weapon2 = "", bool strip = true)
        {
            if (PlayerStop.Contains(player.GetField<string>("name")))
                return false;
            if (strip)
                player.TakeAllWeapons();
            if (add == "akimbo")
                weapon = weapon + "_akimbo";
            player.GiveWeapon(weapon);
            player.SwitchToWeapon(weapon);
            if (!string.IsNullOrEmpty(weapon2))
                player.GiveWeapon(weapon2);
            player.Call("disableweaponpickup");
            Stock(player, 999);
            return true;
        }

        public bool Nades(Entity player, int amount)
        {
            if (PlayerStop.Contains(player.GetField<string>("name")))
                return false;
            var offhand = player.Call<string>("getcurrentoffhand");
            player.Call("setweaponammoclip", offhand, amount);
            player.Call("givemaxammo", offhand);
            return true;
        }

        public bool Speed(Entity player, double scale)
        {
            if (PlayerStop.Contains(player.GetField<string>("name")))
                return false;
            player.Call("setmovespeedscale", new Parameter((float)scale));
            return true;
        }


        private Entity FindByName(string name)
        {
            int num = 0;
            Entity entity = null;
            foreach (Entity entity2 in Players)
            {
                if (0 <= entity2.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    entity = entity2;
                    num++;
                }
            }
            if ((num <= 1) && (num == 1))
            {
                return entity;
            }
            return null;
        }

        private void TellClient(Entity player, string message)
        {
            Utilities.RawSayTo(player, string.Concat("^7", message));
        }

        void bodymodelchanger(Entity player, string mapname)
        {
            switch (mapname)
            {
                case "mp_radar": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a_arctic")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_arctic_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_ghillie_arctic_sniper")); break;
                        case 3: break;
                    } break;
                case "mp_moab": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_assault_a_woodland")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_woodland_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_ghillie_desert_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_desert_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_dome":
                case "mp_restrepo_ss": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_ghillie_desert_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_desert_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_lambeth":
                case "mp_park":
                case "mp_six_ss": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_assault_a_woodland")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_woodland_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_lmg_a_woodland")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_woodland_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_cement":
                case "mp_exchange":
                case "mp_overwatch":
                case "mp_hardhat": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_smg_a_airborne")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_air_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_assault_a_airborne")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_terminal_cls":
                case "mp_alpha":
                case "mp_hillside_ss": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_urban_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_lmg_a")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_smg_a")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_burn_ss": switch (_rnd.Next(4))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_delta_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_henchmen_shotgun_b")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_henchmen_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_desert_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_morningwood":
                case "mp_qadeem": switch (_rnd.Next(4))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_delta_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_henchmen_shotgun_b")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_henchmen_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_aground_ss":
                case "mp_courtyard_ss":
                case "mp_italy":
                case "mp_meteora": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_henchmen_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_henchmen_shotgun_b")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_sas_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_seatown": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_henchmen_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_henchmen_shotgun_b")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_sas_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_desert_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_bravo":
                case "mp_carbon":
                case "mp_mogadishu":
                case "mp_shipbreaker":
                case "mp_village": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_africa_militia_lmg_b")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_africa_militia_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_ghillie_africa_militia_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_africa_militia_lmg_b")); break;
                        case 4: break;
                    } break;
                case "mp_plaza2": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_lmg_a")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_sas_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_underground": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_russian_air_sniper")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a_airborne")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_sas_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;
                case "mp_paris": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_gign_paris_lmg")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_sas_sniper")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_opforce_ghillie_urban_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_smg_a")); break;
                        case 4: break;
                    } break;
                case "mp_bootleg": switch (_rnd.Next(5))
                    {
                        case 0: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_smg_a")); break;
                        case 1: AfterDelay(100, () => player.Call("setmodel", "mp_body_russian_military_shotgun_a")); break;
                        case 2: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_pmc_sniper")); break;
                        case 3: AfterDelay(100, () => player.Call("setmodel", "mp_body_ally_ghillie_urban_sniper")); break;
                        case 4: break;
                    } break;

                default:
                    break;
            }
        }

        public static string GetStringInBetween(string strBegin, string strEnd, string strSource, bool includeBegin, bool includeEnd)
        {
            string[] result = { string.Empty, string.Empty };
            int iIndexOfBegin = strSource.IndexOf(strBegin);

            if (iIndexOfBegin != -1)
            {
                // include the Begin string if desired 
                if (includeBegin)
                    iIndexOfBegin -= strBegin.Length;

                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);

                int iEnd = strSource.IndexOf(strEnd);
                if (iEnd != -1)
                {
                    // include the End string if desired 
                    if (includeEnd)
                        iEnd += strEnd.Length;
                    result[0] = strSource.Substring(0, iEnd);
                    // advance beyond this segment 
                    if (iEnd + strEnd.Length < strSource.Length)
                        result[1] = strSource.Substring(iEnd + strEnd.Length);
                }
            }
            else
                // stay where we are 
                result[1] = strSource;
            return result[0];
        }

        private void WeaponUpgrade(Entity ent)
        {
            string weap = ent.CurrentWeapon;
            string weapname = GetStringInBetween("iw5_", "_", weap, false, false);
            string fullweapname = "iw5_" + GetStringInBetween("iw5_", "_", weap, false, false);
            int upgradeprice = 0;
            string newweaponname = "";

            if (Array.IndexOf(WeaponUtils._pistolList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "pistol");
                newweaponname = fullweapname + "_mp_akimbo";
            }

            if (Array.IndexOf(WeaponUtils._autoPistolList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "auto pistol");
                newweaponname = fullweapname + "_mp_xmags_akimbo";
            }

            if (Array.IndexOf(WeaponUtils._smgList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "smg");
                newweaponname = fullweapname + "_mp_reflexsmg_camo11";
            }

            if (Array.IndexOf(WeaponUtils._arList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "assault rifle");
                newweaponname = fullweapname + "_mp_reflex_camo11";
            }

            if (Array.IndexOf(WeaponUtils._sniperList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "sniper rifle");
                newweaponname = fullweapname + "_mp_acog_camo11";
            }

            if (Array.IndexOf(WeaponUtils._shotgunList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "shotgun");
                newweaponname = fullweapname + "_mp_xmags_camo11";
            }

            if (Array.IndexOf(WeaponUtils._lmgList, fullweapname) >= 0)
            {
                upgradeprice = 1000;
                ent.SetField("weapontype", "light machine");
                newweaponname = fullweapname + "_mp_xmags_reflexlmg_camo11";
            }

            if (weapname == "" || fullweapname.StartsWith("iw5_m60jugg"))
            {
                TellClient(ent, "^1This Weapon cannot Upgrade!");
                return;
            }

            if (ent.GetField<int>("cash") >= upgradeprice)
            {
                ent.TakeWeapon(ent.CurrentWeapon);
                buyItem(ent, upgradeprice, "Upgrade weapon");
                ent.Call("giveweapon", newweaponname);
                ent.SwitchToWeapon(newweaponname);
                OnInterval(100, () =>
                {
                    ent.Call("recoilscaleon", 0f);
                    return true;
                });
            }
            else
            {
                TellClient(ent, "^1you dont have enogh points for Upgrade!");
            }
        }

        public void buyItem(Entity ent, int price, string itemname)
        {
            int pnt = ent.GetField<int>("cash");//MyStats.PointsReader(ent);

            if (price <= pnt)
            {
                ent.SetField("cash", (pnt - price));
                ent.Call("iprintlnbold", "^1You bought : ^5" + itemname);
                ok = true;
            }
            else
            {
                ent.Call("iprintlnbold", "^3You Don't Have Enough Points For : ^1" + itemname);
                ok = false;
            }
        }

        private void ShopFX(Entity player)
        {
            ((BaseScript)this).Call("playfx", (Parameter)((BaseScript)this).Call<int>("loadfx", new Parameter[1] { (Parameter)"props/cash_player_drop" }), (Parameter)player.Call<Vector3>("gettagorigin", new Parameter[1] { (Parameter)"j_spine4" }));//explosions
            player.Call("playsound", new Parameter[1] { (Parameter)"mp_killconfirm_tags_pickup" });
        }
    }
}