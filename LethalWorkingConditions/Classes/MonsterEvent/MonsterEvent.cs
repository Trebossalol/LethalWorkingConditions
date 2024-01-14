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
        public string eventName;
        private string noticeTitle
        {
            get {
                return $"Monster Event: {eventName}"; 
            }
        }

        public MonsterEvent(string eventName) 
        {
            this.eventName = eventName;

            this.IssueEventOccuredNotifaction();
        }

        private void IssueEventOccuredNotifaction()
        {
            HUDManager.Instance.DisplayTip("Monster Event", $"{eventName}");
        }

        protected void IssueNotification(string text)
        {
            HUDManager.Instance.DisplayTip(noticeTitle, text);
            LethalWorkingConditions.mls.LogInfo($"{noticeTitle}: {text}");
        }

        protected virtual void On_LoadNewLevel()
        {

        }

        protected virtual void On_Update() 
        {
        
        }

        protected virtual void On_PlotOutEnemiesForNextHour()
        {

        }
    }
}
