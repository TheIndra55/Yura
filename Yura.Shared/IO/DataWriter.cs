using System;
using System.IO;
using System.Linq;

namespace Yura.Shared.IO
{
    /// <summary>
    /// Writes data types to a binary stream
    /// </summary>
    public class DataWriter
    {
        private readonly Stream _stream;
        private readonly Endianness _endianness;

        /// <summary>
        /// Initializes a new data writer with the the specified stream and endianness
        /// </summary>
        /// <param name="stream">The output stream</param>
        /// <param name="endianness">The endianness of the data</param>
        public DataWriter(Stream stream, Endianness endianness)
        {
            ArgumentNullException.ThrowIfNull(stream);

            _stream = stream;
            _endianness = endianness;
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
        /// Writes a number of bytes to the stream
        /// </summary>
        /// <param name="data">The data to write</param>
        private void InternalWrite(Span<byte> data)
        {
            // Reverse the buffer if the data is big endian
            if (_endianness == Endianness.BigEndian)
            {
                data.Reverse();
            }

            _stream.Write(data);
        }

        /// <summary>
        /// Writes a signed 16-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(short value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 32-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(int value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a signed 64-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(long value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(ushort value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(uint value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream
        /// </summary>
        /// <param name="value">The integer to write</param>
        public void Write(ulong value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Writes a 32-bit floating point value to the stream
        /// </summary>
        /// <param name="value">The value to write</param>
        public void Write(float value)
        {
            InternalWrite(BitConverter.GetBytes(value));
        }
    }
}
