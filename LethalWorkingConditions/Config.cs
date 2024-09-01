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
        public readonly string Name = "LethalChat";

        // PlayerController
        public static readonly bool PlayerControllerUnlimitedSprintDefault = false;
        public static ConfigEntry<bool> PlayerControllerUnlimitedSprint;


        // Chat command stuff
        public static readonly string TerminalCommandPrefixDefault = "----";
        public static ConfigEntry<string> TerminalCommandPrefix;

        public static readonly bool TerminalCommandDisableChatDefault = false;
        public static ConfigEntry<bool> TerminalCommandDisableChat;

        public static readonly bool AllowSpawnCommandIfNotHostDefault = false;
        public static ConfigEntry<bool> AllowSpawnCommandIfNotHost;

        // Monster event stuff
        public static readonly bool MonsterEventsEnabledDefault = false;
        public static ConfigEntry<bool> MonsterEventsEnabled;
    
        public LWCConfig(ConfigFile cfg)
        {
            AllowSpawnCommandIfNotHost = cfg.Bind(
                Name,
                "AllowSpawnCommandIfNotHost",
                AllowSpawnCommandIfNotHostDefault,
                "If enabled, you can use the spawn command even when you are not the host"
            );
       }
    }
}
