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
            bool chatDisabled = LWCConfig.TerminalCommandDisableChat.Value;

            string text = __instance.chatTextField.text;

            if (!text.ToLower().StartsWith(ChatCommand.CommandPrefix)) {
                // This is no command

                // If chat is disabled, do not continue original logic
                if (chatDisabled) return false;

                // If chat is not disabled, continue original logic
                return true;
            }

            // Check if a command "spawn" is called
            if (text.ToLower().StartsWith($"{ChatCommand.CommandPrefix}spawn"))
            {
                // Spawn command
                SpawnCommand spawnCommand = new(ref __instance);
                bool rv = spawnCommand.ExecuteCommand();

                CleanupGUI(ref __instance);

                return rv;
            }

            if (chatDisabled) return false;

            // If text started with prefix but does not match a command, handle orgiginal logic
            return true;       
        }

        static private void CleanupGUI(ref HUDManager __instance)
        {
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            // bro idk
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
