using System.IO;
using Yura.Shared.IO;

namespace Yura.Test
{
    internal class WriterTests
    {
        [Test]
        public void TestSeek()
        {
            var stream = new MemoryStream();
            var writer = new DataWriter(stream, Endianness.LittleEndian);

            writer.Position = 8;

            Assert.That(writer.Position, Is.EqualTo(8));
        }

        [Test]
        public void TestLittleEndian()
        {
            var stream = new MemoryStream();
            var writer = new DataWriter(stream, Endianness.LittleEndian);

            writer.Write(123);

            Assert.That(stream.ToArray(), Is.EquivalentTo((byte[])[0x7B, 0x00, 0x00, 0x00]));
        }

        [Test]
        public void TestBigEndian()
        {
            var stream = new MemoryStream();
            var writer = new DataWriter(stream, Endianness.BigEndian);

            writer.Write(123);

            Assert.That(stream.ToArray(), Is.EquivalentTo((byte[])[0x00, 0x00, 0x00, 0x7B]));
        }
    }
}
