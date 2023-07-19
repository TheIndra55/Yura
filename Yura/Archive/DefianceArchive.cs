using System.Collections.Generic;
using System.IO;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class DefianceArchive : ArchiveFile
    {
        private const int XboxAlignment = 0xFA00000;

        private string _file;
        private TextureFormat _platform;
        private bool _littleEndian;

        private List<DefianceRecord> _files;

        public DefianceArchive(string path, TextureFormat platform, bool littleEndian = true)
            : base(path)
        {
            _file = path;
            _platform = platform;
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
            var file = record as DefianceRecord;

            var filename = _file;
            var offset = file.Offset;

            if (_platform == TextureFormat.Xbox)
            {
                // xbox bigfiles in this game are spread over multiple physical files
                // calculate the bigfile this file is in
                var bigfile = offset / XboxAlignment;
                offset = offset % XboxAlignment;

                var name = Path.GetFileNameWithoutExtension(_file);
                filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + bigfile.ToString("000");
            }

            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = offset;
            stream.Read(bytes, 0, (int)file.Size);

            stream.Close();

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
