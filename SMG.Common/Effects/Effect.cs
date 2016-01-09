using SMG.Common.Code;
using System.Collections.Generic;

namespace SMG.Common.Effects
{
    /// <summary>
    /// Describes an action taken as a result of a trigger or guard firing.
    /// </summary>
    public abstract class Effect
    {
        /// <summary>
        /// Unique identifier for this effect.
        /// </summary>
        public abstract string UniqueID { get; }

        public override string ToString()
        {
            return UniqueID;
        }
    }
}
