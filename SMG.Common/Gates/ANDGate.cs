using SMG.Common.Algebra;
using SMG.Common.Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Gates
{
    /// <summary>
    /// Logical AND combination gate.
    /// </summary>
    class ANDGate : CompositeGate
    {
        #region Properties

        public override GateType Type
        {
            get { return GateType.AND; }
        }

        public override string SeparatorCode
        {
            get { return " && "; }
        }

        /// <summary>
        /// True if all incoming wires are Input gates.
        /// </summary>
        public bool IsProductOfInputs
        {
            get
            {
                return this.GetNestingLevel() < 2;
            }
        }

        #endregion

        #region Public Methods

        public override Product GetProduct()
        {
            if (!IsProductOfInputs)
            {
                // bad bad
                //Debug.WriteLine("PROBLEM:\n" + GateCache.Instance.ToDebugString());

                Debug.WriteLine("not a product of inputs: {0} level {1}", this, this.GetNestingLevel());

                throw new InvalidOperationException("SMG013: expected canonical for AND.");
            }

            var dict = new Product();
            foreach (var i in Inputs.Where(i => i.Type == GateType.Input).OfType<IInput>())
            {
                dict.AddFactor(i.CreateFactor());
            }

            return dict;
        }

        public override IGate Simplify()
        {
            IGate result = this;

            result = SimplifyNormalize(result);

            if(result.Type.IsFixed())
            {
                return result;
            }

            if (IsProductOfInputs)
            {
                result = SimplifyProductOfInputs(result.GetProduct());
            }
            else
            {
                result = SimplifyMultiplicate(result);
                result = Gate.Simplify(result);
            }


            Gate.TraceSimplify(this, result, "simplify AND");

            return result;
        }

        #endregion

        public static IGate SimplifyMultiplicate(IGate g)
        {
            Debug.Assert(g.Type == GateType.AND);

            var or = new ORGate();
            or.AddInput(new TrueGate());

            IGate r = or;

            foreach (var e in g.GetInputs())
            {
                if (e.Type == GateType.OR)
                {
                    r = Gate.Multiply(r.GetInputs(), e.GetInputs());
                }
                else
                {
                    r = Gate.Multiply(r.GetInputs(), e);
                }
            }

            r = Gate.Simplify(r);

            TraceSimplify(g, r, "simplify AND");

            return r;
        }

        #region Private Methods

        private IGate SimplifyNormalize(IGate gate)
        {
            var list = new List<IGate>();
            var inputs = gate.GetInputs();
            foreach (var e in gate.GetInputs())
            {
                if (e.Type == GateType.AND)
                {
                    list.AddRange(e.GetInputs());
                }
                else if (e.Type.IsFixed())
                {
                    if (e is FalseGate)
                    {
                        return e;
                    }
                }
                else
                {
                    list.Add(e);
                }
            }

            var and = new ANDGate();
            and.AddInputRange(list);

            return and;
        }

        private IGate SimplifyProductOfInputs(Product dict)
        {
            IGate result = null;

            var factors = new List<Factor>();

            foreach (var f in dict.Factors)
            {
                if (f.IsConstant)
                {
                    if (f.Constant is FalseGate)
                    {
                        result = f.Constant;
                        break;
                    }
                }
                else
                {
                    factors.Add(f);
                }
            }

            if (null == result)
            {
                var gatelist = factors.SelectMany(e => e.ToGateList()).GetEnumerator();
                if (!gatelist.MoveNext())
                {
                    result = new TrueGate();
                }
                else
                {
                    result = Gate.Simplify(gatelist.Current);
                    if (gatelist.MoveNext())
                    {
                        var and = new ANDGate();
                        and.AddInput(result);

                        do
                        {
                            var e = Gate.Simplify(gatelist.Current);
                            and.AddInput(e);
                        }
                        while (gatelist.MoveNext());

                        result = and;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
