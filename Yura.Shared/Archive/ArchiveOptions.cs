using Yura.Shared.IO;
using Yura.Shared.Util;

namespace Yura.Shared.Archive
{
    public class ArchiveOptions
    {
        /// <summary>
        /// Gets or sets the path to the archive
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the archive endianness
        /// </summary>
        public Endianness Endianness { get; set; }

        /// <summary>
        /// Gets or sets the archive alignment
        /// </summary>
        public int Alignment { get; set; }

        /// <summary>
        /// Gets or sets the file list
        /// </summary>
        public FileList? FileList { get; set; }
    }
}
