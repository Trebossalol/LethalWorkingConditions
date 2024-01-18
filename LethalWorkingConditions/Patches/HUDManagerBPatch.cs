using GameNetcodeStuff;
using HarmonyLib;
using LethalWorkingConditions.Classes.ChatCommand;
using LethalWorkingConditions.Classes.ChatCommand.Commands;
using UnityEngine.EventSystems;

namespace LethalWorkingConditions.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerBPatch
    {

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        static bool HUDManager_SubmitChat_performed_Prefix(ref HUDManager __instance)
        {
            string text = __instance.chatTextField.text;

            // Check if text starts with command prefix, if not continue with original code
            if (!text.ToLower().StartsWith(ChatCommand.CommandPrefix)) return true;

            // Check if a command is called
            if (text.ToLower().StartsWith($"{ChatCommand.CommandPrefix}spawn"))
            {
                // Spawn command
                SpawnCommand spawnCommand = new SpawnCommand(ref __instance);
                bool rv = spawnCommand.ExecuteCommand();

                CleanupCommand(ref __instance);

                return rv;
            }

            // If text started with prefix but does not match a command, handle orgiginal logic
            return true;       
        }

        static private void CleanupCommand(ref HUDManager __instance)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            localPlayer.isTypingChat = false;

            // Reset chat input
            __instance.chatTextField.text = "";

            // Unfocus chat input
            EventSystem.current.SetSelectedGameObject(null);

            // Starts fade-out of chat input
            __instance.PingHUDElement(__instance.Chat);

            // Hide typing indicator
            __instance.typingIndicator.enabled = false;
        }
    }
}
