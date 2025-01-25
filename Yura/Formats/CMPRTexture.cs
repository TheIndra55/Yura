using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yura.Shared.IO;

namespace Yura.Formats
{
    // Texture format used for Wii RAW images
    class CMPRTexture : BitmapSource
    {
        private int _width;
        private int _height;
        private byte[] _buffer;

        public CMPRTexture(int width, int height, byte[] buffer)
        {
            _width = width;
            _height = height;
            _buffer = buffer;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CMPRTexture(_width, _height, _buffer);
        }

        // thanks to https://github.com/zurgeg/riivolution/blob/master/consolehaxx/ConsoleHaxx.Wii/PixelFormats/CMPR.cs#L36
        // most of the code below for CMPR algo is copied from there
        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int _)
        {
            var reader = new DataReader(_buffer, Endianness.LittleEndian);

            for (int y = 0; y < _height; y += 8)
            {
                for (int x = 0; x < _width; x += 8)
                {
                    for (int y2 = 0; y2 < 8; y2 += 4)
                    {
                        for (int x2 = 0; x2 < 8; x2 += 4)
                        {
                            var c0 = reader.ReadUInt16();
                            var c1 = reader.ReadUInt16();
                            var bits = reader.ReadUInt32();

                            ConvertRgb565(c0, out var r0, out var g0, out var b0);
                            ConvertRgb565(c1, out var r1, out var g1, out var b1);

                            for (int y3 = 3; y3 >= 0; y3--)
                            {
                                for (int x3 = 3; x3 >= 0; x3--)
                                {
                                    int newx = x + x2 + x3;
                                    int newy = y + y2 + y3;

                                    uint control = bits & 3;
                                    bits >>= 2;

                                    byte r = 0, g = 0, b = 0, a = 255;

                                    switch (control)
                                    {
                                        case 0:
                                            r = r0;
                                            g = g0;
                                            b = b0;

                                            break;
                                        case 1:
                                            r = r1;
                                            g = g1;
                                            b = b1;

                                            break;
                                        case 2:
                                            if (c0 > c1)
                                            {
                                                r = (byte)((2 * r0 + r1) / 3);
                                                g = (byte)((2 * g0 + g1) / 3);
                                                b = (byte)((2 * b0 + b1) / 3);
                                            }
                                            else
                                            {
                                                r = (byte)((r0 + r1) / 2);
                                                g = (byte)((g0 + g1) / 2);
                                                b = (byte)((b0 + b1) / 2);
                                            }

                                            break;
                                        case 3:
                                            if (c0 > c1)
                                            {
                                                r = (byte)((r0 + 2 * r1) / 3);
                                                g = (byte)((g0 + 2 * g1) / 3);
                                                b = (byte)((b0 + 2 * b1) / 3);
                                            }
                                            else
                                            {
                                                r = 0;
                                                g = 0;
                                                b = 0;
                                            }

                                            break;
                                    }

                                    if ((newx < _width) && (newy < _height))
                                    {
                                        int offset = ((newy * _width) + newx) << 2;

                                        pixels.SetValue(b, offset);
                                        pixels.SetValue(g, offset + 1);
                                        pixels.SetValue(r, offset + 2);
                                        pixels.SetValue(a, offset + 3);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ConvertRgb565(ushort color, out byte r, out byte g, out byte b)
        {
            r = (byte)(((color >> 11) & 0x1f) << 3);
            g = (byte)(((color >> 5) & 0x3f) << 2);
            b = (byte)((color & 0x1f) << 3);
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
