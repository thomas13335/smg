using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Singleton object serving as a location to store simplified gates.
    /// </summary>
    public class GateCache
    {
        #region Private

        private static int _seed;
        private Dictionary<string, IGate> _map = new Dictionary<string, IGate>();
        private List<IGate> _list = new List<IGate>();
        private static Stack<GateCache> _stack = new Stack<GateCache>();
        private GateCache _parent;

        #endregion

        public static GateCache Instance { get { return _stack.Peek(); } }

        static GateCache()
        {
            _stack.Push(new GateCache(null));
        }

        public GateCache(GateCache parent)
        {
            _parent = parent;
        }

        public static void Push()
        {
            _stack.Push(new GateCache(Instance));
        }

        public static void Pop()
        {
            _stack.Pop();
        }

        public IGate AddGate(IGate g)
        {
            if (g.Type.IsFixed())
            {
                return g;
            }
            else
            {
                var gtext = g.ToString();

                // try to find, including parents
                IGate r = Lookup(gtext);

                if (null == r)
                {
                    // add to cache
                    var gateid = "#" + _seed++;
                    g.Freeze(gateid);

                    _map[gtext] = r = g;
                    _list.Add(r);
                }

                return r;
            }
        }

        public string ToDebugString()
        {
            var sb = new StringBuilder();
            foreach(var e in _list)
            {
                IGate original = null;
                if (e is LabelGate)
                {
                    original = ((LabelGate)e).OriginalGate;
                    sb.AppendFormat("  {0,-5} {1,-24} {2,-16} {3}", e.ID, e.GetType().Name, e, original);
                }
                else
                {
                    sb.AppendFormat("  {0,-5} {1,-24} {2}", e.ID, e.GetType().Name, e);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private IGate Lookup(string gtext)
        {
            IGate r;
            if(!_map.TryGetValue(gtext, out r) && null != _parent)
            {
                r = _parent.Lookup(gtext);
            }

            return r;
        }
    }
}
