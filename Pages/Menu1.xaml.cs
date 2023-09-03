using JukeBoxSolutions.Pages;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static JukeBoxSolutions.Library;

namespace JukeBoxSolutions
{
    /// <summary>
    /// Interaction logic for Menu1.xaml
    /// </summary>
    public partial class Menu1 : Page
    {
        Frame mainFrame;
        Button menuButton;
        public Menu1(ref Frame MainFrame, ref Button _menuButton)
        {
            InitializeComponent();
            mainFrame = MainFrame;
            menuButton = _menuButton;
            //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            //{
            //    if (db.TrackLibraries.Count(c1 => c1.Type == "Karaoke") == 0)
            //    {
            //        btnMenuKaraoke.IsEnabled = false;
            //        btnMenuKaraoke.Opacity = 0.15;
            //    }
            //    if (db.TrackLibraries.Count(c1 => c1.Type == "Music") == 0)
            //    {
            //        btnMenuMusic.IsEnabled = false;
            //        btnMenuMusic.Opacity = 0.15;
            //    }
            //    if (db.TrackLibraries.Count(c1 => c1.Type == "Video") <= 5)
            //    {
            //        btnMenuVideo.IsEnabled = false;
            //        btnMenuVideo.Opacity = 0.15;
            //    }
            //    if (db.TrackLibraries.Count(c1 => c1.Type == "Radio") == 0)
            //    {
            //        btnMenuRadio.IsEnabled = false;
            //        btnMenuRadio.Opacity = 0.15;
            //    }
            //}

            setTheme();
        }

        #region functions

