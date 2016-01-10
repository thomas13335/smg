
namespace SMG.Common.Exceptions
{
    public enum ErrorCode
    {
        SyntaxError = 1,
        ConditionNeverSatisfied,
        AmbigousPreCondition,
        AmbigousPostCondition,
        TypeRedefinition,
        GuardNameReused,
        UndefinedVariable,
        VariableRedefinition,
        InvalidStateName,
        UndefinedType
    }
}
