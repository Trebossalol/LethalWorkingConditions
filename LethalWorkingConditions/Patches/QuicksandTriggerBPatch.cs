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
        static void MoreHinderance(ref float ___movementHinderance)
        {
            ___movementHinderance = 6f;
        }

        [HarmonyPatch("OnTriggerStay")]
        [HarmonyPrefix]
        static void MoreSinkingSpeed(ref float ___sinkingSpeedMultiplier)
        {
            ___sinkingSpeedMultiplier = 0.8f;
        }
    }
}
