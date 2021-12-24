using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Yura
{
    /// <summary>
    /// Interaction logic for OpenDialog.xaml
    /// </summary>
    public partial class OpenDialog : Window
    {
        public OpenDialog()
        {
            InitializeComponent();

            // fetch all file lists
            var folder = Path.Combine(AppContext.BaseDirectory, "FileLists");

            var lists = new List<FileListItem>();
            foreach(var file in Directory.GetFiles(folder, "*.txt"))
            {
                var option = new FileListItem()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file
                };

                lists.Add(option);
            }

            FileListSelect.ItemsSource = lists;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public bool LittleEndian
        {
            get
            {
                return (EndiannessSelect.SelectedItem as ComboBoxItem).Content.ToString() == "Little-endian";
            }
        }

        public int Alignment
        {
            get
            {
                return Convert.ToInt32(AlignmentField.Text, 16);
            }
        }

        public string FileList
        {
            get
            {
                return (FileListSelect.SelectedItem as FileListItem)?.Path;
            }
        }
    }

    public class FileListItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
