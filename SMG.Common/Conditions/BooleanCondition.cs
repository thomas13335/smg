using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    public class BooleanCondition : VariableCondition
    {
        #region Properties
        
        public override bool IsInverted { get { return 0 == StateIndex; } }

        public override string CacheKey
        {
            get
            {
                var result = IsInverted ? "!" : string.Empty;
                return result + base.CacheKey;
            }
        }

        #endregion

        #region Construction

        public BooleanCondition(Variable v, int stateindex = 1)
            : base(v)
        {
            StateIndex = stateindex;
        }

        public BooleanCondition(IVariableCondition parent, int stateindex)
            : base(parent)
        {
            if(stateindex > 2)
            {
                throw new ArgumentException("invalid boolean state index.");
            }

            StateIndex = stateindex;
        }

        public override ICondition Clone()
        {
            if (null != Parent)
            {
                return new BooleanCondition(Parent, StateIndex);
            }
            else
            {
                return new BooleanCondition(Variable, StateIndex);
            }
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            if (IsInverted)
            {
                return "!" + base.ToString();
            }
            else
            {
                return base.ToString();
            }
        }

        #endregion

        #region Overrides

        public override Factor CreateFactor()
        {
            return new BooleanFactor(this);
        }

        public override IGate Invert()
        {
            return new BooleanCondition(this, StateIndex == 0 ? 1 : 0);
        }

        public override void Emit(ICodeGateEvaluator writer)
        {
            if(IsInverted)
            {
                writer.Append("!");
            }

            writer.EmitVariable(Variable);
        }

        public override IGate CreateElementaryCondition(int stateindex)
        {
            if (stateindex == StateIndex)
            {
                return this;
            }
            else
            {
                return new BooleanCondition(Parent, stateindex);
            }
        }

        #endregion
    }
}
