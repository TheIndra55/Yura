﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Yura.Shared.IO;
using Yura.Shared.Util;

namespace Yura
{
    /// <summary>
    /// Interaction logic for OpenDialog.xaml
    /// </summary>
    public partial class OpenDialog : Window, IFileSettings
    {
        public OpenDialog()
        {
            InitializeComponent();

            // fetch all file lists
            var folder = Path.Combine(AppContext.BaseDirectory, "FileLists");
            var lists = new List<FileListItem>();

            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder, "*.txt"))
                {
                    var option = new FileListItem()
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Path = file
                    };

                    lists.Add(option);
                }
            }

            FileListSelect.ItemsSource = lists;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public Endianness Endianness
        {
            get
            {
                return (Endianness)EndiannessSelect.SelectedIndex;
            }
        }

        public int Alignment
        {
            get
            {
                return Convert.ToInt32(AlignmentField.Text, 16);
            }
        }

        public Game Game
        {
            get
            {
                return (Game)GameSelect.SelectedIndex;
            }
            set
            {
                GameSelect.SelectedIndex = (int)value;

                AlignmentField.IsEnabled = value < Game.DeusEx;
            }
        }

        public Platform Platform
        {
            get
            {
                return (Platform)TextureFormatSelect.SelectedIndex;
            }
        }

        public string FileList
        {
            get
            {
                return (FileListSelect.SelectedItem as FileListItem)?.Path;
            }
        }

        public class FileListItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        private void GameSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlignmentField.IsEnabled = GameSelect.SelectedIndex <= (int)Game.Legend;
        }
    }

    public enum Game
    {
        Defiance,
        Legend,
        DeusEx,
        Tiger,
    }
}
