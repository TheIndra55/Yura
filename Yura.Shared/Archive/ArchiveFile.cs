using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yura.Shared.Archive
{
    public abstract class ArchiveFile
    {
        protected ArchiveOptions Options { get; }

        public ArchiveFile(ArchiveOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            Options = options;
            Records = [];
        }

        /// <summary>
        /// Gets the records in the archive
        /// </summary>
        public List<ArchiveRecord> Records { get; }

        /// <summary>
        /// Gets the path of the archive
        /// </summary>
        public string Name => Options.Path;

        /// <summary>
        /// Opens the archive
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Reads a file from the archive
        /// </summary>
        /// <param name="record">The record to read</param>
        /// <returns>The contents of the file</returns>
        public abstract byte[] Read(ArchiveRecord record);

        /// <summary>
        /// Gets all files in the specified directory
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns>The contents of the directory</returns>
        public List<ArchiveRecord> GetFiles(string path)
        {
            var hierachy = Split(path);

            // Find all records where the path is equal, removing the file name
            return Records.Where(record => record.Name != null && Split(record.Name).SkipLast(1).SequenceEqual(hierachy)).ToList();
        }

        /// <summary>
        /// Gets the file name of a archive part
        /// </summary>
        /// <param name="part">The part</param>
        /// <param name="extension">The extension</param>
        /// <returns>The formatted file name</returns>
        protected string GetFilePart(int part, string extension = "")
        {
            var path = Options.Path[..^extension.Length];

            var name = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);

            return Path.Combine(directory, name + "." + part.ToString("000") + extension);
        }

        private static string[] Split(string path)
        {
            return path.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
