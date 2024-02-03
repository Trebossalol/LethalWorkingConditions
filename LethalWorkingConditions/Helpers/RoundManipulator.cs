using System;
using System.Collections;
using UnityEngine;

namespace LethalWorkingConditions.Helpers
{
	public class RoundManipulator
	{
		private static FlashlightItem[] GetFlashLightItems()
		{
            FlashlightItem[] flashlights = UnityEngine.Object.FindObjectsOfType<FlashlightItem>();
			return flashlights;
        }

		public static IEnumerator FlickerPoweredLights(float offsetMultiplier = 1f)
		{
			var flashlights = GetFlashLightItems();

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
			int b = 4;
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

            var flashlights2 = GetFlashLightItems();

			foreach (var flashlight in flashlights2)
			{
				flashlight.flashlightInterferenceLevel = 0;
			}

			FlashlightItem.globalFlashlightInterferenceLevel = 0;
        }
	}
}

