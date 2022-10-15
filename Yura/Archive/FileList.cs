using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Yura.Archive
{
    public class FileList
    {
        public Dictionary<ulong, string> Files { get; private set; }
        public string Path { get; private set; }

        /// <summary>
        /// Constructs a new file list with the path as input file
        /// </summary>
        /// <param name="path">The input file</param>
        /// <param name="load">Whether to already load the file, can be false if hashing algorithm is not yet known</param>
        public FileList([NotNull] string path, bool load)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("File list not found", path);
            }

            Path = path;
            Files = new Dictionary<ulong, string>();

            if (load)
            {
                Load(HashingAlgorithm.Crc32);
            }
        }

        /// <summary>
        /// Loads the file list from disk
        /// </summary>
        public void Load(HashingAlgorithm algorithm)
        {
            foreach (var line in File.ReadAllLines(Path))
            {
                ulong hash = 0;

                if (algorithm == HashingAlgorithm.Crc32)
                {
                    hash = CalculateHash32(line);
                }
                else if (algorithm == HashingAlgorithm.Fnv1a)
                {
                    hash = CalculateHash64(line);
                }

                // either a hash collision or double file entry
                if (!Files.ContainsKey(hash))
                {
                    Files.Add(hash, line);
                }
            }
        }

        // CRC32
        public uint CalculateHash32(string name)
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

        // FNV1A
        public ulong CalculateHash64(string name)
        {
            name = name.ToLower();

            ulong hash = 0xcbf29ce484222325;

            foreach (var rune in name)
            {
                hash = hash ^ rune;
                hash *= 0x100000001b3;
            }

            return hash;
        }
    }

    public enum HashingAlgorithm
    {
        Crc32,
        Fnv1a
    }
}
