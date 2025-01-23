using System;
using System.IO;
using Yura.Shared.IO;
using Yura.Shared.Util;

namespace Yura.Shared.Archive
{
    public class TigerArchive : ArchiveFile
    {
        public TigerArchive(ArchiveOptions options) : base(options)
        {
        }

        public override void Open()
        {
            var stream = File.OpenRead(Options.Path);
            var reader = new DataReader(stream, Options.Endianness);

            // Read the magic
            var magic = reader.ReadUInt32();

            if (magic != 0x53464154 && magic != 0x54414653)
            {
                throw new Exception("File is not a tiger archive file");
            }

            var version = reader.ReadUInt32();

            // Check for version 3, 4 or 5
            if (version < 3 || version > 5)
            {
                throw new NotImplementedException($"Tiger archive version {version} is not supported");
            }

            // Load the file list with the correct hashing algorithm
            Options.FileList?.Load(version < 5 ? HashAlgorithm.Crc32 : HashAlgorithm.Fnv1a);

            var numArchives = reader.ReadUInt32();
            var numRecords = reader.ReadUInt32();

            reader.Position += 4;

            // Skip over the config name
            reader.Position += 32;

            // Read the file records
            for (var i = 0; i < numRecords; i++)
            {
                var record = new Record();

                if (version < 5)
                {
                    record.Hash = reader.ReadUInt32();
                    record.Specialisation = reader.ReadUInt32();
                    record.Size = reader.ReadUInt32();
                }
                else
                {
                    record.Hash = reader.ReadUInt64();
                    record.Specialisation = reader.ReadUInt64();
                    record.Size = reader.ReadUInt32();
                }
                
                // Skip in TR2 and later
                if (version >= 4)
                {
                    reader.Position += 4;
                }

                // TRAS
                if (version == 3)
                {
                    var packedOffset = reader.ReadUInt32();

                    record.Index = packedOffset & 0xF;
                    record.Offset = packedOffset & 0xFFFFF800;
                }
                // TR2
                else if (version == 4)
                {
                    var packedOffset = reader.ReadUInt64();

                    record.Index = packedOffset & 0xFFFF;
                    record.Offset = (packedOffset >> 32) & 0xFFFFFFFF;
                }
                // TR11
                else if (version == 5)
                {
                    var packedOffset = reader.ReadUInt64();

                    record.Index = packedOffset & 0xFFFF;
                    record.Offset = (packedOffset >> 32) & 0xFFFFFFFF;
                }

                Records.Add(record);
            }

            stream.Close();
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as Record;

            var path = GetFilePart((int)file.Index, ".tiger");

            // Read the file
            var stream = File.OpenRead(path);
            var data = new byte[file.Size];

            stream.Position = (long)file.Offset;
            stream.ReadExactly(data);

            stream.Close();

            return data;
        }

        private class Record : ArchiveRecord
        {
            public ulong Index { get; set; }
            public ulong Offset { get; set; }
        }
    }
}
