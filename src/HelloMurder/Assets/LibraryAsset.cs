using Murder.Assets;
using Murder.Assets.Graphics;
using Murder.Attributes;
using Murder.Core.Sounds;
using Murder.Utilities;
using System.Numerics;

namespace HelloMurder.Assets
{
    public class LibraryAsset : GameAsset
    {
        #region Editor Settings

        public override string EditorFolder => "#\uf02dLibraries";

        public override char Icon => '\uf02d';

        public override Vector4 EditorColor => "#FA5276".ToVector4Color();

        #endregion

        #region Worlds // Like scenes in Unity

        [GameAssetId<WorldAsset>]
        public Guid MainMenuWorld = Guid.Empty;

        [GameAssetId(typeof(WorldAsset))]
        public Guid GameplayWorld = Guid.Empty;

        [GameAssetId(typeof(WorldAsset))]
        public Guid GameOverWorld = Guid.Empty;

        #endregion

        #region Prefabs

        [GameAssetId<PrefabAsset>]
        public Guid PauseMenuPrefab = Guid.Empty;

        #endregion

        #region Sprites

        [GameAssetId<SpriteAsset>]
        public Guid SplashScreen = Guid.Empty;

        [GameAssetId<SpriteAsset>]
        public Guid GameOverScreen = Guid.Empty;

        [GameAssetId<SpriteAsset>]
        public Guid MurderLogo = Guid.Empty;

        [GameAssetId<SpriteAsset>]
        public Guid FmodLogo = Guid.Empty;

        #endregion

        #region Sounds

        // UI
        public SoundEventId UiNavigate;
        public SoundEventId UiSelect;
        public SoundEventId UiCodeAppear;
        public SoundEventId UiCodeEnter;
        public SoundEventId UiCodeFailed;
        public SoundEventId UiCodeComplete;
        public SoundEventId UiCodeCancel;

        // Gameplay
        public SoundEventId GameAmbience;
        public SoundEventId BreakingGlass;
        public SoundEventId MonsterGroan;
        public SoundEventId MonsterScream;
        public SoundEventId MosterFootsteps;
        public SoundEventId MonsterDripping;
        public SoundEventId MonsterHissing;

        public SoundEventId PlayerFootsteps;
        public SoundEventId PlayerReadPaper;
        public SoundEventId PlayerHeartbeat;
        public SoundEventId PlayerHeavyBreathing;
        public SoundEventId PlayerPickup;


        // Music
        public SoundEventId MainMenuMusic;
        public SoundEventId EndScreen;

        #endregion
    }
}
