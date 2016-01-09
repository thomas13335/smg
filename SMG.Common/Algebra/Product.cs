using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Algebra
{
    /// <summary>
    /// Product (AND combination) of inputs, inverted and not inverted.
    /// </summary>
    /// <remarks>
    /// <para>This is basically a map from [Group] to [Factor].</para></remarks>
    public class Product
    {
        #region Private

        private bool _zero;
        private SortedList<int, Factor> _map = new SortedList<int, Factor>();

        #endregion

        #region Properties

        public IList<Factor> Factors { get { return _map.Values; } }

        public IEnumerable<IInput> Inputs { get { return Factors.SelectMany(e => e.Inputs); } }

        public bool IsEmpty { get { return 0 == _map.Count; } }

        #endregion

        public Product()
        { }

        public Product(Factor f)
        {
            AddFactor(f);
        }

        public Product Clone()
        {
            var p = new Product();
            foreach(var f in _map.Values)
            {
                p.AddFactor(f);
            }

            return p;
        }

        public void Clear()
        {
            _map.Clear();
        }

        public string Signature
        {
            get
            {
                //var n = _map.Keys.Max();
                int j = 0;
                var inputs = Inputs;
                var sb = new StringBuilder();

                foreach(var i in inputs)
                {
                    Debug.Assert(j <= i.Address);
                    while (j < i.Address)
                    {
                        sb.Append("0");
                        j++;
                    }

                    sb.Append(i.IsInverted ? "-" : "+");
                    ++j;
                }

                return sb.ToString();
            }
        }

        public Gate ToGate()
        {
            var and = new ANDGate();
            and.AddInputRange(Factors.SelectMany(e => e.ToGateList()));
            return and;
        }

        public override string ToString()
        {
            return ToGate().ToString();
        }

        #region Factors

        public bool ContainsFactor(IInput input)
        {
            return Inputs.Any(e => e.Address == input.Address);
        }

        public int ContainsFactor(Product seq)
        {
            int result = 0;

            var itseq = seq.Inputs.GetEnumerator();
            itseq.MoveNext();

            var itgg = this.Inputs.GetEnumerator();
            if (!itgg.MoveNext())
            {
                return -1;
            }

            do
            {
                while (itgg.Current.Address < itseq.Current.Address)
                {
                    if (!itgg.MoveNext())
                    {
                        return -1;
                    }
                }

                if (itgg.Current.Address != itseq.Current.Address)
                {
                    return -1;
                }

                // look at invertflag
                var anti = itseq.Current.IsInverted != itgg.Current.IsInverted;
                var flag = anti ? 2 : 1;
                if (result == 0)
                {
                    result = flag;
                }
                else if (result != flag)
                {
                    return -1;
                }
            }
            while (itseq.MoveNext());

            return result;
        }

        public IEnumerable<Product> GetPrimitiveFactors()
        {
            foreach (var f in Factors)
            {
                yield return new Product(f.Clone());
            }
        }

        #endregion

        public void RemoveInput(IInput i)
        {
            if (_map[i.Group].RemoveInput(i))
            {
                _map.Remove(i.Group);
            }
        }

        public void Remove(Product seq)
        {
            foreach (var i in seq.Inputs)
            {
                RemoveInput(i);
            }
        }

        public void AddFactor(Factor f1)
        {
            if(_zero)
            {
                // ignore
                return;
            }

            Factor f0;
            if (!_map.TryGetValue(f1.Group, out f0))
            {
                _map[f1.Group] = f1;
            }
            else
            {
                // factor for the same group already exists in the product
                f0.AddFactor(f1);
            }
        }

        public void Simplify()
        {
            foreach(var f in Factors)
            {
                f.Simplify();
            }
        }

        private void SetZero()
        {
            Clear();
            _zero = true;
        }
    }
}
