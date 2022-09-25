using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yura.Archive;
using Yura.Formats;

namespace Yura
{
    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public ArchiveFile Archive { get; set; }

        public bool LittleEndian { get; set; }

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Archive == null)
            {
                MessageBox.Show(this, "No bigfile is open, open a bigfile under 'File > Open' and reopen this window.", "No bigfile open", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(IdField.Text, out var id))
            {
                MessageBox.Show(this, "Entered ID must be a number.", "Invalid ID", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // get all drm files
            var files = Archive.Records.Where(x => x.Name != null && x.Name.EndsWith(".drm"));

            var sectionType = TypeSelect.SelectedIndex switch
            {
                0 => SectionType.Texture,
                1 => SectionType.Wave,
                2 => SectionType.Animation,
                3 => SectionType.Dtp
            };

            SearchResults.Items.Clear();
            Progress.Value = 0;
            Progress.Maximum = files.Count();

            Task.Run(() => SearchTask(files, id, sectionType));
        }

        private void SearchTask(IEnumerable<ArchiveRecord> files, int id, SectionType type)
        {
            foreach (var file in files)
            {
                var content = Archive.Read(file);

                // check drm version
                if (BitConverter.ToUInt32(content) != 14)
                {
                    MessageBox.Show("This game is currently not supported for deep search.", "Game not supported", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }

                // read drm file
                var drm = new DrmFile(content, LittleEndian);

                // check sections
                foreach (var section in drm.Sections)
                {
                    // check if section id matches and type
                    if (section.Type == type && section.Id == id)
                    {
                        var result = new SearchResult { File = file.Name, Section = section.Index };

                        // update on UI thread
                        Dispatcher.Invoke(() => SearchResults.Items.Add(result));
                    }
                }

                // update progress
                Dispatcher.Invoke(() => Progress.Value++);
            }
        }

        class SearchResult
        {
            public string File { get; set; }
            public int Section { get; set; }
        }
    }
}
