using HarmonyLib;
using LethalWorkingConditions.Classes.ChatCommand;
using LethalWorkingConditions.Classes.ChatCommand.Commands;

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
                return spawnCommand.ExecuteCommand();
            }

            // If text started with prefix but does not match a command, handle orgiginal logic
            return true;            
        }
    }
}
