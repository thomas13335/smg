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

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Collects guard objects for a product trigger of a single event.
    /// </summary>
    class GuardCollection
    {
        #region Private

        class Entry : TriggerTermCollection<Guard>
        {
            public Entry(Guard g) : base(g) { }
        }

        private Dictionary<string, Entry> _map = new Dictionary<string, Entry>();

        #endregion

        #region Construction

        public GuardCollection()
        {
        }

        #endregion

        #region Diagnostics

        private void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a guard to this collection.
        /// </summary>
        /// <param name="tg">The trigger guard to add.</param>
        public void AddGuard(IElementaryTriggerCondition c0, TriggerGuard tg)
        {
            var name = tg.Guard.Name;
            Entry entry;
            if (!_map.TryGetValue(name, out entry))
            {
                _map[name] = entry = new Entry(tg.Guard);
            }

            // calculate effective conditions for the guard
            var c = new TriggerConditions(c0);

            switch (tg.Guard.Type)
            {
                case GuardType.LEAVE:
                    c.PreCondition = Gate.ComposeAND(c.PreCondition, tg.PreCondition);
                    c.PostCondition = Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    break;

                case GuardType.TRANSITION:
                    c.PreCondition = Gate.ComposeAND(c.PreCondition, tg.PreCondition);
                    c.PostCondition = Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    break;

                case GuardType.ENTER:
                    c.PostCondition = Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    break;

                default:
                    throw new ArgumentException("invalid guard type.");
            }

            Log.TraceGuard(c.PreCondition, c.PostCondition, "before join");

            if (!TraceFlags.DisableTriggerJoin)
            {
                // express postconditions by preconditions
                c.JoinTrigger(tg.Trigger);
            }

            Log.TraceGuard(c.PreCondition, c.PostCondition, "add guard");

            // and add to entry
            entry.Add(c);
        }

        public void AddLeaveEffects(EffectsCollection effects)
        {
            var list = _map.Values.Where(e => e.Context.Type == GuardType.LEAVE);
            AddEffects(effects, list);
        }

        public void AddEnterEffects(EffectsCollection effects)
        {
            var list = _map.Values.Where(e => e.Context.Type != GuardType.LEAVE);
            AddEffects(effects, list);
        }

        #endregion

        #region Private Methods

        private void AddEffects(EffectsCollection effects, IEnumerable<Entry> list)
        {
            foreach (var entry in list)
            {
                foreach (var effect in entry.Context.Effects)
                {
                    effects.AddEffect(effect, entry, entry.Context);
                }
            }
        }

        #endregion
    }
}
