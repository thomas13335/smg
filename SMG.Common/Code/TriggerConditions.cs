using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    class TriggerConditions : ITriggerConditions
    {

        public IGate PreCondition { get; set; }

        public IGate PostCondition { get; set; }

        public TriggerConditions()
        {
            PreCondition = Gate.Constant(false);
            PostCondition = Gate.Constant(false);
        }

        public TriggerConditions(ITriggerConditions other)
        {
            PreCondition = other.PreCondition;
            PostCondition = other.PostCondition;
        }

        /// <summary>
        /// Combines this trigger condition with the preconditions of a product trigger.
        /// </summary>
        /// <param name="trigger"></param>
        /// <remarks>
        /// <para>This eliminates the post-condition.</para></remarks>
        public void JoinTrigger(ProductTrigger trigger)
        {
            var tset = trigger.Transitions;

            // express the postcondition by precondition factors
            var postfrompre = PostCondition.Replace(e =>
            {
                var vc = e as IVariableCondition;
                if (null != vc)
                {
                    if (tset.Contains(vc.Variable))
                    {
                        // express variable by preconditions
                        e = tset.InferPostState(vc.Variable, vc.StateIndex);
                    }
                    else
                    {
                        // variable unchanged, keep condition
                    }
                }

                return e;
            }).Simplify();

            // precondition = trigger precondition AND NOT other postcondition
            var notpost = Gate.Invert(postfrompre);

            // move post condition into precondition for evalulation
            PreCondition = Gate.ComposeAND(PreCondition, postfrompre);

            // post condition void
            PostCondition = Gate.Constant(true);
        }
    }
}
