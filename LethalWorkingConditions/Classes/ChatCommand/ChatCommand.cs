using LethalWorkingConditions.Helpers;
using System.Linq;

namespace LethalWorkingConditions.Classes.ChatCommand
{
    public enum CommandStatus
    {
        NOT_SET,
        PREQUISITES_NOT_MET,
        PARAMS_INCOMPLETE,
        OK
    };

    internal abstract class ChatCommand
    {
        private LWCLogger logger;

        public static string CommandPrefix = LWCConfig.TerminalCommandPrefix.Value ?? LWCConfig.TerminalCommandPrefixDefault;

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
                return $"Command: {CommandPrefix}{commandName}";
            }
        }


        // Optional methods which can be overwritten
        protected virtual string GetFullCommandSyntax()
        {
            return $"{CommandPrefix}{commandName}";
        }

        protected virtual void OnInterception() { }


        // Required methods which must be declared
        protected abstract bool CanBeCalled();

        protected abstract bool ParseParameters();

        protected abstract void Execute();


        public ChatCommand(string commandname, ref HUDManager hudManager)
        {
            this.hudManager = hudManager;

            commandName = commandname;
            logger = new LWCLogger($"{commandName}");

            text = hudManager.chatTextField.text;
            parameters = text.Split(' ').Skip(1).ToArray();
        }

        protected void IssueNotification(string message, bool isWarning = false)
        {
            HUDManager.Instance.DisplayTip(noticeTitle, message, isWarning);
            logger.LogInfo($"{noticeTitle}: {message}");
        }


        // Help 
        protected void IssueCommandSyntax()
        {
            IssueNotification($"Wrong Syntax: {GetFullCommandSyntax()}");
        }

        

        // Terminal bind
        public CommandStatus ExecuteCommand()
        {
            bool canBeCalled = CanBeCalled();
            if (!canBeCalled) return CommandStatus.PREQUISITES_NOT_MET;

            bool paramsValid = ParseParameters();
            if (!paramsValid)
            {
                IssueCommandSyntax();
                return CommandStatus.PARAMS_INCOMPLETE;
            }

            Execute();

            return CommandStatus.OK;
        }
    }
}
