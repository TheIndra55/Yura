using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Yura.Formats;

namespace Yura
{
    /// <summary>
    /// Interaction logic for LocaleViewer.xaml
    /// </summary>
    public partial class LocaleViewer : Window
    {
        private LocaleFile _locale;

        public LocaleViewer()
        {
            InitializeComponent();
        }

        public bool LittleEndian { get; set; }

        public byte[] Data
        {
            set
            {
                _locale = new LocaleFile(value, LittleEndian);

                // append current language to title
                Title += " - " + _locale.Language.ToString();

                // add all locale entries
                Entries.ItemsSource = _locale.Entries.Select((x, i) => new LocaleViewerEntry { Index = i, Value = x });
            }
        }

        private void ExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt";

            if (dialog.ShowDialog() == true)
            {
                // write all entries 'index = value' seperated by empty line
                File.WriteAllLines(dialog.FileName, _locale.Entries.Select((x, i) => $"{i} = {x}\n"));
            }
        }

        class LocaleViewerEntry
        {
            public int Index { get; set; }
            public string Value { get; set; }
        }
    }
}
