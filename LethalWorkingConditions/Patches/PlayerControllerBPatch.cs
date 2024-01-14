using GameNetcodeStuff;
using HarmonyLib;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        /*[HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerControllerBPatch_Update_Postfix(ref float ___sprintMeter)
        {
            ___sprintMeter = 1f;
        }*/
    }
}
