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

        internal static void IssueNotification(string title, string message)
        {
            if (activeEvent == null) return;

            HUDManager.Instance.DisplayTip(title, message);

            logger.LogInfo($"{title}: {message}");
        }

        internal static void GenerateNewEvent()
        {
            // Calculate chances
            activeEvent = new SpiderEvent();

            logger.LogInfo("GenerateNewEvent()");

            IssueNotification("Monster Event", activeEvent.eventName);
        }
    }
}
