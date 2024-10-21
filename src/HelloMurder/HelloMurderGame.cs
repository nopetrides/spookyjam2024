using HelloMurder.Assets;
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
        Game.Input.Register(
            InputAxis.Movement,
            new InputButtonAxis(Keys.W, Keys.A, Keys.S, Keys.D)
            );

        Game.Input.Register(
            InputAxis.CodeEntry,
            new InputButtonAxis(Keys.Up, Keys.Left, Keys.Down, Keys.Right)
            );

        Game.Input.Register(1, Keys.Space);
    }
}
