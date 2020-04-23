using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NHotkey;
using NHotkey.Wpf;
using Clipboard = System.Windows.Clipboard;

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

            // clipboard solution for getting text from another app
            // https://blog.jayway.com/2013/02/06/how-to-get-selected-text-from-another-windows-program/
            _wih = new WindowInteropHelper(this); // Argument type 'MiDic.App' is not assignable to parameter type 'System.Windows.Window'
            _hwndSource = HwndSource.FromHwnd(_wih.Handle);
            _hwndSource?.AddHook(MainWindowProc);
            RegisterClipboardViewer();
        }

        private void OnFire(object sender, HotkeyEventArgs e)
        {
            Debug.Write("OnFire");
            var element = AutomationElement.FocusedElement;
            Translate = element.Current.ToString();
            if (element != null)
            {
                Debug.Write(element.Current.ToString());
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out var pattern))
                {
                    // TODO this block doesn't work yet
                    var tp = (TextPattern) pattern;
                    var sb = new StringBuilder();

                    foreach (var r in tp.GetSelection())
                    {
                        sb.AppendLine(r.GetText(-1));
                    }

                    var selectedText = sb.ToString();
                    Translate = selectedText;
                }
                else
                {
                    CopyFromActiveProgram();
                }
            }

            openMainWindow();
        }

        private void openMainWindow()
        {
            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            UnregisterClipboardViewer();
            _notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            base.OnExit(e);
        }


        const int WmDrawclipboard = 0x0308;
        const int WmChangecbchain = 0x030D;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(
            IntPtr hWndRemove, // handle to window to remove
            IntPtr hWndNewNext // handle to next window
        );

        private HwndSource _hwndSource;
        private WindowInteropHelper _wih;

        IntPtr _clipboardViewerNext;

        private void RegisterClipboardViewer()
        {
            _clipboardViewerNext = SetClipboardViewer(_hwndSource.Handle);
        }

        private void UnregisterClipboardViewer()
        {
            ChangeClipboardChain(_hwndSource.Handle, _clipboardViewerNext);
        }

        private bool _getCopyValue;

        private void CopyFromActiveProgram()
        {
            _getCopyValue = true;
            SendKeys.SendWait("^c");
        }

        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmDrawclipboard:

                    if (_getCopyValue && Clipboard.ContainsText())
                    {
                        _getCopyValue = false;
                        var selectedText = Clipboard.GetText();
                        Translate = selectedText;
                        openMainWindow();
                        Clipboard.Clear();
                    }

                    // Send message along, there might be other programs listening to the copy command.
                    SendMessage(_clipboardViewerNext, msg, wParam, lParam);
                    break;

                case WmChangecbchain:
                    if (wParam == _clipboardViewerNext)
                    {
                        _clipboardViewerNext = lParam;
                    }
                    else
                    {
                        SendMessage(_clipboardViewerNext, msg, wParam, lParam);
                    }

                    break;
            }

            return IntPtr.Zero;
        }
    }
}