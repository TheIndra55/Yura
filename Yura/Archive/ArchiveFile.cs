using System;
using System.Collections.Generic;
using System.Linq;

namespace Yura.Archive
{
    abstract class ArchiveFile
    {
        /// <summary>
        /// Gets the records in the archive
        /// </summary>
        public abstract IReadOnlyList<ArchiveRecord> Records { get; }

        /// <summary>
        /// Opens the archive
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Reads a file from the archive and returns the content
        /// </summary>
        /// <param name="record">The file record</param>
        /// <returns>The content of the file</returns>
        public abstract byte[] Read(ArchiveRecord record);

        /// <summary>
        /// Gets or sets the file list
        /// </summary>
        public FileList FileList { get; set; }

        /// <summary>
        /// Gets all records in a folder
        /// </summary>
        /// <param name="path">The folder path</param>
        /// <returns>All records in the folder</returns>
        public List<ArchiveRecord> GetFolder(string path)
        {
            // if root return all unknown files too
            if (path == "\\")
            {
                return Records.Where(
                    x => x.Name == null || x.Name.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length == 1).ToList();
            }

            var hierarchy = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            return Records.Where(x =>
            {
                if (x.Name == null) return false;

                var split = x.Name.Split("\\", StringSplitOptions.RemoveEmptyEntries);

                if (split.Length - 1 != hierarchy.Length) return false;

                // compare both paths
                for (int i = 0; i < split.Length - 1; i++)
                {
                    if (split[i] != hierarchy[i])
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();
        }
    }
}
