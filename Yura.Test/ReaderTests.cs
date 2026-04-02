using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Yura.Shared.IO;

namespace Yura.Test
{
    internal class ReaderTests
    {
        [Test]
        public void TestSeek()
        {
            var stream = new MemoryStream();
            var reader = new DataReader(stream, Endianness.LittleEndian);

            reader.Position = 8;

            Assert.That(reader.Position, Is.EqualTo(8));
        }

        [Test]
        public void TestLittleEndian()
        {
            var stream = new MemoryStream([0x7B, 0x00, 0x00, 0x00]);
            var reader = new DataReader(stream, Endianness.LittleEndian);

            Assert.That(reader.ReadUInt32(), Is.EqualTo(123));
        }

        [Test]
        public void TestBigEndian()
        {
            var stream = new MemoryStream([0x00, 0x00, 0x00, 0x7B]);
            var reader = new DataReader(stream, Endianness.BigEndian);

            Assert.That(reader.ReadUInt32(), Is.EqualTo(123));
        }

        [Test]
        public void TestString()
        {
            var stream = new MemoryStream([0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00]);
            var reader = new DataReader(stream, Endianness.BigEndian);

            Assert.That(reader.ReadString(Encoding.ASCII), Is.EqualTo("Hello"));
        }

        [Test]
        public void TestStruct()
        {
            var stream = new MemoryStream([0x0C, 0x00, 0x0D, 0x00]);
            var reader = new DataReader(stream, Endianness.LittleEndian);

            var structure = reader.ReadStruct<Data>();

            Assert.That(structure.Version, Is.EqualTo(12));
            Assert.That(structure.Id, Is.EqualTo(13));
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Data
        {
            public ushort Version;
            public ushort Id;
        }
    }
}
