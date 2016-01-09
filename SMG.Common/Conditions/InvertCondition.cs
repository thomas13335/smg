using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    class InvertCondition : Condition
    {
        private ICondition _b;

        public InvertCondition(ICondition b)
        {
            _b = b;
        }

        public override string ToString()
        {
            return "NOT " + _b;
        }

        public override IGate Decompose(ConditionMode mode)
        {
            return Gate.Invert(_b.Decompose(mode));
        }

        public override ICondition Clone()
        {
            return new InvertCondition(_b);
        }
    }
}
