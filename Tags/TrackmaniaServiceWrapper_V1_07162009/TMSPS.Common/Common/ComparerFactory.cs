using System;
using System.Collections.Generic;

namespace TMSPS.Core.Common
{
    /// <summary>
    /// A class used to create Comparers
    /// </summary>
    public static class ComparerFactory
    {
        /// <summary>
        /// Creates a comparer using the provided Comparison method
        /// </summary>
        /// <typeparam name="T">Type of class to compare</typeparam>
        /// <param name="delComparisonMethod">The comparison method.</param>
        /// <returns>A Comparer</returns>
        public static Comparer<T> Create<T>(Comparison<T> delComparisonMethod)
        {
            return new MethodEnabledComparer<T>(delComparisonMethod);
        }
    }

    /// <summary>
    /// A Generic Comparer using a method provided in constructor to do the comparison
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class MethodEnabledComparer<T> : Comparer<T>
    {
        #region Non Public Members

        private readonly Comparison<T> _delComparisonMethod;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodEnabledComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="delComparisonMethod">The comparison method.</param>
        public MethodEnabledComparer(Comparison<T> delComparisonMethod)
        {
            if (delComparisonMethod == null)
                throw new ArgumentNullException("delComparisonMethod");

            _delComparisonMethod = delComparisonMethod;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// When overridden in a derived class, performs a comparison of two objects of the same type and returns a value indicating whether one object is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        /// Type <paramref name="{T}"/> does not implement either the <see cref="T:System.IComparable`1"/> generic interface or the <see cref="T:System.IComparable"/> interface.
        /// </exception>
        public override int Compare(T x, T y)
        {
            return _delComparisonMethod(x, y);
        }

        #endregion
    }
}
