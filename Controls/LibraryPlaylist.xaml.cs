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

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryPlaylist.xaml
    /// </summary>
    public partial class LibraryPlaylist : UserControl
    {

        Library parent { get; set; }
        int tracknumber { get; set; }
        PlayListDetail _playlist { get; set; }
        public int PlaylistId { get; set; }
        public string PlaylistName { get; set; }
        PlayListDetail basePlaylistDetail { get; set; }

        public LibraryPlaylist(PlayListDetail playlist, int trackCount, Library Parent)
        {
            InitializeComponent();
            ToggleButtons();
            parent = Parent;

            lblNameAlpha.Content = playlist.Name;
            tracknumber = trackCount;

            lblNameBeta.Content = tracknumber + " tracks";


            

            //lblNameCharlie.Content = "Mostly " + GroupedTags.name;
            _playlist = playlist;
            PlaylistId = playlist.Id;
            PlaylistName = playlist.Name;
            basePlaylistDetail = playlist;
        }

        private void Rectanngle_Expand_Click(object sender, RoutedEventArgs e)
        {
            // Pop Up Library Item
            //parent
            //parent.OpenOverlay(_playlist);

            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
            Global.AppPlaylistModeBufferID = PlaylistId;

            parent.isShowOnlyPlaylist = true;
            //parent.SetPlaylistMode(PlaylistName);
            parent.SetPlaylistMode(basePlaylistDetail, Library.PlaylistModeEnum.ViewPlaylist);
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            Global.AppActionMode = Global.AppActionModeEnum.Playing;
            Global.mPlayer.StartPlaylist(PlaylistName, false);

            Global.MainWindow.HideMenu();
        }

        private void btnAddNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            //Global.mPlayer.AddToPlaylist();
            // get playlist tracks
            // clear now playing
            // add to now playing
            // add new playlist to now playing

            // is idle
            if (Global.mPlayer.isIdle)
            {
                Global.mPlayer.StartPlaylist(PlaylistName, false);
                Global.MainWindow.HideMenu();
            }
            else
            {
                if (Global.mPlayer.playList.PlayListId != Global.NowPlayingId)
                {
                    // Not now playing
                    // ---------------
                    var currentPlayId = Global.mPlayer.playList.PlayListId;
                    var currentPlayingTrack = Global.mPlayer.playList.PlayIndex;

                    Global.mPlayer.dbClearNowPlayingPlaylist();
                    Global.mPlayer.playList = new MainPlayer.PlayList();

                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var a = from t in db.Playlists.Where(a1 => a1.PlaylistId == currentPlayId)
                                select t.TrackLibrary;
                        Global.mPlayer.playList.AddRange(a);

                        var b = from t1 in db.Playlists.Where(b1 => b1.PlaylistId == PlaylistId)
                                select t1.TrackLibrary;
                        Global.mPlayer.playList.AddRange(b);

                        Global.mPlayer.playList.PlayIndex = currentPlayingTrack;
                    }
                }
                else
                {
                    using (JukeboxBrainsDBEntities db = new JukeboxBrainsDBEntities())
                    {
                        var b = from t1 in db.Playlists.Where(b1 => b1.PlaylistId == PlaylistId)
                                select t1.TrackLibrary;

                        Global.mPlayer.playList.AddRange(b);
                    }
                }

            }
        }

        private void lblMore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle Buttons
            if (btnMode == Visibility.Collapsed)
                btnMode = Visibility.Visible;
            else
                btnMode = Visibility.Collapsed;

            ToggleButtons();
        }

        private Visibility btnMode = Visibility.Collapsed;
        private void ToggleButtons()
        {
            //btnAddNowPlaying.Visibility = btnMode;
            //btnDelete.Visibility = btnMode;
            //btnEdit.Visibility = btnMode;
            //btnRename.Visibility = btnMode;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Load Library in Edit Mode
            // need to check if currently playing
            Global.AppControlMode = Global.AppControlModeEnum.Playlist;
            Global.AppPlaylistModeBufferID = PlaylistId;

            // ClearBuffer
            Global.mPlayer.playList.CancelChanges();

            ((Library)Global.MainFrame.Content).SetPlaylistMode(basePlaylistDetail, Library.PlaylistModeEnum.EditPlaylist);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Delete playlist from DB
            Global.mPlayer.dbDeletePlaylist(PlaylistId);
            parent.ReloadLibrary();
            // need to check if currently playing
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            // Rename Playlist
            Global.isKeyboardFocus = true;
            Global.mPlayer.popUp_Grid.Visibility = Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_TextInput(Popup_TextInput.ControlMode.RenamePlaylist, PlaylistId);

            // TODO: need to check if currently playing
        }
    }
}
