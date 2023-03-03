using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Yura.Archive;
using Yura.Formats;

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
        private BitmapImage _stackIcon;
        private BitmapImage _binaryIcon;
        private BitmapImage _classIcon;
        private BitmapImage _imageIcon;
        private BitmapImage _soundIcon;
        private BitmapImage _textIcon;

        // dictionary of ext,file type for names and icons
        private Dictionary<string, Tuple<string, BitmapImage>> _fileTypes;

        private bool _littleEndian;
        private TextureFormat _textureFormat;
        private Game _currentGame;

        // the current open bigfile
        private ArchiveFile _bigfile;

        public MainWindow()
        {
            InitializeComponent();

            // icons from VS2019 icon library
            _folderIcon = new BitmapImage(new Uri("/Images/FolderClosed.png", UriKind.Relative));
            _archiveIcon = new BitmapImage(new Uri("/Images/ZipFile.png", UriKind.Relative));
            _stackIcon = new BitmapImage(new Uri("/Images/ImageStack.png", UriKind.Relative));
            _binaryIcon = new BitmapImage(new Uri("/Images/BinaryFile.png", UriKind.Relative));
            _classIcon = new BitmapImage(new Uri("/Images/ClassFile.png", UriKind.Relative));
            _imageIcon = new BitmapImage(new Uri("/Images/Image.png", UriKind.Relative));
            _soundIcon = new BitmapImage(new Uri("/Images/SoundFile.png", UriKind.Relative));
            _textIcon = new BitmapImage(new Uri("/Images/TextFile.png", UriKind.Relative));

            _fileTypes = new Dictionary<string, Tuple<string, BitmapImage>>
            {
                // game formats
                { ".drm", new Tuple<string, BitmapImage>("Data Ram", _classIcon) },
                { ".vrm", new Tuple<string, BitmapImage>("Video Ram", _stackIcon) },
                { ".mul", new Tuple<string, BitmapImage>("MultiplexStream", _soundIcon) },
                { ".raw", new Tuple<string, BitmapImage>("RAW Image", _imageIcon) },
                { ".mus", new Tuple<string, BitmapImage>("Music", _soundIcon) },
                { ".sam", new Tuple<string, BitmapImage>("Music Sample", _soundIcon) },
                { ".ids", new Tuple<string, BitmapImage>("IDMap", _textIcon) },
                { ".sch", new Tuple<string, BitmapImage>("SchemaFile", _textIcon) },
                { ".tfb", new Tuple<string, BitmapImage>("PadShock Library", _binaryIcon) },

                // regular files
                { ".txt",  new Tuple<string, BitmapImage>("Text File", _textIcon) },
                { ".ini",  new Tuple<string, BitmapImage>("Text File", _textIcon) },
                { ".json", new Tuple<string, BitmapImage>("JavaScript Object Notation", _textIcon) },
                { ".csv",  new Tuple<string, BitmapImage>("Comma Separated Values", _textIcon) },
                { ".ico",  new Tuple<string, BitmapImage>("Icon", _imageIcon) },
                { ".png",  new Tuple<string, BitmapImage>("Portable Network Graphics", _imageIcon) },

                // Wwise
                { ".bnk", new Tuple<string, BitmapImage>("Wwise SoundBank", _soundIcon) },
                { ".wem", new Tuple<string, BitmapImage>("Wwise Encoded Media", _soundIcon) },

                // Wii files (Reference: https://wiki.tockdom.com/wiki/Main_Page)
                { ".tpl", new Tuple<string, BitmapImage>("Texture Palette Library", _imageIcon) },
                { ".arc", new Tuple<string, BitmapImage>("GameCube Archive", _archiveIcon) },
                { ".brsar", new Tuple<string, BitmapImage>("Binary Revolution Sound Archive", _archiveIcon)  }
            };
        }

        /// <summary>
        /// Gets the current open bigfile
        /// </summary>
        public ArchiveFile Bigfile
        {
            get
            {
                return _bigfile;
            }
        }

        /// <summary>
        /// Gets the current game
        /// </summary>
        public Game Game
        {
            get
            {
                return _currentGame;
            }
        }

        /// <summary>
        /// Opens the open bigfile dialog with a bigfile
        /// </summary>
        /// <param name="bigfile">The bigfile to open</param>
        public void OpenBigfileDialog(string bigfile)
        {
            // dialog to set file list, endianness and alignment
            var dialog = new OpenDialog() { Owner = this };

            if (Path.GetExtension(bigfile) == ".tiger")
            {
                dialog.Game = Game.Tiger;
            }

            if (dialog.ShowDialog() == true)
            {
                _littleEndian = dialog.LittleEndian;
                _textureFormat = dialog.TextureFormat;

                OpenBigfile(bigfile, dialog.Game, dialog.FileList, dialog.Alignment);
            }
        }

        private void OpenBigfile(string bigfile, Game game, string fileList, int alignment)
        {
            var list = (fileList == null) ? null : new FileList(fileList, game != Game.Tiger);

            switch(game)
            {
                case Game.Legend:
                    _bigfile = new LegendArchive(bigfile, alignment, _littleEndian);
                    break;
                case Game.DeusEx:
                    _bigfile = new DeusExArchive(bigfile, _littleEndian);
                    break;
                case Game.Defiance:
                    _bigfile = new DefianceArchive(bigfile, _littleEndian);
                    break;
                case Game.Tiger:
                    _bigfile = new TigerArchive(bigfile, _littleEndian);
                    break;
                default:
                    MessageBox.Show(this, "You did not select a game, make sure to select one using the 'Game' dropdown.", "No game selected", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
            }

            _currentGame = game;
            _bigfile.FileList = list;

            PathBox.Text = Path.GetFileName(bigfile);

            try
            {
                _bigfile.Open();
            }
            catch (Exception e)
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
            foreach (var file in _bigfile.Records)
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
            SwitchDirectory(folder.Path == null ? "\\" : folder.Path + "\\");
        }

        public void SwitchDirectory(string path, string selectedFile = null)
        {
            var files = _bigfile.GetFolder(path);
            var bigfile = Path.GetFileName(_bigfile.Filename);

            if (path[0] == '\\')
            {
                PathBox.Text = bigfile;
            }
            else
            {
                PathBox.Text = bigfile + "\\" + path;
            }

            ShowFiles(files, selectedFile);
        }

        private void ShowFiles(List<ArchiveRecord> files, string selectedFile = null)
        {
            var filesview = new List<FileViewFile>();
            FileViewFile selected = null;

            foreach (var file in files)
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

                if (_currentGame >= Game.Legend)
                {
                    view.SpecMask = GetSpecMask(file);
                }

                if (name == selectedFile)
                {
                    selected = view;
                }

                filesview.Add(view);
            }

            FileView.ItemsSource = filesview;
            FileView.Items.SortDescriptions.Clear();

            if (selected != null)
            {
                FileView.SelectedItem = selected;
                FileView.ScrollIntoView(selected);
            }
        }

        private string GetSpecMask(ArchiveRecord record)
        {
            var specMask = _bigfile.GetSpecialisationMask(record);

            switch ((SpecMaskView) Properties.Settings.Default.SpecMaskView)
            {
                default:
                case SpecMaskView.Flags:
                    // unset remaining bits to hide noise
                    specMask &= ~(uint)0x7fffffe0;

                    return ((SpecialisationFlags)specMask).ToString();

                case SpecMaskView.Bits:
                    specMask &= ~(uint)0x7fffffe0;

                    return GetBinaryRepresentation(specMask);

                case SpecMaskView.Hex:
                    return specMask.ToString("X");
            }
        }

        private string GetBinaryRepresentation(uint number)
        {
            var builder = new StringBuilder();

            for (var i = 31; i >= 0; i--)
            {
                builder.Append((number & (1 << i)) != 0 ? "1" : "0");
            }

            return builder.ToString();
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
            var clickAction = (DoubleClickAction)Properties.Settings.Default.ClickAction;

            if (item == null)
            {
                return;
            }

            if (clickAction == DoubleClickAction.Export)
            {
                ExportFile(item);

                return;
            }

            var file = _bigfile.Read(item.File);
            var size = item.File.Size;

            // if RAW magic open in texture viewer
            if (size > 4 && file[0] == 33 && file[1] == 'W' && file[2] == 'A' && file[3] == 'R')
            {
                var viewer = new TextureViewer();
                viewer.TextureFormat = _textureFormat;
                viewer.LittleEndian = _littleEndian;

                viewer.Texture = file;
                viewer.Title = item.Name;

                viewer.Show();

                return;
            }

            if (item.Name == "locals.bin")
            {
                var viewer = new LocaleViewer();
                viewer.LittleEndian = _littleEndian;
                viewer.Data = file;

                viewer.Show();

                return;
            }

            // no yura previews left, check if user setting wants to open file or export
            if (clickAction == DoubleClickAction.PreviewFile)
            {
                ExportFile(item);

                return;
            }

            // no file handler, open file with default Windows file handler
            var path = Path.Combine(Path.GetTempPath(), "Yura", item.Name);
            Directory.CreateDirectory(Path.GetTempPath() + "\\Yura");

            try
            {
                File.WriteAllBytes(path, file);

                Process.Start("explorer", "\"" + path + "\"");
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Failed to write file", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void ContextMenu_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            ExportBtn.IsEnabled = !(FileView.SelectedItem == null);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            // export multiple files
            if (FileView.SelectedItems.Count > 1)
            {
                // since WPF still does not have FolderBrowserDialog in 2021
                var dialog = new System.Windows.Forms.FolderBrowserDialog();

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach(var item in FileView.SelectedItems.Cast<FileViewFile>())
                    {
                        var path = Path.Combine(dialog.SelectedPath, item.Name);

                        var file = _bigfile.Read(item.File);

                        try
                        {
                            File.WriteAllBytes(path, file);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Failed to export files", MessageBoxButton.OK, MessageBoxImage.Warning);

                            break;
                        }
                    }
                }
            }
            else
            {
                // export single file
                var item = FileView.SelectedItem as FileViewFile;

                ExportFile(item);
            }
        }

        private void ExportFile(FileViewFile item)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = item.Name;

            if (dialog.ShowDialog() == true)
            {
                var file = _bigfile.Read(item.File);

                try
                {
                    File.WriteAllBytes(dialog.FileName, file);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message, "Failed to write file", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Bigfile|*.000;*.dat;*.000.wii-w;*.000.tiger";
            dialog.Title = "Select bigfile";

            if (dialog.ShowDialog() == true)
            {
                OpenBigfileDialog(dialog.FileName);
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;

            var currentSort = FileView.Items.SortDescriptions.FirstOrDefault();
            var direction = ListSortDirection.Ascending;

            if (currentSort != default)
            {
                direction = currentSort.Direction == ListSortDirection.Descending ? ListSortDirection.Ascending : ListSortDirection.Descending;

                FileView.Items.SortDescriptions.Clear();
            }

            FileView.Items.SortDescriptions.Add(new SortDescription((string)column.Tag, direction));
        }

        private void SearchCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var searchWindow = new SearchWindow() { Owner = this };
            searchWindow.Archive = _bigfile;
            searchWindow.LittleEndian = _littleEndian;

            searchWindow.Show();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void SettingsCommand_Executed(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow() { Owner = this };

            settings.ShowDialog();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var query = SearchBox.Text.ToLower();

                if (_bigfile == null)
                {
                    MessageBox.Show("No bigfile is open, open a bigfile under 'File > Open'", "No bigfile open", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // find all files
                var results = _bigfile.Records.Where(record =>
                {
                    string filename = null;

                    if (record.Name != null)
                    {
                        // extract filename from full path
                        filename = record.Name.Split("\\", StringSplitOptions.RemoveEmptyEntries).Last();
                    }
                    else
                    {
                        filename = record.Hash.ToString("X");
                    }

                    // simple contains query
                    return filename.ToLower().Contains(query);
                }).ToList();

                // display results in file view
                ShowFiles(results);

                PathBox.Text = "Search results for " + query;

                // TODO clear selection in directory view
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
        public string SpecMask { get; set; }

        public ArchiveRecord File { get; set; }
    }

    public enum SpecMaskView
    {
        Flags,
        Bits,
        Hex
    }

    public enum DoubleClickAction
    {
        OpenFile,
        PreviewFile,
        Export
    }
}
