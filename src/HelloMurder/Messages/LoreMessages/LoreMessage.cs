using Bang.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloMurder.Messages.LoreMessages
{
    public readonly struct LoreMessage : IMessage
    {
        public LoreMessage(LoreState newState) 
        {
            state = newState;
        }

        public readonly LoreState state;

        public enum LoreState
        {
            Open = 0,
            Close = 1,
        }
    }
}
