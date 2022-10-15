using System;
using System.Collections.Generic;
using System.IO;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class TigerArchive : ArchiveFile
    {
        private string _file;
        private bool _littleEndian;

        private List<TigerRecord> _files;

        public TigerArchive(string path, bool littleEndian = true)
            : base(path)
        {
            _file = path;
            _littleEndian = littleEndian;

            _files = new List<TigerRecord>();
        }

        public override void Open()
        {
            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            var magic = reader.ReadUInt32();

            if (magic != 0x53464154)
            {
                throw new Exception("File is not a tiger archive file");
            }

            var version = reader.ReadUInt32();

            // check if version 3 or 4
            if (version < 3 || version > 4)
            {
                throw new Exception($"Tiger archive version {version} is not supported");
            }

            var numArchives = reader.ReadUInt32();
            var numRecords = reader.ReadUInt32();

            reader.BaseStream.Position += 4;

            // skip over config name
            reader.BaseStream.Position += 32;

            // read all records
            for (var i = 0; i < numRecords; i++)
            {
                var file = new TigerRecord();

                file.Hash = reader.ReadUInt32();
                file.Specialisation = reader.ReadUInt32();
                file.Size = reader.ReadUInt32();

                // 2013
                if (version == 3)
                {
                    var packedOffset = reader.ReadUInt32();

                    file.Index = packedOffset & 0xF;
                    file.Offset = packedOffset & 0xFFFFF800;
                }
                // rise/tr2
                else if (version == 4)
                {
                    reader.BaseStream.Position += 4; // padding

                    var packedOffset = reader.ReadUInt64();

                    // not sure about this, need to check LOWORD/HIDWORD
                    file.Index = packedOffset & 0xffff;
                    file.Offset = (packedOffset >> 32) & 0xFFFFFFFF;
                }

                // check if hash exist in file list
                if (FileList != null && FileList.Files.TryGetValue(file.Hash, out string name))
                {
                    file.Name = name;
                }

                _files.Add(file);
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
            var file = record as TigerRecord;

            // get right bigfile
            var name = Path.GetFileNameWithoutExtension(_file);
            name = name.Substring(0, name.Length - 4);

            var filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + file.Index.ToString("000") + ".tiger";

            // read the file
            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = (long)file.Offset;
            stream.Read(bytes, 0, (int)file.Size);

            return bytes;
        }

        public override uint GetSpecialisationMask(ArchiveRecord record)
        {
            var file = record as TigerRecord;

            return file.Specialisation;
        }
    }

    class TigerRecord : ArchiveRecord
    {
        public ulong Index { get; set; }
        public ulong Offset { get; set; }

        public uint Specialisation { get; set; }
    }
}
