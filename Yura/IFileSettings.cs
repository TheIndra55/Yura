using Yura.Shared.IO;

namespace Yura
{
    public interface IFileSettings
    {
        public Game Game { get; }
        public Endianness Endianness { get; }
        public int Alignment { get; }
        public string FileList { get; }
        public TextureFormat TextureFormat { get; }
    }
}
