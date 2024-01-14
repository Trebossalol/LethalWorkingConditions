using HarmonyLib;
using LethalWorkingConditions.Classes.MonsterEvent;

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

            MonsterEventManager.GenerateNewEvent();
        }


        [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_AdvanceHourAndSpawnNewBatchOfEnemies_Prefix(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;
        }

        [HarmonyPatch("PlotOutEnemiesForNextHour")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_PlotOutEnemiesForNextHour()
        {
            MonsterEventManager.activeEvent?.On_PlotOutEnemiesForNextHour();
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_Update_Prefix()
        {
            MonsterEventManager.activeEvent?.On_Update();
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_LoadNewLevel_Prefix(ref SelectableLevel newLevel)
        {
            currentRound = RoundManager.Instance;

            MonsterEventManager.activeEvent?.On_LoadNewLevel();
        }


        /*[HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void AddScrapValueMultiplier(ref float ___scrapValueMultiplier)
        {
            ___scrapValueMultiplier = 1.1f;
        }*/
    }
}
