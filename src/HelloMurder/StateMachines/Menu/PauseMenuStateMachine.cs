using Bang.Entities;
using Bang.StateMachines;
using Bang;
using Murder.Core.Graphics;
using Murder.Services;
using Murder.Core.Input;
using Murder;
using Murder.Assets;
using Murder.Attributes;
using Newtonsoft.Json;
using Murder.Core.Geometry;
using System.Diagnostics;
using HelloMurder.Core;
using HelloMurder.Core.Sounds;
using HelloMurder.Services;
using Murder.Core.Sounds;

namespace HelloMurder.StateMachines.Menu
{
    public class PauseMenuStateMachine : StateMachine
    {

        private MenuInfo GetPauseMenuOptions() =>
            new MenuInfo(options: new MenuOption[] { new("Resume"), new ("Give Up"), new("Quit") });

        private MenuInfo _menuInfo = new();

        public PauseMenuStateMachine()
        {
            State(StartPause);
        }

        private IEnumerator<Wait> StartPause()
        {
            World.Pause();

            Entity.SetCustomDraw(DrawPauseMenu);
            yield return GoTo(Main);
        }

        private IEnumerator<Wait> Main()
        {
            _menuInfo = GetPauseMenuOptions();
            _menuInfo.Select(_menuInfo.NextAvailableOption(-1, 1));

            while (true)
            {
                int previousInput = _menuInfo.Selection;

                if (Game.Input.VerticalMenu(ref _menuInfo))
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiSelect, new PlayEventInfo());
                    switch (_menuInfo.Selection)
                    {
                        case 0: //  Resume
                            World.Resume();
                            Entity.Destroy();

                            break;

                        case 1: // Give Up
                            Game.Instance.QueueWorldTransition(LibraryServices.GetLibrary().GameOverWorld);
                            break;
                        case 2: //  Quit
                            Game.Instance.QueueWorldTransition(LibraryServices.GetLibrary().MainMenuWorld);
                            break;

                        default:
                            break;
                    }
                }
                else if (Game.Input.PressedAndConsume(InputButtons.Pause))
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiSelect, new PlayEventInfo());
                    World.Resume();
                    Entity.Destroy();

                    break;
                }
                if (previousInput != _menuInfo.Selection)
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiNavigate, new PlayEventInfo());
                }
                yield return Wait.NextFrame;
            }
        }

        private void DrawPauseMenu(RenderContext render)
        {
            Debug.Assert(_menuInfo.Options is not null);

            // BG fade
            RenderServices.DrawRectangle(render.GameUiBatch, new(0, 0, render.Camera.Width+5, render.Camera.Height+5), Palette.Colors[3] * 0.6f, .11f);

            Point cameraHalfSize = render.Camera.Size / 2f - new Point(0, _menuInfo.Length * 7);

            RenderServices.DrawVerticalMenu(
                render.UiBatch, 
                cameraHalfSize, 
                new DrawMenuStyle()
                {
                    Color = Palette.Colors[7],
                    Shadow = Palette.Colors[1],
                    SelectedColor = Palette.Colors[9],
                    Origin = new(0.5f, 0.5f),
                    ExtraVerticalSpace = 5,
                },
                _menuInfo);
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
        }
    }
}
