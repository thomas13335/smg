using SMG.Common.Algebra;
using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// An input that is the inversion of another input.
    /// </summary>
    class InvertedInput : Input, IDecomposableCondition, ILogicGate
    {
        #region Private

        private IInput _input;

        #endregion

        #region Properties

        public override int Address { get { return _input.Address; } }

        public override int Group { get { return _input.Group; } }

        public override bool IsInverted { get { return true; } }

        public virtual IEnumerable<IGate> Inputs { get { yield return _input; } }

        #endregion

        public InvertedInput(IInput input)
        {
            _input = input;
        }

        public override string ToString()
        {
            return "!" + _input;
        }

        public override IGate Invert()
        {
            return _input;
        }

        public override Factor CreateFactor()
        {
            var f = _input.CreateFactor();
            f.Invert();
            return f;
        }

        public IGate Decompose()
        {
            if (_input is IDecomposableCondition)
            {
                var r = ((IDecomposableCondition)_input).Decompose();
                return Gate.Invert(r);
            }
            else
            {
                return this;
            }
        }

        public IGate Simplify()
        {
            return this;
        }
    }
}
