using System.Collections.Generic;
using System.IO;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class DefianceArchive : ArchiveFile
    {
        private string _file;
        private bool _littleEndian;

        private List<DefianceRecord> _files;

        public DefianceArchive(string path, bool littleEndian = true)
            : base(path)
        {
            _file = path;
            _littleEndian = littleEndian;

            _files = new List<DefianceRecord>();
        }

        public override void Open()
        {
            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            // extract number of files
            var numFiles = reader.ReadUInt16();
            reader.BaseStream.Position += 2;

            var hashes = new uint[numFiles];

            // read all filename hashes
            for (var i = 0; i < numFiles; i++)
            {
                hashes[i] = reader.ReadUInt32();
            }

            // read all records
            for (var i = 0; i < numFiles; i++)
            {
                var file = new DefianceRecord
                {
                    Hash = hashes[i]
                };

                file.Size = reader.ReadUInt32();
                file.Offset = reader.ReadUInt32();

                // check if hash exist in file list
                if (FileList != null && FileList.Files.TryGetValue(file.Hash, out string name))
                {
                    file.Name = name;
                }

                _files.Add(file);

                reader.BaseStream.Position += 4;
            }
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
            var file = record as DefianceRecord;

            var stream = File.OpenRead(_file);
            var bytes = new byte[file.Size];

            stream.Position = file.Offset;
            stream.Read(bytes, 0, (int)file.Size);

            return bytes;
        }

        public override uint GetSpecialisationMask(ArchiveRecord record)
        {
            // defiance does not use specialisation
            return 0;
        }
    }

    class DefianceRecord : ArchiveRecord
    {
        public uint Offset { get; set; }
    }
}
