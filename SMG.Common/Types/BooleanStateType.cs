using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Types
{
    /// <summary>
    /// The boolean type has two states (0 and 1).
    /// </summary>
    class BooleanStateType : StateType
    {
        public override string Name
        {
            get { return "BOOLEAN"; }
        }

        public override bool IsBoolean
        {
            get { return true; }
        }

        public override int Cardinality
        {
            get { return 1; }
        }

        public override IEnumerable<int> GetIndexesOfNames(IEnumerable<string> names)
        {
            foreach(var name in names)
            {
                if(name == "0")
                {
                    yield return 0;
                }
                else if(name == "1")
                {
                    yield return 1;
                }
                else
                {
                    throw new Exception("SMG011: state '" + name + "' not found in type '" + Name + "'.");
                }
            }
        }

        public override IEnumerable<string> GetStateNames(IEnumerable<int> stateindexes)
        {
            foreach(var index in stateindexes)
            {
                if(index == 0)
                {
                    yield return "0";
                }
                else if(index == 1)
                {
                    yield return "1";
                }
                else
                {
                    throw new ArgumentException("boolean type accepts state '0' and '1' only.");
                }
            }
        }

        public override IEnumerable<string> GetAllStateNames()
        {
            yield return "0";
            yield return "1";
        }

        public override void AddStateNames(IEnumerable<string> list)
        {
            throw new NotImplementedException();
        }

        public override void Freeze()
        {
            throw new NotImplementedException();
        }
    }
}
