using Bang;
using Bang.Entities;
using Bang.Interactions;
using Murder.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Messages
{
    public readonly struct CodeEntryExitInteraction : IInteraction
    {
        public void Interact(World world, Entity interactor, Entity? interacted)
        {
            GameLogger.Log("CodeEntryInteraction received from IInteraction");

            interacted.SendMessage(new CodeEntryExitMessage());
        }
    }
}
