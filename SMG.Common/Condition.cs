using SMG.Common.Algebra;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Base class of state space conditions.
    /// </summary>
    public abstract class Condition : ICondition
    {
        #region Private

        private List<ICondition> _elements = new List<ICondition>();
        private bool _frozen = false;

        #endregion

        #region Properties

        public IList<ICondition> Elements { get { return _elements; } }

        #endregion

        #region Construction

        protected Condition()
        {
        }

        /// <summary>
        /// Initializes the condition from a set of existing conditions.
        /// </summary>
        /// <param name="clist"></param>
        protected Condition(IEnumerable<ICondition> clist)
        {
            _elements.AddRange(clist);
        }

        /// <summary>
        /// Clones this condition object.
        /// </summary>
        /// <returns></returns>
        public virtual ICondition Clone()
        {
            var c = (ICondition)Activator.CreateInstance(GetType());
            foreach (var e in Elements) c.Elements.Add(e);
            return c;
        }

        #endregion

        #region Diagnostics

        protected static void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the state transitions inscribed into this condition.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Transition> GetTransitions()
        {
            foreach(var e in Elements)
            {
                foreach(var t in e.GetTransitions())
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Decomposes this condition into a gate.
        /// </summary>
        /// <param name="mode">Selects pre- or postcondition.</param>
        /// <returns>The resulting gate.</returns>
        public abstract IGate Decompose(ConditionMode mode);

        /// <summary>
        /// Freezes this object to prevent further modifications.
        /// </summary>
        public void Freeze()
        {
            if (!_frozen)
            {
                foreach(var e in _elements)
                {
                    e.Freeze();
                }

                BeforeFreeze();
                _frozen = true;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Invokes before the object is frozen.
        /// </summary>
        protected virtual void BeforeFreeze()
        {
        }

        #endregion
    }
}
