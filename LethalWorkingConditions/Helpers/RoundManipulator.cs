using System;
using System.Collections;
using LethalWorkingConditions.Patches;
using UnityEngine;

namespace LethalWorkingConditions.Helpers
{
	public class RoundManipulator
	{
		private static LWCLogger logger = new LWCLogger("RoundManipulator");

		public static IEnumerator FlickerPoweredLights(int amountMultiplier = 1, float offsetMultiplier = 1f)
		{
			logger.LogInfo("Flickering powered lights");

			var flashlights = ObjectFinder.FindObjectsOfType<FlashlightItem>();

			foreach (var flashlight in flashlights)
			{
				flashlight.flashlightAudio.PlayOneShot(flashlight.flashlightFlicker);
				WalkieTalkie.TransmitOneShotAudio(flashlight.flashlightAudio, flashlight.flashlightFlicker, 0.8f);

				if (!flashlight.playerHeldBy.isInsideFactory) continue;

				flashlight.flashlightInterferenceLevel = 2;
			}

			var lights = RoundManager.Instance.allPoweredLightsAnimators;

			if (lights.Count <= 0) yield break;

			int loopCount = 0;
			int b = 4 * amountMultiplier;
			float delayMultiplier = 1f;

			while (b > 0)
			{
				for (int j = loopCount; j < lights.Count / b; j++)
				{
					loopCount++;
					lights[j].SetTrigger("Flicker");
				}
				yield return new WaitForSeconds(0.02f * delayMultiplier);

                delayMultiplier *= offsetMultiplier;

				b--;
			}

			yield return new WaitForSeconds(0.3f);

            var flashlights2 = ObjectFinder.FindObjectsOfType<FlashlightItem>();

            foreach (var flashlight in flashlights2)
			{
				flashlight.flashlightInterferenceLevel = 0;
			}

			FlashlightItem.globalFlashlightInterferenceLevel = 0;
        }
	
		public static void EnrageNearbyTurrets(Transform source, float range = 20f)
		{
			var turrets = ObjectFinder.FindObjectsInRadius<Turret>(source, range);

			foreach (var turret in turrets)
			{
				turret.EnterBerserkModeServerRpc(-1);
			}

			logger.LogInfo($"Enraged {turrets.Count} turrets");
		}
	}
}

