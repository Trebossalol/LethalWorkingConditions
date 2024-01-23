using LethalWorkingConditions.Helpers;
using LethalWorkingConditions.Patches;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LethalWorkingConditions.Classes
{
    internal class EnemySpawner
    {
        private static LWCLogger logger = new LWCLogger("EnemySpawner");

        // May needs fix because needs to be reinitalized once level changes
        public static List<SpawnableEnemyWithRarity> EnemiesInside
        {
            get { return RoundManagerBPatch.currentLevel.Enemies; }
        }

        public static List<SpawnableEnemyWithRarity> EnemiesOutside
        {
            get { return RoundManagerBPatch.currentLevel.OutsideEnemies;  }
        }

        // has some weird error
        public static bool SpawnEnemy(SpawnableEnemyWithRarity enemy, int amount, bool inside)
        {
            // doesn't work regardless if not host but just in case
            if (!RoundManagerBPatch.isHost)
            {
                logger.LogInfo("Could not spawn enemies because user is not host");
                return false;
            }

            if (inside)
            {
                try
                {
                    Vector3 spawnPosition = RoundManagerBPatch.currentRound.allEnemyVents[UnityEngine.Random.Range(0, RoundManagerBPatch.currentRound.allEnemyVents.Length)]
                                                .floorNode.position;
                    int enemyNumber = RoundManagerBPatch.currentLevel.Enemies.IndexOf(enemy);

                    for (int i = 0; i < amount; i++)
                    {

                        float yRot = RoundManagerBPatch.currentRound.allEnemyVents[i].floorNode.eulerAngles.y;

                        RoundManagerBPatch
                            .currentRound
                            .SpawnEnemyOnServer(
                                spawnPosition, 
                                yRot,
                                RoundManagerBPatch.currentLevel.Enemies.IndexOf(enemy)
                            );
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to spawn enemies: " + ex.ToString());
                    return false;
                }
            } 
            else
            {
               try
               {
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject obj = UnityEngine.Object.Instantiate(
                            RoundManagerBPatch.currentLevel
                                .OutsideEnemies[RoundManagerBPatch.currentLevel.OutsideEnemies.IndexOf(enemy)]
                                .enemyType.enemyPrefab,
                            GameObject.FindGameObjectsWithTag("OutsideAINode")
                                [UnityEngine.Random.Range(0, GameObject.FindGameObjectsWithTag("OutsideAINode").Length - 1)].transform.position,
                            Quaternion.Euler(Vector3.zero));

                        obj.gameObject.GetComponentInChildren<NetworkObject>()
                            .Spawn(destroyWithScene: true);
                    }
               } catch (Exception ex)
               {
                    logger.LogError("Failed to spawn enemies: " + ex.ToString());
                    return false;
                }
            }

            return true;
        }
    
        public static SpawnableEnemyWithRarity FindEnemy(List<SpawnableEnemyWithRarity> list, string search)
        {
            SpawnableEnemyWithRarity enemy = list.Find(e => e.enemyType.enemyName.ToLower().Contains(search.ToLower()));
            return enemy;
        }
    }
}
