using Bang;
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

            // todo - create a message to be sent to CodeInteractableSystem
            var codeEntryComponent = interacted?.TryGetComponent<CodeEntryInteractableComponent>();

            if(codeEntryComponent is null)
            {
                GameLogger.Log("Could not find CodeEntryInteractableComponent");
                return;
            }

            string codeString = "";
            foreach(var code in codeEntryComponent.Value.Code)
            {
                codeString += code.ToString() + " ";
            }

            // todo - send this to DialogueUiSystem
            GameLogger.Log($"Code is {codeString}");
        }
    }
}
