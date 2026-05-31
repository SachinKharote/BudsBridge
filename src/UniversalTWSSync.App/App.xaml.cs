using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace UniversalTWSSync.App
{
    public partial class App : Application
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            WriteLog("Application startup");

            try
            {
                var window = new MainWindow();
                MainWindow = window;
                window.Show();
                WriteLog("Main window shown");
            }
            catch (Exception ex)
            {
                WriteLog("Startup failure: " + ex);
                MessageBox.Show(ex.Message, "Universal TWS Sync startup error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteLog("Dispatcher exception: " + e.Exception);
            MessageBox.Show(e.Exception.Message, "Universal TWS Sync runtime error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLog("Unhandled exception: " + e.ExceptionObject);
        }

        private static void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine);
            }
            catch
            {
            }
        }
    }
}
