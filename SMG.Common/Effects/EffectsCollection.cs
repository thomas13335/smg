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
    /// Collection of effects implied by a trigger or guard 
    /// in the context of an particular event.
    /// </summary>
    public class EffectsCollection
    {
        #region Private

        class Entry : TriggerTermCollection<Effect>
        {
            public Entry(Effect effect) : base(effect) { }

            public IGate ConditionLabel { get; set; }
        }

        private Dictionary<string, Entry> _map = new Dictionary<string, Entry>();

        #endregion

        #region Properties

        /// <summary>
        /// True if the collection is empty.
        /// </summary>
        public bool IsEmpty { get { return !_map.Any(); } }

        #endregion

        #region Construction

        public EffectsCollection()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sums an effect under the given condition.
        /// </summary>
        /// <param name="effect">The effect</param>
        /// <param name="condition">The condition term for the effect.</param>
        public void AddEffect(Effect effect, ICompositeTriggerCondition condition, TransitionMonitor source)
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

            entry.Add(c);
            entry.AddSource(trigger);
        }

        /// <summary>
        /// Schedules the condition for this effect to a gate converter.
        /// </summary>
        /// <param name="gc"></param>
        public void Schedule(GateConverter gc)
        {
            foreach (var entry in _map.Values)
            {
                if(entry.PreCondition.IsFalse())
                {
                    // never reached
                    continue;
                }

                var result = Gate.Constant(false);

                //entry.ConditionLabel = pre;
                foreach(var element in entry.Elements)
                {
                    var pre = gc.ConvertToGate(0, element.PreCondition);
                    var post = gc.ConvertToGate(1, element.PostCondition);

                    // in code label space now
                    var product = Gate.ComposeAND(pre, post);
                    result = Gate.ComposeOR(result, product);
                }

                gc.Schedule(result);

                // assigning a code label makes this entry part of the game
                entry.ConditionLabel = gc.ConvertToGate(1, result);
            }
        }

        public IEnumerable<EffectCondition> GetEffectConditions()
        {
            return _map.Values
                .Where(e => null != e.ConditionLabel)
                .Select(e => CreateEffectCondition(e));
        }

        public ICompositeTriggerCondition GetEffectCondition(Effect effect)
        {
            Entry entry;
            if(_map.TryGetValue(effect.UniqueID, out entry))
            {
                return CreateEffectCondition(entry);
            }
            else
            {
                return new TriggerTermCollection<int>(0);
            }
        }

        #endregion

        private EffectCondition CreateEffectCondition(Entry e)
        {
            return new EffectCondition(e.Context, e, e.Sources)
            {
                ConditionLabel = e.ConditionLabel
            };
        }
    }
}
