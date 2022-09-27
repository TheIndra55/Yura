using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yura
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            SpecMaskView.SelectedIndex = Properties.Settings.Default.SpecMaskView;
            ClickAction.SelectedIndex = Properties.Settings.Default.ClickAction;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void SpecMaskView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SpecMaskView = SpecMaskView.SelectedIndex;
        }

        private void ClickAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ClickAction = ClickAction.SelectedIndex;
        }
    }
}
