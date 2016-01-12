using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    class IntersectCondition : CompositeCondition
    {
        public override IGate InitialGate
        {
            get { return new TrueGate(); }
        }

        public override GateType Type
        {
            get { return GateType.AND; }
        }

        public IntersectCondition()
        { }

        public IntersectCondition(IEnumerable<ICondition> clist)
            : base(clist)
        { }
    }
}
