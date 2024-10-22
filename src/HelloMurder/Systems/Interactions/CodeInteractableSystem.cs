using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using HelloMurder.Components;
using HelloMurder.Core;
using HelloMurder.Core.Input;
using HelloMurder.Messages;
using Murder;
using Murder.Core.Graphics;
using Murder.Diagnostics;
using Murder.Services;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace HelloMurder.Systems.Interactions
{
    [Filter(typeof(CodeEntryInteractableComponent))]
    [Messager(typeof(CodeEntryMessage), typeof(CodeEntryExitMessage))]
    internal class CodeInteractableSystem : IMessagerSystem, IUpdateSystem, IFixedUpdateSystem, IMurderRenderSystem
    {
        public bool IsCodeEntryActive => _currentCode is not null && _enteredCodeIndex < _currentCode.Length;
        private CodeDirections[]? _currentCode;

        private Vector2 _cachedDirectionInput = Vector2.Zero;
        private bool _inputPressed = false;
        private int _enteredCodeIndex = 0;
        private string _codeOutput = "";

        private Entity _doorToOpen;
        private DoorComponent _door;

        public void OnMessage(World world, Entity entity, IMessage message)
        {
            if (message is CodeEntryMessage)
            {
                HandleCodeInteractionStart(entity,world);
                return;
            }

            HandleCodeInteractionEnd(entity);
        }

        private void HandleCodeInteractionStart(Entity entity, World world)
        {
            GameLogger.Log("Entered code interaction area.");
            var codeEntryComponent = entity.TryGetComponent<CodeEntryInteractableComponent>();

            if (codeEntryComponent is null)
            {
                GameLogger.Log("Could not find CodeEntryInteractableComponent");
                return;
            }

            FindDoor(world, codeEntryComponent);
            DisplayCurrentCode();
        }

        private void FindDoor(World world, CodeEntryInteractableComponent? codeEntryComponent)
        {
            _currentCode = codeEntryComponent.Value.Code;
            var doorGuid = codeEntryComponent.Value.DoorToOpen;
            _doorToOpen = world.GetEntity(WorldServices.GuidToEntityId(world, doorGuid));
        }

        private void HandleCodeInteractionEnd(Entity entity)
        {
            GameLogger.Log("Exit code interaction area");
            _currentCode = null;

            _cachedDirectionInput = Vector2.Zero;
            _inputPressed = false;
            _enteredCodeIndex = 0;
            _codeOutput = "";
        }

        private void DisplayCurrentCode()
        {
            if(!IsCodeEntryActive)
            {
                return;
            }

            string codeString = "";
            for(int i = 0; i < _currentCode.Length; i++)
            {
                if (i == _enteredCodeIndex)
                {
                    codeString += $"({_currentCode[i]}) ";
                }
                else
                {
                    codeString += _currentCode[i] + " ";
                }
            }

            // todo - send this to DialogueUiSystem
            GameLogger.Log($"Code is {codeString}");
            _codeOutput = codeString;
        }

        public void Update(Context context)
        {
            if (_inputPressed) 
            {
                HandleCodeEntryInput(_cachedDirectionInput);
                _inputPressed = false;
            }
        }

        private void HandleCodeEntryInput(Vector2 input)
        {
            switch (input.X, input.Y)
            {
                case (1, 0):
                    EnterNextCodeDirection(CodeDirections.RIGHT);
                    break;
                case (-1, 0):
                    EnterNextCodeDirection(CodeDirections.LEFT);
                    break;
                case (0, 1):
                    EnterNextCodeDirection(CodeDirections.DOWN);
                    break;
                case (0, -1):
                    EnterNextCodeDirection(CodeDirections.UP);
                    break;
            }
        }

        private void EnterNextCodeDirection(CodeDirections direction) 
        {
            if(!IsCodeEntryActive)
            {
                return;
            }

            GameLogger.Log($"Code entry detected: {direction.ToString()}");
            if (_currentCode[_enteredCodeIndex] == direction)
            {
                GameLogger.Log($"Code entry match!");
                _enteredCodeIndex++;
                DisplayCurrentCode();
            }
            else
            {
                GameLogger.Log($"Code entry error! Resetting");
                _enteredCodeIndex = 0;
                DisplayCurrentCode();
            }

            if(_enteredCodeIndex >= _currentCode.Length)
            {
                GameLogger.Log($"Code entered Correctly! Good for you.");
                OpenDoor();
            }
        }
        private void OpenDoor()
        {
            if (_doorToOpen is not null)
            {
                _door = _doorToOpen.GetComponent<DoorComponent>();
                _doorToOpen.AddOrReplaceComponent(_door.SetOpenStatus(!_door.IsOpen));
            }
        }

        public void FixedUpdate(Context context)
        {
            if (!IsCodeEntryActive) 
            {
                return;
            }

            var newInput = Game.Input.GetAxis(InputAxis.CodeEntry).Value;

            if(_cachedDirectionInput == newInput)
            {
                return;
            }

            _cachedDirectionInput = newInput;
            _inputPressed = true;
        }

        public void Draw(RenderContext render, Context context)
        {
            if (!IsCodeEntryActive || string.IsNullOrEmpty(_codeOutput))
            {
                return;
            }

            int font = (int)MurderFonts.PixelFont;

            RenderServices.DrawText(
                render.UiBatch,
                font,
                _codeOutput,
                new Vector2(x: render.Camera.Width / 2f, y: render.Camera.Height - 20),
                maxWidth: 200,
                _codeOutput.Length,
                new DrawInfo(0.1f)
                {
                    Origin = new Vector2(.5f, 0),
                    Color = Palette.Colors[1],
                    Shadow = Palette.Colors[3],
                });
        }
    }
}
