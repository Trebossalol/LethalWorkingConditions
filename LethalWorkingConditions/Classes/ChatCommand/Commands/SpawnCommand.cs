using HarmonyLib;
using LethalWorkingConditions.Patches;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LethalWorkingConditions.Classes.ChatCommand.Commands
{
    internal class SpawnCommand : ChatCommand
    {
        // Command parameters
        private string spawnEnemyName = "";
        private int spawnEnemyAmount = 1;

        private string creaturesAvailableString = 
            $"{string.Join("|", RoundManagerBPatch.currentLevel.Enemies.Select(e => e.enemyType.enemyName).ToArray())}|{string.Join("|", RoundManagerBPatch.currentLevel.OutsideEnemies.Select(e => e.enemyType.enemyName).ToArray())}";
        private bool enemyFound = false;
        private string enemyName;

        public SpawnCommand(ref HUDManager hudManager) : base("Spawn", ref hudManager)
        {
        }

        protected override string GetFullCommandSyntax()
        {
            return $"{base.GetFullCommandSyntax()} <{creaturesAvailableString}> [amount=1]";
        }

        protected override bool CanBeCalled()
        {
            if (RoundManagerBPatch.isHost) return true;
            
            noticeBody = "Unable to send commands since you are not the host";
            HUDManager.Instance.DisplayTip(noticeTitle, noticeBody);
            
            return false;
        }

        protected override bool ParseParameters()
        {
            if (parameters.Length == 0) return false;

            // Parse first parameter (target enemy name)
            spawnEnemyName = parameters[0].ToLower();

            // If there are more optional parameters, parse them
            if (parameters.Length > 1) Int32.TryParse(parameters[1], out spawnEnemyAmount);

            return true;
        }

        protected override void Execute()
        {
            var outsideEnemies = RoundManagerBPatch.currentLevel.OutsideEnemies;
            var insideEnemies = RoundManagerBPatch.currentLevel.Enemies;

            // Enemies
            SpawnEnemies(outsideEnemies, false);
            SpawnEnemies(insideEnemies, true);
            
            // If no enemy was found by search
            if (!enemyFound) IssueCommandSyntax();
        }

        private void SpawnEnemies(List<SpawnableEnemyWithRarity> list, bool inside)
        {
            foreach (var enemy in list)
            {
                // If an enemy was found, skip loop to prevent spawning multiple different creatures
                if (enemyFound) continue;

                if (enemy.enemyType.enemyName.ToLower().Contains(spawnEnemyName))
                {

                    enemyFound = true;
                    enemyName = enemy.enemyType.enemyName;

                    try
                    {
                        EnemySpawner.SpawnEnemy(enemy, spawnEnemyAmount, inside);
                        IssueNotification($"Spawned {spawnEnemyAmount} {enemyName}");
                    }
                    catch
                    {
                        IssueNotification("Could not spawn enemies");
                    }
                }
            }
        }
    }
}
