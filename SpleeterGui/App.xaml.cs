using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SpleeterGui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // for debug
            // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            // Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");

            I18n.Compose("message");

            try
            {
                // ApplicationService.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(-1);
            }

            base.OnStartup(e);
        }
    }
}
