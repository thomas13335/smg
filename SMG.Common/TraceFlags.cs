using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Diagnostics options configuration.
    /// </summary>
    public static class TraceFlags
    {
        public static bool ShowDecompose = false;
        public static bool ShowCompose = false;
        public static bool ShowLabel = false;
        public static bool ShowOptimize = false;
        public static bool ShowSimplify = false;
        public static bool ShowGateTypes = false;
        public static bool ShowDepencencyAnalysis = false;
        public static bool ShowVariableAddress = false;
        public static bool EmitComments = true;
        public static bool Verbose = false;

        public static bool DisableTriggerJoin = false;

        public static bool DisableNestedCodeLabelScheduling = false;

        public static bool ShowGuard = false;
    }
}
