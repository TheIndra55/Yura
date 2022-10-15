using Sentry;
using System.IO;
using System.Windows;

namespace Yura
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _window;

        public App()
        {
#if !DEBUG
            // setup sentry
            SentrySdk.Init(options =>
            {
                options.Dsn = "https://2bca1d6075984d7fb74e569f4f6ff3a1@o4503985120215040.ingest.sentry.io/4503985128341504";
                options.IsGlobalModeEnabled = true;

                options.BeforeSend = sentryEvent =>
                {
                    // add information about open bigfile
                    if (_window != null && _window.Bigfile != null)
                    {
                        sentryEvent.SetExtra("bigfile", _window.Bigfile.Filename);
                        sentryEvent.SetExtra("game", _window.Game);
                    }

                    return sentryEvent;
                };
            });
#endif
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _window = new MainWindow();
            _window.Show();

            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
            {
                _window.OpenBigfileDialog(e.Args[0]);
            }
        }

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
