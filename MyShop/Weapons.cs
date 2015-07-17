using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using InfinityScript;

namespace MyShop
{
    class WeaponUtils
    {
        private static Random _rng = new Random();

        public static string weapon_getBasename(string weapon)
        {
            string[] split = weapon.Split('_');
            if (split.Length < 2) return "";
            return split[0] + "_" + split[1];
        }

        public static string[] weapon_getAttachments(string weapon)
        {
            string[] split = weapon.Split('_');
            if (split.Length < 4) return new string[1];
            string[] attachments = new string[5];
            int attach = 0;
            int i = 3;
            while(split.Length > i && attach < 5)
            {
                if (!split[i].Contains("camo") && !split[i].Contains("ret") && !split[i].Contains("scope"))
                {
attachments[attach] = split[i];
attachments[attach] = attachments[attach].Replace("smg", "");
attachments[attach] = attachments[attach].Replace("lmg", "");
attach++;
                }
                i++;
            }
            Array.Resize<string>(ref attachments, attach);
            return attachments;
        }

        // neither of these really work
        public static int weapon_getcamo(string weapon)
        {
            string[] split = weapon.Split('_');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains("camo"))
                {
return int.Parse(split[i].Split('0')[1]);
                }
            }
            return 0;
        }

        public static int weapon_getReticle(string weapon)
        {
            string[] split = weapon.Split('_');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Contains("ret"))
                {
return int.Parse(split[i].Split('t')[1]);
                }
            }
            return 0;
        }

        public static string getRandomPistol(int camo=-1, int reticle=-1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_pistolList[_rng.Next(_pistolList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomAutoPistol(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_autoPistolList[_rng.Next(_autoPistolList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomSMG(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_smgList[_rng.Next(_smgList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomAR(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_arList[_rng.Next(_arList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomShotgun(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_shotgunList[_rng.Next(_shotgunList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomSniper(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_sniperList[_rng.Next(_sniperList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomLMG(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_lmgList[_rng.Next(_lmgList.Length)], "", "none", camo, reticle);
        }
        public static string getRandomLauncher(int camo = -1, int reticle = -1)
        {
            if (camo == -1) camo = _rng.Next(14);
            if (reticle == -1) reticle = _rng.Next(7);
            return Utilities.BuildWeaponName(_launcherList[_rng.Next(_launcherList.Length)], "", "none", camo, reticle);
        }

        public static string getRandomWeapon(int camo = -1, int reticle = -1)
        {
            switch(_rng.Next(7))
            {
                case 0:
return getRandomPistol(camo, reticle);
                case 1:
return getRandomSMG(camo, reticle);
                case 2:
return getRandomAR(camo, reticle);
                case 3:
return getRandomShotgun(camo, reticle);
                case 4:
return getRandomSniper(camo, reticle);
                case 5:
return getRandomLMG(camo, reticle);
                default:
return getRandomLauncher(camo, reticle);
            }
        }

        private static void print(string format, params object[] p)
        {
            Log.Write(LogLevel.All, format, p);
        }
        
        public static string weapon_getWeaponName(string baseName, string []attachments, int camo, int reticle)
        {
            Function.SetEntRef(-1);
            bool _usesRet = false;
            for (int i = 0; i < attachments.Length; i++)
            {
                // why does this matter?
                if (GetAttachmentType(attachments[i]) == "rail")
                {
_usesRet = true;
                }

                // just map them... should get ignored if they arent scopes
                if (GetAttachmentType(attachments[i]) == "rail")
                {
attachments[i] = AttachmentMap(attachments[i], baseName);
                }
            }
            if(_usesRet && reticle > 0)
            {
                reticle = 0;
            }

            var bareWeaponName = "";
            var weaponName = "";

            if (baseName.Contains("iw5_"))
            {
                weaponName = baseName + "_mp";
                bareWeaponName = baseName.Substring(4);
            }
            else
            {
                weaponName = baseName;
            }

            bool _hasScope = false;
            if (GetWeaponClass(baseName) == "weapon_sniper")
            {
                for(int i=0; i<attachments.Length;i++)
                {
if (GetAttachmentType(attachments[i]) == "rail" && attachments[i] == "zoomscope")
{
    _hasScope = true;
}
                }
                if(!_hasScope)
attachments[attachments.Length + 1] = bareWeaponName + "scope";
            }

            for(int i=0; i<attachments.Length; i++)
            {
                if (attachments[i] == "vzscope")
                {
attachments[i] = bareWeaponName + "scopevz";
                }
            }

            attachments = attachments.OrderBy(attachment => attachment ?? "zz").ToArray();

            foreach (var attachment in attachments)
            {
                if (string.IsNullOrEmpty(attachment))
                {
continue;
                }

                weaponName += "_" + attachment;
            }

            var weaponClass = GetWeaponClass(baseName);

            if (weaponName.Contains("iw5_"))
            {
                if (weaponClass != "weapon_pistol" && weaponClass != "weapon_machine_pistol" && weaponClass != "weapon_projectile")
                {
weaponName = BuildWeaponNameCamo(weaponName, camo);
                }

                weaponName = BuildWeaponNameReticle(weaponName, reticle);

                return weaponName;
            }
            else
            {
                if (weaponClass != "weapon_pistol" && weaponClass != "weapon_machine_pistol" && weaponClass != "weapon_projectile")
                {
weaponName = BuildWeaponNameCamo(weaponName, camo);
                }

                weaponName = BuildWeaponNameReticle(weaponName, reticle);

                return weaponName + "_mp";
            }
        }

        private static string BuildWeaponNameCamo(string weaponName, int camo)
        {
            if (camo <= 0)
            {
                return weaponName;
            }

            if (camo < 10)
            {
                weaponName += "_camo0";
            }
            else
            {
                weaponName += "_camo";
            }

            weaponName += camo.ToString();

            return weaponName;
        }

        private static string BuildWeaponNameReticle(string weaponName, int reticle)
        {
            if (reticle <= 0)
            {
                return weaponName;
            }

            return weaponName + "_scope" + reticle.ToString();
        }

        public static string GetWeaponClass(string weapon)
        {
            Function.SetEntRef(-1);

            var tokens = weapon.Split('_');
            var weaponClass = "";

            if (tokens[0] == "iw5")
            {
                var concatName = tokens[0] + "_" + tokens[1];
                weaponClass = Function.Call<string>("tableLookup", "mp/statstable.csv", 4, concatName, 2);
            }
            else if (tokens[0] == "alt")
            {
                var concatName = tokens[1] + "_" + tokens[2];
                weaponClass = Function.Call<string>("tableLookup", "mp/statstable.csv", 4, concatName, 2);
            }
            else
            {
                weaponClass = Function.Call<string>("tableLookup", "mp/statstable.csv", 4, tokens[0], 2);
            }

            if (weaponClass == "")
            {
                weapon = Regex.Replace(weapon, "_mp$", "");

                weaponClass = Function.Call<string>("tableLookup", "mp/statstable.csv", 4, weapon, 2);
            }

            if (weapon == "none" || weaponClass == "")
            {
                weaponClass = "other";
            }

            return weaponClass;
        }

        public static string GetAttachmentType(string attachmentName)
        {
            Function.SetEntRef(-1);
            return Function.Call<string>("tableLookup", "mp/attachmenttable.csv", 4, attachmentName, 2);
        }

        public static string AttachmentMap(string attachmentName, string weaponName)
        {
            Function.SetEntRef(-1);

            var weaponClass = GetWeaponClass(weaponName);

            switch (weaponClass)
            {
                case "weapon_smg":
if (attachmentName == "reflex")
    return "reflexsmg";
else if (attachmentName == "eotech")
    return "eotechsmg";
else if (attachmentName == "acog")
    return "acogsmg";
else if (attachmentName == "thermal")
    return "thermalsmg";

return attachmentName;
                case "weapon_lmg":
if (attachmentName == "reflex")
    return "reflexlmg";
else if (attachmentName == "eotech")
    return "eotechlmg";

return attachmentName;
                case "weapon_machine_pistol":
if (attachmentName == "reflex")
    return "reflexsmg";
else if (attachmentName == "eotech")
    return "eotechsmg";

return attachmentName;
                default:
return attachmentName;
            }
        }

        public static bool checkAttachments(string weapon, string attach)
        {
            string basename = weapon_getBasename(weapon);
            if (attach == "akimbo")
            {
                if (GetWeaponClass(basename) != "weapon_machine_pistol" && GetWeaponClass(basename) != "weapon_pistol")
                {
return false;
                }
            }
            if (GetAttachmentType(attach) == "rail" && GetWeaponClass(basename) == "weapon_sniper")
            {
                return false;
            }
            return true;
        }

        public static string upgradeWeapon(Entity player)
        {
            string weapon = player.CurrentWeapon;
            string basenewweapon = "";
            string basename = WeaponUtils.weapon_getBasename(weapon);
            int camo = player.GetField<int>("camo");
            int reticle = player.GetField<int>("reticle");

            if (weapon.StartsWith("iw5_pp90m1"))
            {
                player.Call("iprintlnbold", "^3IS SMG");
                player.TakeWeapon(weapon);
                basenewweapon = WeaponUtils.getRandomAR(camo, reticle);
            }

            if (Array.IndexOf(WeaponUtils._smgList, basename) > 0)
            {
                player.TakeWeapon(weapon);
                player.Call("iprintlnbold", "^3IS SMG");
                basenewweapon = WeaponUtils.getRandomAR(camo, reticle);
            }

            else if (Array.IndexOf(WeaponUtils._arList, basename) > 0)
            {
                player.TakeWeapon(weapon);
                basenewweapon = WeaponUtils.getRandomLMG(camo, reticle);
            }
            else if (Array.IndexOf(WeaponUtils._pistolList, basename) > 0)
            {
                player.TakeWeapon(weapon);
                basenewweapon = WeaponUtils.getRandomAutoPistol(camo, reticle);
            }
            else
            {
                return "You cannot upgrade this weapon!";
            }

            string[] newattach = WeaponUtils.weapon_getAttachments(weapon);
            string newweapon = WeaponUtils.weapon_getWeaponName(basenewweapon, newattach, player.GetField<int>("camo"), player.GetField<int>("reticle"));
            player.GiveWeapon(newweapon);

            player.AfterDelay(100, entity =>
            {
                player.SwitchToWeaponImmediate(newweapon);
                player.Call("iprintlnbold", "Upgraded Weapon!");
            });

            return "";
        }


        public static string[] _pistolList = new[]
        {
            "iw5_44magnum",
            "iw5_usp45",
            "iw5_mp412",
            "iw5_p99",
            "iw5_fnfiveseven"
        };

        public static string[] _autoPistolList = new[]
        {
            "iw5_fmg9",
            "iw5_skorpion",
            "iw5_mp9",
            "iw5_g18"
        };

        public static string[] _smgList = new[]
        {
            "iw5_mp5",
            "iw5_m9",
            "iw5_p90",
            "iw5_pp90m1",
            "iw5_ump45",
            "iw5_mp7"
        };
        public static string[] _arList = new[]
        {
            "iw5_ak47",
            "iw5_m16",
            "iw5_m4",
            "iw5_fad",
            "iw5_acr",
            "iw5_type95",
            "iw5_mk14",
            "iw5_scar",
            "iw5_g36c",
            "iw5_cm901"
        };
        public static string[] _launcherList = new[]
        {
            "rpg",
            "iw5_smaw",
            "xm25"
        };
        public static string[] _sniperList = new[]
        {
            "iw5_dragunov",
            "iw5_msr",
            "iw5_barrett",
            "iw5_rsass",
            "iw5_as50",
            "iw5_l96a1"
        };
        public static string[] _shotgunList = new[]
        {
            "iw5_1887",
            "iw5_striker",
            "iw5_aa12",
            "iw5_usas12",
            "iw5_spas12",
            "iw5_ksg"
        };

        
        public static string[] _lmgList = new[]
        {
            "iw5_m60",
            "iw5_mk46",
            "iw5_pecheneg",
            "iw5_sa80",
            "iw5_mg36",
        };
        public static string[] _camoList = new[]
        {
            "None",
            "Classic",
            "Snow",
            "Multicam",
            "Digital",
            "Hex",
            "Choco",
            "Snake",
            "Blue",
            "Red",
            "Autumn",
            "Gold",
            "Marine"
        };
    }
}

/*
 * Weapons: iw5_usp45, iw5_mp412, iw5_44magnum, iw5_deserteagle, iw5_p99, iw5_fnfiveseven, 
 * iw5_acr, iw5_type95, iw5_m4, iw5_ak47, iw5_m16, iw5_mk14, iw5_g36c, iw5_scar, iw5_fad, iw5_cm901, 
 * iw5_mp5, iw5_m9, iw5_p90, iw5_pp90m1, iw5_ump45, iw5_mp7, iw5_fmg9, iw5_g18, iw5_mp9, iw5_skorpion, 
 * iw5_spas12, iw5_aa12, iw5_striker, iw5_1887, iw5_usas12, iw5_ksg, iw5_m60, iw5_mk46, iw5_pecheneg, 
 * iw5_sa80, iw5_mg36, iw5_barrett, iw5_msr, iw5_rsass, iw5_dragunov, iw5_as50, iw5_l96a1, rpg, javelin, 
 * stinger, iw5_smaw, m320, riotshield, xm25, iw5_m60jugg_mp

 * Attachments:
 * reflex, acog, grip, akimbo, thermal, shotgun, heartbeat, xmags, rof, eotech, tactical, vzscope, gl, 
 * gp25, m320, silencer, silencer02, silencer03, hamrhybrid, hybrid

 * Red Dots: (referenced as numbers 0-5 and if none specefied default dot)
 * Target Dot, Delta, U-Dot, Mil-Dot, Omega, Lambada

 * Camos: (referenced as numbers 0-13)
 * None, Classic, Snow, Multicam, Digital, Hex, Choco, Snake, Blue, Red, Autumn, Gold, Marine

 * Tactical:
 * flash_grenade_mp, concussion_grenade_mp, specialty_scrambler, emp_grenade_mp, smoke_grenade_mp, 
 * trophy_mp, specialty_tacticalinsertion, specialty_portable_radar

 * Lethal:
 * bouncingbetty_mp, frag_grenade_mp, semtex_mp, throwingknife_mp, claymore_mp, c4_mp

 * Killstreaks:
 * uav, airdrop_assault, ims, predator_missile, airdrop_sentry_minigun, precision_airstrike, helicopter, 
 * littlebird_flock, littlebird_support, remote_mortar, airdrop_remote_tank, ac130, helicopter_flares, 
 * airdrop_juggernaut, osprey_gunner

 * uav_support, counter_uav, deployable_vest, sam_turret, remote_uav, airdrop_trap, triple_uav, 
 * remote_mg_turret, emp, stealth_airstrike, airdrop_juggernaut_recon, escort_airdrop

 * specialty_longersprint_ks, specialty_fastreload_ks, specialty_scavenger_ks, specialty_blindeye_ks, 
 * specialty_paint_ks, specialty_hardline_ks, specialty_coldblooded_ks, specialty_quickdraw_ks, 
 * _specialty_blastshield_ks, specialty_detectexplosive_ks, specialty_autospot_ks, 
 * specialty_bulletaccuracy_ks, specialty_quieter_ks, specialty_stalker_ks

 * Deathstreaks:
 * specialty_juiced, specialty_revenge, specialty_finalstand, specialty_grenadepulldeath, 
 * specialty_c4death, specialty_stopping_power

 * Perks:
 * specialty_paint, specialty_fastreload, specialty_blindeye, specialty_longersprint, specialty_scavenger 
 * specialty_quickdraw, _specialty_blastshield, specialty_hardline, specialty_coldblooded, 
 * specialty_twoprimaries specialty_autospot, specialty_stalker, specialty_detectexplosive, 
 * specialty_bulletaccuracy, specialty_quieter
 
 * Perks Pro:
 * specialty_fastmantle *Extreme Conditoning Pro, specialty_quickswap *sleight of hand Pro*, specialty_extraammo *scavenger pro*, specialty_fasterlockon *Blind Eye Pro Part 1*, specialty_armorpiercing *Blind Eye Pro part 2*, specialty_paint_pro *Recon Pro*, 
 * specialty_rollover *Hardline Pro Part 1*, specialty_assists *Hardline Pro part 2*, specialty_spygame *Assassin Pro Part 1*, specialty_empimmune *Assassin Pro Part 2*, specialty_fastoffhand *Quickdraw Pro*, specialty_overkillpro *Overkill Pro duh*, specialty_stun_resistance *Blastshield Pro*
 * specialty_holdbreath *Marksman pro*, specialty_selectivehearing *Sitrep Pro*, specialty_fastsprintrecovery *Steady Aim Pro*, specialty_falldamage *Dead Silence Pro*, specialty_delaymine *Stalker Pro* 

 * Proficiencies:
 * specialty_marksman, specialty_bulletpenetration, specialty_bling, specialty_sharp_focus, 
 * specialty_holdbreathwhileads, specialty_reducedsway, specialty_longerrange, specialty_fastermelee,
 * specialty_lightweight, specialty_moredamage

*/