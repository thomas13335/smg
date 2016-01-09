using SMG.Common.Code;
using System;
using System.Text;
using System.Linq;

namespace SMG.Common.Transitions
{
    /// <summary>
    /// Describes the transition of the value of a single variable.
    /// </summary>
    public class Transition
    {
        #region Properties

        public IVariableCondition Parent { get; private set; }

        public Variable Variable { get { return Parent.Variable; } }

        public int[] PreStateIndexes { get; set; }

        public int[] NewStateIndexes { get; set; }

        public int SinglePostStateIndex
        {
            get
            {
                if (NewStateIndexes.Length != 1)
                {
                    throw new Exception("unable to perform transition.");
                }

                return NewStateIndexes[0];
            }
        }

        public IGate PreCondition
        {
            get
            {
                var gates = PreStateIndexes.Select(index => Parent.CreateElementaryCondition(index));
                return Gate.ComposeOR(gates);
            }
        }

        #endregion

        #region Construction

        public Transition(IVariableCondition parent)
        {
            Parent = parent; 
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Variable + "(");
            sb.Append(Variable.Type.GetStateNames(PreStateIndexes).ToSeparatorList());
            sb.Append(" => ");
            sb.Append(Variable.Type.GetStateNames(NewStateIndexes).ToSeparatorList());
            sb.Append(")");
            return sb.ToString();
        }

        #endregion

    }
}
