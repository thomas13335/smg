using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    public interface IElementaryTriggerCondition
    {
        IGate PreCondition { get; }

        IGate PostCondition { get; }
    }

    public interface ICompositeTriggerCondition
    {
        IGate PreCondition { get; }

        IEnumerable<IElementaryTriggerCondition> Elements { get; }
    }
}
