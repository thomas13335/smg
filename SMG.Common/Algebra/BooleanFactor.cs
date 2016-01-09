using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMG.Common.Algebra
{
    class BooleanFactor : Factor
    {
        private IInput _value;
        private IVariableCondition _condition;

        public override IEnumerable<IInput> Inputs
        {
            get { return new IInput[] { _value }; }
        }

        public override Variable Variable
        {
            get { return null != _condition ? _condition.Variable : null; }
        }

        public BooleanFactor(IInput input)
            : base(input.Group)
        {
            _value = input;
            _condition = input as IVariableCondition;
        }

        public void AddInput(IInput input)
        {
            if (!IsConstant)
            {
                if (_value.IsInverted != input.IsInverted)
                {
                    SetConstant(new FalseGate());
                }
            }
        }

        public override void AddFactor(Factor f)
        {
            AddInput(f.Inputs.First());
        }

        public override bool RemoveInput(IInput input)
        {
            SetConstant(new TrueGate());
            return true;
        }

        public override IEnumerable<IGate> ToGateList()
        {
            return Inputs;
        }

        public override void Simplify()
        {
            throw new NotImplementedException();
        }

        public override void Invert()
        {
            _value = (IInput)_value.Invert();
        }
    }
}
