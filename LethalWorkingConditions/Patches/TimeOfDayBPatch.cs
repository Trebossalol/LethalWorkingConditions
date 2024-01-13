using HarmonyLib;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayBPatch
    {
        // Needs fix, because when the time is too slow, no enemies can spawn
        /*[HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void TimeOfDayBPatch_Awake_Postfix(ref float ___globalTimeSpeedMultiplier)
        {
            ___globalTimeSpeedMultiplier = 0.75f;
        }*/
    }
}
