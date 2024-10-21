﻿using Bang;
using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using HelloMurder.Components;
using HelloMurder.Core.Input;
using HelloMurder.Messages;
using Murder;
using Murder.Diagnostics;
using System.Numerics;

namespace HelloMurder.Systems.Interactions
{
    [Filter(typeof(CodeEntryInteractableComponent))]
    [Messager(typeof(CodeEntryMessage), typeof(CodeEntryExitMessage))]
    internal class CodeInteractableSystem : IMessagerSystem, IUpdateSystem, IFixedUpdateSystem
    {
        public bool IsCodeEntryActive => _currentCode is not null && _enteredCodeIndex < _currentCode.Length;
        private CodeDirections[]? _currentCode;

        private Vector2 _cachedDirectionInput = Vector2.Zero;
        private bool _inputPressed = false;
        private int _enteredCodeIndex = 0;

        public void OnMessage(World world, Entity entity, IMessage message)
        {
            if (message is CodeEntryMessage)
            {
                HandleCodeInteractionStart(entity);
                return;
            }

            HandleCodeInteractionEnd(entity);
        }

        private void HandleCodeInteractionEnd(Entity entity)
        {
            GameLogger.Log("Exit code interaction area");
            _currentCode = null;

            _cachedDirectionInput = Vector2.Zero;
            _inputPressed = false;
            _enteredCodeIndex = 0;
    }

        private void HandleCodeInteractionStart(Entity entity)
        {
            GameLogger.Log("Entered code interaction area.");
            var codeEntryComponent = entity.TryGetComponent<CodeEntryInteractableComponent>();

            if (codeEntryComponent is null)
            {
                GameLogger.Log("Could not find CodeEntryInteractableComponent");
                return;
            }

            _currentCode = codeEntryComponent.Value.Code;
            DisplayCurrentCode();
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
    }
}
