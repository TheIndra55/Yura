using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Yura.Archive
{
    public class FileList
    {
        public Dictionary<uint, string> Files { get; private set; }

        public FileList([NotNull] string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File list not found", path);
            }   

            Files = new Dictionary<uint, string>();

            foreach (var line in File.ReadAllLines(path))
            {
                var hash = CalculateHash(line);

                // either a hash collision or double file entry
                if (!Files.ContainsKey(hash))
                {
                    Files.Add(hash, line);
                }
            }
        }

        public uint CalculateHash(string name)
        {
            // all paths are lowercase
            name = name.ToLower();

            uint hash = 0xFFFFFFFF;

            foreach(var rune in name)
            {
                hash ^= (uint)rune << 24;

                for(int i = 0; i < 8; i++)
                {
                    if ((hash & 0x80000000) != 0)
                        hash = (hash << 1) ^ 0x4C11DB7;
                    else
                        hash <<= 1;
                }
            }

            return ~hash;
        }
    }
}
