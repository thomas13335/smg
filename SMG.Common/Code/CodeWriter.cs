using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// String buffer for generated target code.
    /// </summary>
    public class CodeWriter
    {
        #region Private

        private StringBuilder _sb = new StringBuilder();
        private int _depth = 0;
        private string _prefix = string.Empty;
        private bool _ateol = true;

        #endregion

        public override string ToString()
        {
            return _sb.ToString();
        }

        #region Basic Methods

        public void Indent()
        {
            _depth++;
            RecalculatePrefix();
        }

        public void Unindent()
        {
            _depth--;
            RecalculatePrefix();
        }

        public void EnterBlock()
        {
            AppendLine("{");
            Indent();
        }

        public void LeaveBlock()
        {
            Unindent();
            AppendLine("}");
        }

        public void Append(string s)
        {
            if (_ateol)
            {
                _sb.Append(_prefix);
                _ateol = false;
            }

            _sb.Append(s);
        }

        public void AppendLine()
        {
            _sb.AppendLine();
            _ateol = true;
        }

        public void AppendLine(string line)
        {
            if (_ateol)
            {
                _sb.AppendLine(_prefix + line);
            }
            else
            {
                _sb.AppendLine(line);
            }

            _ateol = true;
        }

        public void AppendComment(string comment = null)
        {
            if (TraceFlags.EmitComments)
            {
                if (null != comment)
                {
                    AppendLine("/* " + comment + " */ ");
                }
                else
                {
                    AppendLine();
                }
            }
        }

        #endregion

        #region Emit Methods

        public void EmitIfHeader(IGate condition)
        {
            Append("if(");
            condition.Emit(this);
            AppendLine(")");
        }

        public void EmitEnumerationValue(string typename, string valuename)
        {
            Append(typename + "." + valuename);
        }

        public void EmitTypeIdentifier(StateType stype)
        {
            if(stype.IsBoolean)
            {
                Append("bool");
            }
            else
            {
                Append(stype.Name);
            }
        }

        public void EmitVariableAssignment(Variable v, int stateindex)
        {
        }

        internal void EmitHandlerHeader(StateType stype, string name)
        {
            AppendLine("protected virtual void " + name + "_Handler()");
        }

        internal void EmitBinaryOperator(GateType type)
        {
            switch(type)
            {
                case GateType.AND:
                    Append(" && ");
                    break;

                case GateType.OR:
                    Append(" || ");
                    break;

                default:
                    throw new Exception("unsupported gate type.");
            }
        }


        #endregion

        private void RecalculatePrefix()
        {
            _prefix = new string(' ', _depth * 3);
        }




    }
}
