using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Yura
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string ProgId = "Yura";
        private const string ProgFriendlyName = "Crystal Dynamics Bigfile";

        public SettingsWindow()
        {
            InitializeComponent();

            Theme.SelectedIndex = Properties.Settings.Default.Theme;
            SpecMaskView.SelectedIndex = Properties.Settings.Default.SpecMaskView;
            ClickAction.SelectedIndex = Properties.Settings.Default.ClickAction;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.Theme = Theme.SelectedIndex;
        }

        private void SpecMaskView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SpecMaskView = SpecMaskView.SelectedIndex;
        }

        private void ClickAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ClickAction = ClickAction.SelectedIndex;
        }

        private void FileAssociations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var reg = Registry.ClassesRoot;

                // associate .000, .dat and .tiger with Yura
                reg.CreateSubKey(".000").SetValue(string.Empty, ProgId);
                reg.CreateSubKey(".dat").SetValue(string.Empty, ProgId);

                // define yura, see https://learn.microsoft.com/en-us/windows/win32/shell/fa-progids
                var program = reg.CreateSubKey(ProgId);

                program.SetValue(string.Empty, ProgFriendlyName);
                program.SetValue("FriendlyTypeName", ProgFriendlyName);

                var path = Process.GetCurrentProcess().MainModule.FileName;
                program.CreateSubKey(@"shell\open\command").SetValue(string.Empty, $"\"{path}\" \"%1\"");

                // notify the shell about file associations been updated
                SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);

                MessageBox.Show("Yura will now open .000, .tiger and .dat files.", "File associations set", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                // TODO relaunch as administrator
                MessageBox.Show("Yura must be run as administrator to set file associations.", "Missing access", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
