namespace Yura
{
    public interface IFileSettings
    {
        public Game Game { get; }
        public bool LittleEndian { get; }
        public int Alignment { get; }
        public string FileList { get; }
        public TextureFormat TextureFormat { get; }
    }
}
