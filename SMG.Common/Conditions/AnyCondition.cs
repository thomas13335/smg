using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    class AnyCondition : Condition
    {
        public override IGate Decompose(ConditionMode mode)
        {
            return new TrueGate();
        }
    }
}
