﻿using Bang;
using Bang.Entities;
using Bang.StateMachines;
using HelloMurder.Core;
using Murder;
using Murder.Assets;
using Murder.Assets.Localization;
using Murder.Attributes;
using Murder.Core.Geometry;
using Murder.Core.Graphics;
using Murder.Core.Input;
using Murder.Services;
using HelloMurder.Services;
using Murder.Core.Sounds;
using HelloMurder.Core.Sounds.Fmod;
using HelloMurder.Core.Sounds;
using HelloMurder.Data;

namespace HelloMurder.StateMachines
{
    public class MainMenuStateMachine : StateMachine
    {
        private MenuInfo _menuInfo = new();

        private MenuInfo GetMainMenuOptions() =>
            new MenuInfo(new MenuOption[] { //new(LocalizedResources.Menu_Continue, selectable: Game.Data.CanLoadSaveData(0)), 
                new(LocalizedResources.Menu_NewGame), new(LocalizedResources.Menu_Options), new(LocalizedResources.Menu_Exit) });

        private MenuInfo GetOptionOptions() =>
            new(new MenuOption[] {
                new(Game.Preferences.SoundVolume == 1 ? LocalizedResources.Menu_SoundsOn : LocalizedResources.Menu_SoundsOff),
                new(Game.Preferences.MusicVolume == 1 ? LocalizedResources.Menu_MusicOn : LocalizedResources.Menu_MusicOff),
                new(LocalizedResources.Menu_CurrentLanguage),
                new(LocalizedResources.Menu_BackToMenu) });

        public MainMenuStateMachine()
        {
            State(Main);
        }

        protected override void OnStart()
        {
            Entity.SetCustomDraw(DrawMainMenu);

            _menuInfo.Select(Game.Data.CanLoadSaveData(0) ? 0 : 1);

            Game.Sound.PlayEvent(LibraryServices.GetLibrary().MainMenuMusic, new PlayEventInfo() { Properties = SoundProperties.Persist | SoundProperties.StopOtherEventsInLayer | SoundProperties.SkipIfAlreadyPlaying });
        }

        private IEnumerator<Wait> Main()
        {
            _menuInfo = GetMainMenuOptions();
            _menuInfo.Select(_menuInfo.NextAvailableOption(-1, 1));

            while (true)
            {
                int previousInput = _menuInfo.Selection;

                if (Game.Input.VerticalMenu(ref _menuInfo))
                { 
                    //ui input sounds
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiSelect, new PlayEventInfo());

                    switch (_menuInfo.Selection)
                    {
                        /*case 0: //  Continue Game
                            Guid? targetWorld = MurderSaveServices.LoadSaveAndFetchTargetWorld(0);
                            Game.Instance.QueueWorldTransition(targetWorld ?? LibraryServices.GetLibrary().GameplayWorld);
                            break;*/

                        case 0: //  New Game
                            Game.Data.DeleteAllSaves();
                            Game.Instance.QueueWorldTransition(LibraryServices.GetLibrary().GameplayWorld);
                            break;

                        case 1: // Options
                            yield return GoTo(Options);
                            break;

                        case 2: //  Exit
                            Game.Instance.QueueExitGame();
                            break;

                        default:
                            break;
                    }
                }

                if (previousInput != _menuInfo.Selection)
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiNavigate, new PlayEventInfo());
                }

                yield return Wait.NextFrame;
            }
        }
        
        private IEnumerator<Wait> Options()
        {
            _menuInfo = GetOptionOptions();
            _menuInfo.Select(_menuInfo.NextAvailableOption(-1, 1));

            while (true)
            {
                int previousInput = _menuInfo.Selection;

                if (Game.Input.VerticalMenu(ref _menuInfo))
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiSelect, new PlayEventInfo());

                    switch (_menuInfo.Selection)
                    {
                        case 0: // Toggle sound
                            float sound = Game.Preferences.ToggleSoundVolumeAndSave();

                            _menuInfo.Options[0] = sound == 1 ? new(LocalizedResources.Menu_SoundsOn) : new(LocalizedResources.Menu_SoundsOff);
                            break;

                        case 1: // Toggle music
                            float music = Game.Preferences.ToggleMusicVolumeAndSave();

                            _menuInfo.Options[1] = music == 1 ? new(LocalizedResources.Menu_MusicOn) : new(LocalizedResources.Menu_MusicOff);
                            break;

                        case 2: // Language
                            SwitchLanguage();

                            _menuInfo = GetOptionOptions();
                            _menuInfo.Select(2);

                            break;

                        case 3: // Go back
                            yield return GoTo(Main);
                            break;
                            
                        default:
                            break;
                    }
                }

                if (previousInput != _menuInfo.Selection)
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiNavigate, new PlayEventInfo());
                }

                yield return Wait.NextFrame;
            }
        }

        private void SwitchLanguage()
        {
            Game.Data.ChangeLanguage(Languages.Next(Game.Preferences.Language));
        }

        private void DrawMainMenu(RenderContext render)
        {
            Point cameraHalfSize = render.Camera.Size / 2f - new Point(0, _menuInfo.Length * 7);

            _ = RenderServices.DrawVerticalMenu(
                render.UiBatch, 
                cameraHalfSize, 
                new DrawMenuStyle() 
                { 
                    Color = Palette.Colors[7], 
                    Shadow = Palette.Colors[1],
                    SelectedColor = Palette.Colors[9]
                },
                _menuInfo);
        }
    }
}
