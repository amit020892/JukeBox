using JukeBoxSolutions.Class;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using JukeBoxSolutions.Controls;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for FileManagerV2.xaml
    /// </summary>
    public partial class FileManagerV2 : Page
    {
        private enum ImportMode
        {
            Video,
            Music,
            Imported,
            Karaoke,
            All
        }

        private ImportMode mode { get; set; }

        public FileManagerV2()
        {
            InitializeComponent();
            GotFocus += MainWindow_GotFocus;
            tbDirectory.Text = Global.LastImportDirectory;    
        }

        void MainWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)e.OriginalSource;

            if (txtBoxPopup == element || popup == element || element.Parent == popup)
                return;

            popup.IsOpen = !string.IsNullOrEmpty(txtBoxPopup.Text);
        }


        void UpdateControls()
        {
            stack.Children.Clear();

            List<FileManagerItem> fmi = new List<FileManagerItem>();
            switch (mode)
            {
                case ImportMode.All:
                    fmi = (from a1 in Global.importFactory.ImportList.Where(w1 => w1.isDatabase == false)
                           select new FileManagerItem(a1)).ToList();
                    break;
                case ImportMode.Video:
                    fmi = (from a2 in Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isVideo == true)
                           select new FileManagerItem(a2)).ToList();
                    break;
                case ImportMode.Music:
                    fmi = (from a3 in Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isMusic == true)
                           select new FileManagerItem(a3)).ToList();
                    break;
                case ImportMode.Karaoke:
                    fmi = (from a3 in Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isKaraoke == true)
                           select new FileManagerItem(a3)).ToList();
                    break;
                case ImportMode.Imported:
                    fmi = (from a4 in Global.importFactory.ImportList.Where(w => w.isDatabase == true)
                           select new FileManagerItem(a4)).ToList();
                    break;
            }

            foreach (var f in fmi)
            {
                stack.Children.Add(f);
            }

            btnViewImported.Content = "Imported (" + Global.importFactory.ImportList.Where(w => w.isDatabase == true).Count() + ")";
            btnViewMusic.Content = "Music (" + Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isMusic == true).Count() + ")";
            btnViewVideo.Content = "Video (" + Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isVideo == true).Count() + ")";
            btnViewKaraoke.Content = "Karaoke (" + Global.importFactory.ImportList.Where(w => w.isDatabase == false && w.isKaraoke == true).Count() + ")";

            btnImportAll.IsEnabled = true;
            btnViewImported.IsEnabled = true;
            btnViewMusic.IsEnabled = true;
            btnViewVideo.IsEnabled = true;
            btnViewKaraoke.IsEnabled = true;
        }

        private void btnLoadDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbDirectory.Text))
            {
                Global.LastImportDirectory = tbDirectory.Text;
                Global.importFactory = new ImportFactory();

                // List songs
                Global.importFactory.LoadDirlisting(tbDirectory.Text, true);
                //Global.importFactory.AutoDBSaveMusic();
                mode = ImportMode.All;
                UpdateControls();
            }
            else
            {
                tbDirectory.Focus();
            }
        }

        private void btnImportAll_Click(object sender, RoutedEventArgs e)
        {
            switch (mode)
            {
                case ImportMode.Video:
                    Global.importFactory.ImportAllVideoFiles(false,lblPlayListName.Content.ToString());
                    break;
                case ImportMode.Music:
                    Global.importFactory.ImportAllMusicFiles(false, lblPlayListName.Content.ToString());
                    //Global.importFactory.AutoDBSaveMusic();
                    break;
                case ImportMode.Karaoke:
                    Global.importFactory.ImportAllKaraokeFiles(false, lblPlayListName.Content.ToString());
                    break;
                case ImportMode.Imported:
                    Global.importFactory.ImportAddToPlaylistOnly(lblPlayListName.Content.ToString());
                    break;
            }
        }

        private void btnDirectPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Global.isKeyboardFocus = true;
            popup.IsOpen = true;
        }

        private void btnPopUpGo_Click(object sender, RoutedEventArgs e)
        {
            lblPlayListName.Content = txtBoxPopup.Text;
            txtBoxPopup.Text = "";
            popup.IsOpen = false;
            Global.isKeyboardFocus = false;
        }


        private void btnPopUpCancel_Click(object sender, RoutedEventArgs e)
        {
            txtBoxPopup.Text = "";
            popup.IsOpen = false;
        }



        private void btnViewMusic_Click(object sender, RoutedEventArgs e)
        {
            mode = ImportMode.Music;

            UpdateControls();
            btnViewMusic.IsEnabled = false;
        }

        private void btnViewVideo_Click(object sender, RoutedEventArgs e)
        {
            mode = ImportMode.Video;

            UpdateControls();
            btnViewVideo.IsEnabled = false;
        }

        private void btnViewKaraoke_Click(object sender, RoutedEventArgs e)
        {
            mode = ImportMode.Karaoke;

            UpdateControls();
            btnViewKaraoke.IsEnabled = false;
        }

        private void btnViewImported_Click(object sender, RoutedEventArgs e)
        {
            mode = ImportMode.Imported;

            UpdateControls();
            btnViewImported.IsEnabled = false;
        }
    }
}
