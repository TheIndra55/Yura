using System;
using System.IO;
using Yura.Shared.IO;

namespace Yura.Shared.Archive
{
    public class LegendArchive : ArchiveFile
    {
        public LegendArchive(ArchiveOptions options) : base(options)
        {
        }

        public override void Open()
        {
            var stream = File.OpenRead(Options.Path);
            var reader = new DataReader(stream, Options.Endianness);

            // Read the number of files
            var numRecords = reader.ReadUInt32();

            if (numRecords > 1_000_000)
            {
                throw new ArgumentException("Bigfile has more than a million files, did you select the right endianness?");
            }

            // Read the file name hashes
            var hashes = new uint[numRecords];

            for (var i = 0; i < numRecords; i++)
            {
                hashes[i] = reader.ReadUInt32();
            }

            // Read the file records
            for (var i = 0; i < numRecords; i++)
            {
                var record = new Record
                {
                    Hash = hashes[i],

                    Size = reader.ReadUInt32(),
                    Offset = reader.ReadUInt32(),
                    Specialisation = reader.ReadUInt32(),
                    CompressedSize = reader.ReadUInt32(),
                };

                Records.Add(record);
            }

            stream.Close();
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as Record;

            // Calculate the location of the file
            var offset = (long)file.Offset << 11;
            var part = offset / Options.Alignment;

            var path = GetFilePart((int)part);

            // Read the file
            var stream = File.OpenRead(path);
            var data = new byte[file.Size];

            stream.Position = offset % Options.Alignment;
            stream.ReadExactly(data);

            stream.Close();

            return data;
        }

        private class Record : ArchiveRecord
        {
            public uint Offset { get; set; }
            public uint CompressedSize { get; set; }
        }
    }
}
