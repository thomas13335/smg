using SMG.Common.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common.Gates
{
    /// <summary>
    /// Logical OR combination gate.
    /// </summary>
    class ORGate : CompositeGate
    {
        #region Properties

        public override GateType Type
        {
            get { return GateType.OR; }
        }

        public override string SeparatorOperator
        {
            get { return " + "; }
        }

        public override string SeparatorCode
        {
            get { return " || "; }
        }

        #endregion

        #region Public Methods

        public override IGate Simplify()
        {
            IGate gate = this;

            SimplifyConstants(ref gate);
            SimplifyRule2(ref gate);

            while(SimplifyRule1(ref gate))
            {
                SimplifyRule2(ref gate);
            }

            Gate.TraceSimplify(this, gate, "simplify OR (0)");

            return gate;
        }

        #endregion

        #region Private Methods

        private bool SimplifyRule1(ref IGate gate)
        {
            var original = gate;

            var garg = gate as ORGate;
            if(null == garg) return false;

            // handle the rules:
            // A + !A B = A + B
            // A + A B = A
            var gset = garg.GetSOP();

            int changes = 0;

            var gplan = gset.OrderBy(e => e.Inputs.Count()).ToList();
            for (int j = 0; j < gplan.Count; ++j)
            {
                var glist = gplan[j];
                if (glist.IsEmpty) continue;

                TraceSimplify("  product [{0}] : {1}", glist.Signature, glist.ToGate());

                for (int k = j + 1; k < gplan.Count; ++k)
                {
                    var olist = gplan[k];
                    switch (olist.ContainsFactor(glist))
                    {
                        case 1:
                            // contains the sequence => eliminate entry?

                            TraceSimplify("    is contained in {0} ...", olist.ToGate());

                            olist.Clear();
                            changes++;
                            break;

                        case 2:
                            // contains negation => remove negations

                            TraceSimplify("    negation is contained in {0} ...", olist.ToGate());

                            olist.Remove(glist);
                            if(olist.IsEmpty)
                            {
                                // full negation ==> ALL
                                glist.Clear();
                                gset.Fixed = new TrueGate();
                            }
                            changes++;
                            break;
                    }
                }
            }

            gset.Purge();

            if (gset.Fixed != null)
            {
                gate = gset.Fixed;
            }
            else
            {
                // construct an OR expression containing the ordered factors
                var or = new ORGate();
                foreach (var glist in gset.OrderBy(e => e.Signature, StringComparer.Ordinal))
                {
                    or.AddInput(glist.ToGate().Simplify());
                }

                if (or.Inputs.Count() == 0)
                {
                    gate = new FalseGate();
                }
                else if (or.Inputs.Count() == 1)
                {
                    gate = or.Inputs.First();
                }
                else
                {
                    gate = or;
                }
            }

            if (changes > 0)
            {
                TraceSimplify(original, gate, "simplify OR (1)");
            }

            return changes > 0;
        }

        /// <summary>
        /// Looks for common subfactors.
        /// </summary>
        /// <param name="gate"></param>
        private bool SimplifyRule2(ref IGate gate)
        {
            var originalgate = gate;

            var gset = gate.GetSOP();

            // try all elementary factors ...
            foreach (var elementaryfactor in gset.GetPrimitiveFactors().ToArray())
            {
                // Trace("trying factor {0}", elementaryfactor);

                // collect all products that contain the factor as candidates
                var candidates = new List<Product>();
                foreach (var p in gset)
                {
                    if (1 == p.ContainsFactor(elementaryfactor))
                    {
                        candidates.Add(p);
                    }
                }

                // need at least two
                if (candidates.Count < 2)
                {
                    continue;
                }

                TraceSimplify("common factor {0} found in {1} ...", elementaryfactor, candidates.ToSeparatorList());

                // construct a new gate consisting of the sum of remaining products
                var reducedsum = new ORGate();
                Gate newgate = reducedsum;

                foreach (var involvedproduct in candidates)
                {
                    var reducedproduct = involvedproduct.Clone();
                    reducedproduct.Remove(elementaryfactor);
                    if (reducedproduct.Factors.Any())
                    {
                        reducedsum.AddInput(reducedproduct.ToGate());
                    }
                    else
                    {
                        // candidate equals elementary factor => TRUE
                        newgate = new TrueGate();
                        break;
                    }
                }

                // remove original elements from the set
                foreach (var x in candidates)
                {
                    gset.Remove(x);
                }

                // simplify the resulting gate recursively
                var partialsum = newgate.Simplify();

                var factor = elementaryfactor.ToGate();

                TraceSimplify("  is ({0}) AND {1}", partialsum, factor);

                if (partialsum.Type == GateType.Fixed)
                {
                    if (partialsum is TrueGate)
                    {
                        // factor only
                        gate = factor;
                    }
                    else
                    {
                        // simplification yields FALSE
                        gate = partialsum;
                    }
                }
                else if (partialsum.Type == GateType.AND)
                {
                    // multiply with factor
                    gate = Gate.Multiply(factor.GetInputs(), partialsum);
                }
                else if (partialsum.Type == GateType.OR)
                {
                    // multiply with all factors
                    gate = Gate.Multiply(factor.GetInputs(), partialsum.GetInputs());
                }
                else
                {
                    // multiply with factor
                    gate = Gate.Multiply(factor.GetInputs(), partialsum);
                }

                // compose with the remaining terms
                foreach (var term in gset.Select(e => e.ToGate()))
                {
                    gate = Gate.ComposeOR(gate, term, ComposeOptions.NoSimplify);
                }
                break;
            }

            if (gate != originalgate)
            {
                Gate.TraceSimplify(originalgate, gate, "simplify OR (2)");
            }

            return gate != originalgate;
        }


        private static void SimplifyConstants(ref IGate gate)
        {
            var inputs = gate.GetInputs();
            var list = new List<IGate>();

            foreach (var i in inputs)
            {
                if (i is TrueGate)
                {
                    // sum is constant => TRUE
                    gate = new TrueGate();
                    return;
                }
                else if (i is FalseGate)
                {
                    // remove any FALSE gates ...
                }
                else if (i.GetNestingLevel() > 1)
                {
                    var r = i.Simplify();
                    if (r.Type == GateType.OR)
                    {
                        list.AddRange(r.GetInputs());
                    }
                    else
                    {
                        list.Add(r);
                    }
                }
                else
                {
                    var r = i.Simplify();
                    list.Add(r);
                }
            }

            gate = ConsolidateList(list);
        }

        private static IGate ConsolidateList(List<IGate> list)
        {
            IGate gate;

            if (!list.Any())
            {
                // empty sum => FALSE
                gate = new FalseGate();
            }
            else if (list.Count == 1)
            {
                gate = list.First();
            }
            else
            {
                var or = new ORGate();
                or.AddInputRange(list);
                gate = or;
            }

            return gate;
        }

        #endregion
    }
}
