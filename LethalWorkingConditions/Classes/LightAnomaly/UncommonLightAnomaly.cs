using System.Threading;

namespace LethalWorkingConditions.Classes.LightAnomaly
{
    internal class UncommonLightAnomaly : BaseLightAnomaly
    {
        private bool initalized = false;
        private int executionIndex = 0;
        private int maxExecutions;
        private int intervalInSeconds;

        private Timer timer;

        public UncommonLightAnomaly(ref RoundManager manager) : base(ref manager) 
        {
            roundManager = manager;
        }

        public void InitalizeVariables()
        {
            maxExecutions = randomGenerator.Next(5, 6);
            intervalInSeconds = randomGenerator.Next(5, 6);

            initalized = true;
        }
       
        public void Start()
        {
            if (!initalized)
            {
                LethalWorkingConditions.mls.LogWarning($"UncommonLightAnomaly::Start() - Could not start because Instance is not initalized");
                return;
            }

            LethalWorkingConditions.mls.LogInfo($"UncommonLightAnomaly::Start() - {maxExecutions}/{intervalInSeconds}");
            timer = new Timer(Tick, null, 0, intervalInSeconds * 1000);
        }

        public void Dispose()
        {
            LethalWorkingConditions.mls.LogInfo("UncommonLightAnomaly::Dispose()");
            timer.Dispose();
        }
        
        private void Tick(object sender)
        {
            LethalWorkingConditions.mls.LogInfo("UncommonLightAnomaly::Tick()");

            if (executionIndex >= maxExecutions)
            {
                LethalWorkingConditions.mls.LogInfo("UncommonLightAnomaly::Tick() - Cleanup");
                timer.Dispose();
                return;
            }
            
            executionIndex++;

            //base.FlickerLights();
            roundManager.FlickerLights(true, true);
        }
    }
}
