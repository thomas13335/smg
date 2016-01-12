using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    public class AnyCondition : Condition
    {
        public override string ToString()
        {
            return "ANY";
        }

        public override IGate Decompose(ConditionMode mode)
        {
            return Gate.Constant(true);
        }
    }
}
