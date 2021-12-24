using System;
using System.IO;

namespace Yura.IO
{
    /// <summary>
    /// BinaryReader-like class with support for different endianness
    /// </summary>
    class StreamReader
    {
        private Stream _stream;
        private bool _littleEndian;

        /// <summary>
        /// Creates a new StreamReader from a stream
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="littleEndian">Whether the data should be read little endian</param>
        public StreamReader(Stream stream, bool littleEndian = true)
        {
            _stream = stream;
            _littleEndian = littleEndian;
        }

        /// <summary>
        /// Creates a new StreamReader from a buffer
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="littleEndian">Whether the data should be read little endian</param>
        public StreamReader(byte[] buffer, bool littleEndian = true)
        {
            _stream = new MemoryStream(buffer);
            _littleEndian = littleEndian;
        }

        private byte[] Read(int bytes)
        {
            var data = new byte[bytes];
            _stream.Read(data, 0, 4);

            // i guess this kinda assumes the platform is little endian
            if (!_littleEndian)
            {
                Array.Reverse(data);
            }

            return data;
        }

        public int ReadInt16()
        {
            var data = Read(2);

            return BitConverter.ToInt16(data);
        }

        public uint ReadUInt16()
        {
            var data = Read(2);

            return BitConverter.ToUInt16(data);
        }

        public int ReadInt32()
        {
            var data = Read(4);

            return BitConverter.ToInt32(data);
        }

        public uint ReadUInt32()
        {
            var data = Read(4);

            return BitConverter.ToUInt32(data);
        }

        public Stream BaseStream => _stream;
    }
}
