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

    public interface IInput : IGate
    {
        int Group { get; }

        int Address { get; }

        int Cardinality { get; }

        bool IsInverted { get; }

        Factor CreateFactor();

        IGate Invert();
    }
}
