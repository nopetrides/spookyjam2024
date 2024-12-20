﻿using HelloMurder.Assets;
using Murder;
using Murder.Core;
using Murder.Diagnostics;
namespace HelloMurder.Services
{
    internal static class SaveServices
    {
        public static HelloMurderSaveData GetOrCreateSave()
        {
#if DEBUG
            if (Game.Instance.ActiveScene is not GameScene && Game.Data.TryGetActiveSaveData() is null)
            {
                GameLogger.Warning("Creating a save out of the game!");
            }
#endif

            if (Game.Data.TryGetActiveSaveData() is not HelloMurderSaveData save)
            {
                // Right now, we are creating a new save if one is already not here.
                save = (HelloMurderSaveData)Game.Data.CreateSave();
            }

            return save;
        }

        public static HelloMurderSaveData? TryGetSave() => Game.Data.TryGetActiveSaveData() as HelloMurderSaveData;

        public static void QuickSave() => Game.Data.QuickSave();
    }
}
