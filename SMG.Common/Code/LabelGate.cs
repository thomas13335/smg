using SMG.Common.Algebra;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// A gate that represents a previously evaluated condition.
    /// </summary>
    class LabelGate : Input, ILogicGate
    {
        #region Properties
        
        public override GateType Type { get { return GateType.Input; } }

        public IGate OriginalGate { get { return Label.OriginalGate; } }

        public override int Address
        {
            get { return Label.Index; }
        }

        public CodeLabel Label { get; private set; }

        public IEnumerable<IGate> Inputs
        {
            get { return OriginalGate.GetInputs(); }
        }

        #endregion

        #region Construction

        public LabelGate(CodeLabel label)
        {
            Label = label;
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            return "<" + Label.Name + ">";
        }

        #endregion

        #region Public Methods

        public void Schedule()
        {
            Label.Schedule();
        }

        public override void Emit(CodeWriter writer)
        {
            if (Label.IsEvaluated)
            {
                writer.Append(Label.Name);
            }
            else
            {
                if (TraceFlags.EmitComments)
                {
                    writer.Append("/* " + Label + " */ ");
                }

                OriginalGate.Emit(writer);
            }
        }

        public override Factor CreateFactor()
        {
            return new BooleanFactor(this);
        }

        public override IGate Clone()
        {
            return Label.OriginalGate.Clone();
        }

        public IGate Simplify()
        {
            return this;
        }

        #endregion
    }
}
