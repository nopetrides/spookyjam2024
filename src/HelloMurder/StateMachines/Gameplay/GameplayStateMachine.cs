using Bang.StateMachines;
using HelloMurder.Services;
using Murder;

namespace HelloMurder.StateMachines.Gameplay
{
    public class GameplayStateMachine : StateMachine
    {
        public GameplayStateMachine()
        {
            State(Main);
        }

        private float _lastTransition = 0;

        private IEnumerator<Wait> Main()
        {
            Game.Sound.PlayEvent(LibraryServices.GetLibrary().GameAmbience, new Murder.Core.Sounds.PlayEventInfo() { Properties = Murder.Core.Sounds.SoundProperties.Persist | Murder.Core.Sounds.SoundProperties.StopOtherEventsInLayer });
            yield return null;
        }
    }
}
