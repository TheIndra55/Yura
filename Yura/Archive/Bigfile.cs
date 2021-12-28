using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class Bigfile
    {
        private string _file;
        private FileList _fileList;
        private bool _littleEndian;

        private List<uint> _hashes;

        /// <summary>
        /// Gets the list of files in the bigfile
        /// </summary>
        public List<ArchivedFile> Files { get; private set; }

        /// <summary>
        /// Gets or sets the bigfile alignment
        /// </summary>
        public int Alignment { get; set; }

        /// <summary>
        /// Opens a bigfile
        /// </summary>
        /// <param name="path">The pato to the bigfile</param>
        /// <param name="fileList">The file list to use</param>
        /// <param name="littleEndian">Whether the file is little endian</param>
        public Bigfile([NotNull] string path, FileList fileList, bool littleEndian = true)
        {
            _file = path;
            _fileList = fileList;
            _littleEndian = littleEndian;

            _hashes = new List<uint>();

            Files = new List<ArchivedFile>();
        }

        /// <summary>
        /// Opens the bigfile
        /// </summary>
        public void Open()
        {
            if (!File.Exists(_file))
            {
                throw new FileNotFoundException("Bigfile does not exist", _file);
            }

            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            // extract number of files
            var numRecords = reader.ReadUInt32();

            for (int i = 0; i < numRecords; i++)
            {
                _hashes.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < numRecords; i++)
            {
                var file = new ArchivedFile();

                file.Hash = _hashes[i];
                file.Size = reader.ReadUInt32();
                file.Offset = reader.ReadUInt32();

                // check if hash exist in file list
                if (_fileList != null &&_fileList.Files.TryGetValue(file.Hash, out string name))
                {
                    file.Name = name;
                }
                Files.Add(file);

                reader.BaseStream.Position += 8;
            }

            // will not reuse this stream later since files could be in other files
            stream.Close();
        }

        /// <summary>
        /// Gets all files in a folder
        /// </summary>
        /// <param name="path">The path of the folder</param>
        /// <returns></returns>
        public List<ArchivedFile> GetFolder(string path)
        {
            // if root return all unknown files too
            if (path == "\\")
            {
                return Files.Where(
                    x => x.Name == null || x.Name.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length == 1).ToList();
            }

            var hierarchy = path.Split("\\", StringSplitOptions.RemoveEmptyEntries);

            return Files.Where(x =>
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

        /// <summary>
        /// Opens a file from the archive and returns the content
        /// </summary>
        /// <param name="file">The file entry</param>
        /// <returns>The content of the file</returns>
        public byte[] OpenFile(ArchivedFile file)
        {
            var align = Alignment >> 11;
            var bigfile = file.Offset / align;

            string filename;
            if (_file.EndsWith(".000"))
            {
                var name = Path.GetFileNameWithoutExtension(_file);
                filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + bigfile.ToString("000");
            }
            else
            {
                filename = _file;

                // one file, set alignment to that file size
                Alignment = (int)new FileInfo(_file).Length;
                align = Alignment >> 11;
            }

            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = file.Offset % align << 11;
            stream.Read(bytes, 0, (int)file.Size);

            return bytes;
        }
    }
}
