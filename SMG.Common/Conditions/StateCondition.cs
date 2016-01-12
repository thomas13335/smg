using SMG.Common.Algebra;
using SMG.Common.Exceptions;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using SMG.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// Condition on a single variable for set of values of this variable.
    /// </summary>
    /// <remarks>
    /// <para>This may be either a static state or a transition condition.</para>
    /// </remarks>
    public class StateCondition : Condition, IVariableCondition
    {
        #region Properties

        public Variable Variable { get; private set; }

        public bool PreWildcard { get; private set; }

        public bool PostWildcard { get; private set; }

        public int StateIndex { get { throw new InvalidOperationException(); } }

        /// <summary>
        /// Variable input states to which this condition applies.
        /// </summary>
        public List<int> StateIndexes { get; private set; }

        public List<int> PostStateIndexes { get; private set; }

        public bool IsTransition { get { return PostWildcard || PostStateIndexes.Any(); } }

        public IVariableCondition Parent { get; private set; }

        #endregion

        #region Construction

        public StateCondition(Variable v)
        {
            Variable = v;
            StateIndexes = new List<int>();
            PostStateIndexes = new List<int>();
        }

        public StateCondition(Variable v, int stateindex)
            : this(v)
        {
            StateIndexes.Add(stateindex);
        }

        public StateCondition(StateCondition parent)
            : this(parent.Variable)
        {
            Parent = parent;
        }

        public override ICondition Clone()
        {
            var c = new StateCondition(Variable);
            c.StateIndexes.AddRange(StateIndexes);
            c.PreWildcard = PreWildcard;
            c.PostStateIndexes.AddRange(PostStateIndexes);
            c.PostWildcard = PostWildcard;
            c.Freeze();
            return c;
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Variable.Name);
            sb.Append("(");

            sb.Append(Variable.Type.GetStateNames(StateIndexes).ToSeparatorList());

            if (PostStateIndexes.Any())
            {
                sb.Append(" => ");
                sb.Append(Variable.Type.GetStateNames(PostStateIndexes).ToSeparatorList());
            }

            sb.Append(")");

            return sb.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the PRE states of a state transition condition.
        /// </summary>
        /// <param name="names"></param>
        public void SetPreStates(IdList names)
        {
            StateIndexes.AddRange(Variable.Type.GetIndexesOfNames(names));
            PreWildcard = names.Wildcard;
        }

        public void SetPreStates(IEnumerable<int> indexes)
        {
            StateIndexes.AddRange(indexes);
        }

        /// <summary>
        /// Sets the POST states of a state transition condition.
        /// </summary>
        /// <param name="names"></param>
        public void SetPostStates(IdList names)
        {
            PostStateIndexes.AddRange(Variable.Type.GetIndexesOfNames(names));
            PostWildcard = names.Wildcard;
        }

        public void SetPostStates(IEnumerable<int> indexes)
        {
            PostStateIndexes.AddRange(indexes);
        }

        public IGate CreateElementaryCondition(int stateindex)
        {
            if (Variable.Type.IsBoolean)
            {
                return new BooleanCondition(this, stateindex);
            }
            else
            {
                return new ElementaryCondition(this, stateindex);
            }
        }

        #endregion

        #region Overrides

        public override IEnumerable<Transition> GetTransitions()
        {
            if (PostStateIndexes.Any())
            {
                yield return new Transition(this)
                {
                    PreStateIndexes = StateIndexes.ToArray(),
                    NewStateIndexes = PostStateIndexes.ToArray()
                };
            }
            else if (null != Parent)
            {
                /*foreach (var e in Parent.GetTransitions())
                {
                    yield return e;
                }*/
                throw new NotImplementedException();
            }
        }

        protected override void BeforeFreeze()
        {
            // extend state conditions with complementary indexes
            var s0 = this.ToString();
            if (!StateIndexes.Any())
            {
                Debug.Assert(PreWildcard);
                if(!PostStateIndexes.Any())
                {
                    throw new CompilerException(ErrorCode.BadCondition, "wildcard requires transition.");
                }

                // Debug.Assert(PostStateIndexes.Any());

                StateIndexes = Variable.Type.GetExcluding(PostStateIndexes).ToList();
            }
            else if (!PostStateIndexes.Any())
            {
                if (PostWildcard)
                {
                    Debug.Assert(StateIndexes.Any());
                    PostStateIndexes = Variable.Type.GetExcluding(StateIndexes).ToList();
                }
            }
        }

        /// <summary>
        /// Decomposes the state condition into its elementary parts.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override IGate Decompose(ConditionMode mode)
        {
            Freeze();

            if(IsTransition && (mode != ConditionMode.Pre && mode != ConditionMode.Post))
            {
                throw new CompilerException(ErrorCode.BadCondition, 
                    "state transition cannot be decomposed into a static gate.");
            }

            if (Variable.Type.IsBoolean)
            {
                return DecomposeBoolean(mode);
            }
            else
            {
                return DecomposeSimple(mode);
            }
        }

        private IEnumerable<int> GetStateSource(ConditionMode mode)
        {
            IEnumerable<int> source;
            var post = mode == ConditionMode.Post;

            if (IsTransition && post)
            {
                source = PostStateIndexes;
            }
            else
            {
                source = StateIndexes;
            }

            if (!source.Any())
            {
                throw new Exception("unable to decompose " + this + " empty source (" + mode + ")");
            }

            return source;
        }

        private IGate DecomposeBoolean(ConditionMode mode)
        {
            var source = GetStateSource(mode).ToArray();

            if (source.Length > 1)
            {
                throw new CompilerException(ErrorCode.ConditionNeverSatisfied, "boolean state condition never satisfied.");
            }

            var c = CreateElementaryCondition(source.First());

            Gate.TraceDecompose(this, c, "decompose " + mode);

            return c;
        }

        private IGate DecomposeSimple(ConditionMode mode)
        {
            var source = GetStateSource(mode);
            var it = source.GetEnumerator();
            IGate r = null;
            while (it.MoveNext())
            {
                var e = CreateElementaryCondition(it.Current);
                if (null == r)
                {
                    r = e;
                }
                else
                {
                    r = Gate.ComposeOR(r, e);
                }
            }

            Debug.Assert(null != r);
            Gate.TraceDecompose(this, r, "decompose " + mode);

            return r;
        }

        #endregion
    }
}
