using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yura.IO;

namespace Yura.Formats
{
    // TODO move all this game format specific code to another library/project
    class DrmFile
    {
        public List<Section> _sections;

        /// <summary>
        /// Loads a data file
        /// </summary>
        /// <param name="data">The data of the file</param>
        /// <param name="litteEndian">Whether the file should be read as little endian</param>
        /// <param name="relocate">Whether to relocate relocations, currently unsupported</param>
        public DrmFile(byte[] data, bool litteEndian, bool relocate = false)
        {
            _sections = new List<Section>();

            var reader = new StreamReader(data, litteEndian);

            // read file version
            if (reader.ReadInt32() != 14)
            {
                throw new ArgumentException("Data is not a data file or of an unsupported version");
            }

            var numSections = reader.ReadInt32();

            // read all sections
            for (var i = 0; i < numSections; i++)
            {
                // read section header
                var size = reader.ReadUInt32();
                var type = reader.ReadByte();

                reader.BaseStream.Position += 3;

                var packedData = reader.ReadUInt32();

                var id = reader.ReadUInt32();
                var specMask = reader.ReadUInt32();

                // add section
                _sections.Add(new Section(i, size, (SectionType)type, (int)id));
            }

            if (relocate)
            {
                throw new NotImplementedException("Relocating is not yet implemented");
            }
        }

        public IReadOnlyList<Section> Sections => _sections;
    }

    enum SectionType
    {
        Data = 0,
        Animation = 2,
        Texture = 5,
        Wave = 6,
        Dtp = 7,
        Shader = 9
    }

    record Section(int Index, uint Size, SectionType Type, int Id);
}
