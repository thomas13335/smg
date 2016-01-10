using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Exceptions
{
    [Serializable]
    public class SyntaxErrorException : CompilerException
    {
        public SyntaxErrorException(CodeLocation location, string message)
            : base(ErrorCode.SyntaxError, message)
        {
            Location = location;
        }
    }
}