        private void groupMenuButtons()
        {
            stackSubMenu.Visibility = Visibility.Collapsed;
            btnMenuKaraoke.Visibility = Visibility.Visible;
            btnMenuMusic.Visibility = Visibility.Visible;
            btnMenuVideo.Visibility = Visibility.Visible;
            btnMenuRadio.Visibility = Visibility.Visible;
            btnMenuRadioWithoutText.Visibility = Visibility.Collapsed;
            btnMenuKaraokeWithoutText.Visibility = Visibility.Collapsed;
            btnMenuMusicWithoutText.Visibility = Visibility.Collapsed;
            btnMenuVideoWithoutText.Visibility = Visibility.Collapsed;

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
        private void splitMenuButton(System.Windows.Controls.Image menuButton, int i, string type)
        {
            // reset other buttons
            stackSubMenu.Visibility = Visibility.Collapsed;
            btnMenuKaraoke.Visibility = Visibility.Visible;
            btnMenuMusic.Visibility = Visibility.Visible;
            btnMenuVideo.Visibility = Visibility.Visible;
            btnMenuRadio.Visibility = Visibility.Visible;

            // hide menu button
            //menuButton.Visibility = Visibility.Collapsed;
            // show split menu & position in right spot
            stackSubMenu.SetValue(Grid.ColumnProperty, i);
            stackSubMenu.Visibility = Visibility.Visible;

            btnLastPlayed.IsEnabled = Global.mPlayer.hasLoadLastPlaylist(type);
            btnJustPlay.IsEnabled = Global.mPlayer.hasLibraryItems(type);
        }



        #endregion functions

        private void setTheme()
        {
            try
            {
                switch (Global.AdminSettings.currentThemeSequence)
                {
                    case 1:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/KaraokeWT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/MusicWT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/VideoWT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/RadioWT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/Karaoke.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/Music.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/Video.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme1/Radio.png"));

                        break;
                    case 2:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme2/Design2R.png"));

                        break;
                    case 3:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme3/Design3R.png"));

                        break;
                    case 4:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme4/Design4R.png"));
                        Global.AdminSettings.currentThemeSequence++;
                        break;
                    case 5:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme5/Design5R.png"));

                        break;
                    case 6:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme6/Design6R.png"));

                        break;
                    case 7:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme7/Design7R.png"));

                        break;
                    case 8:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme8/Design8R.png"));

                        break;
                    case 9:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme9/Design9R.png"));

                        break;
                    case 10:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme10/Design10R.png"));

                        break;
                    case 11:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme11/Design11R.png"));

                        break;
                    case 12:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme12/Design12R.png"));

                        break;
                    case 13:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme13/Design13R.png"));

                        break;
                    case 14:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme14/Design14R.png"));

                        break;
                    case 15:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme15/Design15R.png"));

                        break;
                    case 16:
                        btnMenuKaraoke.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16KT.png"));
                        btnMenuMusic.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16MT.png"));
                        btnMenuVideo.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16VT.png"));
                        btnMenuRadio.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16RT.png"));
                        btnMenuKaraokeWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16K.png"));
                        btnMenuMusicWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16M.png"));
                        btnMenuVideoWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16V.png"));
                        btnMenuRadioWithoutText.Source = new BitmapImage(new Uri("pack://application:,,,/JukeBoxSolutions;component/Images/Theme16/Design16R.png"));

                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }



        #region Main Buttons


        private void btnMenuKaraoke_Click(object sender, MouseButtonEventArgs e)
        {
            // load library : Karaoke Mode

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Global.AppMode != Global.AppModeEnum.Karaoke)
                {
                    Global.AppMode = Global.AppModeEnum.Karaoke;
                }
                splitMenuButton(btnMenuKaraoke, 0, "Karaoke");
                btnMenuKaraokeWithoutText.Visibility = Visibility.Visible;
                btnMenuKaraoke.Visibility = Visibility.Collapsed;
                btnMenuMusicWithoutText.Visibility = Visibility.Collapsed;
                btnMenuMusic.Visibility = Visibility.Visible;
                btnMenuVideoWithoutText.Visibility = Visibility.Collapsed;
                btnMenuVideo.Visibility = Visibility.Visible;
                btnMenuRadioWithoutText.Visibility = Visibility.Collapsed;
                btnMenuRadio.Visibility = Visibility.Visible;
                Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void btnMenuMusic_Click(object sender, MouseButtonEventArgs e)
        {
            // load library : Music Mode
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Global.AppMode != Global.AppModeEnum.Music)
                {
                    Global.AppMode = Global.AppModeEnum.Music;
                }
                splitMenuButton(btnMenuMusic, 1, "Music");
                btnMenuMusicWithoutText.Visibility = Visibility.Visible;
                btnMenuMusic.Visibility = Visibility.Collapsed;
                btnMenuKaraokeWithoutText.Visibility = Visibility.Collapsed;
                btnMenuKaraoke.Visibility = Visibility.Visible;
                btnMenuVideoWithoutText.Visibility = Visibility.Collapsed;
                btnMenuVideo.Visibility = Visibility.Visible;
                btnMenuRadioWithoutText.Visibility = Visibility.Collapsed;
                btnMenuRadio.Visibility = Visibility.Visible;
            });

        }

