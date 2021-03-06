﻿using SMG.Common.Code;
using SMG.Common.Effects;
using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SMG.Common.Generators
{
    /// <summary>
    /// Generates C# target code.
    /// </summary>
    public class CSharpCodeGenerator : CodeGenerator
    {
        #region Construction

        public CSharpCodeGenerator(CodeWriter writer)
            : base(writer)
        { }

        #endregion

        #region Overrides

        protected override void EmitPreamble()
        {
            Writer.AppendLine("/* class " + SM.Name + " generated by SMG */");
            Writer.AppendLine();
            Writer.AppendLine("using System.Text;");
            Writer.AppendLine();

            if(null != Parameters.Namespace)
            {
                Writer.AppendLine("namespace " + Parameters.Namespace);
                EmitEnterBlock();
            }
        }

        protected override void EmitFooter()
        {
            if (null != Parameters.Namespace)
            {
                EmitLeaveBlock();
            }
        }

        protected override void EmitEventTypeDeclaration()
        {
            EmitEnumeratedType(Parameters.EventTypeName, SM.Events.Select(e => e.Name));
        }

        protected override void EmitTypeDeclaration(string typename, IEnumerable<string> values)
        {
            EmitEnumeratedType(typename, values);
        }

        protected override void EmitClassHeader()
        {
            Writer.Append(Parameters.DefaultProtection + " partial class " + SM.Name);
            if (null != Parameters.BaseClassName)
            {
                Writer.Append(" : " + Parameters.BaseClassName);
                if (Parameters.IsBaseClassTemplate)
                {
                    Writer.Append("<" + Parameters.EventTypeName + ">");
                }
            }

            Writer.AppendLine();
        }

        protected override void EmitConstructor()
        {
            Writer.AppendLine("public " + SM.Name + "()");
            EmitEnterBlock();
            foreach(var v in SM.Variables)
            {
                EmitVariableAssignment(v, 0);
            }
            EmitLeaveBlock();
        }

        protected override void EmitVariableDeclaration(Variable v)
        {
            Writer.AppendLine("private " + GetTypeID(v.Type) + " " + GetVariableCodeName(v) + ";");
        }

        protected override void EmitVariableAccessor(Variable v)
        {
            Writer.AppendLine("public " + GetTypeID(v.Type) + " " + v.Name);
            EmitEnterBlock();
            Writer.AppendLine("get { return " + GetVariableCodeName(v) + "; }");
            EmitLeaveBlock();
            Writer.AppendComment();
        }

        protected override void EmitVariableAssignment(Variable v, int stateindex)
        {
            var stype = v.Type;

            Writer.Append(GetVariableCodeName(v));
            Writer.Append(" = ");
            Writer.Append(GetEnumerationValue(stype, stateindex));
            Writer.AppendLine(";");
        }

        public override void EmitCodeLabelAssignment(string label, IGate gate)
        {
            Writer.Append("var " + label + " = ");
            gate.Emit(this);
            Writer.AppendLine(";");
        }


        protected override void EmitProcessEventMethodHeader()
        {
            var access = Parameters.IsProcessEventPublic ? "public" : "protected";
            var virt = null == Parameters.BaseClassName ? "virtual" : "override";
            Writer.AppendLine(access + " " + virt + " void ProcessEvent(" + Parameters.EventTypeName + " e)");
        }

        protected override void EmitHandlerHeader(string name)
        {
            Writer.AppendLine("protected virtual void " + name + "_Handler()");
        }

        protected override void EmitMethodDeclaration(string method)
        {
            if (!Parameters.IsPartial)
            {
                Writer.AppendLine("protected virtual void " + method + "() { }");
            }
        }

        protected override void EmitMethodDeclarations()
        {
            EmitStateString();

            base.EmitMethodDeclarations();
        }

        protected override void EmitSwitchCaseLabel(Event e)
        {
            var caselabel = Parameters.EventTypeName + "." + e.Name;
            Writer.AppendLine("case " + caselabel + ":");
        }

        protected override void EmitEffect(Effect effect)
        {
            if(effect is CallEffect)
            {
                var call = (CallEffect)effect;
                Writer.AppendLine(call.MethodName + "();");
            }
            else if(effect is SendEffect)
            {
                var send = (SendEffect)effect;
                Writer.AppendLine("PostEvent(" + Parameters.EventTypeName + "." + send.Event.Name + ");");
            }
            else
            {
                throw new NotImplementedException("effect [" + effect + "] is not supported.");
            }
        }

        public override void EmitVariable(Variable v)
        {
            Writer.Append(GetVariableCodeName(v));
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

        public override void EmitVariableStateCondition(Variable v, int stateindex)
        {
            Writer.Append(GetVariableCodeName(v));
            Writer.Append(" == ");
            Writer.Append(GetEnumerationValue(v.Type, stateindex));
        }

        protected override void EmitHandlerInvocation(Event e)
        {
            Writer.AppendLine(e.Name + "_Handler();");
            Writer.AppendLine("break;");
            Writer.AppendLine();
        }


        #endregion

        #region Private Methods

        private string GetTypeID(StateType stype)
        {
            return stype.IsBoolean ? "bool" : stype.Name;
        }

        private string GetEnumerationValue(StateType stype, int index)
        {
            if(stype.IsBoolean)
            {
                return 0 == index ? "false" : "true";
            }
            else
            {
                return GetTypeID(stype) + "." + stype.GetStateName(index);
            }
        }

        private void EmitEnumeratedType(string typename, IEnumerable<string> values)
        {
            Writer.AppendLine(Parameters.DefaultProtection + " enum " + typename);
            Writer.EnterBlock();
            var last = values.Last();
            foreach (var v in values)
            {
                Writer.Append(v.ToString());
                if (v != last) Writer.Append(",");
                Writer.AppendLine();
            }
            Writer.AppendLine();
            Writer.LeaveBlock();
            Writer.AppendLine();
        }

        private void EmitStateString()
        {
            Writer.AppendLine("public string ToStateString()");
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
            return "__" + v.Name;
        }

        #endregion
    }
}
