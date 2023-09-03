using JukeBoxSolutions.Pages;
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
using static JukeBoxSolutions.MainPlayer;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for FavItemBar.xaml
    /// </summary>
    public partial class FavItemBar : UserControl
    {
        public string Artistname { get; }
        public AlbumLibrary _sourceAlbumLibrary { get; }
        public FavItemBar(AlbumLibrary sourceAlbumLibrary)
        {
            InitializeComponent();
        }

        public FavItemBar(AlbumLibrary sourceAlbumLibrary, string artistname, int count)
        {
            InitializeComponent();
            _sourceAlbumLibrary = sourceAlbumLibrary;
            Artistname = artistname;
            lblArtistName.Content = artistname;
            lblAlbumName.Content = sourceAlbumLibrary.AlbumName;
            BtnAlbumCount.Content = count;
        }

        private void BtnTrackMenuActive_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnQueNow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPlayNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<TrackLibrary> albumtracks = new List<TrackLibrary>();
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    // Get tracks
                    albumtracks = (from x in db.AlbumLibraries.Find(_sourceAlbumLibrary.AlbumId).SongLibraries
                                   where x.TrackLibraries.Any(a => a.Type == Global.AppModeString)
                                   select x.TrackLibraries.First(a2 => a2.Type == Global.AppModeString)).ToList();
                    Global.MainWindow.btnLogo.IsEnabled = false;
                    Global.MainWindow.btnKeyboard.IsEnabled = false;
                    Global.MainWindow.btnMediaPanel.IsEnabled = false;
                    var songPlayingArtworkcontrol = new SongPlayingArtwork(db.AlbumLibraries.Find(_sourceAlbumLibrary.AlbumId), albumtracks,null);
                    Global.MainFrame.Content = songPlayingArtworkcontrol;
                    Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                    Global.MainWindow.btnKeyboard.Visibility = Visibility.Collapsed;
                    Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                    Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                    if (Global.AdminSettings.AudioVisualizerSize != Global.ClassAdminSettings.Quality.Off)
                    {
                        Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
                    }
                    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Collapsed;
                    Global.MainWindow.PlaylistControlPanel.Height = 120;
                    //Global.MainWindow.VolumeTracker.Value = 70;
                    Global.mPlayer.popUp_Grid.Visibility = Visibility.Collapsed;
                    Global.mPlayer.popUp_Frame.Content = null;
                    Global.mPlayer.SongPlayingArtWork = songPlayingArtworkcontrol;
                    if (Global.AdminSettings.VolumeIncrement == 40)
                    {
                        Global.MainWindow.VolControlPanel.volLevel1_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 80)
                    {
                        Global.MainWindow.VolControlPanel.volLevel2_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 120)
                    {
                        Global.MainWindow.VolControlPanel.volLevel3_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 160)
                    {
                        Global.MainWindow.VolControlPanel.volLevel4_Click(null, null);
                    }
                    else if (Global.AdminSettings.VolumeIncrement == 200)
                    {
                        Global.MainWindow.VolControlPanel.volLevel5_Click(null, null);
                    }
                    Global.mPlayer.playList.PlayIndex = 0;
                    Global.mPlayer.PlayNow(albumtracks.ToList());
                    Global.AppActionMode = Global.AppActionModeEnum.Playing;
                    Global.MainWindow.btnPlay1.IsEnabled = true;
                    Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                    Global.MainWindow.btnPause.IsEnabled = true;
                    Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                    //Global.MainWindow.VolumnPanel.Visibility = Visibility.Visible;
                    Global.MainWindow.btnLogo.IsEnabled = true;
                    Global.MainWindow.btnKeyboard.IsEnabled = true;
                    Global.MainWindow.btnMediaPanel.IsEnabled = true;
                    Global.MainWindow.btnPreviousMenu.Visibility = Visibility.Visible;
                    Global.MainWindow.btnNextMenu.Visibility = Visibility.Visible;
                    Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                }
            }
            catch (Exception)
            {

            }
            
        }
        private void BtnTrackMenu_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
