using Bang.StateMachines;
using Murder.Assets;
using Murder.Attributes;
using Murder.Core.Input;
using Murder.Services;
using Newtonsoft.Json;
using Bang.Entities;
using Murder.Core.Graphics;
using Murder;
using HelloMurder.Core;
using System.Diagnostics;
using Murder.Core.Geometry;
using HelloMurder.Services;
using System.Numerics;
using HelloMurder.Assets;
using HelloMurder.Core.Sounds;
using Murder.Utilities;
using Murder.Core;
using HelloMurder.Data;

namespace HelloMurder.StateMachines.Menu
{
    public class GameOverMenuStateMachine : StateMachine
    {
        [JsonProperty]
        private readonly float _paperDrawDelay = 5f;
        [JsonProperty]
        private readonly float _paperRestartDelay = 15f;

        private MenuInfo GetGameOverOptions() =>
            new MenuInfo(options: new MenuOption[] { new("Restart"), new("Quit") });

        private MenuInfo _menuInfo = new();

        public GameOverMenuStateMachine()
        {
            State(GameOver);
        }

        private IEnumerator<Wait> GameOver()
        {
            Entity.SetCustomDraw(DrawGameOverMenu);
            yield return GoTo(Main);
        }

        private IEnumerator<Wait> Main()
        {
            _menuInfo = GetGameOverOptions();
            _menuInfo.Select(_menuInfo.NextAvailableOption(-1, 1));

            Game.Sound.PlayEvent(LibraryServices.GetLibrary().EndScreen, new Murder.Core.Sounds.PlayEventInfo() { Properties = Murder.Core.Sounds.SoundProperties.Persist | Murder.Core.Sounds.SoundProperties.StopOtherEventsInLayer});

            while (true)
            {
                if (Game.Input.VerticalMenu(ref _menuInfo))
                {
                    switch (_menuInfo.Selection)
                    {
                        case 0: //  Restart
                            Game.Instance.QueueWorldTransition(LibraryServices.GetLibrary().GameplayWorld);
                            break;

                        case 1: //  Quit

                            Game.Instance.QueueWorldTransition(LibraryServices.GetLibrary().MainMenuWorld);
                            break;

                        default:
                            break;
                    }
                }

                yield return Wait.NextFrame;
            }
        }

        private void DrawGameOverMenu(RenderContext render)
        {
            Debug.Assert(_menuInfo.Options is not null);

            // BG Image
            var skin = LibraryServices.GetLibrary().GameOverScreen;

            RenderServices.DrawSprite(render.GameUiBatch, skin,
                new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f), 
                new DrawInfo(1f)
                {
                    Origin = new Vector2(.5f, .5f)
                });


            // Menu options
            Point cameraHalfSize = render.Camera.Size / 2f - new Point(0, _menuInfo.Length * 18);

            RenderServices.DrawVerticalMenu(
                render.UiBatch,
                cameraHalfSize,
                new DrawMenuStyle()
                {
                    Color = Palette.Colors[7],
                    Shadow = Palette.Colors[1],
                    SelectedColor = Palette.Colors[2],
                    Origin = new(0.5f, 0.5f),
                    ExtraVerticalSpace = 19,
                },
                _menuInfo);


            HelloMurderSaveData save = SaveServices.GetOrCreateSave();
            // Score
            DrawScore(render, save);
            // High score
            DrawHighScore(render, save);
            //Credits
            DrawCredits(render);
        }


        private void DrawScore(RenderContext render, HelloMurderSaveData save)
        {
            var score = save.LastAttemptScore;
            var scoreText = "Score: " + score;
            var textDraw = new DrawInfo() { Sort = 0.4f, Color = Color.Black };
            var position = new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f);
            position.X -= 100;
            position.Y += 8;
            RenderServices.DrawSimpleText(render.UiBatch,
                100,
                scoreText,
                position,
                textDraw);

        }
        private void DrawHighScore(RenderContext render, HelloMurderSaveData save)
        {
            // Workaround, since saves are not working between sessions
            HelloMurderPreferences pref = (HelloMurderPreferences)Game.Preferences;

            var highScore = pref.HighScore;
            var highScoreText = "High Score: " + highScore;
            var textDraw = new DrawInfo() { Sort = 0.4f, Color = Color.Black };
            var position = new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f);
            position.X += 50;
            position.Y += 8;
            RenderServices.DrawSimpleText(render.UiBatch,
                100,
                highScoreText,
                position,
                textDraw);
        }

        private void DrawCredits(RenderContext render)
        {
            var creditsText =
@"                                                          Spooky Jam 2024

                                                           Big Team Vancouver
                                                              Programming:
                                                         Matthew Stevens
                                                         Noah Petrides
                                                                     Art:
                                                           Erin Beardy
                                                           Tanya Motwani
                                                              Narrative Design:
                                                           Tanner 

               A game made in Murder Engine by isadorasophia and saint11
";
            var fmodAttribution = "Audio Engine courtesy of FMOD Studio by Firelight Technologies Pty Ltd.";

            var textDraw = new DrawInfo() { Sort = 0.4f, Color = Color.Black };
            var position = new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f);
            var lineWidth = Game.Data.GetFont(100).GetLineWidth(fmodAttribution);
            creditsText += fmodAttribution;
            position.X -= lineWidth/2f;
            position.Y -= 190;
            RenderServices.DrawSimpleText(render.UiBatch,
                100,
                creditsText,
                position,
                textDraw);
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();
        }
    }
}
