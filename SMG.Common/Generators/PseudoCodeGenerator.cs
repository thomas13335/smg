using SMG.Common.Code;
using SMG.Common.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Generators
{
    public class PseudoCodeGenerator : CodeGenerator
    {
        public PseudoCodeGenerator(CodeWriter writer)
            : base(writer)
        {
        }

        protected override void EmitVariableDeclaration(Variable v)
        {
        }

        protected override void EmitVariableAccessor(Variable v)
        {
        }

        protected override void EmitVariableAssignment(Variable v, int stateindex)
        {
            Writer.Append("SET ");
            EmitStateCondition(v, stateindex);
            Writer.AppendLine();
        }

        public override void EmitCodeLabelAssignment(string label, IGate gate)
        {
            Writer.Append("SET " + label + " = ");
            gate.Emit(this);
            Writer.AppendLine();
        }

        protected override void EmitProcessEventMethodHeader()
        {
        }

        protected override void EmitSwitchCaseLabel(Transitions.Event e)
        {
        }

        protected override void EmitHandlerInvocation(Transitions.Event e)
        {
        }

        protected override void EmitClassHeader()
        {
        }

        public override void EmitVariable(Variable v)
        {
            Writer.Append(v.Name);
        }

        public override void EmitBinaryOperator(GateType type)
        {
            switch (type)
            {
                case GateType.AND:
                    Writer.Append(" AND ");
                    break;

                case GateType.OR:
                    Writer.Append(" OR ");
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        public override void EmitVariableStateCondition(Variable v, int stateindex)
        {
            EmitStateCondition(v, stateindex);
        }

        protected override void EmitHandlerHeader(string name)
        {
            Writer.AppendLine("PROCEDURE " + name);
        }

        protected override void EmitIfHeader(IGate gate)
        {
            Writer.Append("IF ");
            gate.Emit(this);
            Writer.AppendLine(" THEN");
        }

        protected override void EmitEnterBlock()
        {
            Writer.AppendLine("BEGIN");
            Writer.Indent();
        }

        protected override void EmitLeaveBlock()
        {
            Writer.Unindent();
            Writer.AppendLine("END");
        }

        protected override void EmitProcessEventMethod()
        {
        }

        protected override void EmitEffect(Effects.Effect effect)
        {
            Writer.AppendLine(effect.ToString());
        }

        private void EmitStateCondition(Variable v, int stateindex)
        {
            Writer.Append(v.Name + "(" + v.Type.GetStateName(stateindex) + ")");
        }
    }
}
