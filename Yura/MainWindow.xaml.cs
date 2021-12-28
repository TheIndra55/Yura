using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Yura.Archive;

namespace Yura
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // icons, todo better way for this?
        private BitmapImage _folderIcon;
        private BitmapImage _archiveIcon;
        private BitmapImage _binaryIcon;
        private BitmapImage _classIcon;
        private BitmapImage _imageIcon;
        private BitmapImage _soundIcon;
        private BitmapImage _textIcon;

        // dictionary of ext,file type for names and icons
        private Dictionary<string, Tuple<string, BitmapImage>> _fileTypes;

        private bool _littleEndian;
        private TextureFormat _textureFormat;

        // the current open bigfile
        private Bigfile _bigfile;

        public MainWindow()
        {
            InitializeComponent();

            // icons from VS2019 icon library
            _folderIcon = new BitmapImage(new Uri("/Images/FolderClosed.png", UriKind.Relative));
            _archiveIcon = new BitmapImage(new Uri("/Images/ZipFile.png", UriKind.Relative));
            _binaryIcon = new BitmapImage(new Uri("/Images/BinaryFile.png", UriKind.Relative));
            _classIcon = new BitmapImage(new Uri("/Images/ClassFile.png", UriKind.Relative));
            _imageIcon = new BitmapImage(new Uri("/Images/Image.png", UriKind.Relative));
            _soundIcon = new BitmapImage(new Uri("/Images/SoundFile.png", UriKind.Relative));
            _textIcon = new BitmapImage(new Uri("/Images/TextFile.png", UriKind.Relative));

            _fileTypes = new Dictionary<string, Tuple<string, BitmapImage>>
            {
                { ".drm", new Tuple<string, BitmapImage>("Data Ram", _classIcon) },
                { ".mul", new Tuple<string, BitmapImage>("MultiplexStream", _soundIcon) },
                { ".raw", new Tuple<string, BitmapImage>("RAW Image", _imageIcon) },
                { ".mus", new Tuple<string, BitmapImage>("Music", _soundIcon) },
                { ".sam", new Tuple<string, BitmapImage>("Music Sample", _soundIcon) },
                { ".ids", new Tuple<string, BitmapImage>("IDMap", _textIcon) },
                { ".txt", new Tuple<string, BitmapImage>("Text File", _textIcon) },
                { ".sch", new Tuple<string, BitmapImage>("SchemaFile", _textIcon) }
            };
        }

        private void OpenBigfileDialog(string bigfile)
        {
            // dialog to set file list, endianness and alignment
            var dialog = new OpenDialog();

            if (dialog.ShowDialog() == true)
            {
                _littleEndian = dialog.LittleEndian;
                _textureFormat = dialog.TextureFormat;

                OpenBigfile(bigfile, dialog.FileList, dialog.Alignment);
            }
        }

        private void OpenBigfile(string bigfile, string fileList, int alignment)
        {
            var list = (fileList == null) ? null : new FileList(fileList);

            _bigfile = new Bigfile(bigfile, list, _littleEndian);
            _bigfile.Alignment = alignment;

            try
            {
                _bigfile.Open();
            }
            catch(Exception e)
            {
                MessageBox.Show(this, e.Message, "Failed to open bigfile", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            UpdateTree();
        }

        private void UpdateTree()
        {
            var root = new DirectoryViewFolder();
            root.Name = "Bigfile";
            root.Image = _archiveIcon;
            root.Subfolders = new();

            DirectoryView.ItemsSource = new List<DirectoryViewFolder>() { root };

            // try to create a tree from all file paths
            foreach (var file in _bigfile.Files)
            {
                if (file.Name == null)
                {
                    continue;
                }

                var hierarchy = file.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries);
                var parent = root;

                var i = 1;
                foreach (var sub in hierarchy.Take(hierarchy.Length - 1))
                {
                    var path = hierarchy.Take(i).ToArray();

                    var folder = parent.TryCreateFolder(sub, path, _folderIcon);
                    parent = folder;

                    i++;
                }
            }
        }

        private void DirectoryView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var folder = e.NewValue as DirectoryViewFolder;

            // if path is null then get root
            var files = _bigfile.GetFolder(folder.Path == null ? "\\" : folder.Path + "\\");
            var filesview = new List<FileViewFile>();

            foreach(var file in files)
            {
                var type = GetFileType(Path.GetExtension(file.Name ?? ""));

                string name;
                if (file.Name == null)
                {
                    name = file.Hash.ToString("X");
                }
                else
                {
                    name = file.Name.Split("\\").Last();
                }

                var view = new FileViewFile
                {
                    Name = name,
                    Type = type.Item1,
                    Size = file.Size,

                    Image = type.Item2,
                    File = file
                };

                filesview.Add(view);
            }

            FileView.ItemsSource = filesview;
        }

        private Tuple<string, BitmapImage> GetFileType(string ext)
        {
            if (_fileTypes.TryGetValue(ext, out var type))
            {
                return type;
            }

            return new Tuple<string, BitmapImage>("File", _binaryIcon);
        }

        private void FileView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = FileView.SelectedItem as FileViewFile;

            if (item == null)
            {
                return;
            }

            var file = _bigfile.OpenFile(item.File);

            // if RAW magic open in texture viewer
            if (file[0] == 33 && file[1] == 'W' && file[2] == 'A' && file[3] == 'R')
            {
                var viewer = new TextureViewer();
                viewer.TextureFormat = _textureFormat;
                viewer.LittleEndian = _littleEndian;

                viewer.Texture = file;
                viewer.Title = item.Name;

                viewer.Show();

                return;
            }

            // no file handler, open file with default Windows file handler
            var path = Path.Combine(Path.GetTempPath(), "Yura", item.Name);
            Directory.CreateDirectory(Path.GetTempPath() + "\\Yura");

            File.WriteAllBytes(path, file);

            Process.Start("explorer", "\"" + path + "\"");
        }
        
        private void ContextMenu_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            ExportBtn.IsEnabled = !(FileView.SelectedItem == null);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var item = FileView.SelectedItem as FileViewFile;

            var dialog = new SaveFileDialog();
            dialog.FileName = item.Name;

            if (dialog.ShowDialog() == true)
            {
                // export file
                var file = _bigfile.OpenFile(item.File);
                File.WriteAllBytes(dialog.FileName, file);
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Bigfile|*.000;*.dat;*.000.wii-w";
            dialog.Title = "Select bigfile";

            if (dialog.ShowDialog() == true)
            {
                OpenBigfileDialog(dialog.FileName);
            }
        }
    }

    public class DirectoryViewFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Parents { get; set; }

        public BitmapImage Image { get; set; }

        public ObservableCollection<DirectoryViewFolder> Subfolders { get; set; }

        public DirectoryViewFolder TryCreateFolder(string name, string[] hierarchy, BitmapImage icon)
        {
            var sub = Subfolders.FirstOrDefault(x => x.Name == name);

            if (sub == null)
            {
                var folder = new DirectoryViewFolder();
                folder.Name = name;
                folder.Image = icon;
                folder.Path = string.Join("\\", hierarchy);
                folder.Subfolders = new();

                folder.Parents = hierarchy;

                Subfolders.Add(folder);

                return folder;
            }

            return sub;
        }
    }

    public class FileViewFile
    {
        public BitmapImage Image { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }
        public uint Size { get; set; }

        public ArchivedFile File { get; set; }
    }
}
