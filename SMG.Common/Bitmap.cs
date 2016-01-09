using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    class Bitmap
    {
        private List<bool> _values = new List<bool>();

        public int Count { get { return _values.Count; } }

        public int NumberOfBitsSet { get { return _values.Count(e => e); } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("bitmap(");
            foreach(var v in _values)
            {
                sb.Append(v ? "1" : "0");
            }
            sb.Append(")");

            return sb.ToString();
        }

        public void SetBit(int index, bool value)
        {
            while(_values.Count <= index)
            {
                _values.Add(false);
            }

            _values[index] = value;

            while(_values.Count > 0 && !_values[_values.Count - 1])
            {
                _values.RemoveAt(_values.Count - 1);
            }
        }

        public bool GetBit(int index)
        {
            if(index < _values.Count)
            {
                return _values[index];
            }
            else
            {
                return false;
            }
        }

        public int GetNextBitSet(int start)
        {
            while (start < Count && !_values[start]) start++;

            return start < Count ? start : -1;
        }

        public static Bitmap CombineAND(Bitmap a, Bitmap b)
        {
            var r = new Bitmap();
            var n = Math.Min(a.Count, b.Count);

            for(int j = 0; j < n; ++j)
            {
                r.SetBit(j, a.GetBit(j) && b.GetBit(j));
            }

            return r;
        }
    }
}
