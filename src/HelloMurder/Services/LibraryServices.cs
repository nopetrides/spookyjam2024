using Murder;
using Murder.Assets;
using HelloMurder.Assets;

namespace HelloMurder.Services
{
    public static class LibraryServices
    {
        public static LibraryAsset GetLibrary()
        {
            return Game.Data.GetAsset<LibraryAsset>(HelloMurderGame.Profile.Library);
        }

        internal static PrefabAsset GetPauseMenuPrefab()
        {
            return Game.Data.GetAsset<PrefabAsset>(GetLibrary().PauseMenuPrefab);
        }
    }
}
