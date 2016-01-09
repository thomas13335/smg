using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Algebra
{
    class SumOfProducts : List<Product>
    {
        public IGate Fixed { get; set; }

        public IEnumerable<Product> GetPrimitiveFactors()
        {
            foreach(var p in this)
            {
                foreach(var f in p.GetPrimitiveFactors())
                {
                    yield return f;
                }
            }
        }

        internal void SetFixed(IGate gate)
        {
            Fixed = gate;
        }

        public void Purge()
        {
            foreach(var p in this.ToList())
            {
                if(p.IsEmpty)
                {
                    this.Remove(p);
                }
            }
        }
    }
}
