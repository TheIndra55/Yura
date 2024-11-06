using Sentry;
using System;
using System.IO;
using System.Windows;
using Yura.Properties;

namespace Yura
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string SentryDsn = "https://2bca1d6075984d7fb74e569f4f6ff3a1@o4503985120215040.ingest.sentry.io/4503985128341504";

        private MainWindow _window;

        public App()
        {
#if !DEBUG
            // setup sentry
            SentrySdk.Init(options =>
            {
                options.Dsn = SentryDsn;
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

        private void SetTheme(Theme theme)
        {
            var name = theme == Theme.Light ? "Light.xaml" : "Dark.xaml";

            var dictionary = new ResourceDictionary()
            {
                Source = new Uri("/Themes/" + name, UriKind.Relative)
            };

            Resources.MergedDictionaries.Add(dictionary);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // set the theme
            SetTheme((Theme)Settings.Default.Theme);

            // show the main window
            _window = new MainWindow();
            _window.Show();

            if (e.Args.Length > 0 && File.Exists(e.Args[e.Args.Length - 1]))
            {
                var options = new CommandLineOptions(e.Args);

                if (options.HasOption("-game"))
                {
                    _window.OpenBigfile(options.Bigfile, options);
                }
                else
                {
                    _window.OpenBigfileDialog(options.Bigfile);
                }
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
