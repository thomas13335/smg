using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Sum of terms representing a combined pre/post condition pair.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TriggerTermCollection<T> : ICompositeTriggerCondition
    {
        #region Private

        /// <summary>
        /// Terms elements indexed by postcondition.
        /// </summary>
        private Dictionary<string, TriggerConditions> _elements = new Dictionary<string, TriggerConditions>();
        private List<TransitionMonitor> _sources = new List<TransitionMonitor>();

        #endregion

        #region Properties

        public T Context { get; private set; }

        public IEnumerable<TransitionMonitor> Sources { get { return _sources; } }

        public IEnumerable<IElementaryTriggerCondition> Elements { get { return _elements.Values; } }

        public IGate PreCondition { get; private set; }

        /* public IGate PostCondition { get; private set; }*/ 

        #endregion

        public TriggerTermCollection(T context)
        {
            Context = context;
            PreCondition = Gate.Constant(false);
        }

        #region Public Methods

        public void Add(IElementaryTriggerCondition c)
        {
            var key = c.PostCondition.CacheKey;
            TriggerConditions existing;
            if(!_elements.TryGetValue(key, out existing))
            {
                _elements[key] = existing = new TriggerConditions();
            }

            existing.Compose(c);

            PreCondition = Gate.ComposeOR(PreCondition, c.PreCondition);
        }

        public void Add(ICompositeTriggerCondition composite)
        {
            foreach(var e in composite.Elements)
            {
                Add(e);
            }
        }

        public void AddSource(TransitionMonitor source)
        {
            _sources.Add(source);
        }

        protected void AddSources(IEnumerable<TransitionMonitor> sources)
        {
            _sources.AddRange(sources);
        }

        #endregion
    }
}
