using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Transitions
{
    class VariableTransitions : IEnumerable<Transition>
    {
        private List<Transition> _tlist = new List<Transition>();

        public int Count { get { return _tlist.Count; } }

        public Variable Variable { get; private set; }

        public VariableTransitions(Variable v) { Variable = v; }

        public void Add(Transition t)
        {
            foreach(var et in _tlist)
            {
                if(t.IsSubsetOf(et))
                {
                    return;
                }
            }

            _tlist.Add(t);
        }

        public IEnumerator<Transition> GetEnumerator()
        {
            return _tlist.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _tlist.GetEnumerator();
        }
    }
}
