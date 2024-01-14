using LethalWorkingConditions.Classes.MonsterEvent.Events;
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
        internal static MonsterEvent activeEvent = null;

        public MonsterEventManager() 
        {
               
        }

        internal static void GenerateNewEvent()
        {
            // Calculate chances
            activeEvent = new SpiderEvent();
        }
    }
}
