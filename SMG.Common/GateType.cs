using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Identifies the type of a Gate object.
    /// </summary>
    public enum GateType
    {
        None,
        Fixed,
        Label,
        Input,
        AND,
        OR
    }

    static class GateTypeClassifier
    {
        public static bool IsFixed(this GateType type)
        {
            return type == GateType.Fixed;
        }

        public static bool IsLogical(this GateType type)
        {
            return type == GateType.AND || type == GateType.OR;
        }

    }
}
