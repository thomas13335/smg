using SMG.Common.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Gates
{
    abstract class ConstantGate : Gate
    {
    }

    class FalseGate : ConstantGate
    {
        public override GateType Type
        {
            get { return GateType.Fixed; }
        }

        public override string ToString()
        {
            return "0";
        }

        public override Product GetProduct()
        {
            return new Product();
        }
    }

    /// <summary>
    /// Represents a condition that is always true.
    /// </summary>
    class TrueGate : ConstantGate
    {
        public override GateType Type
        {
            get { return GateType.Fixed; }
        }

        public override string ToString()
        {
            return "1";
        }
    }

}
