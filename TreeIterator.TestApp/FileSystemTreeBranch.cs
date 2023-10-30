using System.Runtime.Serialization;
using System.Xml;

namespace TreeIterator.TestApp
{
    ///=================================================================================================
    /// <summary>   A file system tree branch. This class cannot be inherited. </summary>
    ///
    /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
    ///
    /// <seealso cref="T:TreeIterator.TreeBranch"/>
    ///=================================================================================================
    [Serializable]
    public sealed class FileSystemTreeBranch : TreeBranch
    {
        ///=================================================================================================
        /// <summary>   Gets or sets the name. </summary>
        ///
        /// <value> The name. </value>
        ///=================================================================================================
        public string Name { get; private set; } = string.Empty;

        ///=================================================================================================
        /// <summary>   Gets or sets a value indicating whether this instance is directory. </summary>
        ///
        /// <value> true if this instance is directory, false if not. </value>
        ///=================================================================================================
        public bool IsDirectory { get; private set; }

        /// <summary>   The tree. </summary>
        public new Tree<FileSystemTreeBranch> Tree => (Tree<FileSystemTreeBranch>) base.Tree;

        /// <summary>   Default constructor. </summary>
        public FileSystemTreeBranch()
        {
        }

        ///=================================================================================================
        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
        ///
        /// <param name="name">         The name. </param>
        /// <param name="isDirectory">  true if this instance is directory, false if not. </param>
        ///=================================================================================================
        public FileSystemTreeBranch(string name, bool isDirectory)
        {
            Name = name;
            IsDirectory = isDirectory;
        }

        ///=================================================================================================
        /// <summary>   Serialization constructor. </summary>
        ///
        /// <param name="info">     The information. </param>
        /// <param name="context">  The context. </param>
        ///=================================================================================================
        private FileSystemTreeBranch(SerializationInfo info, StreamingContext context)
        {
            Name = info.GetString("Name") ?? string.Empty;
            IsDirectory = info.GetBoolean("IsDirectory");
            int branchCount = info.GetInt32("Branches");
            if (branchCount == 0) return;
            for (int i = 0; i < branchCount; i++)
            {
                FileSystemTreeBranch branch = (FileSystemTreeBranch?)info.GetValue("Branch" + i, typeof(FileSystemTreeBranch)) ??
                                              throw new SerializationException($"Failed to deserialize tree branch (index {i}).");
                AddBranch(branch);
            }
        }

        ///=================================================================================================
        /// <summary>   Dumps this branch and all sub-branches into a file. </summary>
        ///
        /// <remarks>   Manuel Eisenschink, 17.01.2016. </remarks>
        ///
        /// <param name="writer">   The writer. </param>
        /// <param name="level">    The level. </param>
        ///
        /// <seealso cref="M:TreeIterator.TreeBranch.Dump(StreamWriter,int)"/>
        ///=================================================================================================
        public override void Dump(StreamWriter writer, int level)
        {
            writer.WriteLine($"{TabFromLevel(level)}{Name} | " + (IsDirectory ? "Directory" : "File"));
            foreach (var branch in Branches) branch.Dump(writer, level + 1);
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
        /// <seealso cref="M:TreeIterator.TreeBranch.GetObjectData(SerializationInfo,StreamingContext)"/>
        ///=================================================================================================
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", Name);
            info.AddValue("IsDirectory", IsDirectory);
            info.AddValue("Branches", BranchList.Count);
            for (int i = 0; i < Branches.Count; i++)
                info.AddValue("Branch" + i, Branches[i]);
        }

        ///=================================================================================================
        /// <summary>   Writes this branch down to a XML file. </summary>
        ///
        /// <param name="document"> The document. </param>
        /// <param name="parent">   The parent. </param>
        ///
        /// <seealso cref="M:TreeIterator.TreeBranch.WriteXml(XmlDocument,XmlElement)"/>
        ///=================================================================================================
        public override void WriteXml(XmlDocument document, XmlElement parent)
        {
            XmlElement element = document.CreateElement(IsDirectory ? "Directory" : "File");
            element.SetAttribute("Name", Name);

            foreach (var branch in Branches)
                branch.WriteXml(document, element);

            parent.AppendChild(element);
        }

        ///=================================================================================================
        /// <summary>   Parses a XML file and recreates the tree. </summary>
        ///
        /// <param name="element">  Element to process. </param>
        /// <param name="parent">   The parent. </param>
        ///
        /// <seealso cref="M:TreeIterator.TreeBranch.ParseXml(XmlElement,TreeBranch)"/>
        ///=================================================================================================
        public override void ParseXml(XmlElement element, TreeBranch parent)
        {
            Name = element.GetAttribute("Name");
            IsDirectory = element.Name == "Directory";

            foreach (XmlElement sub in element.ChildNodes.Cast<XmlElement>())
            {
                FileSystemTreeBranch branch = new FileSystemTreeBranch();
                branch.ParseXml(sub, this);
            }

            parent?.AddBranch(this);
        }

        ///=================================================================================================
        /// <summary>   Writes the branch down as binary file. </summary>
        ///
        /// <param name="writer">   The writer. </param>
        ///
        /// <seealso cref="M:TreeIterator.TreeBranch.WriteBinary(BinaryWriter)"/>
        ///=================================================================================================
        public override void WriteBinary(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(IsDirectory);
            writer.Write(Branches.Count);

            foreach (var branch in Branches)
                branch.WriteBinary(writer);
        }

        public override void ParseBinary(BinaryReader reader, TreeBranch parent)
        {
            Name = reader.ReadString();
            IsDirectory = reader.ReadBoolean();

            int branches = reader.ReadInt32();
            for (int i = 0; i < branches; i++)
            {
                FileSystemTreeBranch branch = new FileSystemTreeBranch();
                branch.ParseBinary(reader, this);
            }

            parent?.AddBranch(this);
        }

        ///=================================================================================================
        /// <summary>   Returns a string that represents the current object. </summary>
        ///
        /// <returns>   A string that represents the current object. </returns>
        ///
        /// <seealso cref="M:System.Object.ToString()"/>
        ///=================================================================================================
        public override string ToString()
        {
            return Name;
        }
    }
}
