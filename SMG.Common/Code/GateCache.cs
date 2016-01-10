using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMG.Common.Code
{
    /// <summary>
    /// Stackable singleton object serving as a location to store simplified gates.
    /// </summary>
    /// <remarks>
    /// <para>Assigns a unique identifier to each gate cached.</para></remarks>
    public class GateCache
    {
        #region Private

        class CacheStack : Stack<GateCache>
        {
            public CacheStack()
            {
                Push(new GateCache(null));
            }
        }

        private static ThreadLocal<CacheStack> _stack = new ThreadLocal<CacheStack>(() => new CacheStack());

        private static int _seed;
        private Dictionary<string, IGate> _map = new Dictionary<string, IGate>();
        private List<IGate> _list = new List<IGate>();
        private GateCache _parent;

        #endregion

        public static GateCache Instance { get { return _stack.Value.Peek(); } }

        public GateCache(GateCache parent)
        {
            _parent = parent;
        }

        public static void Push()
        {
            _stack.Value.Push(new GateCache(Instance));
        }

        public static void Pop()
        {
            _stack.Value.Pop();
        }

        public void Purge()
        {
            _map.Clear();
            _list.Clear();
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
