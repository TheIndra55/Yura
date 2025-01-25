using System.Collections.Generic;
using System.IO;
using Yura.Shared.Archive;

namespace Yura.Shared.Util
{
    public class FileList
    {
        private readonly Dictionary<ulong, string> _values;
        private readonly string _path;

        public FileList(string path, bool load)
        {
            _values = [];
            _path = path;

            if (load)
            {
                Load(HashAlgorithm.Crc32);
            }
        }

        /// <summary>
        /// Loads the file list with the specified hashing algorithm
        /// </summary>
        /// <param name="algorithm">The hashing algorithm</param>
        public void Load(HashAlgorithm algorithm)
        {
            foreach (var line in File.ReadLines(_path))
            {
                var hash = CalculateHash(line, algorithm);

                _values.TryAdd(hash, line);
            }
        }

        /// <summary>
        /// Resolves a hash
        /// </summary>
        /// <param name="hash">The hash to resolve</param>
        /// <returns>The resolved hash or null</returns>
        public string? Resolve(ulong hash)
        {
            if (_values.TryGetValue(hash, out var value))
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Resolves the hashes of all records
        /// </summary>
        /// <param name="records">The records to resolve the hashes for</param>
        public void Resolve(List<ArchiveRecord> records)
        {
            foreach (var record in records)
            {
                record.Name = Resolve(record.Hash);
            }
        }

        /// <summary>
        /// Calculates the hash of the value with the provided algorithm
        /// </summary>
        /// <param name="value">The value to calculate the hash of</param>
        /// <param name="algorithm">The hashing algorithm</param>
        /// <returns>The computed result</returns>
        public static ulong CalculateHash(string value, HashAlgorithm algorithm)
        {
            value = value.ToLower();

            return algorithm == HashAlgorithm.Crc32 ? CalculateHash32(value) : CalculateHash64(value);
        }

        private static uint CalculateHash32(string value)
        {
            uint hash = 0xffffffff;

            foreach (var c in value)
            {
                hash ^= (uint)c << 24;

                for (int i = 0; i < 8; i++)
                {
                    if ((hash & 0x80000000) != 0)
                        hash = (hash << 1) ^ 0x4c11db7;
                    else
                        hash <<= 1;
                }
            }

            return ~hash;
        }

        private static ulong CalculateHash64(string value)
        {
            ulong hash = 0xcbf29ce484222325;

            foreach (var c in value)
            {
                hash ^= c;
                hash *= 0x100000001b3;
            }

            return hash;
        }
    }

    public enum HashAlgorithm
    {
        Crc32,
        Fnv1a
    }
}
