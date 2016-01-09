using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// An elementary state condition, selecting a single state.
    /// </summary>
    class ElementaryCondition : VariableCondition, IInput
    {
        #region Properties

        /// <summary>
        /// The address of the corresponding wire.
        /// </summary>
        public override int Address
        {
            get { return Variable.Address + StateIndex; }
        }

        #endregion

        public ElementaryCondition(IVariableCondition parent, int i)
            : base(parent)
        {
            Debug.Assert(0 <= i && i < Variable.Cardinality);
            StateIndex = i;
        }

        public ElementaryCondition(Variable v, int i)
            : base(v)
        {
            StateIndex = i;
        }

        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("(");
            sb.Append(Variable.Type.GetStateName(StateIndex));
            sb.Append(")");

            return sb.ToString();
        }

        #endregion

        public override Factor CreateFactor()
        {
            var f = new SimpleFactor(Parent, Variable);
            f.AddInput(this);
            return f;
        }

        public override IGate Invert()
        {
            var sc = new StateCondition(Variable);
            sc.SetPreStates(Variable.Type.GetExcluding(new int[] { StateIndex }));
            var r = sc.Decompose(ConditionMode.Pre);

            //Gate.TraceLabel(this, r, "invert");

            return r;
        }

        public override void Emit(CodeWriter writer)
        {
            var type = Variable.Type;
            var rval = type.Name + "." + type.GetStateName(StateIndex);
            var lval = Variable.Name;
            writer.Append(lval + " == " + rval);
        }

        public override IGate CreateElementaryCondition(int stateindex)
        {
            return Parent.CreateElementaryCondition(stateindex);
        }
    }
}
