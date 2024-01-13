using System;

namespace LethalWorkingConditions.Classes.LightAnomaly
{
    internal class BaseLightAnomaly
    {
        protected private Random randomGenerator = new Random();

        protected private RoundManager roundManager;

        public BaseLightAnomaly(ref RoundManager manager)
        {
            LethalWorkingConditions.mls.LogInfo("BaseLightAnomaly() - Constructed");

            roundManager = manager;
        }
        protected private void FlickerLights()
        {
            roundManager.FlickerLights(true, true);
        }
    }
}
