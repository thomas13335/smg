using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Algebra
{
    /// <summary>
    /// Represents a factor in a <see cref="T:SMG.Common.Algebra.Product"/>.
    /// </summary>
    public abstract class Factor
    {
        #region Private

        private IGate _constant;

        #endregion

        #region Properties

        public abstract Variable Variable { get; }

        /// <summary>
        /// The variable group address of this factor.
        /// </summary>
        public int Group { get; private set; }

        /// <summary>
        /// Optional constant value if the factor evaluates to it.
        /// </summary>
        public IGate Constant { get { return _constant; } }

        /// <summary>
        /// True if the factor evaluates to a constant.
        /// </summary>
        public bool IsConstant { get { return null != Constant; } }

        /// <summary>
        /// Inputs representing the individual contributions of the group.
        /// </summary>
        public abstract IEnumerable<IInput> Inputs { get; }

        #endregion

        #region Construction
        
        /// <summary>
        /// Constructs a new factor for a group.
        /// </summary>
        /// <param name="group"></param>
        protected Factor(int group)
        {
            Group = group;
        }

        /// <summary>
        /// Clones an existing factor.
        /// </summary>
        /// <returns></returns>
        public virtual Factor Clone()
        {
            return (Factor)MemberwiseClone();
        }

        #endregion

        #region Public Methods

        public abstract void AddFactor(Factor f);

        public abstract bool RemoveInput(IInput input);

        public abstract IEnumerable<IGate> ToGateList();

        public abstract void Simplify();

        public abstract void Invert();

        #endregion

        #region Overrideables

        protected virtual void SetConstant(IGate gate)
        {
            _constant = gate;
        }

        #endregion
    }

}
