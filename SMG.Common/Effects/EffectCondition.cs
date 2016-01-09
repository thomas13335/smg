using SMG.Common.Code;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Effects
{
    /// <summary>
    /// Condition for an effect for some event.
    /// </summary>
    class EffectCondition : ITriggerConditions
    {
        public Effect Effect { get; private set; }

        public IGate PreCondition  {get; private set;}

        public IGate PostCondition { get; private set; }

        public IEnumerable<TransitionMonitor> Sources { get; private set; }

        public EffectCondition(Effect effect, ITriggerConditions condition, IEnumerable<TransitionMonitor> sources)
        {
            Effect = effect;
            PreCondition = condition.PreCondition;
            PostCondition = condition.PostCondition;
            Sources = sources;
        }
    }
}
