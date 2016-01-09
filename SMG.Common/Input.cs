using SMG.Common.Algebra;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// A gate that represents the value of a single variable.
    /// </summary>
    public abstract class Input : Gate, IInput
    {
        #region Properties

        public override GateType Type { get { return GateType.Input; } }

        /// <summary>
        /// Logical address of the input wire.
        /// </summary>
        public abstract int Address { get; }

        /// <summary>
        /// Logical address of the input variable.
        /// </summary>
        public virtual int Group { get { return Address; } }

        /// <summary>
        /// Number of wires in this state.
        /// </summary>
        public virtual int Cardinality { get { return 1; } }

        /// <summary>
        /// True if this is an inverted input.
        /// </summary>
        public virtual bool IsInverted { get { return false; } }

        #endregion

        /// <summary>
        /// Creates an inverted input base on this input.
        /// </summary>
        /// <returns></returns>
        public virtual IGate Invert()
        {
            return new InvertedInput(this);
        }

        /// <summary>
        /// Creates a factor object representing the elementary factors this input supplies.
        /// </summary>
        /// <returns></returns>
        public abstract Factor CreateFactor();

        /// <summary>
        /// Returns a product representing the elementary factors of this input.
        /// </summary>
        /// <returns></returns>
        public override Product GetProduct()
        {
            var result = new Product();
            result.AddFactor(CreateFactor());
            return result;
        }
    }
}
