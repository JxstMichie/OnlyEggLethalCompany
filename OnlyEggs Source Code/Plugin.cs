using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EggsplosiveDay
{
  [BepInPlugin("EggsplosiveDay", "EggsplosiveDay", "1.0.1")]

 
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        private Harmony _harmony = new Harmony("EggsplosiveDay");
        public static new ManualLogSource Logger;

        public static ConfigEntry<int> Chance;
        public static ConfigEntry<string> Item;
        public static ConfigEntry<int> Rarity;
        public static ConfigEntry<bool> Debug;
        public static ConfigEntry<string> SpawnType;

    private void Awake()
    {
      Plugin.Logger = BepInEx.Logging.Logger.CreateLogSource("EggsplosiveDay");
      Plugin.Logger.LogInfo("\r\n-\"-.\r\n      .'=^=^='.\r\n     /=^=^=^=^=\\\r\n    :^= Eggs  =^;\r\n    |^   Day   ^|\r\n    :^=^=^=^=^=^:\r\n     \\=^=^=^=^=/\r\n      `.=^=^=.'\r\n        `~~~` \r\nPlugin EggsplosiveDay is loading.");
      Plugin.Logger.LogInfo("Loaded EggsplosiveDay. Patching.");

        Plugin.Instance = this;
        Plugin.Chance = this.Config.Bind<int>("General", "Chance", 20, "Default: 20\n(1 in 20) chance every day");
        Plugin.Item = this.Config.Bind<string>("General", "Item", "Easter egg", "Default: Easter egg");
        Plugin.Debug = this.Config.Bind<bool>("DebugMode", "Debug", true, "Default: True");
        Plugin.SpawnType = this.Config.Bind<string>("General", "SpawnType", "zero", "Default:zero\nAccepted Values are zero,dynamic,default\nzero sets all other item rarities to zero\ndefault leaves other item rarities as it is\ndynamic sets the other item rarities 100 - ChanceConfig = Other Item rarity");
        Plugin.Rarity = this.Config.Bind<int>("General", "Rarity", 100, "Default: 100\nOnly value 0-100 are accepted");
        this._harmony.PatchAll(typeof(Plugin));

            
        }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    [HarmonyPrefix]

    private static bool Roll(ref SelectableLevel newLevel)
    {
        int roll = Random.Range(1, Plugin.Chance.Value);
        if(Plugin.Debug.Value)
            {
                Plugin.Logger.LogInfo($"Rolled a {roll}");
                Plugin.Logger.LogInfo($"Config: Chance:{Plugin.Chance.Value}\nItem:{Plugin.Item.Value},Rarity:{Plugin.Rarity.Value},SpawnType:{Plugin.SpawnType.Value}");
            }

        if (roll == 1)
            {
                if (Plugin.Debug.Value)
                    {
                        Plugin.Logger.LogInfo("Roll is 1. Activating egg-only spawn modifications.");
                    }

                if (Plugin.Rarity.Value < 100 || Plugin.Rarity.Value < 0)
                    {
                        Plugin.Rarity.Value = 100;
                    }

                if (Plugin.SpawnType.Value != "zero" && Plugin.SpawnType.Value != "dynamic" && Plugin.SpawnType.Value != "default")
                    {
                    Plugin.SpawnType.Value = "default";
                    }

                /*
                *   Item Rarity is the percentage for an item to spawn
                *   The foreach loop goes through every Item in the level and sets it to zero (Never Spawns)
                *   
                */

                foreach (SpawnableItemWithRarity spawnableItemWithRarity in newLevel.spawnableScrap)
                {
                    if (Plugin.SpawnType.Value == "zero")
                    {
                        spawnableItemWithRarity.rarity = 0;
                    }
                    else if (Plugin.SpawnType.Value == "dynamic")
                    {
                        spawnableItemWithRarity.rarity = (100 - Plugin.Rarity.Value);
                    }
                    

                    if (spawnableItemWithRarity.spawnableItem.itemName == Plugin.Item.Value)// Check if the item's name matches the specified item exists
                        {
                            spawnableItemWithRarity.rarity = Plugin.Rarity.Value;
                        }
                    else
                        {
                            foreach (Item item in StartOfRound.Instance.allItemsList.itemsList)
                                {
                                    if (item.itemName == Plugin.Item.Value)
                                        {
                                            newLevel.spawnableScrap.Add(new SpawnableItemWithRarity
                                                {

                                                    spawnableItem = item,
                                                    rarity = Plugin.Rarity.Value
                                                }
                                            ); break;
                                        }
                                }
                        }
                }
            }
            else if(Plugin.Debug.Value)
                {
                    Plugin.Logger.LogInfo("Roll did not meet condition (1). Skipping the modification.");
                }
            return true; // Ensures the game continues loading the level regardless.
            }
    }
}
