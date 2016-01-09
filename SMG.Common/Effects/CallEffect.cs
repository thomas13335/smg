using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Effects
{
    /// <summary>
    /// Effect of the CALL action. Invokes a method in the target code class.
    /// </summary>
    public class CallEffect : Effect
    {
        #region Private

        string _name;

        #endregion

        /// <summary>
        /// The name of the method.
        /// </summary>
        public string MethodName { get { return _name; } }

        public override string UniqueID
        {
            get { return "CALL " + _name; }
        }

        public CallEffect(StateMachine sm, string name)
        {
            sm.AddMethod(name);
            _name = name;
        }
    }
}
