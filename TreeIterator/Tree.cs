using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Xml;

namespace TreeIterator
{
    /// <summary>   An abstract typeless tree. </summary>
    [Serializable]
    public abstract class Tree : IEnumerable<TreeBranch>, ISerializable
    {
        private readonly TreeBranch _rootBranch;

        ///=================================================================================================
        /// <summary>   Gets the root. </summary>
        ///
        /// <value> The root. </value>
        ///=================================================================================================
        public TreeBranch Root => _rootBranch;

        ///=================================================================================================
        /// <summary>   Gets or sets the enumeration mode. </summary>
        ///
        /// <value> The enumeration mode. </value>
        ///=================================================================================================
        public TreeEnumerationMode EnumerationMode { get; set; }

        ///=================================================================================================
        /// <summary>   Specialised constructor for use only by derived class. </summary>
        ///
        /// <exception cref="ArgumentNullException">    Thrown when 'rootBranch' is null. </exception>
        ///
        /// <param name="rootBranch">   The root branch. </param>
        /// <param name="mode">         The mode used for enumeration. </param>
        ///=================================================================================================
        protected Tree(TreeBranch rootBranch, TreeEnumerationMode mode = TreeEnumerationMode.DepthFirst)
        {
            _rootBranch = rootBranch ?? throw new ArgumentNullException(nameof(rootBranch));
            rootBranch.Tree = this;
            EnumerationMode = mode;
        }

        ///=================================================================================================
        /// <summary>   Specialised constructor for use only by derived class. </summary>
        ///
        /// <param name="info">     The <see cref="T:System.Runtime.Serialization.SerializationInfo" />
        ///                         to populate with data. </param>
        /// <param name="context">  The destination (see
        ///                         <see cref="T:System.Runtime.Serialization.StreamingContext" />) for
        ///                         this serialization. </param>
        ///=================================================================================================
        protected Tree(SerializationInfo info, StreamingContext context)
        {
            EnumerationMode = (TreeEnumerationMode)info.GetValue("EnumerationMode", typeof(TreeEnumerationMode));
            _rootBranch = (TreeBranch)info.GetValue("Root", typeof(TreeBranch));
            _rootBranch.Tree = this;
        }

        ///=================================================================================================
        /// <summary>   Adds a branch to the root branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///=================================================================================================
        public void AddBranch(TreeBranch branch)
        {
            Root.AddBranch(branch);
        }

        ///=================================================================================================
        /// <summary>   Removes the branch described by branch from the root branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///=================================================================================================
        public bool RemoveBranch(TreeBranch branch)
        {
            return Root.RemoveBranch(branch);
        }

        ///=================================================================================================
        /// <summary>   Dumps the tree into the given file. </summary>
        ///
        /// <param name="file"> The file to dump. </param>
        ///=================================================================================================
        public virtual void Dump(string file)
        {
            using FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
            using StreamWriter writer = new StreamWriter(fs);
            Root.Dump(writer, 0);
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
                ? new DepthTreeEnumerator(Root, null)
                : new BreadthTreeEnumerator(Root, null);
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
        /// <param name="info">     The <see cref="T:System.Runtime.Serialization.SerializationInfo" />
        ///                         to populate with data. </param>
        /// <param name="context">  The destination (see
        ///                         <see cref="T:System.Runtime.Serialization.StreamingContext" />) for
        ///                         this serialization. </param>
        ///
        /// <seealso cref="M:System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo,StreamingContext)"/>
        ///=================================================================================================
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EnumerationMode", EnumerationMode);
            info.AddValue("Root", Root);
        }

        ///=================================================================================================
        /// <summary>   Writes this tree down to a XML file. </summary>
        ///
        /// <param name="file"> The file to dump. </param>
        ///=================================================================================================
        public void WriteXml(string file)
        {
            XmlDocument document = new XmlDocument();
            XmlElement docElement = document.CreateElement("Tree");
            docElement.SetAttribute("Type", GetType().FullName);
            docElement.SetAttribute("BranchType", Root.GetType().FullName);
            docElement.SetAttribute("Mode", ((int)EnumerationMode).ToString());
            document.AppendChild(docElement);
            Root.WriteXml(document, docElement);

            using FileStream fs = new FileStream(file, FileMode.Create);
            document.Save(fs);
        }

        ///=================================================================================================
        /// <summary>   Parses an XML file and re-creates the tree. </summary>
        ///
        /// <param name="file"> The file to dump. </param>
        ///
        /// <returns>   A Tree. </returns>
        ///=================================================================================================
        public static Tree ParseXml(string file)
        {
            XmlDocument document = new XmlDocument();
            using (FileStream fs = new FileStream(file, FileMode.Open))
                document.Load(fs);

            Type treeType = Type.GetType(document.DocumentElement?.GetAttribute("Type") ?? "", true);
            Type branchType = Type.GetType(document.DocumentElement?.GetAttribute("BranchType") ?? "", true);
            var mode = (TreeEnumerationMode)int.Parse(document.DocumentElement?.GetAttribute("Mode") ?? "");

            TreeBranch branch = (TreeBranch)Activator.CreateInstance(branchType);
            Tree tree = (Tree)Activator.CreateInstance(treeType, branch);
            tree.EnumerationMode = mode;

            XmlElement rootElement = document.DocumentElement?.ChildNodes[0] as XmlElement;
            branch.ParseXml(rootElement, null);
            return tree;
        }

