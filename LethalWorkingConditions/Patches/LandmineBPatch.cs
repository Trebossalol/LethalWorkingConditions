using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandmineBPatch
    {
        [HarmonyPatch("Detonate")]
        [HarmonyPrefix]
        static void DetonateWithBiggerExplosion(Landmine __instance) 
        {
            Vector3 position = __instance.transform.position + Vector3.up;
            float killRange = 5.7f;
            float damageRange = 10f;

            Landmine.SpawnExplosion(position, true, killRange, damageRange);
            return;
        }  
    }
}
