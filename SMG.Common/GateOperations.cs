using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{
    static class GateOperations
    {
        public static IGate Simplify(this IGate gate)
        {
            return Gate.Simplify(gate);
        }

        public static bool IsCached(this IGate gate)
        {
            return null != gate.ID;
        }

        public static bool IsFalse(this IGate gate)
        {
            return gate is FalseGate;
        }

        public static IGate Replace(this IGate gate, Func<IGate, IGate> replacer)
        {
            IGate result;

            if (gate.Type.IsLogical())
            {
                result = gate.Clone();

                if (result is IModifyGate)
                {
                    var logic = (IModifyGate)result;
                    var inputs = result.GetInputs().ToList();

                    for (int j = 0; j < inputs.Count; ++j)
                    {
                        var input = inputs[j];
                        var r = input.Replace(replacer);
                        if (r != input)
                        {
                            logic.ReplaceInput(j, r);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("expected logic gate.");
                }
            }
            else
            {
                result = gate;
            }

            result = replacer(result);

            return result;
        }

        public static IEnumerable<IGate> SelectAll(this IGate gate, Predicate<IGate> pred = null)
        {
            foreach(var c in gate.GetInputs())
            {
                // c.Enumerate(action);
                foreach(var e in c.SelectAll(pred))
                {
                    yield return e;
                }
            }

            if (null == pred || pred(gate))
            {
                yield return gate;
            }
        }

        public static IEnumerable<IGate> GetInputs(this IGate gate)
        {
            if(gate is ILogicGate)
            {
                return ((ILogicGate)gate).Inputs;
            }
            else
            {
                return new IGate[0];
            }
        }

        public static void AddInput(this IGate gate, IGate a)
        {
            if (gate is IModifyGate)
            {
                ((IModifyGate)gate).AddInput(a);
            }
            else
            {
                throw new Exception("cannot add input to gate [" + gate.Type + "].");
            }

        }

        public static SumOfProducts GetSOP(this IGate garg)
        {
            var gset = new SumOfProducts();

            foreach (var i in garg.GetInputs())
            {
                if (i is TrueGate)
                {
                    gset.SetFixed(i);
                    break;
                }
                else if (i is FalseGate)
                {
                    // ignore
                }
                else
                {
                    gset.Add(i.GetProduct());
                }
            }

            return gset;
        }

        public static string GetOriginalID(this IGate gate)
        {
            if(gate is LabelGate)
            {
                return ((LabelGate)gate).OriginalGate.ID;
            }
            else
            {
                return gate.ID;
            }
        }

        public static IGate GetOriginalGate(this IGate gate)
        {
            if (gate is LabelGate)
            {
                return ((LabelGate)gate).OriginalGate;
            }
            else
            {
                return gate;
            }
        }

        public static int GetNestingLevel(this IGate gate)
        {
            if(gate.Type.IsLogical())
            {
                if (gate.GetInputs().Any())
                {
                    return 1 + gate.GetInputs().Max(g => g.GetNestingLevel());
                }
                else
                {
                    // possibly too much nesting here.
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
