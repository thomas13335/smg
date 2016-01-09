using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    public class ProductTrigger : Trigger
    {
        /// <summary>
        /// Creates a trigger based on another trigger, with new conditions.
        /// </summary>
        /// <param name="parent">The original trigger.</param>
        /// <param name="tset">The corresponding transition set.</param>
        /// <param name="pre">The precondition for the new trigger.</param>
        /// <param name="post">The postcondition for the new trigger.</param>
        internal ProductTrigger(Trigger parent, TransitionSet tset, IGate pre, IGate post)
            : base(parent, tset, pre, post)
        {
        }

        public ProductTrigger(Trigger parent)
            : base(parent, parent.Transitions, parent.PreCondition, parent.PostCondition)
        {
        }

        public void Qualify()
        {
            Transitions.QualifyForTrigger();
        }
    }
}
