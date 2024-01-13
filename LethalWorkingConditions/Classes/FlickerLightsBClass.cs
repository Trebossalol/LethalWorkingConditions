using HarmonyLib;
using LethalWorkingConditions.Classes.LightAnomaly;
using System.Threading;

namespace LethalWorkingConditions.Classes
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class FlickerLightsBClass
    {
        private static LightAnomalyEventManager lightFlickerEventManager;
        private static bool initaliedYet = false;

        [HarmonyPatch("SetPowerOffAtStart")]
        [HarmonyPrefix]
        static void LoadLightAnomalyEventManager(ref RoundManager __instance)
        {
            if (initaliedYet) return;

            lightFlickerEventManager = new LightAnomalyEventManager(ref __instance);
            lightFlickerEventManager.Awake();

            initaliedYet = true;
        }
    }


    // Spawns every X seconds a light anomaly
    internal class LightAnomalyEventManager
    {
        private readonly int lightAnomalyEventIntervalMS = 60000;

        private RoundManager roundManager;
        private System.Random randomGenerator = new System.Random();
        private Timer lightFlickTimer;

        private readonly UncommonLightAnomaly uncommonLightAnomaly;
        
        public LightAnomalyEventManager(ref RoundManager instance)
        {
            roundManager = instance;
            uncommonLightAnomaly = new UncommonLightAnomaly(ref instance);
        }

        // Initalize timer instance
        public void Awake()
        {
            LethalWorkingConditions.mls.LogInfo("LightAnomalyEventManager::Awake()");
            lightFlickTimer = new Timer(Tick, null, 0, lightAnomalyEventIntervalMS);
        }

        public void Unload()
        {
            uncommonLightAnomaly.Dispose();
            lightFlickTimer.Dispose();
        }

        // Method runs every tick of timer instance
        private void Tick(object state)
        {
            int randomInt = randomGenerator.Next(0, 100);
            
            LethalWorkingConditions.mls.LogInfo($"LightAnomalyEventManager::Tick() - {randomInt}");

            roundManager.FlickerLights(true, true);

            uncommonLightAnomaly.InitalizeVariables();
            uncommonLightAnomaly.Start();
        }
    }
}
