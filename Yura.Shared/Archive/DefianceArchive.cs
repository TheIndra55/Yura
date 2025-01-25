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

                    Size = reader.ReadUInt32(),
                    Offset = reader.ReadUInt32()
                };

                reader.Position += 4;

                Records.Add(record);
            }

            stream.Close();
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as Record;

            // Read the file
            var stream = File.OpenRead(Options.Path);
            var data = new byte[file.Size];

            stream.Position = file.Offset;
            stream.ReadExactly(data);

            stream.Close();

            return data;
        }

        private class Record : ArchiveRecord
        {
            public uint Offset { get; set; }
        }
    }
}
