﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamReader = Yura.IO.StreamReader;

namespace Yura.Archive
{
    class LegendArchive : ArchiveFile
    {
        private string _file;
        private int _alignment;
        private bool _littleEndian;

        private List<LegendRecord> _files;

        public LegendArchive(string path, int alignment, bool littleEndian = true)
        {
            _file = path;
            _littleEndian = littleEndian;
            _alignment = alignment;

            _files = new List<LegendRecord>();
        }

        public override void Open()
        {
            var stream = File.OpenRead(_file);
            var reader = new StreamReader(stream, _littleEndian);

            // extract number of files
            var numRecords = reader.ReadUInt32();
            var hashes = new uint[numRecords];

            // read all filename hashes
            for (var i = 0; i < numRecords; i++)
            {
                hashes[i] = reader.ReadUInt32();
            }

            // read all records
            for (var i = 0; i < numRecords; i++)
            {
                var file = new LegendRecord()
                {
                    Hash = hashes[i]
                };

                file.Size = reader.ReadUInt32();
                file.Offset = reader.ReadUInt32();

                // check if hash exist in file list
                if (FileList != null && FileList.Files.TryGetValue(file.Hash, out string name))
                {
                    file.Name = name;
                }

                _files.Add(file);

                reader.BaseStream.Position += 8;
            }
        }

        public override IReadOnlyList<ArchiveRecord> Records
        {
            get
            {
                return _files;
            }
        }

        public override byte[] Read(ArchiveRecord record)
        {
            var file = record as LegendRecord;

            var align = _alignment >> 11;
            var bigfile = file.Offset / align;

            string filename;
            if (_file.EndsWith(".000"))
            {
                var name = Path.GetFileNameWithoutExtension(_file);
                filename = Path.GetDirectoryName(_file) + Path.DirectorySeparatorChar + name + "." + bigfile.ToString("000");
            }
            else
            {
                filename = _file;

                // one file, set alignment to that file size
                _alignment = (int)new FileInfo(_file).Length;
                align = _alignment >> 11;
            }

            var stream = File.OpenRead(filename);
            var bytes = new byte[file.Size];

            stream.Position = file.Offset % align << 11;
            stream.Read(bytes, 0, (int)file.Size);

            return bytes;
        }
    }

    class LegendRecord : ArchiveRecord
    {
        public uint Offset { get; set; }
    }
}