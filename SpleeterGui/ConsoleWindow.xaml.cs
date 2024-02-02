using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpleeterGui
{
    /// <summary>
    /// Interaction logic for ConsoleWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private ConcurrentQueue<string> _linesQueue = new ConcurrentQueue<string>();

        public ConsoleWindow()
        {
            InitializeComponent();
        }

        public void AddLine(string line)
        {
            _linesQueue.Enqueue(line);

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                while (_linesQueue.TryDequeue(out string line))
                {
                    textBoxOutputLines.Text += line + Environment.NewLine;

                    textBoxOutputLines.CaretIndex = textBoxOutputLines.Text.Length;
                    textBoxOutputLines.ScrollToEnd();
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();

            Owner?.Focus();

            e.Cancel = true;
        }
    }
}
