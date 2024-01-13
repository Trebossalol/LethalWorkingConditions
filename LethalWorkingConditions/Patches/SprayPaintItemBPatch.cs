using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(SprayPaintItem))]
    internal class SprayPaintItemBPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void IncraseCanTankVolume(ref float ___sprayCanTank)
        {
            ___sprayCanTank = 4f;
        }
    }
}
