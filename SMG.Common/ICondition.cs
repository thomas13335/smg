using SMG.Common.Transitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Evaluation mode for conditions.
    /// </summary>
    public enum ConditionMode
    {
        /// <summary>
        /// Evaluate the precondition of a state transition condition.
        /// </summary>
        Pre,
        /// <summary>
        /// Evaluate the postcondition of a state transition condition.
        /// </summary>
        Post,
        Static
    }

    /// <summary>
    /// Semantix object for state conditions and compositions thereof.
    /// </summary>
    public interface ICondition
    {
        string Key { get; }

        /// <summary>
        /// Child elements of this condition.
        /// </summary>
        IList<ICondition> Elements { get; }

        /// <summary>
        /// Creates a writable copy.
        /// </summary>
        /// <returns></returns>
        ICondition Clone();

        /// <summary>
        /// Decomposes the condition into a logical gate.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        IGate Decompose(ConditionMode mode);

        /// <summary>
        /// Enumerates state transition conditions within this condition.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Transition> GetTransitions();

        /// <summary>
        /// Renders this object unmodifiable.
        /// </summary>
        void Freeze();
    }

    /// <summary>
    /// A condition based on the value of a state machine variable.
    /// </summary>
    public interface IVariableCondition : ICondition
    {
        /// <summary>
        /// The affected variable.
        /// </summary>
        Variable Variable { get; }

        IVariableCondition Parent { get; }

        /// <summary>
        /// The index of the state of the variable this condition applies to.
        /// </summary>
        int StateIndex { get; }

        bool IsTransition { get; }

        /// <summary>
        /// Creates an elementary condition corresponding to this condition.
        /// </summary>
        /// <param name="stateindex"></param>
        /// <returns></returns>
        IGate CreateElementaryCondition(int stateindex);

    }
}
