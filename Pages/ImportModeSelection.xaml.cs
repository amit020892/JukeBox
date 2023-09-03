using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for ImportModeSelection.xaml
    /// </summary>
    public partial class ImportModeSelection : Page
    {
        public ImportModeSelection()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            // USB Import
            var drives = DriveInfo.GetDrives().Where(drive => drive.IsReady && drive.DriveType == DriveType.Removable).ToList();

            if (drives.Count == 0)
            {
                Global.mPlayer.ShowNotification("No USB Drive detected");
            }
            else if (drives.Count == 1)
            {
                Global.MainFrame.Content = null;
                Global.MainFrame.Content = new FileManagerV3(FileManagerV3.ImportMode.USBMode, drives.First().RootDirectory.FullName);
            }
            else
            {

            }



        }

        private void Button_Click_1(object sender, MouseButtonEventArgs e)
        {
            // File System Import
            Global.MainFrame.Content = null;
            Global.MainFrame.Content = new FileManagerV3(FileManagerV3.ImportMode.FileMode);
            Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
            if (Global.MainWindow.btnMediaPanel.Visibility == Visibility.Visible)
            {
                Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;
            }
            
            //if (Global.mPlayer.IsPlaying())
            //{
            //    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
            //    Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Collapsed;
            //    Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;
            //}
        }

        private void btnSelectCDRip_Click(object sender, MouseButtonEventArgs e)
        {
            Global.MainFrame.Content = new FileManagerV3(FileManagerV3.ImportMode.CDRipMode);
            Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
            Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
            if (Global.MainWindow.btnMediaPanel.Visibility == Visibility.Visible)
            {
                Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;
            }
            //if (Global.mPlayer.IsPlaying())
            //{
            //    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
            //    Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Collapsed;
            //    Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Collapsed;
            //}
        }

        private void btnSelectOnline_Click(object sender, MouseButtonEventArgs e)
        {
            Global.MainFrame.Content = new FileManagerV3(FileManagerV3.ImportMode.OnlineMode);

        }
    }
}
