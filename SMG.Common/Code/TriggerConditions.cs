using SMG.Common.Transitions;
using SMG.Common.Conditions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    class TriggerConditions : IElementaryTriggerCondition
    {
        public IGate PreCondition { get; set; }

        public IGate PostCondition { get; set; }

        public TriggerConditions()
        {
            PreCondition = Gate.Constant(false);
            PostCondition = Gate.Constant(false);
        }

        public TriggerConditions(IElementaryTriggerCondition other)
        {
            PreCondition = other.PreCondition;
            PostCondition = other.PostCondition;
        }

        public void Compose(IElementaryTriggerCondition c)
        {
            PreCondition = Gate.ComposeOR(PreCondition, c.PreCondition);
            PostCondition = Gate.ComposeOR(PostCondition, c.PostCondition);
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

            Log.TraceGuard("join trigger: " + tset.ToDebugString());

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

                        Log.TraceGuard(vc.Decompose(), e, "ss");
                    }
                    else if (trigger.Event.Transitions.Contains(vc.Variable))
                    {
                        // variable unchanged but affected by trigger, keep post condition
                        e = Gate.Constant(true);
                    }
                    else
                    {
                        // if not in event tset, move to precondition
                    }
                }

                return e;
            }).Simplify();

            var postreduced = PostCondition.Replace(e =>
            {
                var vc = e as IVariableCondition;
                if (null != vc)
                {
                    if (tset.Contains(vc.Variable))
                    {
                        // express variable by preconditions
                        e = Gate.Constant(true);
                    }
                    else if (trigger.Event.Transitions.Contains(vc.Variable))
                    {
                        // keep
                    }
                    else
                    {
                        // move before
                        e = Gate.Constant(true);
                    }
                }

                return e;
            }).Simplify();

            Log.TraceGuard(PostCondition, postfrompre, "post from pre");

            // move post condition into precondition for evalulation
            PreCondition = Gate.ComposeAND(PreCondition, postfrompre);

            Log.TraceGuard(PostCondition, postreduced, "reduced post");

            // post condition void
            PostCondition = postreduced;
        }
    }
}