        public static TTree ParseXml<TTree>(string file) where TTree : Tree
        {
            return (TTree)ParseXml(file);
        }

        ///=================================================================================================
        /// <summary>   Writes the tree down binary. </summary>
        ///
        /// <param name="file">         The file to dump. </param>
        /// <param name="textEncoding"> The text encoding (null uses default: Unicode / UTF-16). </param>
        ///=================================================================================================
        public void WriteBinary(string file, Encoding textEncoding = null)
        {
            using FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None);
            BinaryWriter writer = new BinaryWriter(fs, textEncoding ?? Encoding.Unicode);
            writer.Write(GetType().FullName!);
            writer.Write(Root.GetType().FullName!);
            writer.Write((byte)EnumerationMode);

            Root.WriteBinary(writer);
        }

        ///=================================================================================================
        /// <summary>   Reads a binary file and re-creates the tree. </summary>
        ///
        /// <param name="file">         The file to parse. </param>
        /// <param name="textEncoding"> The text encoding (null uses default: Unicode / UTF-16). </param>
        ///=================================================================================================
        public static Tree ParseBinary(string file, Encoding textEncoding = null)
        {
            Tree tree;
            using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader reader = new BinaryReader(fs, textEncoding ?? Encoding.Unicode);
            Type treeType = Type.GetType(reader.ReadString(), true);
            Type branchType = Type.GetType(reader.ReadString(), true);
            TreeEnumerationMode mode = (TreeEnumerationMode)reader.ReadByte();

            TreeBranch branch = (TreeBranch)Activator.CreateInstance(branchType);
            tree = (Tree)Activator.CreateInstance(treeType, branch);
            tree.EnumerationMode = mode;

            branch.ParseBinary(reader, null);

            return tree;
        }

        public static TTree ParseBinary<TTree>(string file, Encoding textEncoding = null) where TTree : Tree
        {
            return (TTree)ParseBinary(file, textEncoding);
        }
    }

    ///=================================================================================================
    /// <summary>   An abstract generic tree. </summary>
    ///
    /// <typeparam name="TBranch">  Type of the branch. </typeparam>
    ///
    /// <seealso cref="T:TreeIterator.Tree"/>
    /// <seealso cref="T:System.Collections.Generic.IEnumerable{TBranch}"/>
    ///=================================================================================================
    [Serializable]
    public abstract class Tree<TBranch> : Tree, IEnumerable<TBranch> where TBranch : TreeBranch
    {
        /// <summary>   The root. </summary>
        public new TBranch Root => (TBranch)base.Root;

        ///=================================================================================================
        /// <summary>   Specialised constructor for use only by derived class. </summary>
        ///
        /// <param name="root"> The root. </param>
        /// <param name="mode"> The mode used for enumeration. </param>
        ///=================================================================================================
        protected Tree(TBranch root, TreeEnumerationMode mode = TreeEnumerationMode.DepthFirst) : base(root, mode)
        {
        }

        ///=================================================================================================
        /// <summary>   Serialization constructor. </summary>
        ///
        /// <param name="info">     The information. </param>
        /// <param name="context">  The context. </param>
        ///=================================================================================================
        protected Tree(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        ///=================================================================================================
        /// <summary>   Adds a branch to the root branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///=================================================================================================
        public void AddBranch(TBranch branch)
        {
            Root.AddBranch(branch);
        }

        ///=================================================================================================
        /// <summary>   Removes the branch described by branch from the root branch. </summary>
        ///
        /// <param name="branch">   The branch. </param>
        ///=================================================================================================
        public bool RemoveBranch(TBranch branch)
        {
            return Root.RemoveBranch(branch);
        }

        ///=================================================================================================
        /// <summary>   Returns an enumerator that iterates through the collection. </summary>
        ///
        /// <returns>   An enumerator that can be used to iterate through the collection. </returns>
        ///
        /// <seealso cref="M:System.Collections.Generic.IEnumerable{TBranch}.GetEnumerator()"/>
        ///=================================================================================================
        public new IEnumerator<TBranch> GetEnumerator()
        {
            return EnumerationMode == TreeEnumerationMode.DepthFirst
                ? new DepthTreeEnumerator<TBranch>(Root, null)
                : new BreadthTreeEnumerator<TBranch>(Root, null);
        }

        ///=================================================================================================
        /// <summary>   Parses an XML file and re-creates the tree. </summary>
        ///
        /// <param name="file"> The file to dump. </param>
        ///
        /// <returns>   A Tree. </returns>
        ///=================================================================================================
        public new static Tree<TBranch> ParseXml(string file)
        {
            return (Tree<TBranch>)Tree.ParseXml(file);
        }

        ///=================================================================================================
        /// <summary>   Reads a binary file and re-creates the tree. </summary>
        ///
        /// <param name="file">         The file to parse. </param>
        /// <param name="textEncoding"> The text encoding (null uses default: Unicode / UTF-16). </param>
        ///=================================================================================================
        public new static Tree<TBranch> ParseBinary(string file, Encoding textEncoding = null)
        {
            return (Tree<TBranch>)Tree.ParseBinary(file, textEncoding);
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
    }
}