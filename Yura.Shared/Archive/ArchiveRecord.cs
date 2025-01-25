namespace Yura.Shared.Archive
{
    public class ArchiveRecord
    {
        /// <summary>
        /// Gets or sets the hash of the file name
        /// </summary>
        public ulong Hash { get; internal set; }

        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string? Name { get; internal set; }

        /// <summary>
        /// Gets or sets the file size
        /// </summary>
        public uint Size { get; internal set; }

        /// <summary>
        /// Gets the file specialisation mask
        /// </summary>
        public ulong Specialisation { get; internal set; }
    }
}
