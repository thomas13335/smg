using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    class UnionCondition : CompositeCondition
    {
        public override IGate InitialGate
        {
            get { return new FalseGate(); }
        }

        public override GateType Type
        {
            get { return GateType.OR; }
        }

        public UnionCondition()
        { }

        public UnionCondition(IEnumerable<ICondition> clist)
            : base(clist)
        { }
    }
}
