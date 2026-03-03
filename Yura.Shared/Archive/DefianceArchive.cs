using System.IO;
using Yura.Shared.IO;

namespace Yura.Shared.Archive
{
    public class DefianceArchive : ArchiveFile
    {
        public DefianceArchive(ArchiveOptions options) : base(options)
        {
        }

        public override void Open()
        {
            var stream = File.OpenRead(Options.Path);
            var reader = new DataReader(stream, Options.Endianness);

            // TODO
            var is32 = stream.Length < 4L * 1000 * 1000 * 1000;

            // Read the number of files
            var numFiles = reader.ReadUInt16();

            stream.Position += 2;

            // Read the file name hashes
            var hashes = new uint[numFiles];

            for (var i = 0; i < numFiles; i++)
            {
                hashes[i] = reader.ReadUInt32();
            }

            // Read the file records
            for (var i = 0; i < numFiles; i++)
            {
                var record = new Record
                {
                    Hash = hashes[i],

                    Size = is32 ? reader.ReadUInt32() : (uint)reader.ReadUInt64(),
                    Offset = is32 ? reader.ReadUInt32() : reader.ReadInt64()
                };

                reader.Position += is32 ? 4 : 8;

                Records.Add(record);
            }

            stream.Close();
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as Record;

            var (path, offset) = GetFileAndOffset(file.Offset);

            // Read the file
            var stream = File.OpenRead(path);
            var data = new byte[file.Size];

            stream.Position = offset;
            stream.ReadExactly(data);

            stream.Close();

            return data;
        }

        private class Record : ArchiveRecord
        {
            public long Offset { get; set; }
        }
    }
}
