using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace MiDic
{
    public partial class App
    {
        private TaskbarIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // single instance of application
            var mutex = new Mutex(true, "SingleMiDic", out bool isNewInstance);
            if (!isNewInstance)
            {
                Shutdown();
            }

            _notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            Current.MainWindow = new MainWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}