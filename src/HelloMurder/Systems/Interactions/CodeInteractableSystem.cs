using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using HelloMurder.Components;
using Murder.Diagnostics;
using Murder.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Systems.Interactions
{
    [Filter(typeof(CodeEntryInteractableComponent))]
    [Messager(typeof(CollidedWithMessage))]
    internal class CodeInteractableSystem : IMessagerSystem
    {
        public void OnMessage(World world, Entity entity, IMessage message)
        {
            GameLogger.Log("Do the code interaction here!");

            var castMessage = (CollidedWithMessage)message;

            HandleCodeInteraction(entity);
        }

        private void HandleCodeInteraction(Entity entity)
        {
            // present the co
        }
    }
}
