using BepInEx;
using HarmonyLib;
using LethalWorkingConditions.Helpers;
using RuntimeNetcodeRPCValidator;
using System.Reflection;
using UnityEngine;

namespace LethalWorkingConditions
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class LethalWorkingConditions : BaseUnityPlugin
    {
        public const string modGUID = "Trebossa.LethalWorkingConditions";
        public const string modName = "Lethal Working Conditions";
        public const string modVersion = "0.1.4";

        public static readonly Harmony harmony = new Harmony(modGUID);
        
        public static LethalWorkingConditions Instance;

        private static LWCLogger logger;

        public static new LWCConfig Config { get; internal set; }

        void Awake()
        {
            if (Instance == null) Instance = this;

            Config = new LWCConfig(base.Config);

            LWCLogger.Init();
            logger = new LWCLogger("LWC");

            Content.Load();

            logger.LogInfo("Done loading config");

            AllowNetworkPrefabs();
        }

        void AllowNetworkPrefabs()
        {
            // Required by https://github.com/EvaisaDev/UnityNetcodePatcher maybe?
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
