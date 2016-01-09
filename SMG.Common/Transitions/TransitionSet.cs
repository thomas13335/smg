using SMG.Common.Conditions;
using SMG.Common.Exceptions;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Collects a set of transitions during evaluation.
    /// </summary>
    public class TransitionSet : HashSet<Transition>
    {
        #region Private

        class VariableTransitions : List<Transition>
        {
            public Variable Variable { get; private set; }

            public VariableTransitions(Variable v) { Variable = v; }
        }

        private SortedList<int, VariableTransitions> _variables = new SortedList<int, VariableTransitions>();

        #endregion

        #region Properties

        /// <summary>
        /// The variables affected by the transitions in this set.
        /// </summary>
        public IEnumerable<Variable> Variables { get { return _variables.Values.Select(e => e.Variable); } }

        #endregion

        #region Construction

        /// <summary>
        /// Constructs a new, empty transition set.
        /// </summary>
        public TransitionSet()
        {
        }

        /// <summary>
        /// Constructs a transition set from the variable conditions in a gate.
        /// </summary>
        /// <param name="g"></param>
        public TransitionSet(IGate g)
        {
            foreach (var c in g.SelectAll().OfType<IVariableCondition>())
            {
                AddRange(c.GetTransitions());
            }
        }

        #endregion

        #region Diagnostics

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            foreach (var vt in _variables.Values)
            {
                var decl = vt.Variable.Type;
                sb.AppendFormat(" variable {0}", vt.Variable);
                sb.AppendLine();
                foreach (var t in vt)
                {
                    sb.AppendFormat("    {0} => {1}",
                        decl.GetStateNames(t.PreStateIndexes).ToSeparatorList(),
                        decl.GetStateNames(t.NewStateIndexes).ToSeparatorList());
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Public Methods

        public bool Contains(Variable v)
        {
            return _variables.ContainsKey(v.Index);
        }

        public IEnumerable<Transition> GetTransitions(Variable v)
        {
            VariableTransitions vt;
            if(_variables.TryGetValue(v.Index, out vt))
            {
                return vt;
            }
            else
            {
                return new Transition[0];
            }
        }

        public new void Add(Transition t)
        {
            if (!Contains(t))
            {
                base.Add(t);

                var v = t.Variable;
                VariableTransitions vt;
                if (!_variables.TryGetValue(v.Index, out vt))
                {
                    _variables[v.Index] = vt = new VariableTransitions(v);
                }

                vt.Add(t);
            }
        }

        public void AddRange(IEnumerable<Transition> tlist)
        {
            foreach (var t in tlist) Add(t);
        }

        public void QualifyForTrigger()
        {
            foreach(var vt in _variables.Values)
            {
                if(vt.Count > 1)
                {
                    // multiple transitions for the same variable not allowed.
                    throw new CompilerException(14, "ambigous post conditions in state transition [" + vt.ToSeparatorList() + "].");
                }

                foreach(var t in vt)
                {
                    if(t.NewStateIndexes.Length != 1)
                    {
                        // multistate post
                        throw new CompilerException(15, "ambigous post conditions in state transition [" + t.Parent + "].");
                    }
                }
            }
        }

        public IEnumerable<Transition> Match(IGate genter, IGate gleave)
        {
            var f1 = genter.GetProduct();
            var f2 = gleave.GetProduct();

            foreach(var f in f1.Factors)
            {
                // must exist ...
                var t = GetTransitions(f.Variable).First();
                int[] indexes;
                if (f.Variable.Type.IsBoolean)
                { 
                    indexes = new int[] { f.Inputs.First().IsInverted ? 0 : 1 };
                }
                else
                {
                    indexes = f.Inputs.Select(i => i.Address - i.Group).ToArray();
                }

                var q = t.PreStateIndexes.Intersect(indexes);

                if(!q.Any())
                {
                    // transition is not contained
                    yield break;
                }

                yield return t;
            }
        }

        /// <summary>
        /// Expresses a post state of a variable as an expression of variable preconditions.
        /// </summary>
        /// <param name="v">The variables</param>
        /// <param name="poststateindex">The post state to express.</param>
        /// <returns>The resulting gate.</returns>
        public IGate InferPostState(Variable v, int poststateindex)
        {
            IGate r = new FalseGate();
            VariableTransitions vt;
            var match = false;
            if (_variables.TryGetValue(v.Index, out vt))
            {
                foreach (var t in vt.Where(t => t.NewStateIndexes.Contains(poststateindex)))
                {
                    foreach (var i in t.PreStateIndexes)
                    {
                        var e = t.Parent.CreateElementaryCondition(i);
                        r = Gate.ComposeOR(r, e);
                        match = true;
                    }
                }

                if(!match)
                {
                    var sc = new StateCondition(vt.Variable);
                    sc.SetPreStates(new int[] { poststateindex });
                    r = sc.CreateElementaryCondition(poststateindex);
                }
            }

            Gate.TraceLabel(new ElementaryCondition(v, poststateindex), r, "infer state");

            return r;
        }

        #endregion

    }
}
