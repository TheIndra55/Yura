using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            _stream.Read(data, 0, bytes);

            // i guess this kinda assumes the platform is little endian
            if (!_littleEndian)
            {
                Array.Reverse(data);
            }

            return data;
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public short ReadInt16()
        {
            var data = Read(2);

            return BitConverter.ToInt16(data);
        }

        public ushort ReadUInt16()
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

        /// <summary>
        /// Reads a null-terminated string from the stream
        /// </summary>
        /// <returns>The readed string</returns>
        public string ReadString()
        {
            var chars = new List<byte>();

            while(_stream.ReadByte() != 0)
            {
                _stream.Position--;
                chars.Add((byte)_stream.ReadByte());
            }

            return Encoding.UTF8.GetString(chars.ToArray());
        }

        public Stream BaseStream => _stream;
    }
}
