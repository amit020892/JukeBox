using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for SongInfoBar.xaml
    /// </summary>
    public partial class SongInfoBar : UserControl
    {
        public SongInfoBar()
        {
            InitializeComponent();
        }

        // used to ignore kill commands when message was interrupted
        private int messagebacklog = 0;

        private void activate(bool pinMessage = false)
        {
            if (Global.MainWindow.isMenuOpen)
            {
                if (Global.MainWindow.isMinimalMode == true)
                {
                    MenuSpacerMove.Dispatcher.Invoke(() => { MenuSpacerMove.Visibility = Visibility.Collapsed; });
                    MenuSpacerStretch.Dispatcher.Invoke(() => { MenuSpacerStretch.Visibility = Visibility.Collapsed; });
                    MenuSpacerMini.Dispatcher.Invoke(() => { MenuSpacerMini.Visibility = Visibility.Visible; });
                }
                else
                {
                    // If controls are hidden...
                    MenuSpacerMove.Dispatcher.Invoke(() => { MenuSpacerMove.Visibility = Visibility.Collapsed; });
                    MenuSpacerStretch.Dispatcher.Invoke(() => { MenuSpacerStretch.Visibility = Visibility.Visible; });
                    MenuSpacerMini.Dispatcher.Invoke(() => { MenuSpacerMini.Visibility = Visibility.Collapsed; });
                }
            }
            else
            {
                MenuSpacerMove.Dispatcher.Invoke(() => { MenuSpacerMove.Visibility = Visibility.Visible; });
                MenuSpacerStretch.Dispatcher.Invoke(() => { MenuSpacerStretch.Visibility = Visibility.Collapsed; });
                MenuSpacerMini.Dispatcher.Invoke(() => { MenuSpacerMini.Visibility = Visibility.Collapsed; });
            }

            //this.Visibility = Visibility.Visible;
            this.Dispatcher.Invoke(() => { this.Visibility = Visibility.Visible; });

            //sets to 1
            messagebacklog++;

            if (pinMessage == false)
                Delay(8000, ClearMessage);
        }

        public void SetMessageX(string message)
        {
            lblMiniBar.Visibility = Visibility.Hidden;
            lblMainBar.Content = message;
            lblSubBar.Visibility = Visibility.Hidden;
            activate();
        }

        public void PinNowPlaying()
        {
            try
            {
                if (Global.mPlayer.SongPlayingArtWork != null)
                {
                    //using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    //{
                    //    var t = db.TrackLibraries.Find(Global.mPlayer.SongPlayingArtWork.AlbumTracks[Global.mPlayer.SongPlayingArtWork.currentSelectedIndex].Id);
                    //    var songLibrary = t.SongLibraries.First();
                    //    if (songLibrary.Artists.Any())
                    //        lblMiniBar.Content = songLibrary.Artists.First().ArtistName;
                    //    else
                    //        lblMiniBar.Content = "";

                    //    lblMainBar.Content = songLibrary.SongName;

                    //    if (songLibrary.AlbumLibraries.Any())
                    //        lblSubBar.Content = songLibrary.AlbumLibraries.First().AlbumName;
                    //}

                    lblMainBar.Content = Global.mPlayer.CurrentTrackInfo?.SongName;
                    lblMiniBar.Content = Global.mPlayer.CurrentTrackInfo?.ArtistName;
                    lblSubBar.Content = Global.mPlayer.CurrentTrackInfo?.AlbumName;
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var t = db.TrackLibraries.Find(Global.mPlayer.playList.GetCurrentTrack().Id);
                        var songLibrary = t.SongLibraries.First();
                        if (songLibrary.Artists.Any())
                            lblMiniBar.Content = songLibrary.Artists.First().ArtistName;
                        else
                            lblMiniBar.Content = "";

                        lblMainBar.Content = songLibrary.SongName;

                        if (songLibrary.AlbumLibraries.Any())
                            lblSubBar.Content = songLibrary.AlbumLibraries.First().AlbumName;
                    }
                }
                
            }
            catch (Exception)
            {

            }
            

            activate(true);
        }
        public void ShowNowPlaying(SongLibrary songLibrary)
        {
            if (songLibrary.Artists.Any())
                lblMiniBar.Content = songLibrary.Artists.First().ArtistName;

            lblMainBar.Content = songLibrary.SongName;

            if (songLibrary.AlbumLibraries.Any())
                lblSubBar.Content = songLibrary.AlbumLibraries.First().AlbumName;
            activate();
        }

        public void ShowUpNext(SongLibrary songLibrary)
        {

            activate();
        }

        public void ShowNotification(string message)
        {
            lblMiniBar.Dispatcher.Invoke(() => { lblMiniBar.Content = "Notification"; });
            lblMiniBar.Dispatcher.Invoke(() => { lblMainBar.Content = message; });
            lblSubBar.Dispatcher.Invoke(() => { lblSubBar.Visibility = Visibility.Hidden; });
            activate();
        }


        public void ClearMessage()
        {
            if (messagebacklog <= 1)
            {
                lblMiniBar.Content = "";
                lblMiniBar.Visibility = Visibility.Visible;
                lblMainBar.Content = "";
                lblMainBar.Visibility = Visibility.Visible;
                lblSubBar.Content = "";
                lblSubBar.Visibility = Visibility.Visible;

                this.Visibility = Visibility.Hidden;
            }

            // reset to 0 .. eventually
            messagebacklog--;
        }

        #region INotifyPropertyChanged Members

        public static void Delay(int milliseconds, Action action)
        {
            var t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            t.Tick += (o, e) => { t.Stop(); action.Invoke(); };
            t.Start();
        }

        #endregion
    }
}
