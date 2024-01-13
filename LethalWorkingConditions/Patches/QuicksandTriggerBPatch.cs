using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(QuicksandTrigger))]
    internal class QuicksandTriggerBPatch
    {
        [HarmonyPatch("OnTriggerStay")]
        [HarmonyPrefix]
        static void QuicksandTriggerBPatch_OnTriggerStay_Prefix(ref float ___movementHinderance, ref float ___sinkingSpeedMultiplier)
        {
            ___movementHinderance = 3f;
            ___sinkingSpeedMultiplier = 0.5f;
        }
    }
}
