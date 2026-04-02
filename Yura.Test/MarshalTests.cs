using System;
using System.Runtime.InteropServices;
using Yura.Shared.IO;

namespace Yura.Test
{
    internal class MarshalTests
    {
        private static int Size => Marshal.SizeOf<SomeHeader>();

        [Test]
        public void TestMarshal()
        {
            var wrote = new SomeHeader
            {
                Version = 67,
                NumFiles = 1234,
                NumFolders = 12,
                Flags = 255
            };

            Span<byte> data = stackalloc byte[Size];
            DataMarshal.Write(data, wrote, Endianness.LittleEndian);

            Assert.That(data.ToArray(), Is.EquivalentTo((byte[])[0x43, 0x00, 0x00, 0x00, 0xD2, 0x04, 0x0C, 0x00, 0xFF, 0x00, 0x00, 0x00]));

            // And now try to read it back
            var read = DataMarshal.Read<SomeHeader>(data, Endianness.LittleEndian);

            Assert.That(read.Version, Is.EqualTo(67));
            Assert.That(read.NumFiles, Is.EqualTo(1234));
            Assert.That(read.NumFolders, Is.EqualTo(12));
            Assert.That(read.Flags, Is.EqualTo(255));
        }

        [Test]
        public void TestEndianness()
        {
            var wrote = new SomeHeader
            {
                Version = 67,
                NumFiles = 1234,
                NumFolders = 12,
                Flags = 255
            };

            Span<byte> data = stackalloc byte[Size];
            DataMarshal.Write(data, wrote, Endianness.BigEndian);

            Assert.That(data.ToArray(), Is.EquivalentTo((byte[])[0x00, 0x00, 0x00, 0x43, 0x04, 0xD2, 0x00, 0x0C, 0x00, 0x00, 0x00, 0xFF]));

            // And now try to read it back
            var read = DataMarshal.Read<SomeHeader>(data, Endianness.BigEndian);

            Assert.That(read.Version, Is.EqualTo(67));
            Assert.That(read.NumFiles, Is.EqualTo(1234));
            Assert.That(read.NumFolders, Is.EqualTo(12));
            Assert.That(read.Flags, Is.EqualTo(255));
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SomeHeader
        {
            public uint Version;
            public ushort NumFiles;
            public ushort NumFolders;
            public byte Flags;
        }
    }
}
