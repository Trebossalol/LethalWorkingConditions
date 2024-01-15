using BepInEx.Logging;
using LethalWorkingConditions.Classes.MonsterEvent.Events;
using LethalWorkingConditions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Centipede, Bunker Spider, Hoarding bug, Flowerman, Crawler, Blob, Spring, Puffer, Jester, Nutcracker

namespace LethalWorkingConditions.Classes.MonsterEvent
{
    internal class MonsterEventManager
    {
        internal static LWCLogger logger = new LWCLogger("MonsterEventManager");

        internal static MonsterEvent activeEvent = null;

        internal static Random randomGenerator = new Random();

        internal static void IssueNotification(string title, string message)
        {
            if (activeEvent == null) return;

            HUDManager.Instance.DisplayTip(title, message);

            logger.LogInfo($"{title}: {message}");
        }

        private static MonsterEvent GetRandomEvent()
        {
            MonsterEvent monsterEvent;

            int rng = randomGenerator.Next(0, 100);

            if (rng >= 50)
            {
                monsterEvent = new SpiderEvent();
            }
            else
            {
                monsterEvent = new HoardingBugEvent();
            }

            return monsterEvent;
        }


        internal static void GenerateNewEvent()
        {
            activeEvent = GetRandomEvent();

            logger.LogInfo("GenerateNewEvent()");

            IssueNotification("Monster Event", activeEvent.eventName);
        }
    }
}
