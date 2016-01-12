using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// A condition composing multiple other conditions.
    /// </summary>
    public abstract class CompositeCondition : Condition
    {
        public abstract IGate InitialGate { get; }

        public abstract GateType Type { get; }

        public CompositeCondition()
        { }

        public CompositeCondition(IEnumerable<ICondition> clist)
            : base(clist)
        { }

        public override string ToString()
        {
            var separator = " " + Type + " ";
            return Elements.ToSeparatorList(separator);
        }

        public override IGate Decompose(ConditionMode mode)
        {
            var it = Elements.GetEnumerator();
            var r = InitialGate;
            while (it.MoveNext())
            {
                var e = it.Current.Decompose(mode);
                r = Gate.Compose(Type, r, e);
            }

            return r;
        }
    }
}
