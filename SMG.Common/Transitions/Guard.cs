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

    /// <summary>
    /// Represents a guard object.
    /// </summary>
    public class Guard : TransitionMonitor
    {
        #region Private

        private List<Trigger> _triggers = new List<Trigger>();

        #endregion

        public GuardType Type { get; private set; }

        public IEnumerable<Trigger> Triggers { get { return _triggers; } }

        public Guard(string name, GuardType gtype, ICondition c)
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
