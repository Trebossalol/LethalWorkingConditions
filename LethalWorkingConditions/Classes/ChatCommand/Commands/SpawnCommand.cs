﻿using BepInEx.Logging;
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
        private string targetEnemyNameParam = "";
        private int targetEnemyAmountParam = 1;

        private string creaturesAvailableString
        {
            get
            {
                return
                    $"{string.Join("|", RoundManagerBPatch.currentLevel.Enemies.Select(e => e.enemyType.enemyName).ToArray())}|{string.Join("|", RoundManagerBPatch.currentLevel.OutsideEnemies.Select(e => e.enemyType.enemyName).ToArray())}";
            }
        }
        
        private bool targetEnemyFound = false;
        private string targetEnemyName;

        public SpawnCommand(ref HUDManager hudManager) : base("Spawn", ref hudManager)
        {
        }

        protected override string GetFullCommandSyntax()
        {
            return $"{base.GetFullCommandSyntax()} <{creaturesAvailableString}> [amount=1]";
        }

        protected override bool CanBeCalled()
        {
            // check if host
            if (!RoundManagerBPatch.isHost)
            {
                IssueNotification("Only the host is allowed to use this comand");
                return false;
            };

            // check if game has started
            try
            {
                
                var outsideEnemies = EnemySpawner.EnemiesOutside;
                var insideEnemies = EnemySpawner.EnemiesInside;

                if (outsideEnemies.Count <= 0 || insideEnemies.Count <= 0) throw new Exception();
            }
            catch
            {
                IssueNotification("You need to start the game before spawning enemies");
                return false;
            }

            return true;
        }

        protected override bool ParseParameters()
        {
            // 1 param is required
            if (parameters.Length < 1) return false;

            // Parse first parameter (target enemy name)
            targetEnemyNameParam = parameters[0].ToLower();

            // If there are more optional parameters, parse them
            if (parameters.Length > 1) Int32.TryParse(parameters[1], out targetEnemyAmountParam);

            return true;
        }

        protected override void Execute()
        {
            var outsideEnemies = RoundManagerBPatch.currentLevel.OutsideEnemies;
            var insideEnemies = RoundManagerBPatch.currentLevel.Enemies;

            // Enemies
            HandleSpawnEnemies(outsideEnemies, false);
            HandleSpawnEnemies(insideEnemies, true);
            
            // If no enemy was found by search
            if (!targetEnemyFound) IssueCommandSyntax();
        }

        private void HandleSpawnEnemies(List<SpawnableEnemyWithRarity> list, bool inside)
        {
            foreach (var enemy in list)
            {
                // If an enemy was found, skip loop to prevent spawning multiple different creatures
                if (targetEnemyFound) continue;

                if (enemy.enemyType.enemyName.ToLower().Contains(targetEnemyNameParam))
                {

                    targetEnemyFound = true;
                    targetEnemyName = enemy.enemyType.enemyName;

                    try
                    {
                        EnemySpawner.SpawnEnemy(enemy, targetEnemyAmountParam, inside);
                        IssueNotification($"Spawned {targetEnemyAmountParam} {targetEnemyName}");
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
