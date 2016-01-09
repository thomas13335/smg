using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Associates a trigger with a guard.
    /// </summary>
    /// <remarks>
    /// <para>Triggers and guards are in a many-to-many relation, in general.</para>
    /// </remarks>
    public class TriggerGuard
    {
        public ProductTrigger Trigger { get; private set; }

        public Guard Guard { get; private set; }

        public string Name { get { return Guard.Name; } }

        public GuardType GuardType { get { return Guard.Type; } }

        public IGate PreCondition { get; set; }

        public IGate PostCondition { get; set; }

        public TriggerGuard(ProductTrigger t, Guard g)
        {
            Trigger = t;
            Guard = g;
        }

        /*internal IGate EvaluateAndCompose(GateConverter gc, IGate pre, IGate post)
        {
            switch (GuardType)
            {
                case GuardType.LEAVE:
                    pre = Gate.ComposeAND(pre, PreCondition);
                    break;

                case GuardType.TRANSITION:
                    c = Gate.ComposeAND(c, Convert(gc, 0, PreCondition));
                    c = Gate.ComposeAND(c, Convert(gc, 1, PostCondition));
                    break;

                case GuardType.ENTER:
                    c = Gate.ComposeAND(c, Convert(gc, 1, PostCondition));
                    break;

                default:
                    throw new ArgumentException("invalid guard type.");
            }

            return c;
        }*/

        private IGate Compose(GateConverter gc, IGate c, IGate a)
        {
            return Gate.ComposeAND(c, a);
            //return Gate.ComposeAND(c, Convert(gc, 0, a));
        }

        private IGate Convert(GateConverter gc, int stage, IGate c)
        {
            //return gc.ConvertToGate(stage, c);
            return c;
        }
    }
}
