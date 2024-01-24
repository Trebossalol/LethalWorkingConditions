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
using static LethalLib.Modules.Enemies;
using static LethalLib.Modules.Levels;

namespace LethalWorkingConditions
{
    internal class Content
    {
        private static LWCLogger logger = new LWCLogger("Content");

        public static AssetBundle MainAssetsBundle;
        private static readonly string mainAssetBundleName = "lethalworkingconditions";
        public static Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();

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
            LethalWorkingConditions.harmony.PatchAll(typeof(SprayPaintItemBPatch));
            LethalWorkingConditions.harmony.PatchAll(typeof(TimeOfDayBPatch));

            /// Dev only: Unlimited sprint
            /// LethalWorkingConditions.harmony.PatchAll(typeof(PlayerControllerBPatch));

            logger.LogInfo("Patches loaded");
        }

        private static void LoadEnemies()
        {
            LoadEnemy("LethalGiga", 100, LevelTypes.All, SpawnType.Default);
        }

        private static void LoadEnemy(string name, int rarity, LevelTypes levelType, SpawnType spawnType)
        {
            logger.LogInfo($"Loading enemy {name}");

            EnemyType enemy = MainAssetsBundle.LoadAsset<EnemyType>(name);
            var terminalNode = MainAssetsBundle.LoadAsset<TerminalNode>($"{name}TN");
            var terminalKeyword = MainAssetsBundle.LoadAsset<TerminalKeyword>($"{name}TK");

            NetworkPrefabs.RegisterNetworkPrefab(enemy.enemyPrefab);

            RegisterEnemy(enemy, rarity, levelType, spawnType, terminalNode, terminalKeyword);

            logger.LogInfo($"Loaded {name}");
        }

        // Main Loading method
        public static void Load()
        {
            // Begin Patching
            LethalWorkingConditions.harmony.PatchAll(typeof(LethalWorkingConditions));

            LoadPatches();

            TryLoadAssets();

            LoadEnemies();

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
