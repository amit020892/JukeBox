using System.Windows;
using System.Windows.Input;

namespace JukeBoxSolutions.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        private MainWindow _mainWindow;
        public HomePage()
        {
            InitializeComponent();
            _mainWindow=new MainWindow();
        }


        private void btnPlay_clicked(object sender, MouseButtonEventArgs e)
        {
            //Global.mPlayer.Play();
            //BtnPlay.Visibility = Visibility.Collapsed;
            //BtnPause.Visibility = Visibility.Visible;
        }
        private void btnPlaylist_clicked(object sender, MouseButtonEventArgs e)
        {
            var window = new PlaylistWindow();
            window.ShowDialog();
        }

        private void btnKaraoke_clicked(object sender, MouseButtonEventArgs e)
        {
            if (KaraokeMenus.Visibility == Visibility.Visible)
            {
                KaraokeMenus.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.AppMode = Global.AppModeEnum.Karaoke;
                MusicMenus.Visibility = Visibility.Collapsed;
                KaraokeMenus.Visibility = Visibility.Visible;
                VideoMenus.Visibility = Visibility.Collapsed;
                RadioMenus.Visibility = Visibility.Collapsed;
            }
        }

        private void btnMusic_clicked(object sender, MouseButtonEventArgs e)
        {
            if (MusicMenus.Visibility == Visibility.Visible)
            {
                MusicMenus.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.AppMode = Global.AppModeEnum.Music;
                MusicMenus.Visibility = Visibility.Visible;
                KaraokeMenus.Visibility = Visibility.Collapsed;
                VideoMenus.Visibility = Visibility.Collapsed;
                RadioMenus.Visibility = Visibility.Collapsed;
            }
        }


        private void btnVideo_clicked(object sender, MouseButtonEventArgs e)
        {
            if (VideoMenus.Visibility == Visibility.Visible)
            {
                VideoMenus.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.AppMode = Global.AppModeEnum.Video;
                MusicMenus.Visibility = Visibility.Collapsed;
                KaraokeMenus.Visibility = Visibility.Collapsed;
                VideoMenus.Visibility = Visibility.Visible;
                RadioMenus.Visibility = Visibility.Collapsed;
            }
        }


        private void btnRadio_clicked(object sender, MouseButtonEventArgs e)
        {
            if (RadioMenus.Visibility == Visibility.Visible)
            {
                RadioMenus.Visibility = Visibility.Collapsed;
            }
            else
            {
                Global.AppMode = Global.AppModeEnum.Radio;
                MusicMenus.Visibility = Visibility.Collapsed;
                KaraokeMenus.Visibility = Visibility.Collapsed;
                VideoMenus.Visibility = Visibility.Collapsed;
                RadioMenus.Visibility = Visibility.Visible;
            }
        }

        private void LastPlayed_clicked(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.LoadLastPlaylist();
        }

        private void JustPalyed_clicked(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.PlayRandomSongs();
        }

        private void Playlist_clicked(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void LibraryClicked(object sender, MouseButtonEventArgs e)
        {

        }

        private void sliderTrackBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //_mainWindow.sliderTrackBar_ValueChanged(sender, e);
        }

        private void btnPause_clicked(object sender, MouseButtonEventArgs e)
        {
            Global.mPlayer.Pause();
            //BtnPlay.Visibility=Visibility.Visible;
            //BtnPause.Visibility = Visibility.Collapsed;
        }

        private void btnNextMenu_clicked(object sender, MouseButtonEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        private void btnPreviousMenu_clicked(object sender, MouseButtonEventArgs e)
        {
            var settingsWindow = new PlaylistWindow();
            settingsWindow.Show();
        }
    }
}
