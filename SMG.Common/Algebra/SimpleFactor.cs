using SMG.Common.Conditions;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Algebra
{
    /// <summary>
    /// Factors resulting from a variable of simple type.
    /// </summary>
    class SimpleFactor : Factor
    {
        #region Private 

        private bool[] _invert;
        private IVariableCondition _condition;

        #endregion

        #region Properties
        
        public override IEnumerable<IInput> Inputs
        {
            get { return GetInputs(); }
        }

        public override Variable Variable { get { return _condition.Variable; } }

        #endregion

        public SimpleFactor(IVariableCondition condition, Variable v)
            : base(v.Address)
        {
            _condition = condition;
            _invert = new bool[v.Cardinality];
        }

        public override Factor Clone()
        {
            var f = new SimpleFactor(_condition, Variable);
            foreach(var i in GetInputs())
            {
                f.AddInput(i);
            }

            return f;
        }

        #region Diagnostics
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("*(");

            if (IsConstant)
            {
                sb.Append(Constant);
            }
            else
            {
                sb.Append(GetInputs().ToSeparatorList());
            }

            sb.Append(")");
            return sb.ToString();
        }


        private void Trace(string format, params object[] args)
        {
            //  Debug.WriteLine(format, args);
        }

        #endregion

        #region Public Methods


        public override void AddFactor(Factor f)
        {
            var z = f as SimpleFactor;
            if (null == z)
            {
                throw new ArgumentException("expected VariableFactor.");
            }

            if (IsConstant)
            {
                return;
            }
            else if (f.IsConstant)
            {
                //SetConstant(f.Constant);
                throw new NotImplementedException();
            }

            var sb = new StringBuilder();
            sb.AppendFormat("factor addition {0} + {1}", this, f);

            // combine inverters
            for (int j = 0; j < Variable.Cardinality; ++j)
            {
                // contained in left set?
                var f1 = !_invert[j];

                // contained in right set?
                var f2 = !z._invert[j];

                if (f1 && f2)
                {
                    _invert[j] = false;
                }
                else
                {
                    _invert[j] = true;
                }
            }

            CheckZero();

            sb.AppendFormat(" ==> {0}", this);
            Trace("{0}", sb);
        }

        public void AddInput(IInput input)
        {
            var index = input.Address - Group;

            if (index < 0 || index >= Variable.Cardinality)
            {
                throw new ArgumentException("variable factor input mismatch.");
            }


            if (input.IsInverted)
            {
                // inverted input factor
                _invert[index] = true;
            }
            else
            {
                for (int j = 0; j < Variable.Cardinality; ++j)
                {
                    if (j != index)
                    {
                        _invert[j] = true;
                    }
                    else
                    {
                        _invert[j] = false;
                    }
                }
            }
        }

        public override bool RemoveInput(IInput input)
        {
            var index = input.Address - Group;

            var sb = new StringBuilder();
            sb.AppendFormat("remove input {0} - {1}", this, input);

            if (index < 0 || index >= Variable.Cardinality)
            {
                throw new ArgumentException("variable factor input mismatch.");
            }

            if (input.IsInverted)
            {
                _invert[index] = false;
            }
            else
            {
                for (int j = 0; j < Variable.Cardinality; ++j)
                {
                    if (index != j)
                    {
                        _invert[j] = false;
                    }
                }
            }

            sb.AppendFormat(" => {0}", this);
            Trace("{0}", sb);

            CheckZero();

            return true;
        }


        public override void Simplify()
        {
            throw new NotImplementedException();
        }

        public override void Invert()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("factor invert   {0}", this);

            for (int j = 0; j < Variable.Cardinality; ++j)
            {
                _invert[j] = !_invert[j];
            }

            sb.AppendFormat("=> {0}", this);
            Trace("{0}", sb);
        }

        #endregion

        private string ToStateName(IInput input)
        {
            return Variable.Type.GetStateName(input.Address - Group);
        }

        protected override void SetConstant(IGate gate)
        {
            base.SetConstant(gate);
        }

        private void CheckZero()
        {
            if(!_invert.Any(e => !e))
            {
                // all invert lines combined => FALSE
                SetConstant(new FalseGate());
            }
            else if(!_invert.Any(e => e))
            {
                // no input line => FALSE
                SetConstant(new FalseGate());
            }
        }

        private IEnumerable<IInput> GetInputs()
        {
            if (_invert.Count(e => !e) == 1)
            {
                // single state
                int j = 0;
                for (; j < Variable.Cardinality; ++j)
                {
                    if (!_invert[j]) break;
                }

                yield return new ElementaryCondition(_condition, j);
            }
            else
            {
                for (int j = 0; j < Variable.Cardinality; ++j)
                {
                    if (_invert[j])
                    {
                        yield return CreateInvertedInput(j);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the elementary inputs that make up this factor.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IGate> ToGateList()
        {
            var inputs = GetInputs();

            if(!inputs.Any())
            {
                return new[] { new TrueGate() };
            }
            else
            {
                return inputs;
            }
        }

        private IInput CreateInvertedInput(int stateindex)
        {
            return new InvertedInput(new ElementaryCondition(_condition, stateindex));
        }
    }
}
