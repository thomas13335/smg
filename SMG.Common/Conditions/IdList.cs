using System;
using System.Collections.Generic;

namespace SMG.Common.Conditions
{
    /// <summary>
    /// List of identifiers or a wildcard.
    /// </summary>
    public class IdList : List<string>
    {
        /// <summary>
        /// If true, this is a wildcard.
        /// </summary>
        public bool Wildcard { get; set; }

        /// <summary>
        /// Creates a new empty identifier list.
        /// </summary>
        public IdList()
        { }

        public IdList(bool wild)
        {
            Wildcard = wild;
        }

        public IdList(params string[] namelist)
        {
            AddRange(namelist);
        }

        public static implicit operator IdList(string names)
        {
            var namelist = names.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new IdList(namelist);
        }

        public string ToNamespace()
        {
            return this.ToSeparatorList(".");
        }
    }
}
