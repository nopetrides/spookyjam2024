using Bang;
using Bang.Entities;
using Bang.Interactions;
using Murder.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Messages.LoreMessages
{
    public readonly struct LoreOpenInteraction : IInteraction
    {
        public void Interact(World world, Entity interactor, Entity? interacted)
        {
            GameLogger.Log("LoreOpenInteraction received from IInteraction");

            interacted.SendMessage(
                new LoreMessage(LoreMessage.LoreState.Open)
                );
        }
    }
}
