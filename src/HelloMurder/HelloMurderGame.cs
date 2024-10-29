using HelloMurder.Assets;
using HelloMurder.Core;
using HelloMurder.Core.Input;
using HelloMurder.Core.Sounds;
using HelloMurder.Data;
using Microsoft.Xna.Framework.Input;
using Murder;
using Murder.Assets;
using Murder.Core.Input;
using Murder.Core.Sounds;
using Murder.Save;
using Murder.Serialization;
using System.Collections.Immutable;
using System.Text.Json;

namespace HelloMurder;

/// <summary>
/// <inheritdoc cref="IMurderGame"/>
/// </summary>
public class HelloMurderGame : IMurderGame
{
    public static HelloMurderProfile Profile => (HelloMurderProfile)Game.Profile;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Name => "HelloMurder";

    public GameProfile CreateGameProfile() => new HelloMurderProfile();

    public JsonSerializerOptions Options => HelloMurderSerializerOptionsExtensions.Options;

    public ISoundPlayer CreateSoundPlayer() => new FmodSoundPlayer();

    public GamePreferences CreateGamePreferences() => new HelloMurderPreferences();

    public void Initialize()
    {
        Game.Data.CurrentPalette = Palette.Colors.ToImmutableArray();

        // Registers Movement Axis Input
        GamepadAxis[] stick =
        {
                GamepadAxis.LeftThumb,
                GamepadAxis.Dpad
            };

        // Registers movement from left stick or dpad
        Game.Input.RegisterAxes(MurderInputAxis.Movement, stick);

        // Registeres movement from wasd and arrow keys
        Game.Input.Register(MurderInputAxis.Movement,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D));
        
        // Registeres movement from wasd and arrow keys
        //Game.Input.Register(MurderInputAxis.Movement,
        //    new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
        //    new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        // Registers movement from left stick or dpad
        Game.Input.RegisterAxes(MurderInputAxis.Ui, stick);

        // Registers for the UI with wasd and arrow keys
        Game.Input.Register(MurderInputAxis.Ui,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        // Registers movement from left stick or dpad
        Game.Input.RegisterAxes(InputAxis.CodeEntry, stick);

        // Registers for the code entry system with wasd and arrow keys
        Game.Input.Register(InputAxis.CodeEntry,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D),
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right));

        // Interact Key
        Game.Input.Register(InputButtons.Interact, Keys.E);
        Game.Input.Register(InputButtons.Interact, Keys.F);
        Game.Input.Register(InputButtons.Interact, Keys.Z);
        Game.Input.Register(InputButtons.Interact, Buttons.A);
        Game.Input.Register(InputButtons.Interact, Keys.Space);
    }
}
