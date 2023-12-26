using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(Turret))]
    internal class TurretBPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void MakeTurretRotationFaster(ref float ___rotationSpeed)
        {
            ___rotationSpeed = 40f;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void MakeRotationRangeFaster(ref float ___rotationRange) {
            ___rotationRange = 90f;
        }

    }
}
