using LethalWorkingConditions.Helpers;
using System.Threading;

namespace LethalWorkingConditions.Classes.LightAnomaly
{
    internal class UncommonLightAnomaly : BaseLightAnomaly
    {
        private static LWCLogger logger = new LWCLogger("UncommonLightAnomaly");

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
                logger.LogWarning($"UncommonLightAnomaly::Start() - Could not start because Instance is not initalized");
                return;
            }

            logger.LogInfo($"UncommonLightAnomaly::Start() - {maxExecutions}/{intervalInSeconds}");
            timer = new Timer(Tick, null, 0, intervalInSeconds * 1000);
        }

        public void Dispose()
        {
            logger.LogInfo("UncommonLightAnomaly::Dispose()");
            timer.Dispose();
        }
        
        private void Tick(object sender)
        {
            logger.LogInfo("UncommonLightAnomaly::Tick()");

            if (executionIndex >= maxExecutions)
            {
                logger.LogInfo("UncommonLightAnomaly::Tick() - Cleanup");
                timer.Dispose();
                return;
            }
            
            executionIndex++;

            //base.FlickerLights();
            roundManager.FlickerLights(true, true);
        }
    }
}
