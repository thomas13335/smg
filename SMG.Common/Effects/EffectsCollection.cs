using SMG.Common.Code;
using SMG.Common.Effects;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Effects
{
    /// <summary>
    /// Collection of effects implied by triggers or guards.
    /// </summary>
    class EffectsCollection
    {
        #region Private

        class Entry : TriggerTermCollection<Effect>
        {
            public Entry(Effect effect) : base(effect) { }
        }

        private Dictionary<string, Entry> _map = new Dictionary<string, Entry>();
        private int _stage;

        #endregion

        #region Properties

        public bool IsEmpty { get { return !_map.Any(); } }

        #endregion

        #region Construction

        public EffectsCollection(int stage)
        {
            _stage = stage;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sums an effect under the given condition.
        /// </summary>
        /// <param name="effect">The effect</param>
        /// <param name="condition">The condition term for the effect.</param>
        public void AddEffect(Effect effect, ITriggerConditions condition, TransitionMonitor source)
        {
            Entry entry;
            if(!_map.TryGetValue(effect.UniqueID, out entry))
            {
                _map[effect.UniqueID] = entry = new Entry(effect);
            }

            entry.Add(condition);
            entry.AddSource(source);
        }

        public void AddEffect(ProductTrigger trigger, Effect effect)
        {
            Entry entry;
            if (!_map.TryGetValue(effect.UniqueID, out entry))
            {
                _map[effect.UniqueID] = entry = new Entry(effect);
            }

            var c = new TriggerConditions(trigger);
            c.JoinTrigger(trigger);

            entry.Add(c);
            entry.AddSource(trigger);
        }

        public void Schedule(GateConverter gc)
        {
            foreach (var entry in _map.Values)
            {
                // convert precondition at stage 1
                var pre = gc.ConvertToGate(0, entry.PreCondition);

                /*var fac = Gate.ExtractCommonFactors(pre);
                Gate.TraceLabel(pre, fac, "ezu?");*/

                var post = gc.ConvertToGate(1, entry.PostCondition);

                Gate.TraceLabel(pre, post, "Schedule Effect");

                // TODO: may not need post condition
                gc.Schedule(pre);
                gc.Schedule(post);
            }
        }

        public IEnumerable<EffectCondition> GetEffectConditions()
        {
            return _map.Values.Select(e => new EffectCondition(e.Context, e, e.Sources));
        }

        #endregion
    }
}
