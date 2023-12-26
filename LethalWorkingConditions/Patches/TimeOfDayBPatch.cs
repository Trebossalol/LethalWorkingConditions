using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayBPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        static void FasterDaytime(ref float ___globalTimeSpeedMultiplier)
        {
            ___globalTimeSpeedMultiplier = 0.5f;
        }
    }
}
