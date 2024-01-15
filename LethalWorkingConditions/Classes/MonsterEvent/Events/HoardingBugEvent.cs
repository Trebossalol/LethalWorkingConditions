using System;

namespace LethalWorkingConditions.Classes.MonsterEvent.Events
{
    internal class HoardingBugEvent : MonsterEvent
    {

        private readonly Random randomGenerator = new Random();
        private readonly int initialSpawnAmount;

        private int multiplier;

        protected SpawnableEnemyWithRarity hoardingBugEnemy = EnemySpawner.FindEnemy(EnemySpawner.EnemiesInside, "hoarding");

        public HoardingBugEvent() : base("Smaug")
        {
            initialSpawnAmount = randomGenerator.Next(4, 10);
            multiplier = 1;
        }

        protected override void On_LoadNewLevel()
        {
            EnemySpawner.SpawnEnemy(hoardingBugEnemy, initialSpawnAmount, true);
        }

        protected override void On_AdvanceHourAndSpawnNewBatchOfEnemies()
        {
            int amount = 2 * multiplier;
            multiplier *= 3;

            EnemySpawner.SpawnEnemy(hoardingBugEnemy, amount, true);

            IssueNotification($"Spawned {amount} of {hoardingBugEnemy.enemyType.enemyName}");
        }

    }
}
