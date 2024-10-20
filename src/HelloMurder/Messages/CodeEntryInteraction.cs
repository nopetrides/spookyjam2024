﻿using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Interactions;
using HelloMurder.Components;
using Murder.Diagnostics;

namespace HelloMurder.Messages
{
    public readonly struct CodeEntryInteraction : IInteraction
    {
        public void Interact(World world, Entity interactor, Entity? interacted)
        {
            GameLogger.Log("CodeEntryInteraction received from IInteraction");

            interacted.SendMessage(new CodeEntryMessage());
        }
    }
}
