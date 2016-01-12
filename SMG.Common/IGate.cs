using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Gates;
using System.Collections.Generic;

namespace SMG.Common
{
    /// <summary>
    /// Represents a logical combination of state conditions.
    /// </summary>
    public interface IGate
    {
        /// <summary>
        /// The gate type.
        /// </summary>
        GateType Type { get; }

        /// <summary>
        /// Unique cache identifier assigned by the GateCache.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// A key representing the normalized value of this gate.
        /// </summary>
        string CacheKey { get; }

        /// <summary>
        /// Called when the gate is added to the cache.
        /// </summary>
        /// <param name="gateid"></param>
        void Freeze(string gateid);

        Product GetProduct();

        /// <summary>
        /// Creates a modifiable copy of this object.
        /// </summary>
        /// <returns></returns>
        IGate Clone();

        /// <summary>
        /// Emits the code corresponding to the gate into a gate evaluator.
        /// </summary>
        /// <param name="writer"></param>
        void Emit(ICodeGateEvaluator writer);
    }

    /// <summary>
    /// Interface for logical gates providing access to underlying inputs.
    /// </summary>
    public interface ILogicGate
    {
        /// <summary>
        /// The inputs to the logical gate.
        /// </summary>
        IEnumerable<IGate> Inputs { get; }

        /// <summary>
        /// Returns a possibly simplified gate representing the same condition.
        /// </summary>
        /// <returns></returns>
        IGate Simplify();
    }

    /// <summary>
    /// Implemented by modifiable gates.
    /// </summary>
    public interface IModifyGate
    {
        /// <summary>
        /// Adds an input to the gate.
        /// </summary>
        /// <param name="input"></param>
        void AddInput(IGate input);

        /// <summary>
        /// Adds a set of inputs to the gate.
        /// </summary>
        /// <param name="inputs"></param>
        void AddInputRange(IEnumerable<IGate> inputs);

        /// <summary>
        /// Replaces an input with a given index with another input.
        /// </summary>
        /// <param name="j"></param>
        /// <param name="input"></param>
        void ReplaceInput(int j, IGate input);
    }

    public interface IDecomposableCondition
    {
        IGate Decompose();
    }

    /// <summary>
    /// Implemented by gates that represent variable inputs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every input is assigned a group and address attribute. The group value represents 
    /// the variable, while the address value represents the stateindex of the input.
    /// </para>
    /// <para>
    /// The (group, address) serves the logical operations on multistate simple variable.</para>
    /// </remarks>
    public interface IInput : IGate
    {
        /// <summary>
        /// The group number of the variable.
        /// </summary>
        int Group { get; }

        /// <summary>
        /// The address of the variable within the group.
        /// </summary>
        int Address { get; }

        /// <summary>
        /// Number of states in the group.
        /// </summary>
        int Cardinality { get; }

        /// <summary>
        /// True if this input inverts the given wire.
        /// </summary>
        bool IsInverted { get; }

        /// <summary>
        /// Creates a factor object representing this input.
        /// </summary>
        /// <returns>The factor.</returns>
        Factor CreateFactor();

        /// <summary>
        /// Returns a gate representing the inverted condition.
        /// </summary>
        /// <returns></returns>
        IGate Invert();
    }
}
