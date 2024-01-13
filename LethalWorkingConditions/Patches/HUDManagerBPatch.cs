using HarmonyLib;
using System.Linq;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerBPatch
    {

        private static string CommandPrefix = "/";

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        static void HUDManager_SubmitChat_performed_Prefix(ref HUDManager __instance)
        {
            string text = __instance.chatTextField.text;

            LethalWorkingConditions.mls.LogInfo($"Text received {text}");

            if (!text.ToLower().StartsWith(CommandPrefix)) return;

            string noticeTitle = "";
            string noticeBody = "";

            if (!RoundManagerBPatch.isHost)
            {
                noticeTitle = "Command";
                noticeBody = "Unable to send commands since you are not the host";
                HUDManager.Instance.DisplayTip(noticeTitle, noticeBody);
                return;
            }

            if (!text.ToLower().StartsWith(CommandPrefix + "spawn")) return;

            noticeTitle = "Spawned Enemeies";
            
            string[] splittedText = text.Split(' ');
            string[] parameters = splittedText.Skip(1).ToArray();

            LethalWorkingConditions.mls.LogInfo($"Command 'spawn' with params: {parameters.Join()}");

            LethalWorkingConditions.mls.LogInfo($"Creatures available: {RoundManagerBPatch.currentLevel.Enemies.Select(e => e.enemyType.enemyName).ToArray().Join()}");

            foreach (var enemy in RoundManagerBPatch.currentLevel.Enemies)
            {
                if (enemy.enemyType.enemyName.ToLower().Contains(parameters[0].ToLower()))
                {
                    try
                    {
                        string enemyName = enemy.enemyType.enemyName;

                        SpawnEnemy(enemy, 1);

                        noticeBody = "Spawned " + enemyName;

                        LethalWorkingConditions.mls.LogInfo(noticeBody);
                    }
                    catch
                    {
                        LethalWorkingConditions.mls.LogInfo("Could not spawn enemy");
                    }
                }
            }

        }

        private static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount)
        {
            // doesn't work regardless if not host but just in case
            if (!RoundManagerBPatch.isHost) return;

            try
            {
                for (int i = 0; i < amount; i++)
                {
                    RoundManagerBPatch
                        .currentRound
                        .SpawnEnemyOnServer(
                            RoundManagerBPatch.currentRound.allEnemyVents[UnityEngine.Random.Range(0, RoundManagerBPatch.currentRound.allEnemyVents.Length)]
                                .floorNode.position, 
                            RoundManagerBPatch.currentRound.allEnemyVents[i].floorNode.eulerAngles.y, 
                            RoundManagerBPatch.currentLevel.Enemies.IndexOf(enemy)
                        );
                }
            }
            catch
            {
                LethalWorkingConditions.mls.LogInfo("Failed to spawn enemies, check your command.");
            }
        }
    }
}
