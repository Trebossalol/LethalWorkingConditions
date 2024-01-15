using LethalWorkingConditions.Helpers;
using LethalWorkingConditions.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Classes.MonsterEvent
{
    internal abstract class MonsterEvent
    {
        private LWCLogger logger;

        public readonly string eventName;
        private string noticeTitle
        {
            get {
                return $"Monster Event: {eventName}"; 
            }
        }

        protected abstract void On_LoadNewLevel();

        protected abstract void On_PlotOutEnemiesForNextHour();

        public MonsterEvent(string eventName) 
        {
            this.eventName = eventName;

            logger = new LWCLogger($"MonsterEvent::{eventName}");
        }

        protected void IssueNotification(string text)
        {
            HUDManager.Instance.DisplayTip(noticeTitle, text);
            logger.LogInfo($"{noticeTitle}: {text}");
        }


        public void Bind_On_LoadNewLevel()
        {
            logger.LogInfo("On_LoadNewLevel()");
            On_LoadNewLevel();
        }

        public void Bind_On_PlotOutEnemiesForNextHour()
        {
            logger.LogInfo("On_PlotOutEnemiesForNextHour()");
            On_PlotOutEnemiesForNextHour();
        }
    }
}
