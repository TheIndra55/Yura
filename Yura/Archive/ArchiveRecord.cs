namespace Yura.Archive
{
    public class ArchiveRecord
    {
        /// <summary>
        /// Gets the filename hash
        /// </summary>
        public ulong Hash { get; set; }

        /// <summary>
        /// Gets the name of the file
        /// </summary>
#nullable enable
        public string? Name { get; set; }
#nullable restore

        /// <summary>
        /// Gets the size of the file
        /// </summary>
        public uint Size { get; set; }
    }
}
