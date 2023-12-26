using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalWorkingConditions.Classes;
using LethalWorkingConditions.Patches;

namespace LethalWorkingConditions
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class LethalWorkingConditions : BaseUnityPlugin
    {
        private const string modGUID = "Trebossa.Company.BetterWorkingConditions";
        private const string modName = "Lethal Working Conditions";
        private const string modVersion = "0.1.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static LethalWorkingConditions Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            // Reusable Logger instance
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Lethal Working Conditions loaded!");

            harmony.PatchAll(typeof(LethalWorkingConditions));

            // Loading Patches
            harmony.PatchAll(typeof(BridgeTriggerBPatch));
            harmony.PatchAll(typeof(LandmineBPatch));
            // harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(QuicksandTriggerBPatch));
            harmony.PatchAll(typeof(RoundManagerBPatch));
            harmony.PatchAll(typeof(SprayPaintItemBPatch));
            harmony.PatchAll(typeof(TimeOfDayBPatch));
            harmony.PatchAll(typeof(TurretBPatch));

            // Loading Classes
            harmony.PatchAll(typeof(FlickerPoweredLightsBClass));
        }    
    }
}
