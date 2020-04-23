using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;

namespace MiDic
{
    public partial class App
    {
        public static string Translate = "";
        private TaskbarIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            HotkeyManager.Current.AddOrReplace("GrabText", Key.Pause, ModifierKeys.None, OnFire);
            _notifyIcon = (TaskbarIcon) FindResource("NotifyIcon");
            Current.MainWindow = new MainWindow();
        }

        private void OnFire(object sender, HotkeyEventArgs e)
        {
            Debug.Write("OnFire");
            TextSelectionReader ts = new TextSelectionReader();
            Translate = ts.TryGetSelectedTextFromActiveControl();
            openMainWindow();
        }

        private void openMainWindow()
        {
            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }
    }
}