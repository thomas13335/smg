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
    class TriggerTermCollection<T> : ITriggerConditions
    {
        #region Private

        private List<TransitionMonitor> _sources = new List<TransitionMonitor>();

        #endregion

        #region Properties

        public T Context { get; private set; }

        public IEnumerable<TransitionMonitor> Sources { get { return _sources; } }

        public IGate PreCondition { get; private set; }

        public IGate PostCondition { get; private set; }

        #endregion

        public TriggerTermCollection(T context)
        {
            Context = context;
            PreCondition = Gate.Constant(false);
            PostCondition = Gate.Constant(false);
        }

        #region Public Methods

        public void Add(ITriggerConditions c)
        {
            PreCondition = Gate.ComposeOR(PreCondition, c.PreCondition);
            PostCondition = Gate.ComposeOR(PostCondition, c.PostCondition);
        }

        public void AddSource(TransitionMonitor source)
        {
            _sources.Add(source);
        }

        #endregion
    }
}
