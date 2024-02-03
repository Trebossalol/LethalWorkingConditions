using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static int bloodCount;
        public static int DropBlood_BloodPoolsAmount = 4;
        public static float DropBlood_ScaleAmount = 4f;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void PlayerControllerBPatch_Update_Postfix(ref float ___sprintMeter)
        {
            if (LWCConfig.PlayerControllerUnlimitedSprint.Value == true)
            {
                ___sprintMeter = 1f;
            }
        }

        [HarmonyPatch("DropBlood")]
        [HarmonyPostfix]
        public static void PlayerControllerBPatch_DropBloodPostfix(PlayerControllerB __instance, Vector3 direction = default(Vector3), bool leaveBlood = true, bool leaveFootprint = false)
        {
            bloodCount++;

            if (bloodCount < DropBlood_BloodPoolsAmount)
            {
                __instance.DropBlood(direction, leaveBlood, leaveFootprint);
            }
            else
            {
                bloodCount = 0;
            }
        }

        [HarmonyPatch("RandomizeBloodRotationAndScale")]
        [HarmonyPostfix]
        public static void RandomizeBloodScale(ref Transform blood, PlayerControllerB __instance)
        {
            Transform obj = blood;
            obj.localScale *= DropBlood_ScaleAmount;
            blood.position += new Vector3((float)Random.Range(-1, 1) * DropBlood_ScaleAmount, 0.55f, (float)Random.Range(-1, 1) * DropBlood_ScaleAmount);
        }
    }
}
