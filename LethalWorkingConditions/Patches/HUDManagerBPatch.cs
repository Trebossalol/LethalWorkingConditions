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
        private static bool chatDisabled = LWCConfig.TerminalCommandDisableChat.Value;

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        static bool HUDManager_SubmitChat_performed_Prefix(ref HUDManager __instance)
        {
            string text = __instance.chatTextField.text;

            if (!text.ToLower().StartsWith(ChatCommand.CommandPrefix) && !chatDisabled) return true;

            CommandStatus status = HandleCommandLogic(text, ref __instance);

            CleanupGUI(ref __instance);

            if (status == CommandStatus.NOT_SET && !chatDisabled) return true;

            return false;
        }

        static private CommandStatus HandleCommandLogic(string text, ref HUDManager __instance)
        {
            CommandStatus status = CommandStatus.NOT_SET;

            if (text.ToLower().StartsWith($"{ChatCommand.CommandPrefix}spawn"))
            {
                SpawnCommand spawnCommand = new SpawnCommand(ref __instance);
                status = spawnCommand.ExecuteCommand();
            }

            return status;
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
