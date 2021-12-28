using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Yura.Formats
{
    class PS3Texture : BitmapSource
    {
        private int _width;
        private int _height;
        private byte[] _buffer;

        public PS3Texture(int width, int height, byte[] buffer)
        {
            _width = width;
            _height = height;
            _buffer = buffer;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PS3Texture(_width, _height, _buffer);
        }

        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
        {
            var buffer = new byte[_buffer.Length];

            int i = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var index = Morton2D(x, y);

                    buffer[i++] = _buffer[index * 4];
                    buffer[i++] = _buffer[index * 4 + 1];
                    buffer[i++] = _buffer[index * 4 + 2];
                    buffer[i++] = _buffer[index * 4 + 3];
                }
            }

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    i = stride * y + 4 * x;

                    pixels.SetValue(buffer[i + 3], i);
                    pixels.SetValue(buffer[i + 2], i + 1);
                    pixels.SetValue(buffer[i + 1], i + 2);
                    pixels.SetValue(buffer[i], i + 3);
                }
            }
        }

        // https://stackoverflow.com/a/30562230/9398242
        private long Morton2D(int x, int y)
        {
            x = (x | (x << 16)) & 0x0000FFFF;
            x = (x | (x << 8)) & 0x00FF00FF;
            x = (x | (x << 4)) & 0x0F0F0F0F;
            x = (x | (x << 2)) & 0x33333333;
            x = (x | (x << 1)) & 0x55555555;

            y = (y | (y << 16)) & 0x0000FFFF;
            y = (y | (y << 8)) & 0x00FF00FF;
            y = (y | (y << 4)) & 0x0F0F0F0F;
            y = (y | (y << 2)) & 0x33333333;
            y = (y | (y << 1)) & 0x55555555;

            return x | (y << 1);
        }

#pragma warning disable CS0067
        public override event EventHandler<DownloadProgressEventArgs> DownloadProgress;
        public override event EventHandler DownloadCompleted;
        public override event EventHandler<ExceptionEventArgs> DownloadFailed;
        public override event EventHandler<ExceptionEventArgs> DecodeFailed;
#pragma warning restore

        public override PixelFormat Format
        {
            get
            {
                return PixelFormats.Bgra32;
            }
        }

        public override double DpiX
        {
            get
            {
                return 96;
            }
        }

        public override double DpiY
        {
            get
            {
                return 96;
            }
        }

        public override int PixelWidth
        {
            get
            {
                return _width;
            }
        }

        public override int PixelHeight
        {
            get
            {
                return _height;
            }
        }

        public override double Width
        {
            get
            {
                return _width;
            }
        }

        public override double Height
        {
            get
            {
                return _height;
            }
        }
    }
}
