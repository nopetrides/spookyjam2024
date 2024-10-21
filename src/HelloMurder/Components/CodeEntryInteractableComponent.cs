using Bang.Components;
using Bang.Systems;
using Microsoft.Xna.Framework;
using Murder.Attributes;
using Murder.Prefabs;
using Newtonsoft.Json;
using System.Runtime.Versioning;

namespace HelloMurder.Components
{

    public enum CodeDirections
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    public readonly struct CodeEntryInteractableComponent : IComponent
    {
        [JsonProperty, InstanceId]
        public readonly Guid DoorToOpen = Guid.Empty;

        private const int DEFAULT_CODE_LENGTH = 4;
        public readonly CodeDirections[] Code;

        public CodeEntryInteractableComponent()
        {
            Code = new CodeDirections[DEFAULT_CODE_LENGTH];
        }

        public CodeEntryInteractableComponent(int codeLength)
        {
            Code = new CodeDirections[codeLength];
        }
    }
}
