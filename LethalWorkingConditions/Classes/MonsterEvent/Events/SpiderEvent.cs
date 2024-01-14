using LethalLib.Modules;
using LethalWorkingConditions.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            EnemySpawner.SpawnEnemy(spiderEnemy, initialSpawnAmount, true);
        }

        protected override void On_PlotOutEnemiesForNextHour()
        {
            int amount = 2 * multiplier;
            multiplier *= 2;

            EnemySpawner.SpawnEnemy(spiderEnemy, amount, true);

            IssueNotification($"Spawned {amount} of {spiderEnemy.enemyType.enemyName}");
        }
    }
}
