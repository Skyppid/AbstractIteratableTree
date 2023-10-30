using System;
using System.Runtime.Serialization;

namespace TreeIterator
{
    ///=================================================================================================
    /// <summary>   A file system tree. This class cannot be inherited. </summary>
    ///
    /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
    ///
    /// <seealso cref="T:TreeIterator.Tree{TreeIterator.FileSystemTreeBranch}"/>
    ///=================================================================================================
    [Serializable]
    public sealed class FileSystemTree : Tree<FileSystemTreeBranch>
    {
        public FileSystemTree() : base(new FileSystemTreeBranch("Root", true))
        {
        }

        ///=================================================================================================
        /// <summary>   Serialization constructor. </summary>
        ///
        /// <param name="info">     The information. </param>
        /// <param name="context">  The context. </param>
        ///=================================================================================================
        private FileSystemTree(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        ///=================================================================================================
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
        ///
        /// <param name="root"> The root. </param>
        ///=================================================================================================
        public FileSystemTree(FileSystemTreeBranch root) : base(root)
        {
        }
    }
}
