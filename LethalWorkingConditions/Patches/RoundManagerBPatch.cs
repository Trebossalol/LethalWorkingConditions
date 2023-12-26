using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerBPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void AddScrapValueMultiplier(ref float ___scrapValueMultiplier)
        {
            ___scrapValueMultiplier = 1.2f;
        }

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void BiggerMapSizeMultiplier(ref float ___mapSizeMultiplier)
        {
            ___mapSizeMultiplier = 1.5f;
        }
    }
}
