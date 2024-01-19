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
        public readonly string Name = "LethalWorkingConditions";

        // Chat command stuff
        public static readonly string TerminalCommandPrefixDefault = "/";
        public static ConfigEntry<string> TerminalCommandPrefix;

        public static readonly bool TerminalCommandDisableChatDefault = false;
        public static ConfigEntry<bool> TerminalCommandDisableChat;


        // Monster event stuff
        public static readonly bool MonsterEventsEnabledDefault = true;
        public static ConfigEntry<bool> MonsterEventsEnabled;
    
        public LWCConfig(ConfigFile cfg)
        {
            TerminalCommandPrefix = cfg.Bind(
                Name,
                "TerminalCommandPrefix",
                TerminalCommandPrefixDefault,
                "Prefix for chat commands"
            );

            TerminalCommandDisableChat = cfg.Bind(
                Name,
                "TerminalCommandDisableChat",
                TerminalCommandDisableChatDefault,
                "If enabled, your chat messages will not be sent to other clients"
            );

            MonsterEventsEnabled = cfg.Bind(
                Name,
                "MonsterEventsEnabled",
                MonsterEventsEnabledDefault,
                "If enabled, monster events can occure"
            );
        }
    }
}
