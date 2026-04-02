using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Yura.Shared.IO
{
    internal static class DataMarshal
    {
        // This method may modify the original span if it needs to swap endianness
        // which seems acceptable since all consumers pretty much throw away the buffer after
        // and if the caller really needs to it can copy it to another buffer
        public static T Read<T>(Span<byte> source, Endianness endianness) where T : struct
        {
            if (endianness == Endianness.BigEndian)
            {
                // Reverse the endianness for all structs fields
                ReverseEndianness<T>(source);
            }

            return MemoryMarshal.Read<T>(source);
        }

        public static void Write<T>(Span<byte> destination, T value, Endianness endianness) where T : struct
        {
            MemoryMarshal.Write(destination, value);

            if (endianness == Endianness.BigEndian)
            {
                // Reverse the endianness for all structs fields
                ReverseEndianness<T>(destination);
            }
        }

        private static void ReverseEndianness<T>(Span<byte> source)
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Instance))
            {
                var offset = Marshal.OffsetOf<T>(field.Name).ToInt32();
                var size = Marshal.SizeOf(field.FieldType);

                source.Slice(offset, size).Reverse();
            }
        }
    }
}
