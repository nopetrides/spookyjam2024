namespace HelloMurder.Assets
{
    public class HelloMurderSaveData : Murder.Assets.SaveData
    {
        /// <summary>
        /// Not correctly working in Save Data currently. See <see cref="Data.BombsAwayPreferences"/>
        /// </summary>
        public int HighScore = 0;

        public int LastAttemptScore = 0;

        public HelloMurderSaveData(int saveSlot, float saveVersion) : base(saveSlot, saveVersion) { }
    }
}
