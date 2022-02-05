namespace Yura.Archive
{
    /// <summary>
    /// Represents a file in the bigfile
    /// </summary>
    public class ArchivedFile
    {
        /// <summary>
        /// The file name hash
        /// </summary>
        public uint Hash { get; set; }

        /// <summary>
        /// The name of the file if known
        /// </summary>
#nullable enable
        public string? Name { get; set; }
#nullable restore

        /// <summary>
        /// The size of the file
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// The offset in one or more bigfiles
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// The specialisation mask
        /// </summary>
        public uint SpecialisationMask { get; set; }
    }
}
