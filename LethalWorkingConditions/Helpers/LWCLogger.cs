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
        public static ManualLogSource pluginMls = null;

        public readonly string source;

        public LWCLogger(string source)
        {
            this.source = source;
        }

        public static void Init()
        {
            pluginMls = BepInEx.Logging.Logger.CreateLogSource(LethalWorkingConditions.modGUID);
        }

        public void LogInfo(string message)
        {
            if (pluginMls == null) return;
            pluginMls.LogInfo($"[{source}] {message}");
        }

        public void LogWarning(string message) 
        {
            if (pluginMls == null) return;
            pluginMls.LogWarning($"[{source}] {message}");
        }

        public void LogError(string message)
        {
            if (pluginMls == null) return;
            pluginMls.LogError($"[{source}] {message}");
        }
    }
}
