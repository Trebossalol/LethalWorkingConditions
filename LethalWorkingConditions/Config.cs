using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions
{
    public class LWCConfig
    {
        public static readonly string TerminalCommandPrefixDefault = "/";
        public static readonly bool MonsterEventsEnabledDefault = true;

        public static ConfigEntry<string> TerminalCommandPrefix;
        public static ConfigEntry<bool> MonsterEventsEnabled;
    
        public LWCConfig(ConfigFile cfg)
        {
            TerminalCommandPrefix = cfg.Bind(
                "LethalWorkingConditions",
                "TerminalCommandPrefix",
                "/",
                "Prefix for chat commands"
            );
        }
    }
}
