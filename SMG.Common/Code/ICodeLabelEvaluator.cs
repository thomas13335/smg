
namespace SMG.Common.Code
{
    public interface ICodeLabelEvaluator
    {
        void EmitCodeLabelAssignment(string label, IGate gate);
    }

    /// <summary>
    /// Evaluates logical gates into source code.
    /// </summary>
    /// <remarks>
    /// <para>Implemented by the CodeGenerator.</para></remarks>
    public interface ICodeGateEvaluator
    {
        void Append(string text);

        void EmitVariable(Variable v);

        void EmitBinaryOperator(GateType type);

        void EmitVariableStateCondition(Variable v, int stateindex);
    }
}
