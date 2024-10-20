using Bang.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
