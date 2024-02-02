﻿using System;

namespace LethalWorkingConditions.Classes.MonsterEvent.Events
{
    internal class SpiderEvent : MonsterEvent
    {

        private readonly Random randomGenerator = new Random();
        private readonly int initialSpawnAmount;

        private int multiplier;

        protected SpawnableEnemyWithRarity spiderEnemy = EnemySpawner.FindEnemy(EnemySpawner.EnemiesInside, "spider");

        public SpiderEvent() : base("Spider cocoon")
        {
            initialSpawnAmount = randomGenerator.Next(2, 5);
            multiplier = 1;
        }

        protected override void On_LoadNewLevel()
        {
            EnemySpawner.SpawnEnemy(spiderEnemy, initialSpawnAmount);
        }

        protected override void On_AdvanceHourAndSpawnNewBatchOfEnemies()
        {
            int amount = 2 * multiplier;
            multiplier *= 2;

            EnemySpawner.SpawnEnemy(spiderEnemy, amount);

            IssueNotification($"Spawned {amount} of {spiderEnemy.enemyType.enemyName}");
        }

    }
}
