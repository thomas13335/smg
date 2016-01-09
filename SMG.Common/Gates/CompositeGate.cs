using SMG.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Gates
{
    public abstract class CompositeGate : Gate, ILogicGate, IModifyGate
    {
        #region Private

        protected List<IGate> _inputs = new List<IGate>();

        #endregion

        public abstract string SeparatorCode { get; }

        public IEnumerable<IGate> Inputs { get { return _inputs; } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var i in this.GetInputs())
            {
                if (sb.Length > 0)
                {
                    sb.Append(SeparatorOperator);
                }

                var brackets = i.Type > Type;
                if (brackets)
                {
                    sb.Append("(");
                }

                if (TraceOptions.ShowGateTypes)
                {
                    sb.Append(i.Type + ":");
                }

                sb.Append(i.ToString());
                if (brackets)
                {
                    sb.Append(")");
                }
            }

            return sb.ToString();
        }

        public virtual void AddInput(IGate gate)
        {
            ValidateCanModify();
            Debug.Assert(Type.IsLogical());
            _inputs.Add(gate);
        }

        public virtual void AddInputRange(IEnumerable<IGate> gates)
        {
            ValidateCanModify();
            foreach (var gate in gates)
            {
                AddInput(gate);
            }
        }

        public void ReplaceInput(int j, IGate input)
        {
            ValidateCanModify();
            _inputs[j] = input;
        }

        public abstract IGate Simplify();
    }
}
