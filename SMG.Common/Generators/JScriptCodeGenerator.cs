using SMG.Common.Code;
using SMG.Common.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Generators
{
    public class JScriptCodeGenerator : CodeGenerator
    {
        public JScriptCodeGenerator(CodeWriter writer)
            : base(writer)
        { }

        protected override void EmitVariableAccessor(Variable v)
        {
            // Writer.AppendLine("... this." + v + ";");
        }

        protected override void EmitVariableDeclaration(Variable v)
        {
            Writer.AppendLine("this." + v.Name + " = 0;");
        }

        protected override void EmitVariableAssignment(Variable v, int stateindex)
        {
            Writer.AppendLine("this." + v.Name + " = " + stateindex + ";");
        }

        public override void EmitCodeLabelAssignment(string label, IGate gate)
        {
            Writer.Append("var " + label + " = ");
            gate.Emit(this);
            Writer.AppendLine(";");
        }

        public override void EmitVariable(Variable v)
        {
            Writer.Append("this." + v.Name);
        }

        public override void EmitVariableStateCondition(Variable v, int stateindex)
        {
            Writer.Append("this." + v + " == " + stateindex);
        }

        protected override void EmitClassHeader()
        {
            Writer.AppendLine("function " + SM.Name + "()");
        }

        protected override void EmitProcessEventMethodHeader()
        {
            Writer.AppendLine("this.ProcessEvent = function(e)");
        }

        protected override void EmitHandlerHeader(string name)
        {
            Writer.AppendLine("this." + name + "_Handler = function(e)");
        }

        public override void EmitBinaryOperator(GateType type)
        {
            switch (type)
            {
                case GateType.AND:
                    Writer.Append(" && ");
                    break;

                case GateType.OR:
                    Writer.Append(" || ");
                    break;

                default:
                    throw new Exception("unsupported gate type.");
            }
        }

        protected override void EmitEffect(Effect effect)
        {
            if (effect is CallEffect)
            {
                var call = (CallEffect)effect;
                Writer.AppendLine("this." + call.MethodName + "();");
            }
            else if (effect is SendEffect)
            {
                var send = (SendEffect)effect;
                Writer.AppendLine("this.PostEvent(" + Parameters.EventTypeName + "." + send.Event.Name + ");");
            }
            else
            {
                throw new NotImplementedException("effect [" + effect + "] is not supported.");
            }
        }

        protected override void EmitSwitchCaseLabel(Transitions.Event e)
        {
            Writer.AppendLine("case \"" + e.Name + "\":");
        }

        protected override void EmitHandlerInvocation(Transitions.Event e)
        {
            Writer.AppendLine("this." + e.Name + "_Handler();");
            Writer.AppendLine("break;");
        }

        protected override void EmitMethodDeclarations()
        {
            EmitStateString();

            base.EmitMethodDeclarations();
        }

        private void EmitStateString()
        {
            Writer.AppendLine("this.ToStateString = function()");
            Writer.EnterBlock();
            Writer.AppendLine("var result = \"\";");
            var first = true;
            foreach (var v in SM.Variables)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    Writer.AppendLine("result += \" \";");
                }

                Writer.AppendLine("result += \"" + v.Name + "(\";");
                if (v.Type.IsBoolean)
                {
                    Writer.AppendLine("result += " + GetVariableCodeName(v) + " ? \"1\" : \"0\";");
                }
                else
                {
                    Writer.AppendLine("result += " + GetVariableCodeName(v) + ";");
                }

                Writer.AppendLine("result += \")\";");
            }

            Writer.AppendLine("return result;");
            Writer.LeaveBlock();
            Writer.AppendLine();
        }

        private string GetVariableCodeName(Variable v)
        {
            return "this." + v.Name;
        }
    }
}
