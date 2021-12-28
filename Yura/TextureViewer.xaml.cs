using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yura.Formats;
using StreamReader = Yura.IO.StreamReader;

namespace Yura
{
    /// <summary>
    /// Interaction logic for TextureViewer.xaml
    /// </summary>
    public partial class TextureViewer : Window
    {
        public TextureViewer()
        {
            InitializeComponent();
        }
        
        private void CreateImage(int width, int height, StreamReader reader)
        {
            int stride = width * 4 + (width % 4);

            var textureData = new byte[width + height * stride];
            reader.BaseStream.Read(textureData);

            BitmapSource image = null;

            if (TextureFormat == TextureFormat.Pc)
            {
                image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, textureData, stride);
            }
            else if (TextureFormat == TextureFormat.Wii)
            {
                image = new CMPRTexture(width, height, textureData);
            }
            else if (TextureFormat == TextureFormat.Ps3)
            {
                image = new PS3Texture(width, height, textureData);
            }

            TextureImage.Source = image;
        }

        public bool LittleEndian { get; set; }

        public TextureFormat TextureFormat { get; set; }

        public byte[] Texture
        {
            set
            {
                var reader = new StreamReader(value, LittleEndian);

                var magic = reader.ReadInt32();
                var start = reader.ReadInt32();
                reader.BaseStream.Position += 12;

                var width = reader.ReadInt32();
                var height = reader.ReadInt32();

                reader.BaseStream.Position = start;
                CreateImage(width, height, reader);
            }
        }
    }
}
