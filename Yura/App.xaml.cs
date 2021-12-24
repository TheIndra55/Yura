using System.IO;
using System.Windows;

namespace Yura
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // clean up the temp folder
            var folder = Path.Combine(Path.GetTempPath(), "Yura");

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
