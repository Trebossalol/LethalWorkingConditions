using HarmonyLib;
using LethalWorkingConditions.Patches;
using System;
using System.Linq;

namespace LethalWorkingConditions.Classes.ChatCommand.Commands
{
    internal class SpawnCommand : ChatCommand
    {
        // Command parameters
        private string spawnEnemyName = "";
        private int spawnEnemyAmount = 1;

        private string creaturesAvailableString = RoundManagerBPatch.currentLevel.Enemies.Select(e => e.enemyType.enemyName).ToArray().Join();
        private bool enemyFound = false;
        private string enemyName;

        public SpawnCommand(ref HUDManager hudManager) : base("Spawn", ref hudManager)
        {
        }

        protected override string GetCommandHelp()
        {
            return $"Creatures available: {creaturesAvailableString}";
        }

        protected override string GetFullCommandSyntax()
        {
            return $"{base.GetFullCommandSyntax()} <creature_name> [amount=1]";
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
            if (parameters.Length > 1)
            {
                Int32.TryParse(parameters[1], out spawnEnemyAmount);
            }

            return true;
        }

        protected override void Execute()
        {
            foreach (var enemy in RoundManagerBPatch.currentLevel.Enemies)
            {
                // If an enemy was found, skip loop to prevent spawning multiple different creatures
                if (enemyFound) continue;

                if (enemy.enemyType.enemyName.ToLower().Contains(spawnEnemyName))
                {

                    enemyFound = true;
                    enemyName = enemy.enemyType.enemyName;

                    try
                    {
                        EnemySpawner.SpawnEnemy(enemy, spawnEnemyAmount);
                        IssueNotification($"Spawned {spawnEnemyAmount} {enemyName}");
                    }
                    catch
                    {
                        IssueNotification("Could not spawn enemies");
                    }
                }
            }

            // If no enemy was found by search
            if (!enemyFound) IssueCommandHelp();
        }
    }
}
