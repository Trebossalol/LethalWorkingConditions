using HarmonyLib;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(BridgeTrigger))]
    internal class BridgeTriggerBPatch
    {
        [HarmonyPatch("OnEnable")]
        [HarmonyPrefix]
        static void BridgeTriggerBPatch_OnEnable_Prefix(ref float ___bridgeDurability)
        {
            ___bridgeDurability = 0.6f;
        }
    }
}
