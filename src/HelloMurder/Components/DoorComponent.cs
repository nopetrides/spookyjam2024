using Bang.Components;

namespace HelloMurder.Components
{
    public readonly struct DoorComponent : IComponent
    {
        public readonly bool IsOpen = false;
        public DoorComponent() { }
        public DoorComponent(bool isOpen) => IsOpen = isOpen;
        public DoorComponent SetOpenStatus(bool isOpen) => new DoorComponent(isOpen);
    }
}
