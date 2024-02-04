using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using LethalWorkingConditions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalWorkingConditions.Behaviours
{
    [HarmonyPatch(typeof(CrawlerAI))]
    internal class CrawlerAIBPatch
    {
        public static LWCLogger logger = new LWCLogger("CrawlerAIBPatch");
        
        [HarmonyPatch("MakeScreech")]
        [HarmonyPostfix]
        static void CrawlerAIBPatch_MakeScreech_Postfix(ref LethalGigaAI __instance)
        {
            string name = __instance.enemyType.enemyName.ToLower();

            logger.LogInfo($"MakeScreech_Postfix: {name}");
            
            if (name.Contains("giga"))
            {
                RoundManipulator.EnrageNearbyTurrets(__instance.transform, 30f);
                __instance.StartCoroutine(RoundManipulator.FlickerPoweredLights(1, 2));
            }
        }

        [HarmonyPatch("HitEnemy")]
        [HarmonyPostfix]
        static void CrawlerAIBPatch_HitEnemy_Postfix(LethalGigaAI __instance, int force, ref PlayerControllerB playerWhoHit, bool playHitSFX)
        {
            string name = __instance.enemyType.enemyName.ToLower();

            logger.LogInfo($"HitEnemy_Postfix: {name}");

            if (name.Contains("giga"))
            {
                for (int i = 0; i < 10; i++)
                {
                    playerWhoHit.DropBlood(default, true, true);
                }
            }
        }
    }
}
