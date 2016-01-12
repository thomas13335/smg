using SMG.Common.Exceptions;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    public class TransitionCondition : Condition
    {
        private IGate _rgate;
        private IGate _lgate;

        public ICondition Left { get; private set; }

        public ICondition Right { get; private set; }

        public TransitionSet Transitions { get; private set; }

        public TransitionCondition(ICondition lcond, ICondition rcond)
        {
            if (lcond.ContainsTransitions() || rcond.ContainsTransitions())
            {
                throw new CompilerException(ErrorCode.BadCondition, 
                    "arguments to transition condition must not include transitions.");
            }

            // right hand must be a product
            var r = rcond.Decompose(ConditionMode.Static);
            if(r.Type == GateType.OR)
            {
                throw new CompilerException(ErrorCode.BadCondition,
                    "right side of a transition must be a product.");
            }

            _rgate = r;

            var rvclist = r.GetVariableConditions().ToArray();

            var l = lcond.Decompose(ConditionMode.Static);

            // split left side into terms ...
            IEnumerable<IGate> terms = new[] { l };
            if(l.Type == GateType.OR)
            {
                terms = l.GetInputs();
            }

            // construct the transition set from the left side terms and the right side product.
            var tset = BuildTransitionSet(terms, rvclist);

            Left = lcond;
            Right = rcond;
            Transitions = tset;
        }

        public override ICondition Clone()
        {
            return new TransitionCondition(Left, Right);
        }

        private new void Trace(string format, params object[] args)
        {
            Debug.WriteLine(format, args);
        }

        public override IGate Decompose(ConditionMode mode)
        {
            if(mode == ConditionMode.Pre)
            {
                //return Left.Decompose(mode);
                return _lgate;
            }
            else if(mode == ConditionMode.Post)
            {
                //return Right.Decompose(mode);
                return _rgate;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<Transition> GetTransitions()
        {
            return Transitions;
        }

        class ConditionPair
        {
            public IVariableCondition Left;
            public IVariableCondition Right;
        }

        private TransitionSet BuildTransitionSet(IEnumerable<IGate> terms, IEnumerable<IVariableCondition> rvclist)
        {
            var tset = new TransitionSet();

            var result = Gate.Constant(false);
            var staticvariables = new HashSet<int>();

            foreach (var term in terms)
            {
                // populate a dictionary per variable factor
                var dict = new Dictionary<int, ConditionPair>();

                // right sides first
                foreach (var right in rvclist)
                {
                    dict.Add(right.Variable.Index, new ConditionPair { Right = right });
                }

                foreach (var left in term.GetVariableConditions())
                {
                    ConditionPair cp;
                    if(dict.TryGetValue(left.Variable.Index, out cp))
                    {
                        // right side exists
                        cp.Left = left;
                    }
                    else
                    {
                        dict.Add(left.Variable.Index, new ConditionPair { Left = left });
                    }
                }

                // evaluate new term
                var newterm = Gate.Constant(true);
                foreach(var p in dict.Values)
                {
                    if(null == p.Left)
                    {
                        // right side factor not present on left side, move static condition
                        newterm = Gate.ComposeAND(newterm, p.Right.Decompose());
                    }
                    else if(null == p.Right)
                    {
                        // static condition
                        newterm = Gate.ComposeAND(newterm, p.Left.Decompose());
                    }
                    else if(p.Left.StateIndex == p.Right.StateIndex)
                    {
                        // no state change: static condition
                        //newterm = Gate.ComposeAND(newterm, p.Left.Decompose());

                        throw new CompilerException(ErrorCode.BadCondition,
                            "variable consition " + p.Left + " used on both sides of a transition.");
                    }
                    else
                    {
                        newterm = Gate.ComposeAND(newterm, p.Left.Decompose());

                        // add transition
                        var x = new Transition(p.Left)
                        {
                            PreStateIndexes = new[] { p.Left.StateIndex },
                            NewStateIndexes = new[] { p.Right.StateIndex }
                        };

                        tset.Add(x);
                    }
                }

                result = Gate.ComposeOR(result, newterm);
            }

            _lgate = result;

            return tset;
        }
    }
}
