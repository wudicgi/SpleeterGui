using NGettext.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SpleeterGui
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public string WindowTitle
        {
            get => Translation._("About") + " " + ApplicationService.APPLICATION_TITLE;
        }

        public string ApplicationTitle
        {
            get => ApplicationService.APPLICATION_TITLE;
        }

        public string ApplicationVersion
        {
            get => "v" + ApplicationService.GetCurrentVersion();
        }

        public string CopyrightInfo
        {
            get => ApplicationService.GetCopyrightInformation();
        }

        public AboutWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LinkWebsite_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }
    }
}
