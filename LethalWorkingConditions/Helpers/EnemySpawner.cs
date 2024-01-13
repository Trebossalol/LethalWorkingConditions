using LethalWorkingConditions.Patches;

namespace LethalWorkingConditions.Classes
{
    internal class EnemySpawner
    {
        public static void SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount)
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
