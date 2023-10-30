using System.Collections;
using System.Collections.Generic;

namespace TreeIterator
{
    ///=================================================================================================
    /// <summary>   A tree enumerator. </summary>
    ///
    /// <seealso cref="T:System.Collections.Generic.IEnumerator{TreeIterator.TreeBranch}"/>
    ///=================================================================================================
    public class DepthTreeEnumerator : IEnumerator<TreeBranch>
    {
        protected TreeBranch Leaf { get; set; }
        protected DepthTreeEnumerator SubEnumerator { get; private set; }
        private DepthTreeEnumerator ParentEnumerator { get; set; }
        private int _currentIndex;

        public DepthTreeEnumerator(TreeBranch leaf, DepthTreeEnumerator parent)
        {
            Leaf = leaf;
            ParentEnumerator = parent;
        }

        ///=================================================================================================
        /// <summary>   Determines if the enumerator can move to the next element. </summary>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        public bool MoveNext()
        {
            if (SubEnumerator != null) return SubEnumerator.MoveNext();

            // Has no childs, kill parent subenumerator to indicate end of level
            if (Current.Branches.Count == 0) return ParentEnumerator.IndicateEndOfLevel();

            // Has childs so go one level deeper
            SubEnumerator = new DepthTreeEnumerator(Current.Branches[0], this);
            return true;
        }

        ///=================================================================================================
        /// <summary>   Bubbles up and continues at the next element or ends the enumeration. </summary>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        private bool IndicateEndOfLevel()
        {
            SubEnumerator = null;
            // Go to next branch if another exists
            if (Leaf.Branches.Count <= _currentIndex + 1) return ParentEnumerator != null && ParentEnumerator.IndicateEndOfLevel();
            SubEnumerator = new DepthTreeEnumerator(Leaf.Branches[++_currentIndex], this);
            return true;
        }

        /// <summary>   Resets this instance. </summary>
        public void Reset()
        {
            _currentIndex = 0;
            SubEnumerator?.Dispose();
            SubEnumerator = null;
        }

        /// <summary>   Current element the enumerator points at. </summary>
        public TreeBranch Current => SubEnumerator != null ? SubEnumerator.Current : Leaf;

        /// <summary>   Current element the enumerator points at. </summary>
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            SubEnumerator?.Dispose();
            SubEnumerator = null;
            ParentEnumerator = null;
        }
    }

    ///=================================================================================================
    /// <summary>   A generic tree enumerator. This class cannot be inherited. </summary>
    ///
    /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
    ///
    /// <typeparam name="TBranch">  Type of the branch. </typeparam>
    ///
    /// <seealso cref="T:System.Collections.Generic.IEnumerator{TBranch}"/>
    ///=================================================================================================
    public sealed class DepthTreeEnumerator<TBranch> : DepthTreeEnumerator, IEnumerator<TBranch> where TBranch : TreeBranch
    {
        public DepthTreeEnumerator(TBranch leaf, DepthTreeEnumerator parent) : base(leaf, parent)
        {
        }

        public new TBranch Current => (TBranch) base.Current;
    }
}
