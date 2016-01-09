using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMG.Common.Transitions
{
    public class Event
    {
        public string Name { get; private set; }

        public List<ProductTrigger> Triggers = new List<ProductTrigger>();

        public Event(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "event(" + Name + ")";
        }
    }
}
