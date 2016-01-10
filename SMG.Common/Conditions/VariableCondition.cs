using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Gates;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
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

        #endregion

        #region Construction

        public VariableCondition(Variable v)
        {
            Variable = v;
        }

        public VariableCondition(IVariableCondition parent)
        {
            Parent = parent;
            Variable = Parent.Variable;
        }

        #endregion

        public override string ToString()
        {
            if (TraceFlags.ShowVariableAddress)
            {
                return Variable.Name + "<" + Group + "," + Address + ">";
            }
            else
            {
                return Variable.Name;
            }
        }

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

        public abstract void Emit(CodeWriter writer);

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
                foreach (var t in Parent.GetTransitions())
                {
                    if (t.PreStateIndexes.Contains(StateIndex))
                    {
                        t.PreStateIndexes = new[] { StateIndex };
                        yield return t;
                    }
                }
            }
        }

        #endregion
    }
}
