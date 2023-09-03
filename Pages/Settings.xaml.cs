using JukeBoxSolutions.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                btnAdminMode.Background = Brushes.White;
                btnAdminMode.Foreground = Brushes.Black;
            }
            else
            {
                btnAdminMode.Background = Brushes.Black;
                btnAdminMode.Foreground = Brushes.White;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.ShutdownMode=ShutdownMode.OnMainWindowClose;
            System.Windows.Application.Current.Shutdown();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new ChangeLog_PopUp();
        }

        private void btnOpenLogs_Click(object sender, RoutedEventArgs e)
        {
            string logdir = string.Format("{0}\\Logs", Environment.CurrentDirectory);
            if (!Directory.Exists(logdir))
                Directory.CreateDirectory(logdir);

            Process.Start(logdir);

        }

        private void btnAdminMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnAdminMode.IsChecked.Value)
            {
                btnAdminMode.Background = Brushes.White;
                btnAdminMode.Foreground = Brushes.Black;
                Global.AppControlMode = Global.AppControlModeEnum.Admin;
                Global.isAdminOverride = true;
                Global.mPlayer.ShowNotification("Application is now set to Admin Mode");
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
            else
            {
                btnAdminMode.Background = Brushes.Black;
                btnAdminMode.Foreground = Brushes.White;
                Global.AppControlMode = Global.AppControlModeEnum.Normal;
                Global.isAdminOverride = false;
                Global.mPlayer.ShowNotification("Application is now set back to Normal Mode");
                Thread.Sleep(1000);
                Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
            }
        }
    }
}
