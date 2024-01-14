using System.Linq;

namespace LethalWorkingConditions.Classes.ChatCommand
{
    internal abstract class ChatCommand
    {
        public static string CommandPrefix = "/";

        protected HUDManager hudManager;
        protected readonly string text;

        protected string[] parameters;

        // Command name, eg. "spawn"
        protected string commandName = "N/A";

        // Title, which is shown in Notifications
        protected string noticeTitle
        {
            get
            {
                return $"Command: {ChatCommand.CommandPrefix}{commandName}";
            }
        }

        protected virtual string GetFullCommandSyntax()
        {
            return $"{ChatCommand.CommandPrefix}{commandName}";
        }

        protected abstract bool CanBeCalled();

        protected abstract bool ParseParameters();

        protected abstract void Execute();

        public ChatCommand(string commandname, ref HUDManager hudManager) 
        {
            this.commandName = commandname;
            this.hudManager = hudManager;

            text = hudManager.chatTextField.text;
            parameters = text.Split(' ').Skip(1).ToArray();
        }

        protected void IssueNotification(string message, bool isWarning = false)
        {
            HUDManager.Instance.DisplayTip(noticeTitle, message, isWarning);
            LethalWorkingConditions.mls.LogInfo($"{noticeTitle}: {message}");
        }

        protected void IssueCommandSyntax()
        {
            IssueNotification($"Wrong Syntax: {GetFullCommandSyntax()}");
        }

        public bool ExecuteCommand()
        {
            // Check command prequisites (eg. check if user is host)
            bool canBeCalled = CanBeCalled();
            // If prequisites are not met, continue with the original code
            if (!canBeCalled) return true;

            // Try to parse params
            bool paramsValid = ParseParameters();
            // If the required params are found or could not be parsed
            if (!paramsValid)
            {
                // Display a notification with the correct syntax
                IssueCommandSyntax();
                // Do not continue with the original code
                return false;
            }

            // All requirements are checked, execute the command
            Execute();

            // Do not continue with the original code
            return false;
        }
    }
}
