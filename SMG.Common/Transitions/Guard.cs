using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    public enum GuardType
    {
        Undefined,
        ENTER,
        TRANSITION,
        LEAVE
    }

    public class Guard : TransitionMonitor
    {
        private List<Trigger> _triggers = new List<Trigger>();

        public GuardType Type { get; private set; }

        public IEnumerable<Trigger> Triggers { get { return _triggers; } }

        internal Guard(string name, GuardType gtype, ICondition c)
            : base(name, c)
        {
            if (Transitions.Any())
            {
                Type = GuardType.TRANSITION;
            }
            else if (gtype == GuardType.Undefined)
            {
                Type = GuardType.ENTER;
            }
            else
            {
                Type = gtype;
            }
        }

        public override string ToString()
        {
            return "GUARD " + Name + " " + Type + " " + base.ToString();
        }

        internal void AddTrigger(Trigger t)
        {
            _triggers.Add(t);
        }
    }
}
