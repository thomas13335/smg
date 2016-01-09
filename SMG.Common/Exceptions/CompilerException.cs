using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Exceptions
{
    [Serializable]
    public class CompilerException : Exception
    {
        public CompilerException(int errorcode, string message)
            : base("SMG" + errorcode.ToString("D3") + ": " + message)
        { }
    }
}
