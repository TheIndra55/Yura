using System.Collections.Generic;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Formats
{
    class LocaleFile
    {
        public Language Language { get; private set; }
        public List<string> Entries { get; private set; }

        public LocaleFile(byte[] data, bool litteEndian)
        {
            var reader = new StreamReader(data, litteEndian);

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
                var str = reader.ReadString();
                Entries.Add(str);

                reader.BaseStream.Position = cursor;
            }
        }
    }
}
