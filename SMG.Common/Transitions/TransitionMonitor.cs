using SMG.Common.Code;
using SMG.Common.Conditions;
using SMG.Common.Effects;
using SMG.Common.Exceptions;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Base class for Guard and Trigger objects.
    /// </summary>
    /// <remarks>
    /// <para>Each monitor object has a pre- and a post-condition.</para>
    /// <para>The monitor object may have associated actions, called effects.</para>
    /// </remarks>
    public abstract class TransitionMonitor
    {
        #region Private

        private TransitionSet _transitions = new TransitionSet();
        private EffectList _effects = new EffectList();
        private CodeLabel[] _labels = new CodeLabel[2];

        #endregion

        #region Properties

        /// <summary>
        /// Unique name of the monitor.
        /// </summary>
        public string Name { get; private set; }

        public ICondition FullCondition { get; private set; }

        public IGate PreCondition { get; private set; }

        public IGate PostCondition { get; private set; }

        public IEnumerable<Variable> ModifiedVariables { get { return _transitions.Variables; } }

        /// <summary>
        /// The transitions associated with this monitor.
        /// </summary>
        public TransitionSet Transitions { get { return _transitions; } }

        /// <summary>
        /// The effects associated with this monitor.
        /// </summary>
        public IEnumerable<Effect> Effects { get { return _effects; } }

        #endregion

        #region Construction

        protected TransitionMonitor(string name, ICondition c)
        {
            Name = name;
            FullCondition = c;

            DeriveTransitions();
            DerivePostCondition();
        }

        protected TransitionMonitor(TransitionSet tset, IGate pre, IGate post)
        {
            _transitions = tset;

            Name = "internal";
            PreCondition = pre;
            PostCondition = post;
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            var text = null == FullCondition ? PreCondition.ToString() : FullCondition.ToString();
            return "WHEN " + text + " " + Effects.ToSeparatorList();
        }

        #endregion

        #region Public Methods

        public void AddEffects(IEnumerable<Effect> elist)
        {
            _effects.AddRange(elist);
        }

        #endregion

        #region Internal Methods


        internal void SetLabel(int stage, CodeLabel label)
        {
            _labels[stage] = label;
        }

        #endregion

        #region Private Methods

        private void DeriveTransitions()
        {
            var translist = FullCondition.GetTransitions().ToList();
            _transitions.AddRange(translist);
        }

        private void DerivePostCondition()
        {
            PreCondition = FullCondition.Replace(x => ReplaceCondition(x, false))
                .Decompose(ConditionMode.Pre)
                .Simplify();

            PostCondition = FullCondition.Replace(x => ReplaceCondition(x, true))
                .Decompose(ConditionMode.Post)
                .Simplify();
        }

        private ICondition ReplaceCondition(ICondition c, bool post)
        {
            if (c is StateCondition)
            {
                var sc = (StateCondition)c;

                if (_transitions.Contains(sc.Variable))
                {
                    // replace the transition condition with either its pre or post states
                    if(sc.IsTransition)
                    {
                        // transition condition
                        if (!post)
                        {
                            // replace with precondition
                            var rc = new StateCondition(sc);
                            rc.SetPreStates(sc.StateIndexes);
                            c = rc;
                        }
                        else
                        {
                            // replace with postcondition
                            var rc = new StateCondition(sc);
                            rc.SetPreStates(sc.PostStateIndexes);
                            c = rc;
                        }
                    }
                }
            }

            return c;

        }

        #endregion
    }
}
