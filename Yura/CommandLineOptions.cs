using System;
using System.IO;

namespace Yura
{
    internal class CommandLineOptions : IFileSettings
    {
        private string[] _args;

        public CommandLineOptions(string[] args)
        {
            _args = args;
        }

        public string GetOption(string option)
        {
            for (var i = 0; i < _args.Length; i++)
            {
                if (_args[i] == option && _args.Length > i + 1)
                {
                    return _args[i + 1];
                }
            }

            return null;
        }

        public bool HasOption(string option)
        {
            return !string.IsNullOrEmpty(GetOption(option));
        }

        public string Bigfile
        {
            get
            {
                // always the last argument
                return _args[_args.Length - 1];
            }
        }

        public Game Game
        {
            get
            {
                if (Enum.TryParse(typeof(Game), GetOption("-game"), true, out var game))
                {
                    return (Game)game;
                }

                return Game.Legend;
            }
        }

        public bool LittleEndian
        {
            get
            {
                return GetOption("-endianness") != "big";
            }
        }

        public int Alignment
        {
            get
            {
                if (int.TryParse(GetOption("-alignment"), out var alignment))
                {
                    return alignment;
                }

                return 0x9600000;
            }
        }

        public string FileList
        {
            get
            {
                var list = GetOption("-filelist");
                var file = Path.Combine(AppContext.BaseDirectory, "FileLists", list + ".txt");

                if (File.Exists(file))
                {
                    return file;
                }

                return null;
            }
        }

        public TextureFormat TextureFormat
        {
            get
            {
                if (Enum.TryParse(typeof(TextureFormat), GetOption("-platform"), true, out var game))
                {
                    return (TextureFormat)game;
                }

                return TextureFormat.Pc;
            }
        }
    }
}
