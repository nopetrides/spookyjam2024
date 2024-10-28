using Bang.Entities;
using Bang.StateMachines;
using HelloMurder.Assets;
using HelloMurder.Core;
using HelloMurder.Services;
using Murder.Assets;
using Murder.Attributes;
using Murder.Core.Graphics;
using Murder.Services;
using Murder;
using System.Numerics;
using Newtonsoft.Json;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using HelloMurder.Core.Sounds;
using Murder.Core.Sounds;
using HelloMurder.Data;


namespace HelloMurder.StateMachines
{
    public class BootStateMachine : StateMachine
    {
        private bool _showFmod = true;

        [JsonProperty, GameAssetId<WorldAsset>]
        public readonly Guid _nextWorld = Guid.Empty;

        private float _alpha = 0f;

        public BootStateMachine()
        {
            State(Main);
        }

        private float _lastTransition = 0;

        private IEnumerator<Wait> Main()
        {
            Entity.SetCustomDraw(Draw);

            Game.Sound.PlayEvent(LibraryServices.GetLibrary().MainMenuMusic, new PlayEventInfo() { Properties = SoundProperties.Persist | SoundProperties.StopOtherEventsInLayer });

            // silly workaround
            /*if (Game.Preferences is HelloMurderPreferences gamePref)
            {
                gamePref.ToggleSoundVolumeAndSave();
                gamePref.ToggleMusicVolumeAndSave();
            }*/
            
            if (Murder.Game.Preferences.SoundVolume == 0)
            {
                Murder.Game.Preferences.ToggleSoundVolumeAndSave();
                Murder.Game.Preferences.ToggleSoundVolumeAndSave();
            }
            if (Murder.Game.Preferences.MusicVolume == 0)
            {
                Murder.Game.Preferences.ToggleMusicVolumeAndSave();
                Murder.Game.Preferences.ToggleMusicVolumeAndSave();
            }

            // fade in
            float duration = 0.5f;
            _lastTransition = Game.Now;
            while (Game.Now - _lastTransition < duration)
            {
                _alpha = (Game.Now - _lastTransition) / duration;
                yield return Wait.NextFrame;
            }

            _alpha = 1.0f;

            // wait, skippable
            duration = 2f;
            _lastTransition = Game.Now;
            while (Game.Now - _lastTransition < duration && !Game.Input.PressedAndConsume(InputButtons.Interact))
            {
                yield return Wait.NextFrame;
            }


            // fade out
            duration = 0.5f;
            _lastTransition = Game.Now;
            while (Game.Now - _lastTransition < duration)
            {
                _alpha = 1 - ((Game.Now - _lastTransition) / duration);
                yield return Wait.NextFrame;
            }
            _alpha = 0.0f;

            yield return Wait.NextFrame;

            LevelServices.SwitchScene(_nextWorld);
        }

        private void Draw(RenderContext render)
        {
            LibraryAsset library = LibraryServices.GetLibrary();

            var murder = library.MurderLogo;

            // Murder Logo
            Color logoBlack = Color.Black;
            logoBlack = logoBlack.FadeAlpha(1 - _alpha);

            RenderServices.DrawSprite(render.GameUiBatch, murder,
                new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f - 200f), 
                new DrawInfo(logoBlack, 0.8f) { Origin = new Vector2(.5f, .5f), BlendMode = BlendStyle.Wash});

            // Credits text
            Color textWhite = Color.White;
            textWhite = textWhite.FadeAlpha(_alpha);
            var creditsText =
@"               A Game For 
     Andtech Studios Spooky Game Jam 2024
                 Created by 
            Big Team Vancouver";

            var textDraw = new DrawInfo(textWhite, 0.4f);
            var position = new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f);
            var lineWidth = Game.Data.GetFont(100).GetLineWidth(creditsText);
            position.X -= lineWidth / 2f;
            position.Y -= 30;
            RenderServices.DrawSimpleText(render.UiBatch,
                100,
                creditsText,
                position,
                textDraw);

            if (_showFmod)
            {
                var fmod = library.FmodLogo;
                RenderServices.DrawSprite(render.GameUiBatch, fmod,
                    new Vector2(render.Camera.Size.X / 2f, render.Camera.Size.Y / 2f + 200f), 
                    new DrawInfo(logoBlack, 0.8f) { Origin = new Vector2(.5f, .5f), BlendMode = BlendStyle.Wash });
            }
        }
    }
}
