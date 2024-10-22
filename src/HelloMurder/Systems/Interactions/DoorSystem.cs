using Bang;
using Bang.Components;
using Bang.Entities;
using Bang.Systems;
using HelloMurder.Components;
using Murder.Diagnostics;
using System.Collections.Immutable;

namespace HelloMurder.Systems.Interactions
{
    [Watch(typeof(DoorComponent))]
    internal class DoorSystem : IReactiveSystem
    {
        public void OnAdded(World world, ImmutableArray<Entity> entities)
        {

        }

        public void OnModified(World world, ImmutableArray<Entity> entities)
        {
            GameLogger.Log("Door state changed");
            UpdateDoors(entities);
        }

        private void UpdateDoors(ImmutableArray<Entity> entities)
        {
            foreach(var entity in entities)
            {
                var door = entity.GetComponent<DoorComponent>();
                if (door.IsOpen)
                {
                    OpenDoor(entity);
                    continue;
                }
                CloseDoor(entity);
            }
        }

        private void OpenDoor(Entity entity)
        {
            entity.Deactivate();
        }
        private void CloseDoor(Entity entity)
        {
            entity.Activate();
        }

        public void OnRemoved(World world, ImmutableArray<Entity> entities)
        {

        }
    }
}
