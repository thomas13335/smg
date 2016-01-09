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

        public static ICondition Invert(this ICondition c)
        {
            return new InvertCondition(c);
        }

        public static ICondition ComposeUnion(this ICondition a, ICondition b)
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

        public static ICondition ComposeIntersection(this ICondition a, ICondition b)
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

    }
}
