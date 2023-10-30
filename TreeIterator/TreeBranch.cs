using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;

namespace TreeIterator
{
    ///=================================================================================================
    /// <summary>   A tree branch. </summary>
    ///
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{TreeIterator.TreeBranch}"/>
    ///=================================================================================================
    [Serializable]
    public abstract class TreeBranch : IEnumerable<TreeBranch>, ISerializable
    {
        /// <summary>   List of branches. </summary>
        protected List<TreeBranch> BranchList;

        ///=================================================================================================
        /// <summary>   Gets or sets the tree. </summary>
        ///
        /// <value> The tree. </value>
        ///=================================================================================================
        public Tree Tree { get; internal set; }

        ///=================================================================================================
        /// <summary>   Gets or sets the parent. </summary>
        ///
        /// <value> The parent. </value>
        ///=================================================================================================
        public TreeBranch Parent { get; private set; }

        ///=================================================================================================
        /// <summary>   Gets or sets the enumeration mode. </summary>
        ///
        /// <value> The enumeration mode. </value>
        ///=================================================================================================
        public TreeEnumerationMode EnumerationMode { get; set; }

        /// <summary>   The branches. </summary>
        public IReadOnlyList<TreeBranch> Branches => BranchList.AsReadOnly();

        ///=================================================================================================
        /// <summary>   Specialised default constructor for use only by derived class. </summary>
        ///
        /// <param name="mode"> The mode used for enumeration. </param>
        ///=================================================================================================
        protected TreeBranch(TreeEnumerationMode mode = TreeEnumerationMode.DepthFirst)
        {
            BranchList = new();
            EnumerationMode = mode;
        }

        ///=================================================================================================
        /// <summary>   Adds a branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///=================================================================================================
        public void AddBranch(TreeBranch branch)
        {
            if (!BranchList.Contains(branch))
            {
                BranchList.Add(branch);
                branch.Parent = this;
                branch.Tree = Tree;
            }
        }

        ///=================================================================================================
        /// <summary>   Removes the branch described by branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        public bool RemoveBranch(TreeBranch branch)
        {
            return BranchList.Remove(branch);
        }

        ///=================================================================================================
        /// <summary>   Dumps the branch and it's sub-branches to the given file. </summary>
        ///
        /// <param name="file"> The dump file. </param>
        ///=================================================================================================
        public void Dump(string file)
        {
            using FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
            using StreamWriter writer = new StreamWriter(fs);
            Dump(writer, 0);
        }

        ///=================================================================================================
        /// <summary>   Dumps the branch and it's sub-branches to the given file. </summary>
        ///
        /// <param name="writer">   The writer. </param>
        /// <param name="level">    The level. </param>
        ///=================================================================================================
        public abstract void Dump(StreamWriter writer, int level);

        ///=================================================================================================
        /// <summary>
        ///     Returns a string of tab chars '\t' depending on the level (for dump file structure).
        /// </summary>
        ///
        /// <param name="level">    The level. </param>
        ///
        /// <returns>   A string. </returns>
        ///=================================================================================================
        protected string TabFromLevel(int level)
        {
            string tabs = "";
            for (int i = 1; i <= level; i++)
                tabs += '\t';
            return tabs;
        }

        ///=================================================================================================
        /// <summary>   Returns an enumerator that iterates through the collection. </summary>
        ///
        /// <returns>   An enumerator that can be used to iterate through the collection. </returns>
        ///
        /// <seealso cref="M:System.Collections.Generic.IEnumerable{TreeIterator.TreeBranch}.GetEnumerator()"/>
        ///=================================================================================================
        public IEnumerator<TreeBranch> GetEnumerator()
        {
            return EnumerationMode == TreeEnumerationMode.DepthFirst
                ? new DepthTreeEnumerator<TreeBranch>(this, null)
                : new BreadthTreeEnumerator<TreeBranch>(this, null);
        }

        ///=================================================================================================
        /// <summary>   Gets the enumerator. </summary>
        ///
        /// <returns>   The enumerator. </returns>
        ///=================================================================================================
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ///=================================================================================================
        /// <summary>
        ///     Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data
        ///     needed to serialize the target object.
        /// </summary>
        ///
        /// <exception cref="T:System.Security.SecurityException">  The caller does not have the required
        ///                                                         permission. </exception>
        ///
        /// <param name="info">     The <see cref="T:System.Runtime.Serialization.SerializationInfo" />
        ///                         to populate with data. </param>
        /// <param name="context">  The destination (see
        ///                         <see cref="T:System.Runtime.Serialization.StreamingContext" />) for
        ///                         this serialization. </param>
        ///
        /// <seealso cref="M:System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo,StreamingContext)"/>
        ///=================================================================================================
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public abstract void GetObjectData(SerializationInfo info, StreamingContext context);

        ///=================================================================================================
        /// <summary>   Writes this branch down to a XML file. </summary>
        ///
        /// <param name="document"> The document. </param>
        /// <param name="parent">   The parent. </param>
        ///=================================================================================================
        public abstract void WriteXml(XmlDocument document, XmlElement parent);

        ///=================================================================================================
        /// <summary>   Parses a XML file and recreates the tree. </summary>
        ///
        /// <param name="element">  Element to process. </param>
        /// <param name="parent">   The parent. </param>
        ///=================================================================================================
        public abstract void ParseXml(XmlElement element, TreeBranch parent);

        ///=================================================================================================
        /// <summary>   Writes the branch down as binary file. </summary>
        ///
        /// <param name="writer">   The writer. </param>
        ///=================================================================================================
        public abstract void WriteBinary(BinaryWriter writer);

        ///=================================================================================================
        /// <summary>   Parses a binary file and re-creates the branch. </summary>
        ///
        /// <param name="reader">   The reader. </param>
        /// <param name="parent">   The parent. </param>
        ///=================================================================================================
        public abstract void ParseBinary(BinaryReader reader, TreeBranch parent);
    }
}