        private void btnMenuVideo_Click(object sender, MouseButtonEventArgs e)
        {
            // load library : Video Mode
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Global.AppMode != Global.AppModeEnum.Video)
                {
                    Global.AppMode = Global.AppModeEnum.Video;
                }
                splitMenuButton(btnMenuVideo, 2, "Video");
                btnMenuVideoWithoutText.Visibility = Visibility.Visible;
                btnMenuVideo.Visibility = Visibility.Collapsed;
                btnMenuKaraokeWithoutText.Visibility = Visibility.Collapsed;
                btnMenuKaraoke.Visibility = Visibility.Visible;
                btnMenuMusicWithoutText.Visibility = Visibility.Collapsed;
                btnMenuMusic.Visibility = Visibility.Visible;
                btnMenuRadioWithoutText.Visibility = Visibility.Collapsed;
                btnMenuRadio.Visibility = Visibility.Visible;
                Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
            });
        }

        private void btnMenuRadio_Click(object sender, MouseButtonEventArgs e)
        {
            // load library : Radio Mode

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (Global.AppMode != Global.AppModeEnum.Radio)
                {
                    Global.AppMode = Global.AppModeEnum.Radio;
                }
                splitMenuButton(btnMenuRadio, 3, "Radio");
                btnMenuRadioWithoutText.Visibility = Visibility.Visible;
                btnMenuRadio.Visibility = Visibility.Collapsed;
                btnMenuKaraokeWithoutText.Visibility = Visibility.Collapsed;
                btnMenuKaraoke.Visibility = Visibility.Visible;
                btnMenuMusicWithoutText.Visibility = Visibility.Collapsed;
                btnMenuMusic.Visibility = Visibility.Visible;
                btnMenuVideoWithoutText.Visibility = Visibility.Collapsed;
                btnMenuVideo.Visibility = Visibility.Visible;
                Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;
            });
        }


        #endregion Main Buttons



        private void btnLastPlayed_Click(object sender, RoutedEventArgs e)
        {
            // start playing last tracks - or - go to library with notification
            if (Global.mPlayer.LoadLastPlaylist())
            {
                // close menu
                menuButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                // could not find tracks
                mainFrame.Content = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
            }

            groupMenuButtons();
        }

        private void btnFavourites_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new Library(ref mainFrame, Library.LibraryMode.Favourites, currentAlbumIndex: 0);
            groupMenuButtons();
        }

        private void btnJustPlay_Click(object sender, RoutedEventArgs e)
        {
            buildPlaylistLibrary();
            groupMenuButtons();
        }

        private void btnPlaylists_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Content = new Library(ref mainFrame, Library.LibraryMode.Playlists, currentAlbumIndex: 0);
            groupMenuButtons();
        }

        private void btnLibrary_Click(object sender, RoutedEventArgs e)
        {
            Global.MainWindow.btnPreviousMenu.Visibility = Visibility.Collapsed;
            Global.MainWindow.btnNextMenu.Visibility = Visibility.Collapsed;
            if (Global.AppMode == Global.AppModeEnum.Karaoke)
            {
                LoadKaraokeLibrary();
            }
            else if (Global.AppMode == Global.AppModeEnum.Video)
            {
                LoadVideoLibrary();
            }
            else if (Global.AppMode == Global.AppModeEnum.Radio)
            {
                LoadRadioLibrary();
            }
            else if (Global.AppMode == Global.AppModeEnum.Music)
            {
                LoadMusicLibrary();
            }
            groupMenuButtons();
        }

        private void LoadKaraokeLibrary()
        {
            try
            {
                Library instance = null;
                if (Global.MainWindow.CurrentKaraokeLibraryInstnace == null || !Global.MainWindow.KaraokeLibraryInstancesLoaded)
                {
                    var libraryInstance = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                    Global.MainWindow.CurrentKaraokeLibraryInstnace = libraryInstance;

                }

                instance = Global.MainWindow.CurrentKaraokeLibraryInstnace;

                Global.MainWindow.KaraokeLibraryFrame.Visibility = Visibility.Visible;
                Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;

                Global.MainWindow.MainFrame.Visibility = Visibility.Collapsed;
                if (!Global.MainWindow.KaraokeLibraryInstancesLoaded)
                {
                    instance.LoadingLabel.Visibility = Visibility.Visible;
                    Global.MainWindow.KaraokeLibraryFrame.Content = null;
                    Global.MainWindow.KaraokeLibraryFrame.Content = instance;
                    Global.MainWindow.KaraokeLibraryFrame.ContentRendered += MainFrame_ContentRendered;
                }
            }
            catch (Exception)
            {

            }
        }

        private void LoadVideoLibrary()
        {
            try
            {
                try
                {
                    Library instance = null;
                    if (Global.MainWindow.CurrentVideoLibraryInstnace == null || !Global.MainWindow.VideoLibraryInstancesLoaded)
                    {
                        var libraryInstance = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                        Global.MainWindow.CurrentVideoLibraryInstnace = libraryInstance;

                    }

                    instance = Global.MainWindow.CurrentVideoLibraryInstnace;

                    Global.MainWindow.VideoLibraryFrame.Visibility = Visibility.Visible;
                    Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;

                    Global.MainWindow.MainFrame.Visibility = Visibility.Collapsed;
                    if (!Global.MainWindow.VideoLibraryInstancesLoaded)
                    {
                        instance.LoadingLabel.Visibility = Visibility.Visible;
                        Global.MainWindow.VideoLibraryFrame.Content = null;
                        Global.MainWindow.VideoLibraryFrame.Content = instance;
                        Global.MainWindow.VideoLibraryFrame.ContentRendered += MainFrame_ContentRendered;
                    }
                }
                catch (Exception)
                {

                }
            }
            catch (Exception)
            {

            }
        }

        private void LoadRadioLibrary()
        {
            try
            {
                try
                {
                    Library instance = null;
                    if (Global.MainWindow.CurrentRadioLibraryInstnace == null || !Global.MainWindow.RadioLibraryInstancesLoaded)
                    {
                        var libraryInstance = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                        Global.MainWindow.CurrentRadioLibraryInstnace = libraryInstance;

                    }

                    instance = Global.MainWindow.CurrentRadioLibraryInstnace;

                    Global.MainWindow.RadioLibraryFrame.Visibility = Visibility.Visible;
                    Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Collapsed;

                    Global.MainWindow.MainFrame.Visibility = Visibility.Collapsed;
                    if (!Global.MainWindow.RadioLibraryInstancesLoaded)
                    {
                        instance.LoadingLabel.Visibility = Visibility.Visible;
                        Global.MainWindow.RadioLibraryFrame.Content = null;
                        Global.MainWindow.RadioLibraryFrame.Content = instance;
                        Global.MainWindow.RadioLibraryFrame.ContentRendered += MainFrame_ContentRendered;
                    }
                }
                catch (Exception)
                {

                }
            }
            catch (Exception)
            {

            }
        }

        private void LoadMusicLibrary()
        {
            try
            {
                try
                {
                    Library instance = null;
                    if (Global.MainWindow.CurrentLibraryInstnace == null || !Global.MainWindow.MusicLibraryInstancesLoaded)
                    {
                        var libraryInstance = new Library(ref mainFrame, Library.LibraryMode.Library, currentAlbumIndex: 0);
                        Global.MainWindow.CurrentLibraryInstnace = libraryInstance;

                    }

                    instance = Global.MainWindow.CurrentLibraryInstnace;

                    Global.MainWindow.MusicLibraryFrame.Visibility = Visibility.Visible;
                    Global.MainWindow.LibraryButtonControlPanel.Visibility = Visibility.Visible;

                    Global.MainWindow.MainFrame.Visibility = Visibility.Collapsed;
                    if (!Global.MainWindow.MusicLibraryInstancesLoaded)
                    {
                        instance.LoadingLabel.Visibility = Visibility.Visible;
                        Global.MainWindow.MusicLibraryFrame.Content = null;
                        Global.MainWindow.MusicLibraryFrame.Content = instance;
                        Global.MainWindow.MusicLibraryFrame.ContentRendered += MainFrame_ContentRendered;
                    }

                }
                catch (Exception)
                {

                }
            }
            catch (Exception)
            {

            }
        }

        private int iN1 { get; set; }
        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            try
            {
                if (Global.AppMode == Global.AppModeEnum.Karaoke && Global.MainWindow.CurrentKaraokeLibraryInstnace != null)
                {
                    //iN1=Global.mPlayer.Notifactions.NewNotification("Loading Library...");
                    Global.MainWindow.CurrentKaraokeLibraryInstnace.Dispatcher.InvokeAsync(() => Global.MainWindow.CurrentKaraokeLibraryInstnace.LoadData(LibraryMode.Library)).Completed += Karaoke_loaded;
                }
                if (Global.AppMode == Global.AppModeEnum.Video && Global.MainWindow.CurrentVideoLibraryInstnace != null)
                {
                    //iN1=Global.mPlayer.Notifactions.NewNotification("Loading Library...");
                    Global.MainWindow.CurrentVideoLibraryInstnace.Dispatcher.InvokeAsync(() => Global.MainWindow.CurrentVideoLibraryInstnace.LoadData(LibraryMode.Library)).Completed += Videos_Loaded;
                }
                if (Global.AppMode == Global.AppModeEnum.Music && Global.MainWindow.CurrentLibraryInstnace != null)
                {
                    //iN1=Global.mPlayer.Notifactions.NewNotification("Loading Library...");
                    Global.MainWindow.CurrentLibraryInstnace.Dispatcher.InvokeAsync(() => Global.MainWindow.CurrentLibraryInstnace.LoadData(LibraryMode.Library)).Completed += Musics_loaded;
                }
                if (Global.AppMode == Global.AppModeEnum.Radio && Global.MainWindow.CurrentRadioLibraryInstnace != null)
                {
                    //iN1=Global.mPlayer.Notifactions.NewNotification("Loading Library...");
                    Global.MainWindow.CurrentRadioLibraryInstnace.Dispatcher.InvokeAsync(() => Global.MainWindow.CurrentRadioLibraryInstnace.LoadData(LibraryMode.Library)).Completed += Radios_loaded;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Radios_loaded(object sender, EventArgs e)
        {
            Global.MainWindow.RadioLibraryInstancesLoaded = true;
            Global.MainWindow.RadioLibraryFrame.ContentRendered -= MainFrame_ContentRendered;
        }

        private void Musics_loaded(object sender, EventArgs e)
        {
            Global.MainWindow.MusicLibraryInstancesLoaded = true;
            Global.MainWindow.MusicLibraryFrame.ContentRendered -= MainFrame_ContentRendered;
        }

        private void Videos_Loaded(object sender, EventArgs e)
        {
            Global.MainWindow.VideoLibraryInstancesLoaded = true;
            Global.MainWindow.VideoLibraryFrame.ContentRendered -= MainFrame_ContentRendered;
        }

        private void Karaoke_loaded(object sender, EventArgs e)
        {
            Global.MainWindow.KaraokeLibraryInstancesLoaded = true;
            Global.MainWindow.KaraokeLibraryFrame.ContentRendered -= MainFrame_ContentRendered;
        }

        private void btnLibraryMK2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buildPlaylistLibrary()
        {
            try
            {
                using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                {
                    var playlist = db.Playlists.ToList();
                    var rawplaylists = (from pl in db.Playlists
                                        where pl.TrackLibrary.Type == Global.AppModeString
                                        select pl).ToList();
                    if (!rawplaylists.Any())
                    {
                        return;
                    }

                    Global.mPlayer.PlayNow(rawplaylists.Select(x => x.TrackLibrary).ToList());
                    Global.MainWindow.Dispatcher.InvokeAsync(() =>
                    {
                        Global.MainWindow.btnLogo.IsEnabled = false;
                        Global.MainWindow.btnKeyboard.IsEnabled = false;
                        Global.MainWindow.btnMediaPanel.IsEnabled = false;
                        Global.MainWindow.sParentPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnMediaPanel.Visibility = Visibility.Collapsed;
                        Global.MainWindow.PlaylistControlPanel.Visibility = Visibility.Visible;
                        Global.MainWindow.MainBackgroundBG.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnPlay1.IsEnabled = true;
                        Global.MainWindow.btnPlay1.Visibility = Visibility.Collapsed;
                        Global.MainWindow.btnPause.IsEnabled = true;
                        Global.MainWindow.btnPause.Visibility = Visibility.Visible;
                        Global.MainWindow.btnLogo.IsEnabled = true;
                        Global.MainWindow.btnKeyboard.IsEnabled = true;
                        Global.MainWindow.btnMediaPanel.IsEnabled = true;
                        Global.MainWindow.ControlPanelBG.Visibility = Visibility.Visible;
                        Global.MainWindow.btnInfo.IsEnabled = true;
                        Global.MainWindow.btnInfo.Opacity = 1;
                    });
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
