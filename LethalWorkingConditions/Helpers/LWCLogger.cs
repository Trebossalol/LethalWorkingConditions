using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Helpers
{
    internal class LWCLogger
    {
        public static ManualLogSource mls = null;

        public readonly string source;

        public LWCLogger(string source)
        {
            this.source = source;
        }

        public static void Init()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource(LethalWorkingConditions.modGUID);
        }

        public void LogInfo(string message)
        {
            if (mls == null) return;
            mls.LogInfo($"[{source}] {message}");
        }

        public void LogWarning(string message) 
        {
            if (mls == null) return;
            mls.LogWarning($"[{source}] {message}");
        }

        public void LogError(string message)
        {
            if (mls == null) return;
            mls.LogError($"[{source}] {message}");
        }
    }
}
