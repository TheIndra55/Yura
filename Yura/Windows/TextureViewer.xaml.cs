﻿using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yura.Formats;
using Yura.Shared.IO;
using Yura.Shared.Util;

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
        
        private void CreateImage(int width, int height, DataReader reader)
        {
            int stride = width * 4 + (width % 4);

            var textureData = new byte[width + height * stride];
            reader.BaseStream.Read(textureData);

            BitmapSource image;

            switch (Platform)
            {
                case Platform.Wii:
                    image = new CMPRTexture(width, height, textureData);
                    break;

                case Platform.Ps3:
                    image = new CMPRTexture(width, height, textureData);
                    break;

                default:
                    image = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, textureData, stride);
                    break;
            }

            TextureImage.Source = image;
        }

        public Endianness Endianness { get; set; }

        public Platform Platform { get; set; }

        public byte[] Texture
        {
            set
            {
                var reader = new DataReader(value, Endianness);

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
