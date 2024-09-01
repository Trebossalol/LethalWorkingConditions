using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        /*[HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerControllerBPatch_Update_Postfix(ref float ___sprintMeter)
        {
            if (LWCConfig.PlayerControllerUnlimitedSprint.Value == true)
             {
                ___sprintMeter = 1f;
            }
        }*/
    }
}
