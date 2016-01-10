using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    public class CodeLocation
    {
        public string SourceFile { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public CodeLocation(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            var s = SourceFile ?? "<sourcecode>";
            return s + "(" + Line + "," + Column + ")";
        }
    }
}
