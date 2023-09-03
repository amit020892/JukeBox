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
using System.Windows.Threading;
using iTunesSearch.Library;
using JukeBoxSolutions.Pages;

namespace JukeBoxSolutions.Controls
{
    /// <summary>
    /// Interaction logic for LibraryCardSong.xaml
    /// </summary>
    public partial class LibraryCardSong2 : UserControl
    {
        //SongLibrary song { get; set; }
        public bool? IsButtonActive
        {
            get { return baseButton.IsChecked; }
            set { baseButton.IsChecked = value; }
        }

        TrackLibrary baseTrack { get; set; }
        int PlaylistId { get; set; }
        private int? PlaylistSequenceNumber { get; set; }
        private LibraryPopup ParentAlbum { get; set; }

        //public LibraryCardSong(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)

        public LibraryCardSong2(TrackLibrary trackLibrary, string trackNumber = "", LibraryPopup parentAlbum = null)
        {
            InitializeComponent();
            //ParentAlbum = parentAlbum;
            //lblTrackNo.Content = trackNumber;
            //baseTrack = trackLibrary;


            //lblTrackName.Content = trackLibrary.SongLibraries.First().SongName;
            //if (trackLibrary.SongLibraries.First().Artists.Any())
            //    lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;


            //// Disabled for speed
            ////InitializeDetails();


            //PlaylistId = Global.NowPlayingId;
            
            //// Disabled for speed
            ////ButtonSetup();
        }

        public void InitializeDetails()
        {
            // Is Now Playing
            //bool isNowPlaying = trackLibrary.Playlists.Where(w => w.PlaylistId == Global.mPlayer.playList.PlayListId).Any();
            bool isNowPlaying = false;
            SetAppActionMode(isNowPlaying, Global.LibraryForceViewMode);

            if (isNowPlaying && ParentAlbum != null)
                ParentAlbum.SongActiveCheckIn();
        }

        public void ButtonSetup()
        {
            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && Global.LibraryForceViewMode == false)
            {
                PlaylistId = Global.AppPlaylistModeBufferID;

                if (baseTrack.Playlists.Where(w => w.PlaylistId == PlaylistId).Any())
                {
                    btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                    btnAddToPlaylistx.Tag = "hide";
                }
                else
                    btnAddToPlaylistx.Visibility = Visibility.Visible;
            }
            else
            {
                btnAddToPlaylistx.Visibility = Visibility.Collapsed;
                btnAddToPlaylistx.Tag = "hide";
            }


            if (Global.AppControlMode == Global.AppControlModeEnum.Admin)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnRemove.Visibility = Visibility.Visible;
            }
            else
            {
                btnEdit.Visibility = Visibility.Collapsed;
                btnEdit.Tag = "hide";
            }

            btnRemove.Visibility = Visibility.Collapsed;
            btnRemove.Tag = "hide";


            // Manual Application Mode
            //btnPlayTrack.Visibility = Visibility.Visible;
            //btnRemove.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// Used for Playlist Mode
        /// </summary>
        /// <param name="trackLibrary"></param>
        /// <param name="isPlaying"></param>
        /// <param name="trackNumber"></param>
        public LibraryCardSong2(TrackLibrary trackLibrary, bool isPlaying, int playlistSequence)
        {
            bool showPlaylistIndexNumber = true;
            InitializeComponent();
            baseTrack = trackLibrary;
            if (showPlaylistIndexNumber)
                lblTrackName.Content = playlistSequence.ToString() + " : " + trackLibrary.SongLibraries.First().SongName;
            else
                lblTrackName.Content = trackLibrary.SongLibraries.First().SongName;
            if (trackLibrary.SongLibraries.First().Artists.Any())
                lblArtistName.Content = trackLibrary.SongLibraries.First().Artists.First().ArtistName;
            else
                lblArtistName.Content = "";
            lblTrackNo.Content = (playlistSequence + 1).ToString();
            PlaylistSequenceNumber = playlistSequence;
            IsButtonActive = isPlaying;

            // Manual Application Mode
            btnPlayTrack.Visibility = Visibility.Visible;
            btnRemove.Visibility = Visibility.Visible;

            btnAddToPlaylist.Visibility = Visibility.Collapsed;
            btnAddToPlaylist.Tag = "hide";
            btnEdit.Visibility = Visibility.Collapsed;
            btnEdit.Tag = "hide";
            btnPlayNow.Visibility = Visibility.Collapsed;
            btnPlayNow.Tag = "hide";
            btnAddToPlaylistx.Visibility = Visibility.Collapsed;
            btnAddToPlaylistx.Tag = "hide";
        }

