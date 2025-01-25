using System.Collections.Generic;
using System.Text;
using Yura.Shared.IO;

namespace Yura.Formats
{
    class LocaleFile
    {
        public Language Language { get; private set; }
        public List<string> Entries { get; private set; }

        public LocaleFile(byte[] data, Endianness endianness)
        {
            var reader = new DataReader(data, endianness);

            Entries = new List<string>();
            Language = (Language)reader.ReadUInt32();

            // read all strings
            var numStrings = reader.ReadUInt32();

            for (int i = 0; i < numStrings; i++)
            {
                // offset of the string
                var offset = reader.ReadUInt32();

                // preserve current cursor position
                var cursor = reader.BaseStream.Position;
                reader.BaseStream.Position = offset;

                // read the string
                var str = reader.ReadString(Encoding.UTF8);
                Entries.Add(str);

                reader.BaseStream.Position = cursor;
            }
        }
    }
}
