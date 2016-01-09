using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    interface ITriggerConditions
    {
        IGate PreCondition { get; }

        IGate PostCondition { get; }
    }
}
