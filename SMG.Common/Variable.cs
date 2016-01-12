using SMG.Common.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// A variable in a state machine.
    /// </summary>
    /// <remarks>
    /// <para>New variables are created by 
    /// the <see cref="M:SMG.Common.StateMachine.AddVariable(System.String,SMG.Common.StateType)"/> method.</para>
    /// <para>Every variable can take values from 0 to Type.Cardinality - 1, called the state index.</para>
    /// </remarks>
    public class Variable
    {
        #region Properties

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The state type declaration.
        /// </summary>
        public StateType Type { get; private set; }

        /// <summary>
        /// The index of the variable within the statemachine.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Base address of the wires associated with this variable.
        /// </summary>
        /// <remarks>
        /// <para>The state index is added to this value to get the wire index.</para></remarks>
        public int Address { get; set; }

        /// <summary>
        /// The cardinality of the set of possible states of this variable.
        /// </summary>
        public int Cardinality { get { return Type.Cardinality; } }

        internal Condition InitialCondition { get; set; }

        #endregion

        #region Construction

        public Variable(string id, StateType decl)
        {
            Name = id;
            Type = decl;
            // InitialCondition = new StateCondition(this, 0);
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
