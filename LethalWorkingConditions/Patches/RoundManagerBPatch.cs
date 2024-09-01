using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerBPatch
    {
        internal static bool isHost = false;
        internal static RoundManager currentRound;
        internal static SelectableLevel currentLevel;
        internal static EnemyVent[] currentLevelVents;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_Start_Prefix(ref float ___mapSizeMultiplier)
        {
            // Make map bigger
            ___mapSizeMultiplier = 2f;

            isHost = RoundManager.Instance.NetworkManager.IsHost;
        }

        [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_AdvanceHourAndSpawnNewBatchOfEnemies_Prefix(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_LoadNewLevel_Prefix(ref SelectableLevel newLevel)
        {
            currentRound = RoundManager.Instance;
        }
    }
}
