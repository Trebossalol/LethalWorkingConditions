using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalWorkingConditions.Helpers;

namespace LethalWorkingConditions
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class LethalWorkingConditions : BaseUnityPlugin
    {
        public const string modGUID = "Trebossa.LethalWorkingConditions";
        public const string modName = "Lethal Working Conditions";
        public const string modVersion = "0.1.5";

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
        }
    }
}
