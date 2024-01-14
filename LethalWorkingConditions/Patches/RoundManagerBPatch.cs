﻿using HarmonyLib;
using LethalWorkingConditions.Classes.MonsterEvent;
using LethalWorkingConditions.Classes.MonsterEvent.Events;

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
            ___mapSizeMultiplier = 1.5f;

            isHost = RoundManager.Instance.NetworkManager.IsHost;

            LethalWorkingConditions.mls.LogInfo("IsHost: " + isHost);
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

            if (MonsterEventManager.activeEvent == null) MonsterEventManager.GenerateNewEvent();
        }


        /*[HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void AddScrapValueMultiplier(ref float ___scrapValueMultiplier)
        {
            ___scrapValueMultiplier = 1.1f;
        }*/
    }
}
