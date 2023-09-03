using JukeBoxSolutions.Controls;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for PlayListView.xaml
    /// </summary>
    public partial class PlayListView : Page
    {
        public PlayListView()
        {
            InitializeComponent();
            LoadPlaylist();
        }

        public string AlbumName
        {
            set
            {
                lblAlubmNameFg.Content = value;
                lblAlubmNameBg.Content = value;
            }
        }

        public void SetPlaylist(string playlistName, int playlistID)
        {
            // remember track
            Global.mPlayer.playList.PlayListId = playlistID;
            AlbumName = playlistName;
            Global.mPlayer.dbMoveTracksFromNowPlaying(playlistID);
            Global.mPlayer.TogglePopUp();
        }

        public void LoadPlaylist()
        {
            // pull playlist
            // get current position
            // highlight current position



            //Global.mPlayer.playList
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                if (Global.mPlayer.playList.Any())
                {
                    var playlist = db.PlayListDetails.Where(w => w.Id == Global.mPlayer.playList.PlayListId).FirstOrDefault();

                    stackTrackList.Children.Clear();
                    if (playlist != null)
                    {
                        if ((playlist.Type == 0 && (playlist.Name == "NowPlaying" || playlist.Name == "PlaylistBuffer")) || (playlist.Type > 0))
                        {
                            if (playlist.Type == 0 && playlist.Name == "PlaylistBuffer")
                            {
                                AlbumName = db.PlayListDetails.Where(w => w.Id == Global.mPlayer.playList._bufferPlayListId).First().Name + " (Modified)";
                                SetPlaylistModifiedButtons();
                            }
                            else if (playlist.Type == 0 && playlist.Name == "NowPlaying")
                            {
                                SetNowPlayingButtons();
                                AlbumName = playlist.Name;
                            }
                            else
                            {
                                SetPlaylistButtons();
                                AlbumName = playlist.Name;
                            }

                            lblPlayListName.Content = "";
                            foreach (var s in playlist.Playlists.OrderBy(o => o.SequenceNumber))
                            {
                                var card = new LibraryCardSong(s.TrackLibrary, Global.mPlayer.playList.GetCurrentTrack().Id == s.TrackId, s.SequenceNumber);
                                stackTrackList.Children.Add(card);
                            }
                        }
                    }
                    
                }
                
            }

            scrollViewLibrary.ScrollToTop();
        }

        private void SetNowPlayingButtons()
        {
            var a = Visibility.Visible;
            var b = Visibility.Collapsed;

            btnClearList.Visibility = a;
            btnSaveAsPlaylistList.Visibility = a;

            btnSaveChanges.Visibility = b;
            btnUnloadList.Visibility = b;
        }
        private void SetPlaylistButtons()
        {
            var a = Visibility.Collapsed;
            var b = Visibility.Visible;

            btnClearList.Visibility = a;
            btnSaveAsPlaylistList.Visibility = a;

            btnSaveChanges.Visibility = a;
            btnUnloadList.Visibility = b;
        }
        private void SetPlaylistModifiedButtons()
        {
            var a = Visibility.Collapsed;
            var b = Visibility.Visible;

            btnClearList.Visibility = a;
            btnSaveAsPlaylistList.Visibility = a;

            btnSaveChanges.Visibility = b;
            btnUnloadList.Visibility = b;
        }
        private void btnShuffleList_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.dbShufflePlaylist(Global.mPlayer.playList.PlayListId);
            LoadPlaylist();
        }

        private void btnClearList_Click(object sender, RoutedEventArgs e)
        {
            if (Global.NowPlayingId == Global.mPlayer.playList.PlayListId)
            {
                Global.mPlayer.playList.Unload();
                Global.mPlayer.SetIdle();
                //LoadPlaylist();
                Global.MainWindow.MenuToggle(Visibility.Visible);
                Global.mPlayer.TogglePopUp();
            }
            else
            {
                // Unload Playlist

            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            scrollViewLibrary.PageDown();
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            scrollViewLibrary.PageUp();
        }

        private void btnSaveAsPlaylistList_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.PopUp_NewPlaylist(true);

        }

        private void btnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // remember track
            Global.mPlayer.playList.SaveChanges();
            Global.mPlayer.TogglePopUp();
        }

        private void CreateNewPlaylist()
        {
            string playlistName = string.Empty;
            using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
            {
                var p = db.PlayListDetails.Where(w => playlistName.ToLower() == w.Name.ToLower());

                if (p.Any())
                {
                    string error = "A playlist by that name already exist";
                    Global.mPlayer.ShowNotification("Playlist already exists, selecting Playlist");
                    Thread.Sleep(1000);
                    Global.MainWindow.lblNowPlaying.Dispatcher.Invoke(new Action(() => { Global.MainWindow.lblNowPlaying.Visibility = System.Windows.Visibility.Collapsed; }));
                    Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                    Global.AppPlaylistModeBufferID = p.First().Id;
                    //basePlaylistDetail = p.First();
                }
                else
                {
                    PlayListDetail newPlaylist = new PlayListDetail() { Name = playlistName, Type = 1 };


                    db.PlayListDetails.Add(newPlaylist);
                    db.SaveChanges();

                    // Global.mPlayer.SetNewPlaylist();

                    Global.AppControlMode = Global.AppControlModeEnum.Playlist;
                    Global.AppPlaylistModeBufferID = newPlaylist.Id;
                    //basePlaylistDetail = newPlaylist;
                }
            }


        }

        private void btnUnloadList_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.playList.Unload();
            Global.mPlayer.SetIdle();
            LoadPlaylist();
            Global.MainWindow.MenuToggle(Visibility.Visible);
            Global.mPlayer.TogglePopUp();
        }
    }
}
