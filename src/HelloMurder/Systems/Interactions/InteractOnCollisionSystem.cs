//using Bang;
//using Bang.Components;
//using Bang.Entities;
//using Bang.Systems;
//using Murder.Components;
//using Murder.Messages;
//using Murder.Messages.Physics;

//namespace HelloMurder.Systems.Interactions
//{
//    [Filter(typeof(InteractOnCollisionComponent))]
//    [Messager(typeof(OnCollisionMessage))]
//    internal class InteractOnCollisionSystem : IMessagerSystem
//    {
//        public void OnMessage(World world, Entity entity, IMessage message)
//        {
//            var castMessage = (OnCollisionMessage)message;

//            if(world.TryGetEntity(castMessage.EntityId) is not Entity interactorEntity)
//            {
//                return;
//            }

//            if(interactorEntity.IsDestroyed || !interactorEntity.HasInteractor())
//            {
//                return;
//            }

//            if (entity.IsDestroyed)
//            {
//                return;
//            }

//            var interaction = entity.GetInteractOnCollision();

//            if(interaction.PlayerOnly && !entity.HasPlayer())
//            {
//                return;
//            }

//            if (castMessage.Movement == Murder.Utilities.CollisionDirection.Exit) 
//            {
//                foreach(var exitMessage in interaction.CustomExitMessages)
//                {
//                    exitMessage.Interact(world, interactorEntity, entity);
//                }

//                if (!interaction.SendMessageOnExit)
//                {
//                    return;
//                }
//            }

//            // Checks complete, let's trigger

//            // todo - confirm this is the layer we want!
//            entity.SendMessage(
//                    new CollidedWithMessage(interactorEntity.EntityId, interactorEntity.GetCollider().Layer)
//                );

//            if (interaction.OnlyOnce)
//            { 
//                entity.RemoveInteractOnCollision();
//            }

//        }
//    }
//}
