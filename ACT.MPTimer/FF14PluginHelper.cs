namespace ACT.MPTimer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    using Advanced_Combat_Tracker;

    public static partial class FF14PluginHelper
    {
        private static object lockObject = new object();
        private static object plugin;
        private static dynamic pluginConfig;
        private static object pluginMemory;
        private static dynamic pluginScancombat;

        public static Process GetFFXIVProcess
        {
            get
            {
                try
                {
                    Initialize();

                    if (pluginConfig == null)
                    {
                        return null;
                    }

                    var process = pluginConfig.Process;

                    return (Process)process;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static List<Combatant> GetCombatantList()
        {
            Initialize();

            var result = new List<Combatant>();

            if (plugin == null)
            {
                return result;
            }

            if (GetFFXIVProcess == null)
            {
                return result;
            }

            if (pluginScancombat == null)
            {
                return result;
            }

            dynamic list = pluginScancombat.GetCombatantList();
            foreach (dynamic item in list.ToArray())
            {
                if (item == null)
                {
                    continue;
                }

                var combatant = new Combatant();

                combatant.Name = (string)item.Name;
                combatant.ID = (uint)item.ID;
                combatant.Job = (int)item.Job;
                combatant.CurrentMP = (int)item.CurrentMP;
                combatant.MaxMP = (int)item.MaxMP;

                result.Add(combatant);
            }

            return result;
        }

        public static Combatant GetCombatantPlayer()
        {
            var result = default(Combatant);

            Initialize();

            if (plugin == null)
            {
                return result;
            }

            if (GetFFXIVProcess == null)
            {
                return result;
            }

            if (pluginScancombat == null)
            {
                return result;
            }

            object[] list = pluginScancombat.GetCombatantList().ToArray();
            if (list.Length > 0)
            {
                var item = (dynamic)list[0];
                var combatant = new Combatant();

                combatant.Name = (string)item.Name;
                combatant.ID = (uint)item.ID;
                combatant.Job = (int)item.Job;
                combatant.CurrentMP = (int)item.CurrentMP;
                combatant.MaxMP = (int)item.MaxMP;

                result = combatant;
            }

            return result;
        }

        public static List<uint> GetCurrentPartyList(
            out int partyCount)
        {
            Initialize();

            var partyList = new List<uint>();
            partyCount = 0;

            if (plugin == null)
            {
                return partyList;
            }

            if (GetFFXIVProcess == null)
            {
                return partyList;
            }

            if (pluginScancombat == null)
            {
                return partyList;
            }

            partyList = pluginScancombat.GetCurrentPartyList(
                out partyCount) as List<uint>;

            return partyList;
        }

        public static Player GetPlayerData()
        {
            Initialize();

            var result = new Player();

            if (plugin == null)
            {
                return result;
            }

            if (GetFFXIVProcess == null)
            {
                return result;
            }

            if (pluginScancombat == null)
            {
                return result;
            }

            dynamic playerData = pluginScancombat.GetPlayerData();
            if (playerData != null)
            {
                result.JobID = playerData.JobID;
                result.Pie = playerData.Pie;
                /*
                result.Str = playerData.Str;
                result.Dex = playerData.Dex;
                result.Vit = playerData.Vit;
                result.Intel = playerData.Intel;
                result.Mnd = playerData.Mnd;
                result.Attack = playerData.Attack;
                result.Accuracy = playerData.Accuracy;
                result.Crit = playerData.Crit;
                result.AttackMagicPotency = playerData.AttackMagicPotency;
                result.HealMagicPotency = playerData.HealMagicPotency;
                result.Det = playerData.Det;
                result.SkillSpeed = playerData.SkillSpeed;
                result.SpellSpeed = playerData.SpellSpeed;
                result.WeaponDmg = playerData.WeaponDmg;
                */
            }

            return result;
        }

        public static void Initialize()
        {
            lock (lockObject)
            {
                if (!ActGlobals.oFormActMain.Visible)
                {
                    return;
                }

                if (plugin == null)
                {
                    foreach (var item in ActGlobals.oFormActMain.ActPlugins)
                    {
                        if (item.pluginFile.Name.ToUpper() == "FFXIV_ACT_Plugin.dll".ToUpper() &&
                            item.lblPluginStatus.Text.ToUpper() == "FFXIV Plugin Started.".ToUpper())
                        {
                            plugin = item.pluginObj;
                            break;
                        }
                    }
                }

                if (plugin != null)
                {
                    FieldInfo fi;

                    if (pluginMemory == null)
                    {
                        fi = plugin.GetType().GetField("_Memory", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        pluginMemory = fi.GetValue(plugin);
                    }

                    if (pluginMemory == null)
                    {
                        return;
                    }

                    if (pluginConfig == null)
                    {
                        fi = pluginMemory.GetType().GetField("_config", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        pluginConfig = fi.GetValue(pluginMemory);
                    }

                    if (pluginConfig == null)
                    {
                        return;
                    }

                    if (pluginScancombat == null)
                    {
                        fi = pluginConfig.GetType().GetField("ScanCombatants", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        pluginScancombat = fi.GetValue(pluginConfig);
                    }
                }
                else
                {
                    Trace.WriteLine("Error!, FFXIV_ACT_Plugin.dll not found.");
                }
            }
        }
    }

    public class Combatant
    {
        public int CurrentHP;
        public int CurrentMP;
        public int CurrentTP;
        public uint ID;
        public int Job;
        public int Level;
        public int MaxHP;
        public int MaxMP;
        public string Name = string.Empty;
        public int Order;
        public uint OwnerID;
        public byte type;
    }

    public class Player
    {
        public int Accuracy;
        public int Attack;
        public int AttackMagicPotency;
        public int Crit;
        public int Det;
        public int Dex;
        public int HealMagicPotency;
        public int Intel;
        public int JobID;
        public int Mnd;
        public int Pie;
        public int SkillSpeed;
        public int SpellSpeed;
        public int Str;
        public int Vit;
        public int WeaponDmg;
    }
}
