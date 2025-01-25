using Yura.Shared.IO;
using Yura.Shared.Util;

namespace Yura
{
    public interface IFileSettings
    {
        public Game Game { get; }
        public Endianness Endianness { get; }
        public int Alignment { get; }
        public string FileList { get; }
        public Platform Platform { get; }
    }
}
