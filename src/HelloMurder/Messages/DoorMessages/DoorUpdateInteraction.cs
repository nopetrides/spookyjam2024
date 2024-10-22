using Bang;
using Bang.Entities;
using Bang.Interactions;
using HelloMurder.Components;
using Murder.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Messages.DoorMessages
{
    public readonly struct DoorUpdateInteraction : IInteraction
    {
        public void Interact(World world, Entity interactor, Entity? interacted)
        {
            var doorTriggerComponent = interacted.GetComponent<DoorTriggerComponent>();
            var doorEntity = world.GetEntity(WorldServices.GuidToEntityId(world, doorTriggerComponent.DoorToUpdate));
            var doorComponent = doorEntity.GetComponent<DoorComponent>();

            doorEntity.AddOrReplaceComponent(doorComponent.SetOpenStatus(doorTriggerComponent.ShouldOpen));
        }
    }
}
