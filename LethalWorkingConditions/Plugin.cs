using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LethalWorkingConditions
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class LethalWorkingConditions : BaseUnityPlugin
    {
        public const string modGUID = "Trebossa.LethalWorkingConditions";
        public const string modName = "Lethal Working Conditions";
        public const string modVersion = "0.1.3";

        public static readonly Harmony harmony = new Harmony(modGUID);
        
        public static LethalWorkingConditions Instance;

        internal static ManualLogSource mls;

        void Awake()
        {
            if (Instance == null) Instance = this;

            // Assign Logger Instance
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            Content.Load();

            mls.LogInfo("LWC loaded");
        }
    }
}
