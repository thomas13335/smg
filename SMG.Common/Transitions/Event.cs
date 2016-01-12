using SMG.Common.Code;
using SMG.Common.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Represents an input event to the state machine.
    /// </summary>
    public class Event
    {
        #region Private

        private List<ProductTrigger> _triggers = new List<ProductTrigger>();
        private TransitionSet _tset = null;

        #endregion

        #region Properties

        public string Name { get; private set; }

        /// <summary>
        /// Collection of product triggers associated with this event.
        /// </summary>
        public IList<ProductTrigger> Triggers { get { return _triggers; } }

        public IGate PreCondition { get; private set; }

        public EffectsCollection EffectsBefore { get; private set; }

        public EffectsCollection EffectsAfter { get; private set; }

        public TransitionSet Transitions { get { return _tset; } }

        #endregion

        public Event(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return "event(" + Name + ")";
        }

        public void CalculateEffects()
        {
            _tset = new TransitionSet();

            // overall precondition for the event
            var c = new TriggerTermCollection<bool>(true);

            foreach (var t in Triggers)
            {
                c.Add(t);

                // add to event wide transition set
                _tset.AddRange(t.Transitions);
            }

            var guards = new GuardCollection();
            var before = new EffectsCollection();
            var after = new EffectsCollection();

            foreach (var t in Triggers)
            {
                foreach (var g in t.Guards)
                {
                    guards.AddGuard(t, g);
                }

                foreach (var effect in t.Effects)
                {
                    after.AddEffect(t, effect);
                }
            }

            guards.AddLeaveEffects(before);
            guards.AddEnterEffects(after);

            // make results visible
            EffectsBefore = before;
            EffectsAfter = after;
            PreCondition = c.PreCondition;
        }

        internal void ClearCalculation()
        {
            foreach(var t in _triggers)
            {
                t.Clear();
            }

            EffectsAfter = null;
            EffectsBefore = null;
            PreCondition = null;
        }
    }
}
