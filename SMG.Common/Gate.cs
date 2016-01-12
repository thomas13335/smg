using SMG.Common.Algebra;
using SMG.Common.Code;
using SMG.Common.Exceptions;
using SMG.Common.Gates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMG.Common
{

    public enum ComposeOptions
    {
        None,
        NoSimplify
    }

    /// <summary>
    /// A truth value derived from inputs or logical combinations thereof.
    /// </summary>
    public abstract class Gate : IGate
    {
        #region Private

        private string _id;

        #endregion

        #region Properties

        public abstract GateType Type { get; }

        public string ID { get { return _id; } }

        public bool IsFrozen { get { return null != _id; } }

        public virtual string SeparatorOperator { get { return string.Empty; } }

        public string CacheKey { get { return ToString(); } }

        #endregion

        #region Construction

        public Gate()
        {
        }

        public virtual IGate Clone()
        {
            var result = (Gate)Activator.CreateInstance(GetType());
            if(result is IModifyGate)
            {
                ((IModifyGate)result).AddInputRange(this.GetInputs());
            }
            return result;
        }

        #endregion

        #region Diagnostics

        protected static void Trace(string format, params object[] args)
        {
            Log.Trace(format, args);
        }

        public static void TraceLabel(IGate a, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowLabel)
            {
                Log.TraceGateOp2(a, r, format, args);
            }
        }

        public static void TraceDependencies(IGate a, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowDepencencyAnalysis)
            {
                Log.TraceGateOp2(a, r, format, args);
            }
        }

        public static void TraceCompose(IGate a, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowCompose)
            {
                Log.TraceGateOp2(a, r, format, args);
            }
        }

        public static void TraceCompose(IGate a, IGate b, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowCompose)
            {
                Log.TraceGateOp3(a, b, r, format, args);
            }
        }

        public static void TraceDecompose(object a, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowDecompose)
            {
                Log.TraceGateOp2(a, r, format, args);
            }
        }

        protected static void TraceSimplify(IGate a, IGate r, string format, params object[] args)
        {
            if (TraceFlags.ShowSimplify)
            {
                Log.TraceGateOp2(a, r, format, args);
            }
        }

        protected static void TraceSimplify(string format, params object[] args)
        {
            if (TraceFlags.ShowSimplify)
            {
                // Debug.WriteLine(format, args);
            }
        }

        protected static void TraceOptimize(string format, params object[] args)
        {
            Debug.WriteLine("   " + format, args);
        }

        #endregion

        #region Overrideable Methods

        /*public virtual IGate Simplify()
        {
            return this;
        }*/

        public virtual Product GetProduct()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected Methods

        protected void ValidateCanModify()
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException("frozen gate cannot be modified.");
            }
        }

        #endregion

        #region IGate

        public virtual void Freeze(string gateid)
        {
            if (null != _id)
            {
                throw new InvalidOperationException("gate identifier already assigned.");
            }

            _id = gateid;
        }

        /// <summary>
        /// Emits the gate into a code writer.
        /// </summary>
        /// <param name="writer"></param>
        public virtual void Emit(ICodeGateEvaluator writer)
        {
            if(Type.IsFixed())
            {
                writer.Append(this.ToString());
                return;
            }

            Debug.Assert(Type.IsLogical());
            var first = true;
            foreach(var i in this.GetInputs())
            {
                if(first)
                {
                    first = false;
                }
                else
                {
                    writer.EmitBinaryOperator(Type);
                }

                var brackets = i.Type.IsLogical() && Type != i.Type;
                if(brackets)
                {
                    writer.Append("(");
                }

                i.Emit(writer);

                if (brackets)
                {
                    writer.Append(")");
                }
            }
        }

        #endregion

        #region Gate Factory

        public static IGate Constant(bool value)
        {
            if(value)
            {
                return new TrueGate();
            }
            else
            {
                return new FalseGate();
            }
        }

        /// <summary>
        /// Returns a gate that represents the inverted output of another gate.
        /// </summary>
        /// <param name="a">The gate to be inverted.</param>
        /// <returns>The inverted gate.</returns>
        public static IGate Invert(IGate g)
        {
            IGate result;
            IGate a = Decompose(g);

            if (a.Type == GateType.OR)
            {
                var it = a.GetInputs().GetEnumerator();
                it.MoveNext();
                var r = Invert(it.Current);
                while (it.MoveNext())
                {
                    r = ComposeAND(r, Invert(it.Current));
                }

                result = r;
            }
            else if (a.Type == GateType.AND)
            {
                var it = a.GetInputs().GetEnumerator();
                it.MoveNext();
                var r = Invert(it.Current);
                while (it.MoveNext())
                {
                    r = ComposeOR(r, Invert(it.Current));
                }

                result = r;
            }
            else if (a.Type == GateType.Input)
            {
                result = ((IInput)a).Invert();
            }
            else if(a.Type == GateType.Fixed)
            {
                if(a is TrueGate)
                {
                    result = new FalseGate();
                }
                else
                {
                    result = new TrueGate();
                }
            }
            else
            {
                throw new ArgumentException();
            }

            result = Simplify(result);

            TraceCompose(g, result, "invert");

            return result;
        }

        /// <summary>
        /// Composes two gates with the logical AND operator.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The resulting gate.</returns>
        public static IGate ComposeAND(IGate a, IGate b)
        {
            IGate result;

            DecomposeExchange(ref a, ref b);

            if(a.Type == GateType.Fixed && b.Type == GateType.Fixed)
            {
                if(a is TrueGate && b is TrueGate)
                {
                    result = new TrueGate();
                }
                else
                {
                    result = new FalseGate();
                }
            }
            else if(b.Type == GateType.Fixed)
            {
                if(b is TrueGate)
                {
                    result = a;
                }
                else
                {
                    result = new FalseGate();
                }
            }
            else if (a.Type == GateType.OR && b.Type == GateType.OR)
            {
                result = Multiply(a.GetInputs(), b.GetInputs());
            }
            else if (a.Type == GateType.OR)
            {
                result = Multiply(a.GetInputs(), b);
            }
            else if (a.Type == GateType.AND && b.Type == GateType.AND)
            {
                // compose 
                var r = new ANDGate();
                r.AddInputRange(a.GetInputs());
                r.AddInputRange(b.GetInputs());
                result = r;
            }
            else if (a.Type == GateType.AND)
            {
                // compose 
                var r = new ANDGate();
                r.AddInputRange(a.GetInputs());
                r.AddInput(b);
                result = r;
            }
            else
            {
                var r = new ANDGate();
                r.AddInput(a);
                r.AddInput(b);
                result = r;
            }

            // simplify and cache
            result = Simplify(result);

            TraceCompose(a, b, result, "AND");

            return result;
        }

        public static IGate ComposeAND(IEnumerable<IGate> gates)
        {
            IGate r = new TrueGate();
            var it = gates.GetEnumerator();
            while (it.MoveNext())
            {
                r = ComposeAND(r, it.Current);
            }

            return r;
        }

        /// <summary>
        /// Composes two gates with the logical OR operator.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The resulting gate.</returns>
        public static IGate ComposeOR(IGate a, IGate b, ComposeOptions options = ComposeOptions.None)
        {
            IGate result;

            DecomposeExchange(ref a, ref b);

            if (a.Type == GateType.OR && b.Type == GateType.OR)
            {
                var r = new ORGate();
                r.AddInputRange(a.GetInputs());
                r.AddInputRange(b.GetInputs());
                result = r;
            }
            else if (a.Type == GateType.OR)
            {
                var r = new ORGate();
                r.AddInputRange(a.GetInputs());
                r.AddInput(b);
                result = r;
            }
            else
            {
                var r = new ORGate();
                r.AddInput(a);
                r.AddInput(b);
                result = r;
            }

            if (0 == (options & ComposeOptions.NoSimplify))
            {
                result = Simplify(result);
            }

            TraceCompose(a, b, result, "OR");

            return result;
        }

        public static IGate ComposeOR(IEnumerable<IGate> gates)
        {
            IGate r = new FalseGate();
            var it = gates.GetEnumerator();
            while (it.MoveNext())
            {
                r = ComposeOR(r, it.Current);
            }

            return r;
        }

        /// <summary>
        /// Composes two gates given the operator as a parameter.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The resulting gate.</returns>
        public static IGate Compose(GateType type, IGate a, IGate b)
        {
            switch(type)
            {
                case GateType.OR:
                    return ComposeOR(a, b);

                case GateType.AND:
                    return ComposeAND(a, b);

                default:
                    throw new ArgumentException();
            }
        }

        public static IGate Compose(GateType type, IEnumerable<IGate> gates)
        {
            switch (type)
            {
                case GateType.OR:
                    return ComposeOR(gates);

                case GateType.AND:
                    return ComposeAND(gates);

                default:
                    throw new ArgumentException();
            }
        }

        #endregion

        #region Simplification

        public static IGate Simplify(IGate gate)
        {
            IGate result;
            if(null != gate.ID)
            {
                return gate;
            }


            if(gate is ILogicGate)
            {
                result = ((ILogicGate)gate).Simplify();
            }
            else
            {
                result = gate;
            }

            result = GateCache.Instance.AddGate(result);

            return result;
        }

        public static IGate ExtractCommonFactors(IGate gate)
        {
            var original = gate;

            if (gate.Type == GateType.OR)
            {
                // TraceOptimize("extract common factors from {0} ...", gate);
                var sop = gate.GetSOP();

                // count how many times each factor appears
                var inmap = new Dictionary<int, IInput>();
                var dict = new SortedList<int, int>();

                foreach (var p in sop.GetPrimitiveFactors())
                {
                    // a gate representing the factors (may be multiple per state variable)
                    var pg = p.ToGate();

                    foreach (var i in pg.GetInputs().OfType<IInput>())
                    {
                        var address = i.Address;
                        // TraceOptimize("check factor {0} @ {1:X6}", i, address);

                        if (!inmap.ContainsKey(i.Address))
                        {
                            inmap.Add(i.Address, i);
                        }

                        if (!dict.ContainsKey(address))
                        {
                            dict[address] = 1;
                        }
                        else
                        {
                            dict[address]++;
                        }

                    }

                }

                var m = dict.Values.Max();
                // TraceOptimize("maximum factor count {0}", m);

                if (m > 1)
                {
                    // go for it, take the first input with maximum multiplicity, inputs are ordered.
                    var pivotindex = dict.Where(e => e.Value == m).Select(e => e.Key).First();
                    var pivot = inmap[pivotindex];

                    var pivotlist = new List<Product>();
                    var otherlist = new List<Product>();

                    TraceOptimize("use pivot {0:X6} ...", pivot);

                    // split sop into two groups: factor or not
                    foreach (var p in sop)
                    {
                        if (p.ContainsFactor(pivot))
                        {
                            p.RemoveInput(pivot);
                            pivotlist.Add(p);
                        }
                        else
                        {
                            otherlist.Add(p);
                        }
                    }

                    IGate and = new ANDGate();
                    and.AddInput(pivot);

                    IGate inneror = new ORGate();
                    foreach (var p in pivotlist)
                    {
                        var z = p.ToGate().Simplify();
                        // Debug.Assert(z.GetInputs().Count() > 1);

                        Trace("adding pivot {0} [{1}]", z, z.GetType().Name);

                        inneror.AddInput(z);
                    }

                    inneror = ExtractCommonFactors(inneror);

                    and.AddInput(inneror);

                    

                    if (otherlist.Any())
                    {
                        //var rh = ExtractCommonFactors(otherlist);

                        var or = new ORGate();
                        or.AddInput(and);

                        foreach (var p in otherlist)
                        {
                            var z = p.ToGate();
                            or.AddInput(z.Simplify());
                        }

                        gate = or;
                    }
                    else
                    {
                        gate = and;
                    }
                }
            }

            if (gate != original && TraceFlags.ShowOptimize)
            {
                Log.TraceGateOp2(original, gate, "optimize AND");
            }

            return gate;
        }

        #endregion

        #region Private Methods

        public static IGate Decompose(IGate a)
        {
            IGate r;

            if(a is IDecomposableCondition)
            {
                r = ((IDecomposableCondition)a).Decompose();
                // Trace("decomposing {0} -> {1}", a, r);
            }
            else
            {
                r = a;
            }

            return r;
        }

        private static void DecomposeExchange(ref IGate a, ref IGate b)
        {
            a = Decompose(a);
            b = Decompose(b);

            if (a.Type < b.Type)
            {
                var t = a;
                a = b;
                b = t;
            }
        }

        /// <summary>
        /// Multiplies a sum of products with a gate other than an OR gate.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IGate Multiply(IEnumerable<IGate> list, IGate b)
        {
            var r = new ORGate();
            foreach(var a in list)
            {
                r.AddInput(ComposeAND(a, b));
            }

            return r;
        }

        /// <summary>
        /// Multiplies two sums of products
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static IGate Multiply(IEnumerable<IGate> list1, IEnumerable<IGate> list2)
        {
            var r = new ORGate();
            foreach (var a in list1)
            {
                foreach (var b in list2)
                {
                    r.AddInput(ComposeAND(a, b));
                }
            }

            return r;
        }

        #endregion

    }
}
