using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Parameters for code generation.
    /// </summary>
    public class CodeParameters
    {
        public string BaseClassName { get; set; }

        public string EventTypeName { get; set; }

        public bool IsBaseClassTemplate { get; set; }

        public bool IsPartial { get; set; }

        public string DefaultProtection { get; set; }

        public bool IsProcessEventPublic { get; set; }

        public string Namespace { get; set; }

        public string Language { get; set; }

        public CodeParameters()
        {
            EventTypeName = "EventCode";
            DefaultProtection = "public";
            Language = "C#";
        }

    }
}
