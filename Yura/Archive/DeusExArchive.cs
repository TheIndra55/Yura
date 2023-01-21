using System;
using System.Collections.Generic;
using System.IO;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class DeusExArchive : ArchiveFile
    {
        private string _file;
        private uint _alignment;
        private bool _littleEndian;

        private List<DeusExRecord> _files;

        public DeusExArchive(string path, bool littleEndian = true)
            : base(path)
        {
            _file = path;
            _littleEndian = littleEndian;

            _files = new List<DeusExRecord>();
        }

        public override void Open()
        {
            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            _alignment = reader.ReadUInt32();

            // skip over config name
            reader.BaseStream.Position += 64;

            // same as legend
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
                var file = new DeusExRecord()
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

        public override bool CanWrite => false;

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as DeusExRecord;

            // calculate in which bigfile the data is
            var offset = (long)file.Offset << 11;
            var bigfile = offset / _alignment;

            // get the right bigfile filename
            var name = Path.GetFileNameWithoutExtension(_file);
            var filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + bigfile.ToString("000");

            // read data
            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = offset % _alignment;
            stream.Read(bytes, 0, (int)file.Size);

            stream.Close();

            return bytes;
        }

        public override void Write(ArchiveRecord record, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override uint GetSpecialisationMask(ArchiveRecord record)
        {
            var file = record as DeusExRecord;

            return file.Specialisation;
        }
    }

    class DeusExRecord : ArchiveRecord
    {
        public uint Offset { get; set; }
        public uint Specialisation { get; set; }
    }
}
