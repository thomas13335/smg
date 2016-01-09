using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// A transaction monitor describing a state transition.
    /// </summary>
    public class Trigger : TransitionMonitor
    {
        #region Private

        private List<TriggerGuard> _guards = new List<TriggerGuard>();

        #endregion

        #region Properties

        public Event Event { get; private set; }

        public IList<TriggerGuard> Guards { get { return _guards; } }

        #endregion

        #region Construction

        public Trigger(Event e, ICondition c)
            : base(e.Name, c)
        {
            Event = e;
        }

        /// <summary>
        /// Creates a trigger based on another trigger, with new conditions.
        /// </summary>
        /// <param name="parent">The original trigger.</param>
        /// <param name="tset">The corresponding transition set.</param>
        /// <param name="pre">The precondition for the new trigger.</param>
        /// <param name="post">The postcondition for the new trigger.</param>
        protected Trigger(Trigger parent, TransitionSet tset, IGate pre, IGate post)
            : base(tset, pre, post)
        {
            Event = parent.Event;
            AddEffects(parent.Effects);
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            return "TRIGGER " + Event.Name + " " + base.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Associates a guard object with this trigger.
        /// </summary>
        /// <param name="g"></param>
        public void AddGuard(TriggerGuard g)
        {
            _guards.Add(g);
        }

        #endregion
    }
}
