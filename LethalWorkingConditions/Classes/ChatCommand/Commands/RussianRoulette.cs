using System;

namespace LethalWorkingConditions.Classes.ChatCommand.Commands
{
    internal enum RussianRouletteState
    {
        NOT_PLAYING,
        PLAYING
    };

    internal class RussianRoulette : ChatCommand
    {
        private Random randomGenerator;
        public RussianRouletteState state;

        public RussianRoulette(ref HUDManager hudManager) : base("rr", ref hudManager)
        {
            state = 0;
            randomGenerator = new Random();
        }

        protected override bool CanBeCalled()
        {
            return true;
        }

        protected override bool ParseParameters()
        {
            return true;
        }

        protected override void Execute()
        {
            switch(state)
            {
                case RussianRouletteState.NOT_PLAYING:
                    HandleNotPlaying();
                    break;

                case RussianRouletteState.PLAYING:
                    HandlePlaying();
                    break;
            }
        }

        private void HandleNotPlaying()
        {

        }

        private void HandlePlaying()
        {

        }
    }
}
