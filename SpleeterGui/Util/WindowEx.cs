using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace SpleeterGui.Util
{
    // https://stackoverflow.com/questions/2341230/removing-icon-from-a-wpf-window/2341385
    // https://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window
    public class WindowEx
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;

        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;

        const uint WM_SETICON = 0x0080;

        const int GWL_STYLE = -16;
        const int WS_MAXIMIZEBOX = 0x00010000;
        const int WS_MINIMIZEBOX = 0x00020000;

        public static readonly DependencyProperty HideIconProperty =
            DependencyProperty.RegisterAttached("HideIcon", typeof(bool), typeof(WindowEx),
            new FrameworkPropertyMetadata(false, OnHideIconPropertyChanged));

        public static readonly DependencyProperty HideMinimizeButtonProperty =
            DependencyProperty.RegisterAttached("HideMinimizeButton", typeof(bool), typeof(WindowEx),
            new FrameworkPropertyMetadata(false, OnHideMinimizeButtonPropertyChanged));

        public static bool GetHideIcon(UIElement element)
        {
            return (bool)element.GetValue(HideIconProperty);
        }

        public static void SetHideIcon(UIElement element, bool value)
        {
            element.SetValue(HideIconProperty, value);
        }

        public static bool GetHideMinimizeButton(UIElement element)
        {
            return (bool)element.GetValue(HideMinimizeButtonProperty);
        }

        public static void SetHideMinimizeButton(UIElement element, bool value)
        {
            element.SetValue(HideMinimizeButtonProperty, value);
        }

        private static void OnHideIconPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window == null)
            {
                return;
            }

            if ((bool)e.NewValue == true)
            {
                RemoveIcon(window);
            }
        }

        private static void OnHideMinimizeButtonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Window window = sender as Window;
            if (window == null)
            {
                return;
            }

            if ((bool)e.NewValue == true)
            {
                RemoveMinimizeButtonIcon(window);
            }
        }

        private static void RemoveIcon(Window window)
        {
            window.SourceInitialized += delegate {
                // https://stackoverflow.com/questions/2341230/removing-icon-from-a-wpf-window/2341385
                // Get this window's handle
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                // Change the extended window style to not show a window icon
                int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);
                /*
                // Update the window's non-client area to reflect the changes
                SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                */

                // https://stackoverflow.com/questions/18580430/hide-the-icon-from-a-wpf-window
                SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
                SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);
            };
        }

        private static void RemoveMinimizeButtonIcon(Window window)
        {
            window.SourceInitialized += delegate {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;

                // https://stackoverflow.com/questions/1553715/is-it-possible-to-display-a-wpf-window-without-an-icon-in-the-title-bar
                int style = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, style & ~(WS_MINIMIZEBOX));
            };
        }
    }
}
