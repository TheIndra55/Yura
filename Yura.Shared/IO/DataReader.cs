using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Yura.Shared.IO
{
    /// <summary>
    /// Reads data types from a binary stream
    /// </summary>
    public class DataReader
    {
        private readonly Stream _stream;
        private readonly Endianness _endianness;

        /// <summary>
        /// Initializes a new data reader with the specified stream and endianness
        /// </summary>
        /// <param name="stream">The input stream</param>
        /// <param name="endianness">The endianness of the data</param>
        public DataReader(Stream stream, Endianness endianness)
        {
            ArgumentNullException.ThrowIfNull(stream);

            _stream = stream;
            _endianness = endianness;
        }

        /// <summary>
        /// Initializes a new data reader from a buffer
        /// </summary>
        /// <param name="data">The input buffer</param>
        /// <param name="endianness">The endianness of the data</param>
        public DataReader(byte[] data, Endianness endianness) : this(new MemoryStream(data), endianness)
        {
        }

        /// <summary>
        /// Gets the underlying stream
        /// </summary>
        public Stream BaseStream => _stream;

        /// <summary>
        /// Gets the endianness
        /// </summary>
        public Endianness Endianness => _endianness;

        /// <summary>
        /// Gets or sets the position within the stream
        /// </summary>
        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        /// <summary>
        /// Reads a number of bytes from the stream into the buffer
        /// </summary>
        /// <param name="data">The buffer to read into</param>
        /// <returns>The buffer</returns>
        private ReadOnlySpan<byte> InternalRead(Span<byte> data)
        {
            _stream.ReadExactly(data);

            // Reverse the buffer if the data is big endian
            if (_endianness == Endianness.BigEndian)
            {
                data.Reverse();
            }

            return data;
        }

        /// <summary>
        /// Reads a byte from the stream
        /// </summary>
        /// <returns>The read byte</returns>
        public byte ReadByte()
        {
            var value = _stream.ReadByte();

            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (byte)value;
        }

        /// <summary>
        /// Reads a signed 16-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public short ReadInt16()
        {
            return BitConverter.ToInt16(InternalRead(stackalloc byte[2]));
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public int ReadInt32()
        {
            return BitConverter.ToInt32(InternalRead(stackalloc byte[4]));
        }

        /// <summary>
        /// Reads a signed 64-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public long ReadInt64()
        {
            return BitConverter.ToInt64(InternalRead(stackalloc byte[8]));
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(InternalRead(stackalloc byte[2]));
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(InternalRead(stackalloc byte[4]));
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream
        /// </summary>
        /// <returns>The read integer</returns>
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(InternalRead(stackalloc byte[8]));
        }

        /// <summary>
        /// Reads a 32-bit floating point value from the stream
        /// </summary>
        /// <returns>The read value</returns>
        public float ReadSingle()
        {
            return BitConverter.ToSingle(InternalRead(stackalloc byte[4]));
        }

        /// <summary>
        /// Reads a null-terminated string from the stream
        /// </summary>
        /// <param name="encoding">The encoding of the string</param>
        /// <returns>The read string</returns>
        public string ReadString(Encoding encoding)
        {
            List<byte> data = [];

            byte value;
            while ((value = ReadByte()) != 0)
            {
                data.Add(value);
            }

            return encoding.GetString(data.ToArray());
        }
    }
}
