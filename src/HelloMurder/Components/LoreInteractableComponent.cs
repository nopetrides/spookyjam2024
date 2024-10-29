using Bang.Components;
using HelloMurder.Systems.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Components
{
    public readonly struct LoreInteractableComponent : IComponent
    {
        public readonly string LoreSnippet = "Enter your lore dump here";
        public readonly LoreScreenSide ScreenSide = LoreScreenSide.Left;

        public LoreInteractableComponent()
        {
        }
    }
}
