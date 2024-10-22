using Bang.Components;
using Murder.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Components
{
    public readonly struct DoorTriggerComponent : IComponent
    {
        [JsonProperty, InstanceId]
        public readonly Guid DoorToUpdate = Guid.Empty;
        public readonly bool ShouldOpen = false;

        public DoorTriggerComponent() { }
    }
}
