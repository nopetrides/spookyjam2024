using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Murder.Components;
using Murder;
using HelloMurder.Components;
using Murder.Utilities;
using Murder.Helpers;
using System.Numerics;
using HelloMurder.Core.Input;
using HelloMurder.Services;
using Murder.Core.Sounds;

namespace HelloMurder.Systems
{
    /// <summary>
    ///     System intended to capture and relay player inputs to entities.
    ///     System is called during frame updates and fixed updates thanks to interfaces.<br/>
    ///     Targets only entities with <b>both</b> PlayerComponent and AgentComponent
    ///     Example usage:<br/>
    ///     1. Poll input system with: <br/>
    ///         Game.Input <see cref="Game.Input"/><br/>
    ///     2. Send entity messages or call extension functions in FixedUpdate within the foreach:<br/>
    ///         entity.SendMessage <see cref="Entity.SendMessage{T}()"/><br/>
    ///         entity.SetImpulse <see cref="MurderEntityExtensions.SetAgentImpulse(Entity)"/><br/>
    /// </summary>
    [Filter(kind: ContextAccessorKind.Read, typeof(PlayerComponent), typeof(AgentComponent))]
    public class PlayerInputSystem : IUpdateSystem, IFixedUpdateSystem
    {

        private Vector2 _cachedInputAxis = Vector2.Zero;
        private bool _inputPressed = false;

        /// <summary>
        ///     Called every fixed update.
        ///     We can apply input values to fixed updating components such as physics components.
        ///     For example the <see cref="AgentComponent"/>
        /// </summary>
        /// <param name="context"></param>
        public void FixedUpdate(Context context)
        {
            foreach (Entity entity in context.Entities)
            {
                // Send entity messages or use entity extensions to update relevant entities
                bool moved = _cachedInputAxis.HasValue();

                if (moved)
                {
                    Direction direction = DirectionHelper.FromVector(_cachedInputAxis);
                    entity.SetAgentImpulse(_cachedInputAxis, direction);
                }

                if (_inputPressed)
                {
                    Game.Sound.PlayEvent(LibraryServices.GetLibrary().UiSelect, new PlayEventInfo());
                    _inputPressed = false;
                }
            }
        }

        /// <summary>
        ///     Called every frame
        ///     This is where we should poll our input system
        ///     We can optionally cache these values and use them in the <see cref="FixedUpdate(Context)"/>
        /// </summary>
        /// <param name="context"></param>
        public void Update(Context context)
        {
            // Read from Game.Input

            _cachedInputAxis = Game.Input.GetAxis(InputAxis.Movement).Value;
            if (Game.Input.Pressed(1))
            {
                _inputPressed = true;
            }
        }
    }
}
