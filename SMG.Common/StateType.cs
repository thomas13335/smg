using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    /// <summary>
    /// Describes the type of a state variable.
    /// </summary>
    public abstract class StateType
    {
        #region Properties

        /// <summary>
        /// The type identifier of this type.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// True if this is the boolean type.
        /// </summary>
        public abstract bool IsBoolean { get; }

        /// <summary>
        /// Number of states the type has.
        /// </summary>
        public abstract int Cardinality { get; }

        #endregion

        /// <summary>
        /// Translates state names into state indexes.
        /// </summary>
        /// <param name="names">The names to translate.</param>
        /// <returns>Sequence of state indexes.</returns>
        public abstract IEnumerable<int> GetIndexesOfNames(IEnumerable<string> names);

        public abstract IEnumerable<string> GetStateNames(IEnumerable<int> stateindexes);

        /// <summary>
        /// Returns the names of all states in order of their index.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetAllStateNames();

        public string GetStateName(int index)
        {
            return GetStateNames(new int[] { index }).First();
        }

        public IEnumerable<int> GetExcluding(IEnumerable<int> exclude)
        {
            for (int j = 0; j < Cardinality; ++j)
            {
                if (!exclude.Contains(j))
                {
                    yield return j;
                }
            }
        }

        public abstract void AddStateNames(IEnumerable<string> list);

        public abstract void Freeze();
    }
}
