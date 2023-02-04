using System;
using System.Collections.Generic;
using System.IO;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class LegendArchive : ArchiveFile
    {
        private string _file;
        private int _alignment;
        private bool _littleEndian;

        private List<LegendRecord> _files;

        public LegendArchive(string path, int alignment, bool littleEndian = true)
            : base(path)
        {
            _file = path;
            _littleEndian = littleEndian;
            _alignment = alignment;

            _files = new List<LegendRecord>();
        }

        public override void Open()
        {
            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            // extract number of files
            var numRecords = reader.ReadUInt32();

            if (numRecords > 1_000_000)
            {
                throw new ArgumentException("Bigfile has more than a million files, did you select the wrong endianness?");
            }

            var hashes = new uint[numRecords];

            // read all filename hashes
            for (var i = 0; i < numRecords; i++)
            {
                hashes[i] = reader.ReadUInt32();
            }

            // read all records
            for (var i = 0; i < numRecords; i++)
            {
                var file = new LegendRecord()
                {
                    Hash = hashes[i]
                };

                file.Size = reader.ReadUInt32();
                file.Offset = reader.ReadUInt32();
                file.Specialisation = reader.ReadUInt32();

                // check if hash exist in file list
                if (FileList != null && FileList.Files.TryGetValue(file.Hash, out string name))
                {
                    file.Name = name;
                }

                _files.Add(file);

                reader.BaseStream.Position += 4;
            }

            stream.Close();
        }

        public override IReadOnlyList<ArchiveRecord> Records
        {
            get
            {
                return _files;
            }
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as LegendRecord;

            var offset = (long)file.Offset << 11;
            string filename = null;

            // check whether the bigfile is split over multiple files
            if (_file.EndsWith(".000"))
            {
                // calculate which bigfile the file is in, and get the file offset
                var bigfile = offset / _alignment;
                offset = offset % _alignment;

                var name = Path.GetFileNameWithoutExtension(_file);
                filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + bigfile.ToString("000");
            }
            else
            {
                filename = _file;
            }

            // read the file
            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = offset;
            stream.Read(bytes, 0, (int)file.Size);

            stream.Close();

            return bytes;
        }

        public override uint GetSpecialisationMask(ArchiveRecord record)
        {
            var file = record as LegendRecord;

            return file.Specialisation;
        }
    }

    class LegendRecord : ArchiveRecord
    {
        public uint Offset { get; set; }
        public uint Specialisation { get; set; }
    }
}
