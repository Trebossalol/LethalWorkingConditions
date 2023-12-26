using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(BridgeTrigger))]
    internal class BridgeTriggerBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void MakeBridgeMoreInstable(ref float ___bridgeDurability)
        {
            ___bridgeDurability = 0.4f;
        }
    }
}
