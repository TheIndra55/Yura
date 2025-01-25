using System.IO;
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
    }
}
