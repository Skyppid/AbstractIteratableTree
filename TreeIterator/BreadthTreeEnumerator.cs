using System.Collections;
using System.Collections.Generic;

namespace TreeIterator
{
    ///=================================================================================================
    /// <summary>   A breadth tree enumerator. </summary>
    ///
    /// <seealso cref="T:System.Collections.Generic.IEnumerator{TreeIterator.TreeBranch}"/>
    ///=================================================================================================
    public class BreadthTreeEnumerator : IEnumerator<TreeBranch>
    {
        private int _currentIndex = -1;

        protected TreeBranch Leaf { get; set; }

        protected BreadthTreeEnumerator SubEnumerator { get; private set; }

        private BreadthTreeEnumerator ParentEnumerator { get; set; }


        ///=================================================================================================
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="leaf">     The leaf. </param>
        /// <param name="parent">   The parent. </param>
        ///=================================================================================================
        public BreadthTreeEnumerator(TreeBranch leaf, BreadthTreeEnumerator parent)
        {
            Leaf = leaf;
            ParentEnumerator = parent;
        }

        ///=================================================================================================
        /// <summary>   Determines if we can move next. </summary>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        public bool MoveNext()
        {
            if (SubEnumerator != null) return SubEnumerator.MoveNext();

            // First iterate through  all branches on the same level
            if (Leaf.Branches.Count > _currentIndex + 1)
            {
                _currentIndex++;
                return true;
            }

            // This level has ben enumerated, so go back to the first element and go one level deeper
            _currentIndex = -1;
            return PointToNext();
        }

        ///=================================================================================================
        /// <summary>   Determines if we can point to next. </summary>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        private bool PointToNext()
        {
            int start = _currentIndex < 0 ? 0 : _currentIndex;
            for (_currentIndex = start + 1; _currentIndex < Leaf.Branches.Count; _currentIndex++)
            {
                if (Leaf.Branches[_currentIndex].Branches.Count <= 0) continue;
                SubEnumerator = new BreadthTreeEnumerator(Leaf.Branches[_currentIndex], this) { _currentIndex = 0 };
                return true;
            }

            SubEnumerator = null;
            return ParentEnumerator != null && ParentEnumerator.PointToNext();
        }

        /// <summary>   Resets this instance. </summary>
        public void Reset()
        {
            _currentIndex = -1;
            SubEnumerator = null;
        }

        /// <summary>   Gets the element in the collection at the current position of the enumerator. </summary>
        public TreeBranch Current => SubEnumerator != null ? SubEnumerator.Current : Leaf.Branches[_currentIndex];

        /// <summary>   Gets the element in the collection at the current position of the enumerator. </summary>
        object IEnumerator.Current => Current;

        ///=================================================================================================
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        ///=================================================================================================
        public void Dispose()
        {
            SubEnumerator?.Dispose();
            SubEnumerator = null;
            ParentEnumerator = null;
        }
    }

    ///=================================================================================================
    /// <summary>   A breadth tree enumerator. </summary>
    ///
    /// <typeparam name="T">    Generic type parameter. </typeparam>
    ///
    /// <seealso cref="T:TreeIterator.BreadthTreeEnumerator"/>
    /// <seealso cref="T:System.Collections.Generic.IEnumerator{T}"/>
    ///=================================================================================================
    public class BreadthTreeEnumerator<T> : BreadthTreeEnumerator, IEnumerator<T> where T : TreeBranch
    {
        ///=================================================================================================
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="leaf">     The leaf. </param>
        /// <param name="parent">   The parent. </param>
        ///=================================================================================================
        public BreadthTreeEnumerator(TreeBranch leaf, BreadthTreeEnumerator parent) : base(leaf, parent)
        {
        }

        /// <summary>   Gets the element in the collection at the current position of the enumerator. </summary>
        public new T Current => (T)base.Current;
    }
}