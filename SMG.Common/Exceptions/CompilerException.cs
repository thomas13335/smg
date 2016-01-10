using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SMG compiler.
    /// </summary>
    [Serializable]
    public class CompilerException : Exception
    {
        public ErrorCode Code { get; set; }

        /// <summary>
        /// Optional source code location.
        /// </summary>
        public CodeLocation Location { get; set; }

        public CompilerException(ErrorCode code, string message)
            : base("SMG" + ((int)code).ToString("D3") + ": " + message)
        {
            Code = code;
        }

    }
}
