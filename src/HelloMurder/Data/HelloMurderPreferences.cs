using Bang;
using Murder;
using Murder.Save;

namespace HelloMurder.Data
{
    public class HelloMurderPreferences : GamePreferences
    {
        public HelloMurderPreferences() : base() { }

        [Serialize]
        protected int _highScore = 0;

        public override void OnPreferencesChangedImpl()
        {
            Game.Sound.SetVolume(id: HelloMurderGame.Profile.MusicBus, _musicVolume);
            Game.Sound.SetVolume(id: HelloMurderGame.Profile.SoundBus, _soundVolume);
        }

        public int HighScore => _highScore;

        /// <summary>
        /// Preferences is currently acting like game save. Game save is not retaining data between sessions
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int SetHighScore(int value)
        {
            _highScore = value;

            OnPreferencesChanged();
            return _highScore;
        }
    }
}
