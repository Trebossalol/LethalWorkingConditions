using GameNetcodeStuff;
using HarmonyLib;
using LethalWorkingConditions.Classes.ChatCommand;
using LethalWorkingConditions.Classes.ChatCommand.Commands;
using System;
using System.Net.NetworkInformation;
using UnityEngine.EventSystems;
using static UnityEngine.TouchScreenKeyboard;

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

            if (!text.ToLower().StartsWith(ChatCommand.CommandPrefix) && !chatDisabled)
            {
                return true;
            }

            CommandStatus status = HandleCommandLogic(text, ref __instance);

            CleanupGUI(ref __instance);

            if (chatDisabled) return false;

            switch (status)
            {
                // Either no command was found or the requirements are not met -> Continue with networking logic
                case CommandStatus.NOT_SET:
                case CommandStatus.PREQUISITES_NOT_MET:
                    return true;

                // EIther params are not complete or command was executed -> Do not continue original logic
                case CommandStatus.PARAMS_INCOMPLETE:
                case CommandStatus.OK:
                default:
                    return false;
            }
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
