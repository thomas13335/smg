using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Types
{
    /// <summary>
    /// A finite type with a fixed number of states (0, 1, ...).
    /// </summary>
    public class SimpleStateType : StateType
    {
        #region Private

        private string _name;
        private List<string> _names;

        #endregion

        #region Properties

        public override string Name { get { return _name; } }

        public override bool IsBoolean { get { return false; } }

        public override int Cardinality { get { return _names.Count; } }

        #endregion

        #region Construction

        public SimpleStateType(string name)
        {
            _name = name;
        }

        #endregion

        public override void AddStateNames(IEnumerable<string> names)
        {
            if (null != _names)
            {
                throw new Exception("SMG010: object is frozen.");
            }

            _names = names.ToList();
        }

        public override void Freeze()
        {

        }

        public int GetIndexOfName(string name)
        {
            var index = _names.IndexOf(name);
            if (index < 0)
            {
                throw new Exception("SMG011: state '" + name + "' not found in type '" + Name + "'.");
            }

            return index;
        }

        #region Overrides

        public override IEnumerable<int> GetIndexesOfNames(IEnumerable<string> names)
        {
            return names.Select(e => GetIndexOfName(e));
        }

        public override IEnumerable<string> GetStateNames(IEnumerable<int> stateindexes)
        {
            foreach (var i in stateindexes)
            {
                yield return _names[i];
            }
        }

        public override IEnumerable<string> GetAllStateNames()
        {
            return _names;
        }

        #endregion

        public override string ToString()
        {
            return "ATOM(" + Name + ")";
        }
    }
}
