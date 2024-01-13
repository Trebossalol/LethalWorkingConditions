using System.Collections;
using UnityEngine;

namespace LethalWorkingConditions.Helpers
{
    // Needs to inherit from MonoBehaviour because "StartCoroutine" method is needed
    internal class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper instance;

        public CoroutineHelper() 
        {
            if (instance == null) instance = this;
        }

        public static void Sleep(float seconds)
        {
            if (instance != null) instance.StartCoroutine(SleepCoroutine(seconds));
            else LethalWorkingConditions.mls.LogError("CoroutineHelper instance is null. Make sure its constructed somwhere.");
        }

        private static IEnumerator SleepCoroutine(float durationSeconds)
        {
            yield return new WaitForSecondsRealtime(durationSeconds);
        }
    }
}
