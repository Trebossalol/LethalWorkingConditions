using LethalLib;
using LethalLib.Modules;
using LethalWorkingConditions.Helpers;
using LethalWorkingConditions.MonoBehaviours;
using LethalWorkingConditions.Patches;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Netcode.Components;
using UnityEngine;
using static LethalWorkingConditions.AssetBundleStuff;

namespace LethalWorkingConditions
{
    internal class Content
    {
        private static LWCLogger logger = new LWCLogger("Content");

        public static AssetBundle MainAssetsBundle;
        private static readonly string mainAssetBundleName = "lethalworkingconditions";
        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

        public static List<CustomEnemy> customEnemies;
        public static List<CustomItem> customItems = new List<CustomItem>
        {
            /*CustomScrap.Add(
                "GamingPC",
                "GamingPC.asset",//"Assets/LethalWorkingConditions/Scrap/GamingPC/GamingPC.asset",
                Levels.LevelTypes.All,
                90
            )*/
        };


        // Loading logic
        private static void TryLoadAssets()
        {
            MainAssetsBundle = AssetBundle.LoadFromFile(
                Path.Combine(
                    Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location
                    ),
                    mainAssetBundleName
                )
             );

            if (MainAssetsBundle != null) logger.LogInfo("AssetBundle loaded");
            else logger.LogError($"Could not load AssetBundle from {mainAssetBundleName}");
        }

        private static void LoadPatches()
        {
            
            LethalWorkingConditions.harmony.PatchAll(typeof(RoundManagerBPatch));
            // HUDManagerBPatch depends on RoundManagerBPatch
            LethalWorkingConditions.harmony.PatchAll(typeof(HUDManagerBPatch));


            // Load other patches and game modifications
            LethalWorkingConditions.harmony.PatchAll(typeof(BridgeTriggerBPatch));
            LethalWorkingConditions.harmony.PatchAll(typeof(QuicksandTriggerBPatch));
            LethalWorkingConditions.harmony.PatchAll(typeof(SprayPaintItemBPatch));
            LethalWorkingConditions.harmony.PatchAll(typeof(TimeOfDayBPatch));

            /// Dev only: Unlimited sprint
            /// LethalWorkingConditions.harmony.PatchAll(typeof(PlayerControllerBPatch));

            logger.LogInfo("Patches loaded");
        }

        private static void RegisterCustomScrapItems()
        {

            if (MainAssetsBundle == null)
            {
                logger.LogError("Cannot register custom items because AssetBundle is empty");
                return;
            }

            foreach (var item in customItems)
            {
                if (!item.enabled) continue;

                var itemAsset = MainAssetsBundle.LoadAsset<Item>(item.itemPath);
                if (itemAsset.spawnPrefab.GetComponent<NetworkTransform>() == null && itemAsset.spawnPrefab.GetComponent<CustomNetworkTransform>() == null)
                {
                    var networkTransform = itemAsset.spawnPrefab.AddComponent<NetworkTransform>();
                    networkTransform.SlerpPosition = false;
                    networkTransform.Interpolate = false;
                    networkTransform.SyncPositionX = false;
                    networkTransform.SyncPositionY = false;
                    networkTransform.SyncPositionZ = false;
                    networkTransform.SyncScaleX = false;
                    networkTransform.SyncScaleY = false;
                    networkTransform.SyncScaleZ = false;
                    networkTransform.UseHalfFloatPrecision = true;
                }
                Prefabs.Add(item.name, itemAsset.spawnPrefab);
                LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(itemAsset.spawnPrefab);
                item.itemAction(itemAsset);


                if (item is CustomShopItem shopitem)
                {
                    var itemInfo = MainAssetsBundle.LoadAsset<TerminalNode>(shopitem.infoPath);
                    Plugin.logger.LogInfo($"Registering shop item {item.name} with price {shopitem.itemPrice}");
                    Items.RegisterShopItem(itemAsset, null, null, itemInfo, shopitem.itemPrice);
                }
                else if (item is CustomScrap scrap)
                {
                    Plugin.logger.LogInfo($"Registering scrap item {scrap.name} with price rarity:{scrap.rarity} and leveltype:{scrap.levelType}");
                    Items.RegisterScrap(itemAsset, scrap.rarity, scrap.levelType);
                }
            }

            logger.LogInfo("CustomItems loaded");
        }


        // Main Loading method
        public static void Load()
        {
            // Begin Patching
            LethalWorkingConditions.harmony.PatchAll(typeof(LethalWorkingConditions));

            LoadPatches();

            TryLoadAssets();

            RegisterCustomScrapItems();

            // loop through prefabs
            foreach (var prefabSet in Prefabs)
            {
                var prefab = prefabSet.Value;

                // get prefab name
                var prefabName = prefabSet.Key;

                // get all AudioSources
                var audioSources = prefab.GetComponentsInChildren<AudioSource>();

                // if has any AudioSources
                if (audioSources.Length > 0)
                {
                    // loop through AudioSources, adjust volume by multiplier
                    foreach (var audioSource in audioSources)
                    {
                        audioSource.volume *= (100 / 100);
                    }
                }
            }

            logger.LogInfo("Mod content loaded");
        }
    }
}
