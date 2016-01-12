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
    public class EffectCondition : TriggerTermCollection<int>, ICompositeTriggerCondition
    {
        public Effect Effect { get; private set; }

        public IGate ConditionLabel { get; set; }

        public EffectCondition(Effect effect, ICompositeTriggerCondition condition, IEnumerable<TransitionMonitor> sources)
            : base(0)
        {
            Effect = effect;
            AddSources(sources);

            Add(condition);
        }

    }
}