        public void SetAppActionMode(bool isNowPlaying = false, bool forceLibraryViewMode = false)
        {
            //default
            CardMode = CardModeEnum.NormalPlay;

            if (Global.AppControlMode == Global.AppControlModeEnum.Playlist && forceLibraryViewMode == false)
            {
                if (IsButtonActive.Value)
                    CardMode = CardModeEnum.RemoveFromPlaylist;
                else
                    CardMode = CardModeEnum.AddToPlaylist;
            }
            else if (Global.AppActionMode == Global.AppActionModeEnum.Idle)
            {
                CardMode = CardModeEnum.NormalPlay;
            }
            else
            {
                if (IsButtonActive.Value || isNowPlaying == true)
                    CardMode = CardModeEnum.RemoveFromPlaylist;
                else
                    CardMode = CardModeEnum.AddToPlaylist;
            }

            setMode();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistSequenceNumber == null)
            {
                Global.mPlayer.PlayNow(baseTrack);
                Global.AppActionMode = Global.AppActionModeEnum.Playing;

                CardMode = CardModeEnum.RemoveFromPlaylist;
                IsButtonActive = true;

                if (ParentAlbum != null)
                {
                    ParentAlbum.UpdateAppActionMode();
                }

                Global.LibraryUpdateActionMode(ParentAlbum.baseAlbum.AlbumId);
            }
            else
            {
                // Index vs Sequence
                Global.mPlayer.PlayPlaylistIndex(PlaylistSequenceNumber.Value);
            }
        }

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {

            int i = Global.AppPlaylistModeBufferID;
            int i2 = Global.BufferPlaylistID;

            if (Global.AppPlaylistModeBufferID > 0 && Global.LibraryForceViewMode == false)
            {
                int i3 = PlaylistId;
                Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.AppPlaylistModeBufferID);
                Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = true;
            }
            else
            {
                Global.mPlayer.playList.Add(baseTrack);
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = true;
            }

            //var x = Global.mPlayer.playList.PlayListId;
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
            CardMode = CardModeEnum.RemoveFromPlaylist;
            setMode();
        }
        private void setMode()
        {
            switch (CardMode)
            {
                case CardModeEnum.NormalPlay:
                    // Set Play Mode
                    btnPlayTrack.Visibility = Visibility.Visible;
                    btnAddToPlaylist.Visibility = Visibility.Collapsed;
                    btnRemoveFromPlaylist.Visibility = Visibility.Collapsed;
                    break;
                case CardModeEnum.AddToPlaylist:
                    // Set Add / Append Mode
                    btnPlayTrack.Visibility = Visibility.Collapsed;
                    btnAddToPlaylist.Visibility = Visibility.Visible;
                    btnRemoveFromPlaylist.Visibility = Visibility.Collapsed;
                    break;
                case CardModeEnum.RemoveFromPlaylist:
                    btnPlayTrack.Visibility = Visibility.Collapsed;
                    btnAddToPlaylist.Visibility = Visibility.Collapsed;
                    btnRemoveFromPlaylist.Visibility = Visibility.Visible;
                    break;
            }
        }

        private CardModeEnum CardMode = CardModeEnum.NormalPlay;
        public enum CardModeEnum
        {
            NormalPlay,
            AddToPlaylist,
            RemoveFromPlaylist
        }

        private void btnUpdateSong_Click(object sender, RoutedEventArgs e)
        {
            iTunesSearchManager ism = new iTunesSearchManager();

            ism.UpdateTrack2(baseTrack.SongLibraries.First());
            //ism.UpdateAlbumTrackAsync(song);
        }

        private void btnAddToPlaylistx_Click(object sender, RoutedEventArgs e)
        {
            // Glow to indicate playlist thingy
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, PlaylistId);
            Global.mPlayer.dbAddTrackToPlaylist(baseTrack, Global.BufferPlaylistID);
            btnAddToPlaylistx.Visibility = Visibility.Collapsed;
        }

        private void btnExpand_ClickX(object sender, RoutedEventArgs e)
        {
            btnExpand.Visibility = Visibility.Collapsed;
        }

        public void BuildBButtons()
        {

        }

        private void btnExpand_Click(object sender, MouseButtonEventArgs e)
        {
            //btnExpand.Visibility = Visibility.Collapsed;
            //btnAddToPlaylistx.Visibility = Visibility.Visible;
            //stackInfo.Visibility = Visibility.Visible;

            //if (Global.ButtonBuffer_StackPannel != null)
            //    Global.ButtonBuffer_StackPannel.Visibility = Visibility.Collapsed;
            //if (Global.ButtonBuffer_CoverButton != null)
            //    Global.ButtonBuffer_CoverButton.Visibility = Visibility.Visible;

            //Global.ButtonBuffer_StackPannel = stackInfo;
            //Global.ButtonBuffer_CoverButton = btnExpand;
            ShowButtons();
        }

        private void btnRemoveFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (Global.mPlayer.playList.Remove(baseTrack))
            {
                IsButtonActive = false;
                Global.LibraryUpdateActionMode();
            }
            else
            {
                CardMode = CardModeEnum.AddToPlaylist;
                setMode();
                if (ParentAlbum != null)
                    ParentAlbum.SetBaseAlbumSelected = false;
            }
        }


        public void ShowButtons()
        {
            btnAddToPlaylistx.Visibility = Visibility.Visible;

            btnExpand.Visibility = Visibility.Collapsed;
            stackInfo.Visibility = Visibility.Visible;
            foreach (var b in stackInfo.Children.OfType<Button>())
            {
                b.Visibility = Visibility.Collapsed;
            }

            if (Global.ButtonBuffer_StackPannel != null)
                Global.ButtonBuffer_StackPannel.Visibility = Visibility.Collapsed;
            if (Global.ButtonBuffer_CoverButton != null)
                Global.ButtonBuffer_CoverButton.Visibility = Visibility.Visible;

            Global.ButtonBuffer_StackPannel = stackInfo;
            Global.ButtonBuffer_CoverButton = btnExpand;

            showbutton();
        }

        public void unpackButtons(Visibility v, int buttoncount)
        {

            for (int i = buttoncount - 1; i >= 0; i--)
            {
                Thread.Sleep(30);
                //string s = ((Button)stackInfo.Children[i]).Tag == null ? "" : ((Button)stackInfo.Children[i]).Tag.ToString();

                this.Dispatcher.Invoke(() =>
                {
                    string s = ((Button)stackInfo.Children[i]).Tag == null ? "" : ((Button)stackInfo.Children[i]).Tag.ToString();
                    if (s == "hide")
                    {
                        ((Button)stackInfo.Children[i]).Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ((Button)stackInfo.Children[i]).Visibility = v;
                    }
                });
            }

        }

        private void showbutton()
        {
            int i = stackInfo.Children.Count;

            new Task(() => { unpackButtons(Visibility.Visible, i); }).Start();
        }

        public void hideButtons()
        {
            int i = stackInfo.Children.Count;
            new Task(() => { unpackButtons(Visibility.Collapsed, i); }).Start();
        }

        #region INotifyPropertyChanged Members


        public static void Delay(int milliseconds, Action action)
        {
            var t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            t.Tick += (o, e) => { t.Stop(); action.Invoke(); };
            t.Start();
        }


        #endregion

        private void btnPlayNow_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.PlayNow(baseTrack);
            Global.AppActionMode = Global.AppActionModeEnum.Playing;
            if (ParentAlbum != null)
                ParentAlbum.UpdateAppActionMode();

            CardMode = CardModeEnum.RemoveFromPlaylist;
            IsButtonActive = true;
            Global.LibraryUpdateActionMode();
        }

        private void btnQueNow_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistSequenceNumber == null)
            {
                Global.mPlayer.PlayNext(baseTrack);
            }
            else
            {
                Global.mPlayer.RemoveFromPlaylist(baseTrack);
                Global.mPlayer.PlayNext(baseTrack);

                if (Global.mPlayer.popUp_Frame.Content.GetType().Name == "PlayListView")
                {
                    PlayListView p = (PlayListView)Global.mPlayer.popUp_Frame.Content;
                    p.LoadPlaylist();
                }
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            // Editor?
            // Smart Updater?
            // Popup Smart Updater
            Global.mPlayer.popUp_Grid.Visibility = System.Windows.Visibility.Visible;
            Global.mPlayer.popUp_Frame.Content = new Popup_SmartUpdater(baseTrack);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Confirm?
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            Global.mPlayer.RemoveFromPlaylist(baseTrack);
            // Reload Playlist
            string s = Global.mPlayer.popUp_Frame.Content.GetType().Name;

            if (Global.mPlayer.popUp_Frame.Content.GetType().Name == "PlayListView")
            {
                PlayListView p = (PlayListView)Global.mPlayer.popUp_Frame.Content;
                p.LoadPlaylist();
            }
        }
    }
}
