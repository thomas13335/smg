using SMG.Common.Effects;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Collects guard objects for trigger of a single event during code generation.
    /// </summary>
    class GuardCollection
    {
        #region Private

        class Entry : TriggerTermCollection<Guard>
        {
            public Entry(Guard g) : base(g) { }
        }

        private Dictionary<string, Entry> _map = new Dictionary<string, Entry>();
        private GateConverter _gc;

        #endregion

        public GuardCollection(GateConverter gc)
        {
            _gc = gc;
        }

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
        /// <param name="tg"></param>
        public void AddGuard(ITriggerConditions c0, TriggerGuard tg)
        {
            var name = tg.Guard.Name;
            Entry entry;
            if(!_map.TryGetValue(name, out entry))
            {
                _map[name] = entry = new Entry(tg.Guard);
            }

            var c = new TriggerConditions(c0);

            // combine with condition for this path
            //var c = _gc.ConvertToGate(0, precondition);
            //var c = Gate.Constant(true);

            // evaluate guard conditions
            // c = tg.EvaluateAndCompose(_gc, c);

            switch (tg.Guard.Type)
            {
                case GuardType.LEAVE:
                    c.PreCondition = Gate.ComposeAND(c.PreCondition, tg.PreCondition);
                    c.PostCondition = Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    c.JoinTrigger(tg.Trigger);
                    break;

                case GuardType.TRANSITION:
                    c.PreCondition= Gate.ComposeAND(c.PreCondition, tg.PreCondition);
                    c.PostCondition= Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    break;

                case GuardType.ENTER:
                    c.PostCondition= Gate.ComposeAND(c.PostCondition, tg.PostCondition);
                    c.JoinTrigger(tg.Trigger);
                    break;

                default:
                    throw new ArgumentException("invalid guard type.");
            }


            Gate.TraceLabel(c.PreCondition, c.PostCondition, "add guard");

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
