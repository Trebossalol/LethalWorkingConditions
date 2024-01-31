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

            // Not done yet
            // MonsterEventManager.GenerateNewEvent();
        }


        [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_AdvanceHourAndSpawnNewBatchOfEnemies_Prefix(ref EnemyVent[] ___allEnemyVents, ref SelectableLevel ___currentLevel)
        {
            currentLevel = ___currentLevel;
            currentLevelVents = ___allEnemyVents;

            //MonsterEventManager.activeEvent?.Bind_AdvanceHourAndSpawnNewBatchOfEnemies();
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        static void RoundManagerBPatch_LoadNewLevel_Prefix(ref SelectableLevel newLevel)
        {
            currentRound = RoundManager.Instance;

            //MonsterEventManager.activeEvent?.Bind_On_LoadNewLevel();
        }
    }
}
