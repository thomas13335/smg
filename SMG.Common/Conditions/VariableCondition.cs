using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// Base class for conditions based on variables values.
    /// </summary>
    public abstract class VariableCondition : Condition, IInput, IVariableCondition
    {
        #region Private

        private string _gateid;
        private IVariableCondition _transparent;

        #endregion

        #region Properties

        public IVariableCondition Parent { get; private set; }

        public Variable Variable { get; private set; }

        /// <summary>
        /// The index of the selected state.
        /// </summary>
        public int StateIndex { get; protected set; }

        public GateType Type { get { return GateType.Input; } }

        public string ID { get { return _gateid; } }

        public int Group { get { return Variable.Address; } }

        public virtual int Address { get { return Variable.Address; } }

        public int Cardinality { get { return Variable.Type.Cardinality; } }

        public virtual bool IsInverted { get { return false; } }

        public virtual bool IsTransition { get { return false; } }

        public virtual string CacheKey
        {
            get
            {
                var key = Variable.Name;

                // add parent key if this refers to a state transition condition.
                /*if(null != _transparent)
                {
                    key += "$" + _transparent.Key;
                }*/

                return key;
            }
        }

        #endregion

        #region Construction

        public VariableCondition(Variable v)
        {
            Variable = v;
        }

        public VariableCondition(IVariableCondition parent)
        {
            Debug.Assert(parent != this);
            Parent = parent;
            Variable = Parent.Variable;

            var top = Parent;
            while(null != top)
            {
                if(top.IsTransition)
                {
                    _transparent = top;
                    break;
                }

                top = top.Parent;
            }
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            string result;
            if (TraceFlags.ShowVariableAddress)
            {
                result = Variable.Name + "<" + Group + "," + Address + ">";
            }
            else
            {
                result = Variable.Name;
            }

            return result;
        }

        #endregion

        #region IInput

        public void Freeze(string gateid)
        {
            if (null != _gateid)
            {
                throw new Exception("gate identifier already assigned.");
            }

            _gateid = gateid;
        }

        public virtual Factor CreateFactor()
        {
            throw new NotImplementedException();
        }

        public virtual IGate Invert()
        {
            return new InvertedInput(this);
        }

        public bool IsFixed
        {
            get { return false; }
        }

        public Product GetProduct()
        {
            return new Product(CreateFactor());
        }

        public IGate Simplify()
        {
            throw new NotImplementedException();
        }

        IGate IGate.Clone()
        {
            throw new NotImplementedException();
        }

        public abstract void Emit(ICodeGateEvaluator writer);

        public abstract IGate CreateElementaryCondition(int stateindex);

        #endregion

        #region ICondition

        public override IGate Decompose(ConditionMode mode)
        {
            return this;
        }

        #endregion

        #region IVariableCondition

        public override IEnumerable<Transition> GetTransitions()
        {
            if (null != Parent)
            {
                /*foreach (var t in Parent.GetTransitions())
                {
                    if (t.PreStateIndexes.Contains(StateIndex))
                    {
                        t.PreStateIndexes = new[] { StateIndex };
                        yield return t;
                    }
                }*/

                throw new NotImplementedException();
            }

            yield break;
        }

        #endregion
    }
}
