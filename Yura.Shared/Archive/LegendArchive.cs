using System;
using System.IO;
using System.IO.Compression;
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

            // Convert sectors to file position
            var position = (long)file.Offset << 11;

            var (path, offset) = GetFileAndOffset(position);

            // Read the file
            var stream = File.OpenRead(path);
            var data = new byte[file.Size];

            if (file.CompressedSize == 0)
            {
                stream.Position = offset;
                stream.ReadExactly(data);
            }
            else
            {
                ReadCompressed(stream, offset, file.CompressedSize, data);
            }

            stream.Close();

            return data;
        }

        private static void ReadCompressed(Stream stream, long offset, uint size, Span<byte> buffer)
        {
            // Read the compressed data
            var data = new byte[size];

            stream.Position = offset;
            stream.ReadExactly(data);

            // Decompress the data
            var zlib = new ZLibStream(new MemoryStream(data), CompressionMode.Decompress);

            zlib.ReadExactly(buffer);
        }

        private class Record : ArchiveRecord
        {
            public uint Offset { get; set; }
            public uint CompressedSize { get; set; }
        }
    }
}
