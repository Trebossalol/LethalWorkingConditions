using HarmonyLib;
using System.Threading;

namespace LethalWorkingConditions.Classes
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class FlickerPoweredLightsBClass
    {
        private static FlickerPoweredLightsTimerManager manager;

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        static void AddRandomLightFlickers(ref RoundManager __instance)
        {
            manager = new FlickerPoweredLightsTimerManager(ref __instance);

            manager.Start();
        }

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        static void UnloadTimer(RoundManager __instance)
        {
            manager.Clear();
        }
    }

    internal class FlickerPoweredLightsTimerManager
    {
        private RoundManager roundManager;
        private Timer timer;

        public FlickerPoweredLightsTimerManager(ref RoundManager instance)
        {
            roundManager = instance;
        }

        public void Start()
        {
            timer = new Timer(Tick, null, 0, 30000);
        }

        public void Clear()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void Tick(object state)
        {
            roundManager.FlickerLights(true, true);
        }
    }
}
