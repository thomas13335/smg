using SMG.Common.Transitions;
using System;
using System.Collections.Generic;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// Static operations and extensions on the ICondition interface.
    /// </summary>
    public static class ConditionOperations
    {
        /// <summary>
        /// Calls a replace handler recursively.
        /// </summary>
        /// <param name="c">The condition object to convert.</param>
        /// <param name="replacer">The replace method.</param>
        /// <returns>The converted object.</returns>
        /// <remarks>
        /// <para>This method leaves the original condition intact.</para>
        /// </remarks>
        public static ICondition Replace(this ICondition c, Func<ICondition, ICondition> replacer)
        {
            var r = c.Clone();

            for(int j = 0; j < r.Elements.Count; ++j)
            {
                r.Elements[j] = replacer(r.Elements[j]);
            }

            return replacer(r);
        }

        /// <summary>
        /// Creates a condition that represents the negation of the argument.
        /// </summary>
        /// <param name="c">The condition to invert.</param>
        /// <returns>The inverted condition.</returns>
        public static ICondition Invert(this ICondition c)
        {
            return new InvertCondition(c);
        }

        /// <summary>
        /// Creates a condition that represents the union (sum) of two conditions.
        /// </summary>
        /// <param name="a">Argument condition.</param>
        /// <param name="b">Argument condition.</param>
        /// <returns>The union condition.</returns>
        public static ICondition Union(this ICondition a, ICondition b)
        {
            var list = new List<ICondition>();

            foreach (var e in new[] { a, b })
            {
                if (e is UnionCondition)
                {
                    list.AddRange(e.Elements);
                }
                else
                {
                    list.Add(e);
                }
            }

            return new UnionCondition(list);
        }

        /// <summary>
        /// Creates a condition that represents the intersection (product) of two conditions.
        /// </summary>
        /// <param name="a">Argument condition.</param>
        /// <param name="b">Argument condition.</param>
        /// <returns>The resulting intersection condition.</returns>
        public static ICondition Intersection(this ICondition a, ICondition b)
        {
            var list = new List<ICondition>();

            foreach (var e in new[] { a, b })
            {
                if (e is IntersectCondition)
                {
                    list.AddRange(e.Elements);
                }
                else
                {
                    list.Add(e);
                }
            }

            return new IntersectCondition(list);
        }

        public static bool ContainsTransitions(this ICondition c)
        {
            var tset = new TransitionSet(c);
            return !tset.IsEmpty;
        }

        public static IGate Decompose(this IVariableCondition vc)
        {
            return vc.CreateElementaryCondition(vc.StateIndex);
        }
    }
}
